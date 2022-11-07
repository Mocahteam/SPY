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
	private Vector3 targetPos; // if defined camera focus in this position (grid definition)
	private float smoothSpeed = 0.125f;
	private Vector3 offset = new Vector3(0, 2f, 0);

	private Camera mainCamera;

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
		if (Input.GetAxis("Vertical") != 0)
		{
			Transform cameraTarget = mainCamera.transform.parent.parent;
			cameraTarget.Translate(-Input.GetAxis("Vertical") * Time.deltaTime * cameraMovingSpeed, 0, 0);
			unfocusAgent();
		}
		// move camera left/right de pending on Horizontal axis
		if (Input.GetAxis("Horizontal") != 0)
		{
			Transform cameraTarget = mainCamera.transform.parent.parent;
			cameraTarget.Translate(0, 0, Input.GetAxis("Horizontal") * Time.deltaTime * cameraMovingSpeed);
			unfocusAgent();
		}

		// rotate camera with "A" and "E" keys
		if (Input.GetKey(KeyCode.A))
		{
			rotateCamera(-1, 0);
		}
		else if (Input.GetKey(KeyCode.E))
		{
			rotateCamera(1, 0);
		}

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
		else if(Input.GetAxis("Mouse ScrollWheel") < 0 && f_UIfocused.Count == 0) // Zoom out
	    {
			// compute distance between camera position and camera target
			Vector3 relativePos = mainCamera.transform.InverseTransformPoint(mainCamera.transform.parent.parent.position);
			if (relativePos.z < cameraZoomMax)
			{
				mainCamera.transform.position -= new Vector3(mainCamera.transform.forward.x * cameraZoomSpeed, mainCamera.transform.forward.y * cameraZoomSpeed, mainCamera.transform.forward.z * cameraZoomSpeed);
			}
		}
	    else if(Input.GetAxis("Mouse ScrollWheel") > 0 && f_UIfocused.Count == 0) // Zoom in
		{
			// compute distance between camera position and camera target
			Vector3 relativePos = mainCamera.transform.InverseTransformPoint(mainCamera.transform.parent.parent.position);
			if (relativePos.z > cameraZoomMin)
			{
				mainCamera.transform.position += new Vector3(mainCamera.transform.forward.x * cameraZoomSpeed, mainCamera.transform.forward.y * cameraZoomSpeed, mainCamera.transform.forward.z * cameraZoomSpeed);
			}
			// check if we don't go beyond the target
			relativePos = mainCamera.transform.InverseTransformPoint(mainCamera.transform.parent.parent.position);
			if (relativePos.z < 0)
				mainCamera.transform.localPosition = Vector3.zero;
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
		GameObjectManager.setGameObjectParent(mainCamera.transform.parent.parent.gameObject, agent, true);
		MainLoop.instance.StartCoroutine(travelingOnAgent());
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