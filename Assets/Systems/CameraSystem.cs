using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

/// <summary>
/// This system manages main camera (movement, rotation, focus on/follow agent...)
/// </summary>
public class CameraSystem : FSystem {
	// Contains main camera
	private Family cameraGO = FamilyManager.getFamily(new AllOfComponents(typeof(CameraComponent)), new AnyOfTags("MainCamera"));
	// In game agents containing reference to a Script
	private Family agents = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef)));
	// In games agents controlable by the eplayer
	private Family playerGO = FamilyManager.getFamily(new AnyOfTags("Player"));
	// Contains current UI focused
	private Family UIfocused = FamilyManager.getFamily(new AllOfComponents(typeof(RectTransform), typeof(PointerOver))); 

	private Transform target; // if defined camera follow this target
	private float smoothSpeed = 0.125f;
	private Vector3 offset = new Vector3(6,15,0);

	protected override void onStart()
	{
		// Backup initial camera data
		cameraGO.addEntryCallback(onNewCamera);
		foreach (GameObject go in cameraGO)
			onNewCamera(go);

		// set current camera target (the first player)
		playerGO.addEntryCallback(delegate (GameObject go) { target = go.transform; });

		// add listener on locate button to move camera on focused agent
		agents.addEntryCallback(setLocateButtons);
	}

	private void onNewCamera(GameObject go)
	{
        go.GetComponent<CameraComponent>().orbitH = go.GetComponent<CameraComponent>().transform.eulerAngles.y;
        go.GetComponent<CameraComponent>().orbitV = go.GetComponent<CameraComponent>().transform.eulerAngles.x;
        go.GetComponent<CameraComponent>().init_X = go.transform.position.x;
        go.GetComponent<CameraComponent>().init_Y = go.transform.position.y;
        go.GetComponent<CameraComponent>().init_Z = go.transform.position.z;
        go.GetComponent<CameraComponent>().initRotation = go.transform.localRotation.eulerAngles;
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		// manage all cameras
		foreach( GameObject camera in cameraGO ){
			// if target is defined move smoothy camera to it 
			if(target){
				camera.transform.position = Vector3.MoveTowards(camera.transform.position, (target.position+offset), smoothSpeed);
				camera.transform.LookAt(target); 
			}

			// move camera front/back depending on Vertical axis
			if (Input.GetAxis("Vertical") != 0)
			{
				camera.transform.position += new Vector3(camera.transform.forward.x + camera.transform.up.x, 0, camera.transform.forward.z + camera.transform.up.z) * Input.GetAxis("Vertical") * camera.GetComponent<CameraComponent>().cameraSpeed * Time.deltaTime;
				target = null;
			}
			// move camera left/right de pending on Horizontal axis
			if (Input.GetAxis("Horizontal") != 0)
			{
				camera.transform.position += new Vector3(camera.transform.right.x, 0, camera.transform.right.z) * Input.GetAxis("Horizontal") * camera.GetComponent<CameraComponent>().cameraSpeed * Time.deltaTime;
				target = null;
			}

			// rotate camera with "A" and "E" keys
			if (Input.GetKey(KeyCode.A))
			{
				camera.transform.Rotate(Vector3.up * 90 * Time.deltaTime, Space.World);
				target = null;
			}
			else if (Input.GetKey(KeyCode.E))
			{
				camera.transform.Rotate(-Vector3.up * 90 * Time.deltaTime, Space.World);
				target = null;
			}

			// Move camera with wheel click
	        if (Input.GetMouseButton(2))
            {
            	Cursor.lockState = CursorLockMode.Locked;
	        	Cursor.visible = false;
                camera.transform.Translate(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * camera.GetComponent<CameraComponent>().dragSpeed, -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * camera.GetComponent<CameraComponent>().dragSpeed, 0);
				target = null;
			}

			// Zoom with scroll wheel only if UI element is not focused
			else if(Input.GetAxis("Mouse ScrollWheel") < 0 && UIfocused.Count == 0)
	        {
	            if(camera.GetComponent<CameraComponent>().ScrollCount >= camera.GetComponent<CameraComponent>().ScrollWheelminPush && camera.GetComponent<CameraComponent>().ScrollCount < camera.GetComponent<CameraComponent>().ScrollWheelLimit)
	            {
	                camera.transform.position += new Vector3(0, camera.GetComponent<CameraComponent>().zoomSpeed, 0);
	                camera.GetComponent<CameraComponent>().ScrollCount++;
				}
				target = null;
			}
	        else if(Input.GetAxis("Mouse ScrollWheel") > 0 && UIfocused.Count == 0)
	        {
	            if(camera.GetComponent<CameraComponent>().ScrollCount > camera.GetComponent<CameraComponent>().ScrollWheelminPush && camera.GetComponent<CameraComponent>().ScrollCount <= camera.GetComponent<CameraComponent>().ScrollWheelLimit)
	            {
	                camera.transform.position -= new Vector3(0, camera.GetComponent<CameraComponent>().zoomSpeed, 0);
	                camera.GetComponent<CameraComponent>().ScrollCount--;
				}
				target = null;
			}
	        
            // Orbit rotation
            else if (Input.GetMouseButton(1))
            {
            	camera.GetComponent<CameraComponent>().orbitH += camera.GetComponent<CameraComponent>().lookSpeedH * Input.GetAxis("Mouse X");
                camera.GetComponent<CameraComponent>().orbitV -= camera.GetComponent<CameraComponent>().lookSpeedV * Input.GetAxis("Mouse Y");
                camera.GetComponent<CameraComponent>().transform.eulerAngles = new Vector3(camera.GetComponent<CameraComponent>().orbitV, camera.GetComponent<CameraComponent>().orbitH, 0f);
				target = null;
			}
	        else
	        {
	        	Cursor.lockState = CursorLockMode.None;
	        	Cursor.visible = true;
			}

			// Clamp camera position
			float min_x = camera.GetComponent<CameraComponent>().init_X + camera.GetComponent<CameraComponent>().MIN_X;
			float max_x = camera.GetComponent<CameraComponent>().init_X + camera.GetComponent<CameraComponent>().MAX_X;
			float min_y = camera.GetComponent<CameraComponent>().init_Y + camera.GetComponent<CameraComponent>().MIN_Y;
			float max_y = camera.GetComponent<CameraComponent>().init_Y + camera.GetComponent<CameraComponent>().MAX_Y;
			float min_z = camera.GetComponent<CameraComponent>().init_Z + camera.GetComponent<CameraComponent>().MIN_Z;
			float max_z = camera.GetComponent<CameraComponent>().init_Z + camera.GetComponent<CameraComponent>().MAX_Z;
			camera.transform.position = new Vector3(
			   Mathf.Clamp(Camera.main.transform.position.x, min_x, max_x),
			   Mathf.Clamp(Camera.main.transform.position.y, min_y, max_y),
			   Mathf.Clamp(Camera.main.transform.position.z, min_z, max_z));
		}		
	}

	// add callback to locateButton inside UI container associated to the agent
	public void setLocateButtons(GameObject go){
		go.GetComponent<ScriptRef>().uiContainer.transform.Find("Header").Find("locateButton").GetComponent<Button>().onClick.AddListener(delegate{target = go.transform;});
	}
}