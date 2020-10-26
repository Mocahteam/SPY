using UnityEngine;
using FYFY;

public class UISystem : FSystem {
	//private Family controllableGO = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver)));
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
		/*foreach( GameObject go in controllableGO){
			Debug.Log("ok");
		}*/
	}
}