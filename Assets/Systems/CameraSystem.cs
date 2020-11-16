using UnityEngine;
using FYFY;

public class CameraSystem : FSystem {
	private Family controllableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Camera)));
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
			go.GetComponent<Camera>().currentRotation = go.transform.rotation;
        	go.GetComponent<Camera>().desiredRotation = go.transform.rotation;
        	go.GetComponent<Camera>().rotation = go.transform.rotation;
        	go.GetComponent<Camera>().xDeg = Vector3.Angle(Vector3.right, go.transform.right );
        	go.GetComponent<Camera>().yDeg = Vector3.Angle(Vector3.up, go.transform.up );
			*/
			go.transform.position = go.transform.position + new Vector3(0,0,Input.GetAxis("Horizontal")* go.GetComponent<Camera>().cameraSpeed * Time.deltaTime);
			go.transform.position = go.transform.position + new Vector3(-Input.GetAxis("Vertical")* go.GetComponent<Camera>().cameraSpeed * Time.deltaTime,0,0);

			// Zoom
			if(Input.GetAxis("Mouse ScrollWheel") < 0)
	        {
	            if(go.GetComponent<Camera>().ScrollCount >= go.GetComponent<Camera>().ScrollWheelminPush && go.GetComponent<Camera>().ScrollCount < go.GetComponent<Camera>().ScrollWheelLimit)
	            {
	                go.transform.position += new Vector3(0, go.GetComponent<Camera>().zoomSpeed, 0);
	                go.GetComponent<Camera>().ScrollCount++;
	            }
	        }
 
	        if(Input.GetAxis("Mouse ScrollWheel") > 0)
	        {
	            if(go.GetComponent<Camera>().ScrollCount > go.GetComponent<Camera>().ScrollWheelminPush && go.GetComponent<Camera>().ScrollCount <= go.GetComponent<Camera>().ScrollWheelLimit)
	            {
	                go.transform.position -= new Vector3(0, go.GetComponent<Camera>().zoomSpeed, 0);
	                go.GetComponent<Camera>().ScrollCount--;
	            }
	        }

	        // Déplacement avec la molette comme dans l'éditeur
	        if (Input.GetMouseButton(2))
            {
                go.transform.Translate(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * go.GetComponent<Camera>().dragSpeed, -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * go.GetComponent<Camera>().dragSpeed, 0);
            }

	        // Déplacement de type orbite
	        if (Input.GetMouseButton(1))
	        {
	            go.GetComponent<Camera>().xDeg += Input.GetAxis("Mouse X") * go.GetComponent<Camera>().xSpeed * 0.02f;
	            go.GetComponent<Camera>().yDeg -= Input.GetAxis("Mouse Y") * go.GetComponent<Camera>().ySpeed * 0.02f;
	 
	            ////////OrbitAngle
	 
	            //Clamp the vertical axis for the orbit
	            go.GetComponent<Camera>().yDeg = ClampAngle(go.GetComponent<Camera>().yDeg, go.GetComponent<Camera>().yMinLimit, go.GetComponent<Camera>().yMaxLimit);
	            // set camera rotation 
	            go.GetComponent<Camera>().desiredRotation = Quaternion.Euler(go.GetComponent<Camera>().yDeg, go.GetComponent<Camera>().xDeg, 0);
	            go.GetComponent<Camera>().currentRotation = go.transform.rotation;
	 
	            go.GetComponent<Camera>().rotation = Quaternion.Lerp(go.GetComponent<Camera>().currentRotation, go.GetComponent<Camera>().desiredRotation, Time.deltaTime * go.GetComponent<Camera>().zoomDampening);
	            go.transform.rotation = go.GetComponent<Camera>().rotation;
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