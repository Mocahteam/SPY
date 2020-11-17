using UnityEngine;
using FYFY;

public class CameraSystem : FSystem {
	private Family controllableGO = FamilyManager.getFamily(new AllOfComponents(typeof(CameraComponent)));

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
		go.GetComponent<CameraComponent>().currentRotation = go.transform.rotation;
	    go.GetComponent<CameraComponent>().desiredRotation = go.transform.rotation;
	    go.GetComponent<CameraComponent>().rotation = go.transform.rotation;
	    go.GetComponent<CameraComponent>().xDeg = Vector3.Angle(Vector3.right, go.transform.right );
	    go.GetComponent<CameraComponent>().yDeg = Vector3.Angle(Vector3.up, go.transform.up );
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
			/*
			go.GetComponent<CameraComponent>().currentRotation = go.transform.rotation;
        	go.GetComponent<CameraComponent>().desiredRotation = go.transform.rotation;
        	go.GetComponent<CameraComponent>().rotation = go.transform.rotation;
        	go.GetComponent<CameraComponent>().xDeg = Vector3.Angle(Vector3.right, go.transform.right );
        	go.GetComponent<CameraComponent>().yDeg = Vector3.Angle(Vector3.up, go.transform.up );
			*/

			go.transform.position = go.transform.position + new Vector3(0,0,Input.GetAxis("Horizontal")* go.GetComponent<CameraComponent>().cameraSpeed * Time.deltaTime);
			go.transform.position = go.transform.position + new Vector3(-Input.GetAxis("Vertical")* go.GetComponent<CameraComponent>().cameraSpeed * Time.deltaTime,0,0);
			//Cursor.lockState = CursorLockMode.None;
	        //Cursor.visible = true;
			// Zoom
			if(Input.GetAxis("Mouse ScrollWheel") < 0)
	        {
	            if(go.GetComponent<CameraComponent>().ScrollCount >= go.GetComponent<CameraComponent>().ScrollWheelminPush && go.GetComponent<CameraComponent>().ScrollCount < go.GetComponent<CameraComponent>().ScrollWheelLimit)
	            {
	                go.transform.position += new Vector3(0, go.GetComponent<CameraComponent>().zoomSpeed, 0);
	                go.GetComponent<CameraComponent>().ScrollCount++;
	            }
	        }
 
	        if(Input.GetAxis("Mouse ScrollWheel") > 0)
	        {
	            if(go.GetComponent<CameraComponent>().ScrollCount > go.GetComponent<CameraComponent>().ScrollWheelminPush && go.GetComponent<CameraComponent>().ScrollCount <= go.GetComponent<CameraComponent>().ScrollWheelLimit)
	            {
	                go.transform.position -= new Vector3(0, go.GetComponent<CameraComponent>().zoomSpeed, 0);
	                go.GetComponent<CameraComponent>().ScrollCount--;
	            }
	        }

	        // Déplacement avec la molette comme dans l'éditeur
	        if (Input.GetMouseButton(2))
            {
            	Cursor.lockState = CursorLockMode.Locked;
	        	Cursor.visible = false;
                go.transform.Translate(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * go.GetComponent<CameraComponent>().dragSpeed, -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * go.GetComponent<CameraComponent>().dragSpeed, 0);
            }
	        // Déplacement de type orbite
	        else if (Input.GetMouseButton(1))
	        {
	        	Cursor.lockState = CursorLockMode.Locked;
	        	Cursor.visible = false;
	        	
	            go.GetComponent<CameraComponent>().xDeg += Input.GetAxis("Mouse X") * go.GetComponent<CameraComponent>().xSpeed * 0.02f;
	            go.GetComponent<CameraComponent>().yDeg -= Input.GetAxis("Mouse Y") * go.GetComponent<CameraComponent>().ySpeed * 0.02f;
	 
	            ////////OrbitAngle
	 
	            //Clamp the vertical axis for the orbit
	            go.GetComponent<CameraComponent>().yDeg = ClampAngle(go.GetComponent<CameraComponent>().yDeg, go.GetComponent<CameraComponent>().yMinLimit, go.GetComponent<CameraComponent>().yMaxLimit);
	            // set camera rotation 
	            go.GetComponent<CameraComponent>().desiredRotation = Quaternion.Euler(go.GetComponent<CameraComponent>().yDeg, go.GetComponent<CameraComponent>().xDeg, 0);
	            go.GetComponent<CameraComponent>().currentRotation = go.transform.rotation;
	 
	            go.GetComponent<CameraComponent>().rotation = Quaternion.Lerp(go.GetComponent<CameraComponent>().currentRotation, go.GetComponent<CameraComponent>().desiredRotation, Time.deltaTime * go.GetComponent<CameraComponent>().zoomDampening);
	            go.transform.rotation = go.GetComponent<CameraComponent>().rotation;
	        }
	        else
	        {
	        	Cursor.lockState = CursorLockMode.None;
	        	Cursor.visible = true;
	    	}
	        

	        /*
	        // Déplacement clic droit
	        if(Input.GetMouseButtonDown(1))
	        {
	            go.GetComponent<Camera>().DragOrigin = Input.mousePosition;
	            //return;
	        }
 
	        if(!Input.GetMouseButton(1)) return;
	 
	        if(go.GetComponent<Camera>().ReverseDrag)
	        {
	            Vector3 pos = UnityEngine.Camera.main.ScreenToViewportPoint(Input.mousePosition - go.GetComponent<Camera>().DragOrigin);
	            go.GetComponent<Camera>().Move = new Vector3(pos.x * go.GetComponent<Camera>().DragSpeed, 0, pos.y * go.GetComponent<Camera>().DragSpeed);
	        }
	        else
	        {
	            Vector3 pos = UnityEngine.Camera.main.ScreenToViewportPoint(Input.mousePosition - go.GetComponent<Camera>().DragOrigin);
	            go.GetComponent<Camera>().Move = new Vector3(pos.x * -go.GetComponent<Camera>().DragSpeed, 0, pos.y * -go.GetComponent<Camera>().DragSpeed);
	        }
	 
	        go.transform.Translate(go.GetComponent<Camera>().Move, Space.World);
	        */
		}
		
	}
	
	private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
    
}