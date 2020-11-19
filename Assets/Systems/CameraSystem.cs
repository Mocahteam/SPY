using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;

public class CameraSystem : FSystem {
	private Family controllableGO = FamilyManager.getFamily(new AllOfComponents(typeof(CameraComponent)));

	private Family UIGO = FamilyManager.getFamily(new AnyOfComponents(typeof(UIActionType), typeof(UITypeContainer), typeof(ElementToDrag)),
													new AllOfComponents(typeof(PointerOver)));

	public CameraSystem()
	{
		foreach( GameObject go in controllableGO)
		{
			onGOEnter(go);
	    }
	    controllableGO.addEntryCallback(onGOEnter);
	}

	private void onGOEnter(GameObject go)
	{
        go.GetComponent<CameraComponent>().orbitH = go.GetComponent<CameraComponent>().transform.eulerAngles.y;
        go.GetComponent<CameraComponent>().orbitV = go.GetComponent<CameraComponent>().transform.eulerAngles.x;
        //go.GetComponent<CameraComponent>().movementRotation = new Vector3 (Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));

	}

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
		
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		foreach( GameObject go in controllableGO){
			//go.transform.position = go.transform.position + new Vector3(0,0,Input.GetAxis("Horizontal")* go.GetComponent<CameraComponent>().cameraSpeed * Time.deltaTime);
			//go.transform.position = go.transform.position + new Vector3(-Input.GetAxis("Vertical")* go.GetComponent<CameraComponent>().cameraSpeed * Time.deltaTime,0,0);
			//go.transform.position = go.transform.position + new Vector3(0,0,Input.GetAxis("Horizontal")* go.GetComponent<CameraComponent>().cameraSpeed * Time.deltaTime);
			//go.transform.position = go.transform.position + new Vector3(-Input.GetAxis("Vertical")* go.GetComponent<CameraComponent>().cameraSpeed * Time.deltaTime,0,0);
			//go.transform.Translate(0,Input.GetAxis("Horizontal") * go.GetComponent<CameraComponent>().cameraSpeed * Time.deltaTime,0);
			//go.transform.Translate(0,-Input.GetAxis("Vertical") * go.GetComponent<CameraComponent>().cameraSpeed * Time.deltaTime,0);
			//go.GetComponent<CameraComponent>().movementRotation = Camera.main.transform.rotation * movementRotation;
			//go.transform.Rotate(Quaternion.Euler(0, go.transform.eulerAngles.y, 0), Space.Self);
			
			//go.transform.Translate(Vector3.forward*go.GetComponent<CameraComponent>().cameraSpeed*Time.deltaTime);
			//go.transform.position += go.transform.forward*go.GetComponent<CameraComponent>().cameraSpeed*Time.deltaTime;

			//Vector3 Movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        	//go.transform.position += Movement * go.GetComponent<CameraComponent>().cameraSpeed * Time.deltaTime;

			//go.transform.position += -transform.right * Time.deltaTime * go.GetComponent<CameraComponent>().cameraSpeed;

			if (Input.GetKey(KeyCode.D))
	        {
	            go.transform.position += go.transform.right * Time.deltaTime * go.GetComponent<CameraComponent>().cameraSpeed;;
	        }
        	if (Input.GetKey(KeyCode.Q))
	        {
	            go.transform.position += -go.transform.right * Time.deltaTime * go.GetComponent<CameraComponent>().cameraSpeed;
	        }
	        
	        // Autre type de déplacement...
	        if (Input.GetKey(KeyCode.S))
	        {
	            go.transform.position += -go.transform.forward * Time.deltaTime * go.GetComponent<CameraComponent>().cameraSpeed;
	            //go.transform.position += Vector3.right * Time.deltaTime * go.GetComponent<CameraComponent>().cameraSpeed;	            
	        }
	        if (Input.GetKey(KeyCode.Z))
	        {
	            go.transform.position += go.transform.forward * Time.deltaTime * go.GetComponent<CameraComponent>().cameraSpeed;
	            //go.transform.position += Vector3.left * Time.deltaTime * go.GetComponent<CameraComponent>().cameraSpeed;
	        }
	        

			//------------------------------------------------------------------------------------

			// Déplacement avec la molette comme dans l'éditeur
	        if (Input.GetMouseButton(2))
            {
            	Cursor.lockState = CursorLockMode.Locked;
	        	Cursor.visible = false;
                go.transform.Translate(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * go.GetComponent<CameraComponent>().dragSpeed, -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * go.GetComponent<CameraComponent>().dragSpeed, 0);
            }

			// Zoom avec la molette
			else if(Input.GetAxis("Mouse ScrollWheel") < 0)
	        {
	            if(go.GetComponent<CameraComponent>().ScrollCount >= go.GetComponent<CameraComponent>().ScrollWheelminPush && go.GetComponent<CameraComponent>().ScrollCount < go.GetComponent<CameraComponent>().ScrollWheelLimit)
	            {
	                go.transform.position += new Vector3(0, go.GetComponent<CameraComponent>().zoomSpeed, 0);
	                go.GetComponent<CameraComponent>().ScrollCount++;
	            }
	        }
	        else if(Input.GetAxis("Mouse ScrollWheel") > 0)
	        {
	            if(go.GetComponent<CameraComponent>().ScrollCount > go.GetComponent<CameraComponent>().ScrollWheelminPush && go.GetComponent<CameraComponent>().ScrollCount <= go.GetComponent<CameraComponent>().ScrollWheelLimit)
	            {
	                go.transform.position -= new Vector3(0, go.GetComponent<CameraComponent>().zoomSpeed, 0);
	                go.GetComponent<CameraComponent>().ScrollCount--;
	            }
	        }
	        
            // Déplacement de type orbite
            else if (Input.GetMouseButton(1))
            {
            	go.GetComponent<CameraComponent>().orbitH += go.GetComponent<CameraComponent>().lookSpeedH * Input.GetAxis("Mouse X");
                go.GetComponent<CameraComponent>().orbitV -= go.GetComponent<CameraComponent>().lookSpeedV * Input.GetAxis("Mouse Y");
                go.GetComponent<CameraComponent>().transform.eulerAngles = new Vector3(go.GetComponent<CameraComponent>().orbitV, go.GetComponent<CameraComponent>().orbitH, 0f);
            	//go.transform.position = go.transform.position + new Vector3(0,go.GetComponent<CameraComponent>().orbitH,0);
            }
	        else
	        {
	        	Cursor.lockState = CursorLockMode.None;
	        	Cursor.visible = true;
	    	}
			//------------------------------------------------------------------------------------
		}		
	}
}