using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections;

/// <summary>
/// This system manages main camera (movement, rotation, focus on/follow agent...)
/// </summary>
public class CameraSystem : FSystem {
	// In games agents controlable by the player
	private Family f_player = FamilyManager.getFamily(new AnyOfTags("Player"));
	// Contains current UI focused
	private Family f_UIfocused = FamilyManager.getFamily(new AllOfComponents(typeof(RectTransform), typeof(PointerOver)));
	private Family f_focusOn = FamilyManager.getFamily(new AllOfComponents(typeof(FocusCamOn)));

	private Transform targetAgent; // if defined camera follow this target
	private Transform lastAgentFocused = null;
	private Vector3 targetPos; // if defined camera focus in this position (grid definition)
	private float smoothSpeed = 0.125f;
	private Vector3 offset = new Vector3(0, 2f, 0);

	private Camera mainCamera;

	private float UI_frontBackValue = 0;
	private float UI_leftRightValue = 0;
	private float UI_rotateValue = 0;
	private float UI_zoomValue = 0;

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
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		// move camera front/back depending on Vertical axis
		if (Input.GetAxis("Vertical") != 0 || UI_frontBackValue != 0)
		{
			if (UI_frontBackValue == 0)
				moveFrontBack(Input.GetAxis("Vertical"));
			else
				moveFrontBack(UI_frontBackValue);
		}
		// move camera left/right de pending on Horizontal axis
		if (Input.GetAxis("Horizontal") != 0 || UI_leftRightValue != 0)
		{
			if (UI_leftRightValue == 0)
				moveLeftRight(Input.GetAxis("Horizontal"));
			else
				moveLeftRight(UI_leftRightValue);
		}

		// rotate camera with "A" and "E" keys
		if (Input.GetKey(KeyCode.A))
			rotateCamera(-1, 0);
		else if (Input.GetKey(KeyCode.E))
			rotateCamera(1, 0);
		else if (UI_rotateValue != 0)
			rotateCamera(UI_rotateValue, 0);

		// Move camera with wheel click
		if (Input.GetMouseButton(2))
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			Transform cameraTarget = mainCamera.transform.parent.parent;
			cameraTarget.Translate(Input.GetAxisRaw("Mouse Y") * Time.deltaTime * dragSpeed, 0, -Input.GetAxisRaw("Mouse X") * Time.deltaTime * dragSpeed);
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
			rotateCamera(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
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
	}

	private void moveLeftRight(float value)
    {
		Transform cameraTarget = mainCamera.transform.parent.parent;
		cameraTarget.Translate(0, 0, value * Time.deltaTime * cameraMovingSpeed);
		unfocusAgent();
	}

	private void zoomOut(float value)
    {
		// compute distance between camera position and camera target
		Vector3 relativePos = mainCamera.transform.InverseTransformPoint(mainCamera.transform.parent.parent.position);
		if (relativePos.z < cameraZoomMax)
		{
			mainCamera.transform.position -= new Vector3(value * mainCamera.transform.forward.x * cameraZoomSpeed, value * mainCamera.transform.forward.y * cameraZoomSpeed, value * mainCamera.transform.forward.z * cameraZoomSpeed);
		}
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

	private void rotateCamera(float x, float y)
	{
		mainCamera.transform.parent.parent.Rotate(Vector3.up, 90f * x * cameraRotationSpeed * Time.deltaTime);
		mainCamera.transform.parent.Rotate(Vector3.back, 90f * y * cameraRotationSpeed * Time.deltaTime);
		if (mainCamera.transform.position.y < 8f)
			// cancel previous y rotation
			mainCamera.transform.parent.Rotate(Vector3.back, 90f * -y * cameraRotationSpeed * Time.deltaTime);
	}

	public void focusOnAgent(GameObject agent)
    {
		targetAgent = agent.transform;
		lastAgentFocused = targetAgent;
		GameObjectManager.setGameObjectParent(mainCamera.transform.parent.parent.gameObject, agent, true);
		MainLoop.instance.StartCoroutine(travelingOnAgent());
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
					break;
				}
				else
				{
					focusOnAgent(f_player.getAt(i + 1));
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
		while (targetAgent != null && mainCamera.transform.parent.parent.position != targetAgent.position + offset)
		{
			mainCamera.transform.parent.parent.position = Vector3.MoveTowards(mainCamera.transform.parent.parent.position, targetAgent.position + offset, smoothSpeed);
			mainCamera.transform.LookAt(mainCamera.transform.parent.parent);
			yield return null;
		}
	}

	private IEnumerator travelingOnPos()
	{
		while (targetAgent == null && mainCamera.transform.parent.parent.localPosition != targetPos)
		{
			mainCamera.transform.parent.parent.localPosition = Vector3.MoveTowards(mainCamera.transform.parent.parent.localPosition, targetPos, smoothSpeed);
			yield return null;
		}
	}
}