using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This system manages main camera (movement, rotation, focus on/follow agent...)
/// </summary>
public class CameraSystem : FSystem {
	// In games agents controlable by the player
	private Family f_player = FamilyManager.getFamily(new AnyOfTags("Player"));
	// Contains current UI focused
	private Family f_UIfocused = FamilyManager.getFamily(new AllOfComponents(typeof(RectTransform), typeof(PointerOver)));
	private Family f_focusOn = FamilyManager.getFamily(new AllOfComponents(typeof(FocusCamOn)));
	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));

	private Transform targetAgent; // if defined camera follow this target
	private Transform lastAgentFocused = null;
	private Vector3 targetPos; // if defined camera focus in this position (grid definition)
	private Vector3 offset = new Vector3(0, 2f, 0);

	private Camera mainCamera;

	private float UI_frontBackValue = 0;
	private float UI_leftRightValue = 0;
	private float UI_rotateValue = 0;
	private float UI_zoomValue = 0;

	private float last_action_time = Time.time;
	private float logging_padding = 1f;

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

	public static CameraSystem instance;

	public CameraSystem()
    {
		instance = this;
    }

	protected override void onStart()
	{
		mainCamera = Camera.main;
		if (PlayerPrefs.GetInt("orthographicView", 0) == 1)
			ToggleOrthographicPerspective();

		// set current camera target (the first player)
		f_player.addEntryCallback(delegate (GameObject go) { focusOnAgent(go); });

		f_focusOn.addEntryCallback(delegate (GameObject go)
		{
			FocusCamOn newTarget = go.GetComponent<FocusCamOn>();
			unfocusAgent();
			targetPos = new Vector3(newTarget.camY * 3, 3.5f, newTarget.camX * 3);
			MainLoop.instance.StartCoroutine(travelingOnPos());
			GameObjectManager.removeComponent(newTarget);
		});

		f_playingMode.addEntryCallback(focusOnNearestAgent);
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		// move camera front/back depending on Vertical axis
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || UI_frontBackValue != 0) // set for qwerty
		{
			if (UI_frontBackValue == 0)
				moveFrontBack(Input.GetKey(KeyCode.W) ? 1 : -1);
			else
				moveFrontBack(UI_frontBackValue);
		}
		// move camera left/right de pending on Horizontal axis
		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || UI_leftRightValue != 0) // set for qwerty
		{
			if (UI_leftRightValue == 0)
				moveLeftRight(Input.GetKey(KeyCode.A) ? -1 : 1);
			else
				moveLeftRight(UI_leftRightValue);
		}

		// rotate camera with "Q" and "E" keys (qwerty keyboard)
		if (Input.GetKey(KeyCode.Q))
		{
			rotateCamera(1, 0);
			Debug.Log(KeyCode.Q);
		}
		else if (Input.GetKey(KeyCode.E))
			rotateCamera(-1, 0);
		else if (UI_rotateValue != 0)
			rotateCamera(UI_rotateValue, 0);

		// manage orthographic/perspective
		if (Input.GetKeyDown(KeyCode.V))
			ToggleOrthographicPerspective();

		// autofocus on nearest agent
		if (Input.GetKeyDown(KeyCode.C))
			focusOnNearestAgent(null);

		// Zoom in/out with keyboard
		if (Input.GetKey(KeyCode.R))
			zoomIn(0.1f);
		else if (Input.GetKey(KeyCode.F))
			zoomOut(0.1f);

		// Move camera with wheel click
		if (Input.GetMouseButton(2))
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			float mouseY = Input.GetAxisRaw("Mouse Y");
			float mouseX = Input.GetAxisRaw("Mouse X");

			float dist = Mathf.Abs(Mathf.Abs(mouseX) - Mathf.Abs(mouseY));
			if (Mathf.Abs(mouseY) > Mathf.Abs(mouseX))
			{
				mouseY = mouseY > 0 ? -1 : (mouseY < 0 ? 1 : 0);
				mouseX = (mouseX > 0 ? -dist : (mouseX < 0 ? dist : 0));
			}
			else if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY))
			{
				mouseY = mouseY > 0 ? -dist : (mouseY < 0 ? dist : 0);
				mouseX = (mouseX > 0 ? -1 : (mouseX < 0 ? 1 : 0));
			}
			else
			{
				mouseY = mouseY > 0 ? -1 : (mouseY < 0 ? 1 : 0);
				mouseX = (mouseX > 0 ? -1 : (mouseX < 0 ? 1 : 0));
			}

			if (mouseY != 0)
				moveFrontBack(mouseY * dragSpeed);
			if (mouseX != 0)
				moveLeftRight(mouseX * dragSpeed);
			unfocusAgent();
		}

		// Zoom with scroll wheel only if UI element is not focused
		else if ((Input.GetAxis("Mouse ScrollWheel") < 0 && f_UIfocused.Count == 0) || UI_zoomValue > 0) // Zoom out
		{
			if (UI_zoomValue > 0)
				zoomOut(UI_zoomValue);
			else
				zoomOut(1);
		}
		else if ((Input.GetAxis("Mouse ScrollWheel") > 0 && f_UIfocused.Count == 0) || UI_zoomValue < 0) // Zoom in
		{
			if (UI_zoomValue < 0)
				zoomIn(-UI_zoomValue);
			else
				zoomIn(1);
		}

		// Orbit rotation
		else if (Input.GetMouseButton(1))
		{
			rotateCamera(Input.GetAxis("Mouse X"), !mainCamera.orthographic ? Input.GetAxis("Mouse Y"): 0);
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}
	
	private void moveFrontBack(float value)
    {
		Transform cameraTarget = mainCamera.transform.parent.parent;
		cameraTarget.Translate(-value * Time.deltaTime * cameraMovingSpeed, 0, 0);
		unfocusAgent();

		camera_logging("moveFrontBack", value.ToString());
	}

	private void moveLeftRight(float value)
    {
		Transform cameraTarget = mainCamera.transform.parent.parent;
		cameraTarget.Translate(0, 0, value * Time.deltaTime * cameraMovingSpeed);
		unfocusAgent();

		camera_logging("moveLeftRight", value.ToString());
	}

	private void zoomOut(float value)
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

		camera_logging("zoomOut", value.ToString());
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

		camera_logging("zoomIn", value.ToString());
	}

	public void ToggleOrthographicPerspective()
    {
		mainCamera.orthographic = !mainCamera.orthographic;
		mainCamera.transform.parent.rotation = new Quaternion(0, 0, 0, 0);
		if (mainCamera.orthographic)
			mainCamera.transform.parent.Rotate(Vector3.back, -27); // -27 is a magic constant to put camera in direction of ground
		PlayerPrefs.SetInt("orthographicView", mainCamera.orthographic ? 1 : 0);

		camera_logging("ToggleOrthographicPerspective", mainCamera.orthographic.ToString());
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

	public void set_UIZoom(float value)
	{
		UI_zoomValue = value;
	}

	public void submitRotate(float value)
    {
		rotateCamera(value, 0);
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
		if (mainCamera.transform.position.y < 8f)
			// cancel previous y rotation
			mainCamera.transform.parent.Rotate(Vector3.back, angle * -y );
		else
			camera_logging("rotateCamera", angle.ToString());
	}

	public void focusOnAgent(GameObject agent)
    {
		targetAgent = agent.transform;
		lastAgentFocused = targetAgent;
		GameObjectManager.setGameObjectParent(mainCamera.transform.parent.parent.gameObject, agent, true);
		MainLoop.instance.StartCoroutine(travelingOnAgent());

		camera_logging("focusOnAgent", agent.ToString());
	}

	private void focusOnNearestAgent(GameObject unused)
    {
		if (targetAgent != null)
			return;
		Transform cameraTarget = mainCamera.transform.parent.parent;
		float minDistance = float.MaxValue;
		GameObject agentCandidate = null;
		foreach (GameObject agent in f_player) {
			float localDistance = Vector3.Distance(cameraTarget.position, agent.transform.position);
			if (localDistance < minDistance)
			{
				agentCandidate = agent;
				minDistance = localDistance;
			}
		}
		if (agentCandidate != null)
		{
			focusOnAgent(agentCandidate);
			camera_logging("focusOnNearestAgent", agentCandidate.ToString());
		}
	}

	public void focusNextAgent()
    {
		if (lastAgentFocused == null)
		{
			focusOnAgent(f_player.First());
			return;
		}

		for (int i = 0; i < f_player.Count; i++)
        {
			if (f_player.getAt(i).transform == lastAgentFocused)
			{
				if (i == f_player.Count - 1)
				{
					focusOnAgent(f_player.First());
					camera_logging("focusNextAgent", f_player.First().ToString());
					break;
				}
				else
				{
					focusOnAgent(f_player.getAt(i + 1));
					camera_logging("focusNextAgent", f_player.getAt(i + 1).ToString());
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

	private void camera_logging(string camera_action, string param){
		float cur_time = Time.time;
		if( (cur_time - last_action_time) > logging_padding ){

			GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
			{
				verb = "interacted",
				objectType = "camera",
				activityExtensions = new Dictionary<string, string>() {
					{ "value", camera_action },
					{ "content", param }
				}
			});
			last_action_time = cur_time;
			//Debug.Log("Log " + camera_action);
		}
	}
}