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
			
		}
		
	}
}