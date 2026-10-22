using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// This system manages main camera (movement, rotation, focus on/follow agent...)
/// </summary>
public class CameraSystem : FSystem {
	// In games agents 
	private readonly Family f_agent = FamilyManager.getFamily(new AnyOfTags("Player", "Drone"));
	// Contains current UI focused
	private readonly Family f_UIfocused = FamilyManager.getFamily(new AllOfComponents(typeof(RectTransform), typeof(PointerOver)));
	private readonly Family f_focusOn = FamilyManager.getFamily(new AllOfComponents(typeof(FocusCamOn)));
	private readonly Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private readonly Family f_dragging = FamilyManager.getFamily(new AllOfComponents(typeof(Dragging)));
	private readonly Family f_billboard = FamilyManager.getFamily(new AllOfComponents(typeof(BillboardOrientation)));
    private readonly Family f_fadeOutEnd = FamilyManager.getFamily(new AllOfComponents(typeof(FadeOutEnd)));
    private readonly Family f_omniscientFocus = FamilyManager.getFamily(new AnyOfTags("Player", "Drone", "Exit", "Door", "Terminal"));

    private Transform targetAgent; // if defined camera follow this target
	private Transform lastAgentFocused = null;
	private Vector3 targetPos; // if defined camera focus in this position (grid definition)
	private Vector3 offset = new Vector3(0, 2f, 0);

	private Camera mainCamera;

	private float UI_frontBackValue = 0;
	private float UI_leftRightValue = 0;
	private float UI_rotateValue = 0;
    private float UI_pitchingValue = 0; // tangage
    private float UI_zoomValue = 0;

	private float lastTimeCameraAction = Time.time;
	private float logging_padding = 5f;
	private string lastActionLogged = "";

    private InputAction middleClick;
	private InputAction rightClick;

	// Déplacement aux touches du clavier
	public float cameraMovingSpeed;
	// Rotation au clic droit
	public float cameraRotationSpeed;
	// Vitesse de Zoom
	public float cameraZoomSpeed;
	// Distance minimale de zoom
	public float cameraZoomMin;
	// Distance maximale de zoom
	public float cameraZoomMax;
	// Déplacement avec la molette
	public float dragSpeed;

    public CurrentSettingsValues currentSettingsValues;

	public GameObject dialogPanel;
	public RectTransform LeftPanel;
	public RectTransform ExecutableCanvas;

	public static CameraSystem instance;

	public CameraSystem()
    {
		instance = this;
    }

	protected override void onStart()
	{
		middleClick = InputSystem.actions.FindAction("MiddleClick");
		rightClick = InputSystem.actions.FindAction("RightClick");

        mainCamera = Camera.main;
		if (currentSettingsValues.values.currentGameView == 1)
			ToggleOrthographicPerspective();

		// set current camera target (the first player)
		f_agent.addEntryCallback(delegate (GameObject go) { if (go.CompareTag("Player")) FocusOnAgent(go, false); });

		f_focusOn.addEntryCallback(delegate (GameObject go)
		{
			FocusCamOn newTarget = go.GetComponent<FocusCamOn>();
			UnfocusAgent();
			targetPos = new Vector3(newTarget.camX * 3, 3.5f, -newTarget.camY * 3);
			MainLoop.instance.StartCoroutine(TravelingOnPos());
			GameObjectManager.removeComponent(newTarget);
		});

		f_playingMode.addEntryCallback(delegate (GameObject go) {
			if (currentSettingsValues.values.currentCameraTracking == 1)
				FocusOnNearestAgent();
			else
				UnfocusAgent();
		});

		EnhancedTouchSupport.Enable();

		// Activer ou pas la vue omnisciente
        GameObject go = GameObject.Find("GameData");
		if (go != null)
		{
			GameData gameData = go.GetComponent<GameData>();
			if (gameData != null && gameData.omniscientView)
				MainLoop.instance.StartCoroutine(WaitDialogAppearsToEnableOmniscientView());
		}
    }

	private IEnumerator WaitDialogAppearsToEnableOmniscientView()
	{
		// On attend que l'animation d'arrivée dans le jeu soit terminée
		yield return new WaitWhile(() => f_fadeOutEnd.Count == 0);
        // On attend 2 frames pour que le dialogue de début de niveau soit affiché s'il devait y en avoir un
        yield return null;
        yield return null;
        // Attendre que le dialogue soit fermé avant de mettre la vue omnisciente
        yield return new WaitWhile (() => dialogPanel.activeInHierarchy);
		
        UnfocusAgent();
        // calculer le barycentre des agents pour centrer la caméra
		float sumX = 0;
		float sumY = 0;
        foreach (GameObject obj in f_omniscientFocus)
		{
			Position pos = obj.GetComponent<Position>();
			sumX += pos.x;
			sumY += pos.y;
        }
        targetPos = new Vector3(sumX / f_omniscientFocus.Count * 3, 3.5f, -sumY / f_omniscientFocus.Count * 3);
        yield return TravelingOnPos();
		// Dézoomer la caméra jusqu'à ce que tous les agents soient visibles
		while (!AllImportantObjectsInview()) { 
			ZoomOut(0.1f, false);
            yield return null;
        }
    }

	private bool AllImportantObjectsInview()
	{
		bool visible = true;
        float leftPanelRatio = (LeftPanel.rect.width+25) / Screen.width * currentSettingsValues.values.currentUIScale;
        float rightPanelRatio = (ExecutableCanvas.rect.width+25) / Screen.width * currentSettingsValues.values.currentUIScale;

		//Debug.Log(LeftPanel.rect.width+" "+(LeftPanel.rect.width / Screen.width) + " " + leftPanelRatio);

        foreach (GameObject obj in f_omniscientFocus)
		{
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(obj.transform.position);
            visible = visible && viewportPos.z > 0 && viewportPos.x >= leftPanelRatio && viewportPos.x <= 1f-rightPanelRatio && viewportPos.y >= 0.1 && viewportPos.y <= 0.9;
        }
		return visible;
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
        // move camera front/back depending on Vertical axis
        if (UI_frontBackValue != 0)
			MoveFrontBack(UI_frontBackValue);

		// move camera left/right depending on Horizontal axis
		if (UI_leftRightValue != 0)
			MoveLeftRight(UI_leftRightValue);

		// rotate camera (right/left)
		if (UI_rotateValue != 0)
			RotateCamera(UI_rotateValue, 0);

        // rotate camera (up/down) if not in orthographic mode
        if (UI_pitchingValue != 0 && !mainCamera.orthographic)
            RotateCamera(0, UI_pitchingValue);

        // Move camera with wheel click
        if (middleClick.IsPressed() && f_UIfocused.Count == 0)
		{
			UnityEngine.Cursor.visible = false;

			DeltaControl delta = Pointer.current.delta;

			MoveFrontBack(-delta.y.value * dragSpeed);
			MoveLeftRight(-delta.x.value * dragSpeed);
			UnfocusAgent();
		}
		// Zoom with scroll wheel only if UI element is not focused
		else if ((Mouse.current.scroll.y.value < 0 && f_UIfocused.Count == 0) || UI_zoomValue > 0) // Zoom out
		{
			if (UI_zoomValue > 0)
				ZoomOut(UI_zoomValue);
			else
				ZoomOut(2);
		}
		else if ((Mouse.current.scroll.y.value > 0 && f_UIfocused.Count == 0) || UI_zoomValue < 0) // Zoom in
		{
			if (UI_zoomValue < 0)
				ZoomIn(-UI_zoomValue);
			else
				ZoomIn(2);
		}

		// Orbit rotation
		else if (rightClick.IsPressed() && f_UIfocused.Count == 0)
        {
            UnityEngine.Cursor.visible = false;
			DeltaControl delta = Pointer.current.delta;
			RotateCamera(delta.x.value/2, !mainCamera.orthographic ? delta.y.value/2 : 0);
		}

		if (middleClick.WasReleasedThisFrame() || rightClick.WasReleasedThisFrame())
            UnityEngine.Cursor.visible = true;

		// Gestion du déplacement de la camera au tactile
		if (f_UIfocused.Count == 0 && f_dragging.Count == 0)
		{
			ReadOnlyArray<Touch> touches = Touch.activeTouches;

			if (touches.Count == 1)
			{
				// Rotation
				if (touches[0].phase == UnityEngine.InputSystem.TouchPhase.Moved)
				{
					Vector2 delta = touches[0].delta;
					RotateCamera(delta.x / 4, !mainCamera.orthographic ? delta.y / 4 : 0);
				}
			}
			else if (touches.Count == 2)
			{
				// Distance actuelle et précédente
				float currentDistance = Vector2.Distance(touches[0].screenPosition, touches[1].screenPosition);
				float previousDistance = Vector2.Distance(
					touches[0].screenPosition - touches[0].delta,
					touches[1].screenPosition - touches[1].delta
				);

				float distanceDelta = currentDistance - previousDistance;

				// Zoom (pincement)
				if (Mathf.Abs(distanceDelta) > 0.01f)
				{
					if (distanceDelta > 0)
					{
						ZoomIn(distanceDelta / 32);
					}
					else
					{
						ZoomOut(-distanceDelta / 32);
					}
				}

				// Pan (déplacement à deux doigts)
				Vector2 move1 = touches[0].delta;
				Vector2 move2 = touches[1].delta;

				// Vérifie que les doigts bougent dans une direction similaire
				if (Vector2.Dot(move1.normalized, move2.normalized) > 0.7f)
				{
					Vector2 averageMove = (move1 + move2) / 2f;
					MoveFrontBack(-averageMove.y / 4 * dragSpeed);
					MoveLeftRight(-averageMove.x / 4 * dragSpeed);
				}
			}
		}

        // positionnement des billboards en fonction de la position de la caméra
        foreach (GameObject go in f_billboard)
		{
			if (currentSettingsValues.values.currentGameView == 0)
			{
				// Rotation Y du parent
				Vector3 dir = mainCamera.transform.position - go.transform.position;
				dir.y = 0f;

				if (dir.sqrMagnitude > 0.001f)
					go.transform.rotation = Quaternion.LookRotation(-dir);
			}
			else
			{
                go.transform.rotation = Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y-180f, 0f);
            }

            // Rotation X de l'enfant
            float pitch = mainCamera.transform.eulerAngles.x;
            if (pitch > 180f)
                pitch -= 360f;

            go.transform.GetChild(0).localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        // cela fait plus de 5 secondes que la dernière action de caméra a été effectuée, on réinitialise le dernier type d'action de caméra loggé pour éventuellement permettre de logger à nouveau le même type d'action si l'utilisateur recommence la même action après un certain temps
        if ((Time.time - lastTimeCameraAction) > logging_padding && lastActionLogged != "")
			lastActionLogged = "";
    }

    private void MoveFrontBack(float value)
    {
		Transform cameraTarget = mainCamera.transform.parent.parent;
		cameraTarget.Translate(-value * Time.deltaTime * cameraMovingSpeed, 0, 0);
		UnfocusAgent();

		if (lastActionLogged != "moving")
			Camera_logging("moving");
        lastActionLogged = "moving";

        lastTimeCameraAction = Time.time;
    }

	private void MoveLeftRight(float value)
    {
		Transform cameraTarget = mainCamera.transform.parent.parent;
		cameraTarget.Translate(0, 0, value * Time.deltaTime * cameraMovingSpeed);
		UnfocusAgent();

        if (lastActionLogged != "moving")
            Camera_logging("moving");
        lastActionLogged = "moving";

        lastTimeCameraAction = Time.time;
    }

	private void ZoomOut(float value, bool log = true)
    {
		// compute distance between camera position and camera target
		Vector3 relativePos = mainCamera.transform.InverseTransformPoint(mainCamera.transform.parent.parent.position);
		if (relativePos.z < cameraZoomMax)
		{
			mainCamera.transform.position -= new Vector3(value * mainCamera.transform.forward.x * cameraZoomSpeed, value * mainCamera.transform.forward.y * cameraZoomSpeed, value * mainCamera.transform.forward.z * cameraZoomSpeed);
		}
		// sync orthographic view
		mainCamera.orthographicSize += value;
		if (mainCamera.orthographicSize > cameraZoomMax / 2)
			mainCamera.orthographicSize = cameraZoomMax / 2;

        if (lastActionLogged != "zooming" && log)
            Camera_logging("zooming");
        lastActionLogged = "zooming";

        lastTimeCameraAction = Time.time;
    }

	private void ZoomIn(float value)
    {
		// compute distance between camera position and camera target
		Vector3 relativePos = mainCamera.transform.InverseTransformPoint(mainCamera.transform.parent.parent.position);
		if (relativePos.z > cameraZoomMin)
		{
			mainCamera.transform.position += new Vector3(value * mainCamera.transform.forward.x * cameraZoomSpeed, value * mainCamera.transform.forward.y * cameraZoomSpeed, value * mainCamera.transform.forward.z * cameraZoomSpeed);
		}
		// check if we don't go beyond the target
		relativePos = mainCamera.transform.InverseTransformPoint(mainCamera.transform.parent.parent.position);
		if (relativePos.z < 0)
			mainCamera.transform.localPosition = Vector3.zero;

		// sync orthographic view
		mainCamera.orthographicSize -= value;
		if (mainCamera.orthographicSize < cameraZoomMin / 4)
			mainCamera.orthographicSize = cameraZoomMin / 4;

        if (lastActionLogged != "zooming")
            Camera_logging("zooming");
        lastActionLogged = "zooming";

        lastTimeCameraAction = Time.time;
    }

	public void ToggleOrthographicPerspective()
    {
		SetOrthographicView(!mainCamera.orthographic);

		Camera_logging("ToggleOrthographicPerspective", mainCamera.orthographic.ToString());

        lastTimeCameraAction = Time.time;
    }

	public void SetOrthographicView(bool state)
	{
		if (mainCamera != null)
		{
			mainCamera.orthographic = state;
			mainCamera.transform.parent.rotation = new Quaternion(0, 0, 0, 0);
			if (mainCamera.orthographic)
				mainCamera.transform.parent.Rotate(Vector3.back, -27); // -27 is a magic constant to put camera in direction of ground
			currentSettingsValues.values.currentGameView = mainCamera.orthographic ? 1 : 0;
		}
	}

	public void Set_UIFrontBack(float value)
    {
		UI_frontBackValue = value;
	}

	public void Set_UILeftRight(float value)
	{
		UI_leftRightValue = value;
	}

	public void Set_UIRotate(float value)
	{
		UI_rotateValue = value;
    }

    public void Set_UIPitching(float value)
    {
        UI_pitchingValue = value;
    }

    public void Set_UIZoom(float value)
	{
		UI_zoomValue = value;
	}

	public void SubmitRotate(float value)
    {
		RotateCamera(value, 0);
    }

    public void SubmitPitching(float value)
    {
        RotateCamera(0, value);
    }

    public void SubmitFrontBack(float value)
	{
		MoveFrontBack(value);
	}

	public void SubmitLeftRight(float value)
    {
		MoveLeftRight(value);
	}

	public void SubmitZoom(float value)
    {
		if (value > 0)
			ZoomOut(value);
		else
			ZoomIn(-value);
	}

	private void RotateCamera(float x, float y)
	{
		float angle = 90f * cameraRotationSpeed * Time.deltaTime;
		mainCamera.transform.parent.parent.Rotate(Vector3.up, angle * x );
		mainCamera.transform.parent.Rotate(Vector3.back, angle * y );
		float verticalAngle = Mathf.DeltaAngle(0f, mainCamera.transform.parent.eulerAngles.z)+60; // +60 pour compenser l'angle par défaut de la caméra
        if (verticalAngle < 0f || verticalAngle > 90f)
			// cancel previous y rotation
			mainCamera.transform.parent.Rotate(Vector3.back, angle * -y );

        if (lastActionLogged != "rotating")
            Camera_logging("rotating");
		lastActionLogged = "rotating";

        lastTimeCameraAction = Time.time;
    }

	public void FocusOnAgent(GameObject agent, bool log=true)
    {
		UnfocusAgent();
		targetAgent = agent.transform;
		lastAgentFocused = targetAgent;
		GameObjectManager.setGameObjectParent(mainCamera.transform.parent.parent.gameObject, agent, true);
		targetAgent.Find("HaloSelection").gameObject.SetActive(true);
		MainLoop.instance.StartCoroutine(TravelingOnAgent());

		if (log)
			Camera_logging("focusOnAgent", agent.ToString());

        lastTimeCameraAction = Time.time;
    }

	private void FocusOnNearestAgent()
    { 
		if (targetAgent != null)
			return;
		Transform cameraTarget = mainCamera.transform.parent.parent;
		float minDistance = float.MaxValue;
		GameObject agentCandidate = null;
		foreach (GameObject agent in f_agent) {
			float localDistance = Vector3.Distance(cameraTarget.position, agent.transform.position);
			if (localDistance < minDistance && agent.CompareTag("Player"))
			{
				agentCandidate = agent;
				minDistance = localDistance;
			}
		}
		if (agentCandidate != null)
			FocusOnAgent(agentCandidate);
	}

	public void FocusNextAgent()
    {
		if (lastAgentFocused == null)
		{
			FocusOnNearestAgent();
			return;
		}

		for (int i = 0; i < f_agent.Count; i++)
        {
			if (f_agent.getAt(i).transform == lastAgentFocused)
			{
				if (i == f_agent.Count - 1)
				{
					FocusOnAgent(f_agent.First());
					break;
				}
				else
				{
					FocusOnAgent(f_agent.getAt(i + 1));
					break;
				}
			}
        }
    }

	private void UnfocusAgent()
    {
		if (targetAgent != null)
        {
			GameObjectManager.setGameObjectParent(mainCamera.transform.parent.parent.gameObject, targetAgent.parent.gameObject, true);
			targetAgent.Find("HaloSelection").gameObject.SetActive(false);
			targetAgent = null;
		}
    }

	private IEnumerator TravelingOnAgent()
    {
		float distance = 0;
		if (targetAgent != null)
			distance = Vector3.Distance(mainCamera.transform.parent.parent.position, targetAgent.position + offset);
		while (targetAgent != null && mainCamera.transform.parent.parent.position != targetAgent.position + offset)
		{
			mainCamera.transform.parent.parent.position = Vector3.MoveTowards(mainCamera.transform.parent.parent.position, targetAgent.position + offset, distance*Time.deltaTime);
			if (!mainCamera.orthographic)
				mainCamera.transform.LookAt(mainCamera.transform.parent.parent);
			yield return null;
		}
	}

	private IEnumerator TravelingOnPos()
	{
		float distance = Vector3.Distance(mainCamera.transform.parent.parent.localPosition, targetPos);
		while (targetAgent == null && mainCamera.transform.parent.parent.localPosition != targetPos)
		{
			mainCamera.transform.parent.parent.localPosition = Vector3.MoveTowards(mainCamera.transform.parent.parent.localPosition, targetPos, distance*Time.deltaTime);
			yield return null;
		}
	}

	private void Camera_logging(string camera_action, string param = ""){
        Dictionary<string, string> actExt = new Dictionary<string, string>();
		actExt["value"] = camera_action;
		if (param != "")
			actExt["content"] = param;

        GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
		{
			verb = "interacted",
			objectType = "camera",
			activityExtensions = actExt
        });
	}
}