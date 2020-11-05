using UnityEngine;
using FYFY;

public class CheckEventsSystem : FSystem {

	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(Script)), new AnyOfTags("Player"));
	private Family exitGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Exit"));

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

		//Check if the player is on the end of the level
		foreach( GameObject player in playerGO){
			foreach( GameObject exit in exitGO){
				if(player.GetComponent<Position>().x == exit.GetComponent<Position>().x && player.GetComponent<Position>().z == exit.GetComponent<Position>().z){
					//end level
					Debug.Log("Fin du niveau");
				}
			}
		}

	}
}