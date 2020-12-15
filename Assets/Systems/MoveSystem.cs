using UnityEngine;
using FYFY;

public class MoveSystem : FSystem {

	private float turnSpeed = 150f;
	private float moveSpeed = 7f;
	private Family controllableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new NoneOfLayers(8));
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

			if(go.transform.localPosition.x/3 != go.GetComponent<Position>().x || go.transform.localPosition.z/3 != go.GetComponent<Position>().z){
				if(go.GetComponent<Animator>())
					go.GetComponent<Animator>().SetFloat("Run", 1f);
				go.transform.localPosition = Vector3.MoveTowards(go.transform.localPosition, new Vector3(go.GetComponent<Position>().x*3,go.transform.localPosition.y,go.GetComponent<Position>().z*3), moveSpeed* Time.deltaTime);
			}
			else{
				if(go.GetComponent<Animator>())
					go.GetComponent<Animator>().SetFloat("Run", -1f);
			}

			Quaternion target = Quaternion.Euler(0, 0, 0);

			switch(go.GetComponent<Direction>().direction){
				case Direction.Dir.East:
					target = Quaternion.Euler(0, 90, 0);
					break;
				case Direction.Dir.North:
					target = Quaternion.Euler(0, 0, 0);
					break;
				case Direction.Dir.South:
					target = Quaternion.Euler(0, 180, 0);
					break;
				case Direction.Dir.West:
					target = Quaternion.Euler(0, -90, 0);
					break;
			}
			//Debug.Log("target " + target.eulerAngles.y);
			//Debug.Log("player " +go.transform.eulerAngles.y);
			if(target.eulerAngles.y != go.transform.eulerAngles.y){
				go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, target, turnSpeed*Time.deltaTime);
				if(go.GetComponent<Animator>())
					go.GetComponent<Animator>().SetFloat("Turn", 1f);
			}
			else{
				if(go.GetComponent<Animator>())
					go.GetComponent<Animator>().SetFloat("Turn", -1f);
			}
		}
	}
}