using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;

public class DragDropSystem : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	private GameObject actionContainer;
	private Family PointedGO = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver)));
	private GameObject itemDragged;
	private bool drag;
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		foreach( GameObject go in PointedGO){
			if(Input.GetMouseButtonDown(0)){
				itemDragged = Object.Instantiate(go, go.transform);
				//itemDragged = go;
				drag = true;
				break;
			}
		}

		if(Input.GetMouseButtonUp(0)){
			Object.Destroy(itemDragged);
			itemDragged = null;
			drag = false;
			
		}


		if(itemDragged != null){
			itemDragged.transform.position = Input.mousePosition;
		}
	}
}