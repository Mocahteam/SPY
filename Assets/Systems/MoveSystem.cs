using UnityEngine;
using FYFY;

public class MoveSystem : FSystem {

	private Family controllableGO = FamilyManager.getFamily(new AllOfComponents(typeof(MoveTarget)));
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
			
			if(go.GetComponent<MoveTarget>().x != go.GetComponent<Position>().x || go.GetComponent<MoveTarget>().z != go.GetComponent<Position>().z){
				go.transform.localPosition = Vector3.MoveTowards(go.transform.localPosition, new Vector3(go.GetComponent<MoveTarget>().x*3,go.transform.localPosition.y,go.GetComponent<MoveTarget>().z*3), 5* Time.deltaTime);
				if(go.transform.localPosition == new Vector3(go.GetComponent<MoveTarget>().x*3,go.transform.localPosition.y,go.GetComponent<MoveTarget>().z*3)){
					go.GetComponent<Position>().x = go.GetComponent<MoveTarget>().x;
					go.GetComponent<Position>().z = go.GetComponent<MoveTarget>().z;
				}
			}

			Quaternion target = Quaternion.Euler(0, 0, 0);

			switch(go.GetComponent<Direction>().direction){
				case Direction.Dir.East:
					target = Quaternion.Euler(0, 0, 0);
					break;
				case Direction.Dir.North:
					target = Quaternion.Euler(0, -90, 0);
					break;
				case Direction.Dir.South:
					target = Quaternion.Euler(0, 90, 0);
					break;
				case Direction.Dir.West:
					target = Quaternion.Euler(0, 180, 0);
					break;
			}
			go.transform.rotation = Quaternion.Slerp(go.transform.rotation, target, 2*Time.deltaTime);
		}
	}
}