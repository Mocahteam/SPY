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

	private Transform target; // if defined camera follow this target
	private float smoothSpeed = 0.125f;
	private Vector3 offset = new Vector3(0, 2f, 0);

	// Déplacement aux touches du clavier
	public float cameraMovingSpeed;
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
		// set current camera target (the first player)
		f_player.addEntryCallback(delegate (GameObject go) { focusOnAgent(go); });
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		// move camera front/back depending on Vertical axis
		if (Input.GetAxis("Vertical") != 0)
		{
			Transform camera = Camera.main.transform;
			Transform cameraTarget = Camera.main.transform.parent.parent;
			cameraTarget.Translate(-Input.GetAxis("Vertical") * Time.deltaTime * cameraMovingSpeed, 0, 0);
			unfocusAgent();
		}
		// move camera left/right de pending on Horizontal axis
		if (Input.GetAxis("Horizontal") != 0)
		{
			Transform camera = Camera.main.transform;
			Transform cameraTarget = Camera.main.transform.parent.parent;
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
			Transform cameraTarget = Camera.main.transform.parent.parent;
			cameraTarget.Translate(-Input.GetAxisRaw("Mouse Y") * Time.deltaTime * dragSpeed, 0, Input.GetAxisRaw("Mouse X") * Time.deltaTime * dragSpeed);
			unfocusAgent();
		}

		// Zoom with scroll wheel only if UI element is not focused
		else if(Input.GetAxis("Mouse ScrollWheel") < 0 && f_UIfocused.Count == 0) // Zoom out
	    {
			// compute distance between camera position and camera target
			Vector3 relativePos = Camera.main.transform.InverseTransformPoint(Camera.main.transform.parent.parent.position);
			if (relativePos.z < cameraZoomMax)
			{
				Camera.main.transform.position -= new Vector3(Camera.main.transform.forward.x * cameraZoomSpeed, Camera.main.transform.forward.y * cameraZoomSpeed, Camera.main.transform.forward.z * cameraZoomSpeed);
			}
		}
	    else if(Input.GetAxis("Mouse ScrollWheel") > 0 && f_UIfocused.Count == 0) // Zoom in
		{
			// compute distance between camera position and camera target
			Vector3 relativePos = Camera.main.transform.InverseTransformPoint(Camera.main.transform.parent.parent.position);
			if (relativePos.z > cameraZoomMin)
			{
				Camera.main.transform.position += new Vector3(Camera.main.transform.forward.x * cameraZoomSpeed, Camera.main.transform.forward.y * cameraZoomSpeed, Camera.main.transform.forward.z * cameraZoomSpeed);
			}
			// check if we don't go beyond the target
			relativePos = Camera.main.transform.InverseTransformPoint(Camera.main.transform.parent.parent.position);
			if (relativePos.z < 0)
				Camera.main.transform.localPosition = Vector3.zero;
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
		Camera.main.transform.parent.parent.Rotate(Vector3.up, 90f * x * Time.deltaTime);
		Camera.main.transform.parent.Rotate(Vector3.back, 90f * y * Time.deltaTime);
	}

	public void focusOnAgent(GameObject agent)
    {
		target = agent.transform;
		GameObjectManager.setGameObjectParent(Camera.main.transform.parent.parent.gameObject, agent, true);
		MainLoop.instance.StartCoroutine(travelingOnAgent());
	}

	private void unfocusAgent()
    {
		if (target != null)
        {
			GameObjectManager.setGameObjectParent(Camera.main.transform.parent.parent.gameObject, target.parent.gameObject, true);
			target = null;
		}
    }

	private IEnumerator travelingOnAgent()
    {
		while (target != null && Camera.main.transform.parent.parent.position != target.position)
		{
			Camera.main.transform.parent.parent.position = Vector3.MoveTowards(Camera.main.transform.parent.parent.position, target.position+offset, smoothSpeed);
			Camera.main.transform.LookAt(Camera.main.transform.parent.parent);
			yield return null;
		}
    }
}