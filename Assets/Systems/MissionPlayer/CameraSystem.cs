using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UIElements;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// This system manages main camera (movement, rotation, focus on/follow agent...)
/// </summary>
public class CameraSystem : FSystem {
	// In games agents 
	private Family f_agent = FamilyManager.getFamily(new AnyOfTags("Player", "Drone"));
	// Contains current UI focused
	private Family f_UIfocused = FamilyManager.getFamily(new AllOfComponents(typeof(RectTransform), typeof(PointerOver)));
	private Family f_focusOn = FamilyManager.getFamily(new AllOfComponents(typeof(FocusCamOn)));
	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family f_dragging = FamilyManager.getFamily(new AllOfComponents(typeof(Dragging)));
	private Family f_billboard = FamilyManager.getFamily(new AllOfComponents(typeof(BillboardOrientation)));
    private Family f_fadeOutEnd = FamilyManager.getFamily(new AllOfComponents(typeof(FadeOutEnd)));
    private Family f_omniscientFocus = FamilyManager.getFamily(new AnyOfTags("Player", "Drone", "Exit", "Door", "Terminal"));

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

	public LocalizeStringEvent lseMoveUp;
	public LocalizeStringEvent lseMoveLeft;
    public LocalizeStringEvent lseTurnUp;
    public LocalizeStringEvent lseTurnLeft;

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

		// synchronise le contenu des tooltip en fonction du clavier utilisé (azerty vs qwerty)
		(lseMoveUp.StringReference["shortcut"] as StringVariable).Value = InputSystem.actions.FindAction("CameraMoveUp").GetBindingDisplayString(0);
		lseMoveUp.RefreshString();
		(lseMoveLeft.StringReference["shortcut"] as StringVariable).Value = InputSystem.actions.FindAction("CameraMoveLeft").GetBindingDisplayString(0);
		lseMoveLeft.RefreshString();
        (lseTurnUp.StringReference["shortcut"] as StringVariable).Value = InputSystem.actions.FindAction("CameraRotateUp").GetBindingDisplayString(0);
        lseTurnUp.RefreshString();
        (lseTurnLeft.StringReference["shortcut"] as StringVariable).Value = InputSystem.actions.FindAction("CameraRotateLeft").GetBindingDisplayString(0);
        lseTurnLeft.RefreshString();

        mainCamera = Camera.main;
		if (currentSettingsValues.values.currentGameView == 1)
			ToggleOrthographicPerspective();

		// set current camera target (the first player)
		f_agent.addEntryCallback(delegate (GameObject go) { if (go.CompareTag("Player")) focusOnAgent(go, false); });

		f_focusOn.addEntryCallback(delegate (GameObject go)
		{
			FocusCamOn newTarget = go.GetComponent<FocusCamOn>();
			unfocusAgent();
			targetPos = new Vector3(newTarget.camX * 3, 3.5f, -newTarget.camY * 3);
			MainLoop.instance.StartCoroutine(travelingOnPos());
			GameObjectManager.removeComponent(newTarget);
		});

		f_playingMode.addEntryCallback(delegate (GameObject go) {
			if (currentSettingsValues.values.currentCameraTracking == 1)
				focusOnNearestAgent();
			else
				unfocusAgent();
		});

		EnhancedTouchSupport.Enable();

		// Activer ou pas la vue omnisciente
        GameObject go = GameObject.Find("GameData");
		if (go != null)
		{
			GameData gameData = go.GetComponent<GameData>();
			if (gameData != null && gameData.omniscientView)
				MainLoop.instance.StartCoroutine(waitDialogAppearsToEnableOmniscientView());
		}
    }

	private IEnumerator waitDialogAppearsToEnableOmniscientView()
	{
		// On attend que l'animation d'arrivée dans le jeu soit terminée
		while (f_fadeOutEnd.Count == 0)
            yield return null;
        // On attend 2 frames pour que le dialogue de début de niveau soit affiché s'il devait y en avoir un
        yield return null;
        yield return null;
        // Attendre que le dialogue soit fermé avant de mettre la vue omnisciente
        while (dialogPanel.activeInHierarchy)
            yield return null;
		
        unfocusAgent();
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
        yield return travelingOnPos();
		// Dézoomer la caméra jusqu'à ce que tous les agents soient visibles
		while (!allImportantObjectsInview()) { 
			zoomOut(0.1f, false);
            yield return null;
        }
    }

	private bool allImportantObjectsInview()
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
			moveFrontBack(UI_frontBackValue);

		// move camera left/right depending on Horizontal axis
		if (UI_leftRightValue != 0)
			moveLeftRight(UI_leftRightValue);

		// rotate camera (right/left)
		if (UI_rotateValue != 0)
			rotateCamera(UI_rotateValue, 0);

        // rotate camera (up/down) if not in orthographic mode
        if (UI_pitchingValue != 0 && !mainCamera.orthographic)
            rotateCamera(0, UI_pitchingValue);

        // Move camera with wheel click
        if (middleClick.IsPressed() && f_UIfocused.Count == 0)
		{
			UnityEngine.Cursor.visible = false;

			DeltaControl delta = Pointer.current.delta;

			moveFrontBack(-delta.y.value * dragSpeed);
			moveLeftRight(-delta.x.value * dragSpeed);
			unfocusAgent();
		}
		// Zoom with scroll wheel only if UI element is not focused
		else if ((Mouse.current.scroll.y.value < 0 && f_UIfocused.Count == 0) || UI_zoomValue > 0) // Zoom out
		{
			if (UI_zoomValue > 0)
				zoomOut(UI_zoomValue);
			else
				zoomOut(2);
		}
		else if ((Mouse.current.scroll.y.value > 0 && f_UIfocused.Count == 0) || UI_zoomValue < 0) // Zoom in
		{
			if (UI_zoomValue < 0)
				zoomIn(-UI_zoomValue);
			else
				zoomIn(2);
		}

		// Orbit rotation
		else if (rightClick.IsPressed() && f_UIfocused.Count == 0)
        {
            UnityEngine.Cursor.visible = false;
			DeltaControl delta = Pointer.current.delta;
			rotateCamera(delta.x.value/2, !mainCamera.orthographic ? delta.y.value/2 : 0);
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
					rotateCamera(delta.x / 4, !mainCamera.orthographic ? delta.y / 4 : 0);
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
						zoomIn(distanceDelta / 32);
					}
					else
					{
						zoomOut(-distanceDelta / 32);
					}
				}

				// Pan (déplacement à deux doigts)
				Vector2 move1 = touches[0].delta;
				Vector2 move2 = touches[1].delta;

				// Vérifie que les doigts bougent dans une direction similaire
				if (Vector2.Dot(move1.normalized, move2.normalized) > 0.7f)
				{
					Vector2 averageMove = (move1 + move2) / 2f;
					moveFrontBack(-averageMove.y / 4 * dragSpeed);
					moveLeftRight(-averageMove.x / 4 * dragSpeed);
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

    private void moveFrontBack(float value)
    {
		Transform cameraTarget = mainCamera.transform.parent.parent;
		cameraTarget.Translate(-value * Time.deltaTime * cameraMovingSpeed, 0, 0);
		unfocusAgent();

		if (lastActionLogged != "moving")
			camera_logging("moving");
        lastActionLogged = "moving";

        lastTimeCameraAction = Time.time;
    }

	private void moveLeftRight(float value)
    {
		Transform cameraTarget = mainCamera.transform.parent.parent;
		cameraTarget.Translate(0, 0, value * Time.deltaTime * cameraMovingSpeed);
		unfocusAgent();

        if (lastActionLogged != "moving")
            camera_logging("moving");
        lastActionLogged = "moving";

        lastTimeCameraAction = Time.time;
    }

	private void zoomOut(float value, bool log = true)
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
            camera_logging("zooming");
        lastActionLogged = "zooming";

        lastTimeCameraAction = Time.time;
    }

	private void zoomIn(float value)
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
            camera_logging("zooming");
        lastActionLogged = "zooming";

        lastTimeCameraAction = Time.time;
    }

	public void ToggleOrthographicPerspective()
    {
		setOrthographicView(!mainCamera.orthographic);

		camera_logging("ToggleOrthographicPerspective", mainCamera.orthographic.ToString());

        lastTimeCameraAction = Time.time;
    }

	public void setOrthographicView(bool state)
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

	public void set_UIFrontBack(float value)
    {
		UI_frontBackValue = value;
	}

	public void set_UILeftRight(float value)
	{
		UI_leftRightValue = value;
	}

	public void set_UIRotate(float value)
	{
		UI_rotateValue = value;
    }

    public void set_UIPitching(float value)
    {
        UI_pitchingValue = value;
    }

    public void set_UIZoom(float value)
	{
		UI_zoomValue = value;
	}

	public void submitRotate(float value)
    {
		rotateCamera(value, 0);
    }

    public void submitPitching(float value)
    {
        rotateCamera(0, value);
    }

    public void submitFrontBack(float value)
	{
		moveFrontBack(value);
	}

	public void submitLeftRight(float value)
    {
		moveLeftRight(value);
	}

	public void submitZoom(float value)
    {
		if (value > 0)
			zoomOut(value);
		else
			zoomIn(-value);
	}

	private void rotateCamera(float x, float y)
	{
		float angle = 90f * cameraRotationSpeed * Time.deltaTime;
		mainCamera.transform.parent.parent.Rotate(Vector3.up, angle * x );
		mainCamera.transform.parent.Rotate(Vector3.back, angle * y );
		float verticalAngle = Mathf.DeltaAngle(0f, mainCamera.transform.parent.eulerAngles.z)+60; // +60 pour compenser l'angle par défaut de la caméra
        if (verticalAngle < 0f || verticalAngle > 90f)
			// cancel previous y rotation
			mainCamera.transform.parent.Rotate(Vector3.back, angle * -y );

        if (lastActionLogged != "rotating")
            camera_logging("rotating");
		lastActionLogged = "rotating";

        lastTimeCameraAction = Time.time;
    }

	public void focusOnAgent(GameObject agent, bool log=true)
    {
		unfocusAgent();
		targetAgent = agent.transform;
		lastAgentFocused = targetAgent;
		GameObjectManager.setGameObjectParent(mainCamera.transform.parent.parent.gameObject, agent, true);
		GameObjectManager.setGameObjectState(targetAgent.Find("HaloSelection").gameObject, true);
		MainLoop.instance.StartCoroutine(travelingOnAgent());

		if (log)
			camera_logging("focusOnAgent", agent.ToString());

        lastTimeCameraAction = Time.time;
    }

	private void focusOnNearestAgent()
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
			focusOnAgent(agentCandidate);
	}

	public void focusNextAgent()
    {
		if (lastAgentFocused == null)
		{
			focusOnNearestAgent();
			return;
		}

		for (int i = 0; i < f_agent.Count; i++)
        {
			if (f_agent.getAt(i).transform == lastAgentFocused)
			{
				if (i == f_agent.Count - 1)
				{
					focusOnAgent(f_agent.First());
					break;
				}
				else
				{
					focusOnAgent(f_agent.getAt(i + 1));
					break;
				}
			}
        }
    }

	private void unfocusAgent()
    {
		if (targetAgent != null)
        {
			GameObjectManager.setGameObjectParent(mainCamera.transform.parent.parent.gameObject, targetAgent.parent.gameObject, true);
			GameObjectManager.setGameObjectState(targetAgent.Find("HaloSelection").gameObject, false);
			targetAgent = null;
		}
    }

	private IEnumerator travelingOnAgent()
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

	private IEnumerator travelingOnPos()
	{
		float distance = Vector3.Distance(mainCamera.transform.parent.parent.localPosition, targetPos);
		while (targetAgent == null && mainCamera.transform.parent.parent.localPosition != targetPos)
		{
			mainCamera.transform.parent.parent.localPosition = Vector3.MoveTowards(mainCamera.transform.parent.parent.localPosition, targetPos, distance*Time.deltaTime);
			yield return null;
		}
	}

	private void camera_logging(string camera_action, string param = ""){
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