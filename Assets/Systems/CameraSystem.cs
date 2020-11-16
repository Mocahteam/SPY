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
}