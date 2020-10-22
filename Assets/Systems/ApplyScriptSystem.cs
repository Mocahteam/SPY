using UnityEngine;
using FYFY;

public class ApplyScriptSystem : FSystem {

	private float cooldown = 0;
	private Family controllableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Script)));
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
		if(cooldown <= 0){
		foreach( GameObject go in controllableGO){
			cooldown = 1;
			switch (go.GetComponent<Script>().actions[go.GetComponent<Script>().currentAction]){
				case Script.Actions.Forward:
					switch (go.GetComponent<Direction>().direction){
						case Direction.Dir.North:
							go.GetComponent<MoveTarget>().x = go.GetComponent<Position>().x;
							go.GetComponent<MoveTarget>().z = go.GetComponent<Position>().z + 1;
							break;
						case Direction.Dir.South:
							go.GetComponent<MoveTarget>().x = go.GetComponent<Position>().x;
							go.GetComponent<MoveTarget>().z = go.GetComponent<Position>().z - 1;
							break;
						case Direction.Dir.East:
							go.GetComponent<MoveTarget>().x = go.GetComponent<Position>().x + 1;
							go.GetComponent<MoveTarget>().z = go.GetComponent<Position>().z;
							break;
						case Direction.Dir.West:
							go.GetComponent<MoveTarget>().x = go.GetComponent<Position>().x - 1;
							go.GetComponent<MoveTarget>().z = go.GetComponent<Position>().z;
							break;
					}
					break;

				case Script.Actions.TurnLeft:
					switch (go.GetComponent<Direction>().direction){
						case Direction.Dir.North:
							go.GetComponent<Direction>().direction = Direction.Dir.West;
							break;
						case Direction.Dir.South:
							go.GetComponent<Direction>().direction = Direction.Dir.East;
							break;
						case Direction.Dir.East:
							go.GetComponent<Direction>().direction = Direction.Dir.North;
							break;
						case Direction.Dir.West:
							go.GetComponent<Direction>().direction = Direction.Dir.South;
							break;
					}
					break;

				case Script.Actions.TurnRight:
					switch (go.GetComponent<Direction>().direction){
						case Direction.Dir.North:
							go.GetComponent<Direction>().direction = Direction.Dir.East;
							break;
						case Direction.Dir.South:
							go.GetComponent<Direction>().direction = Direction.Dir.West;
							break;
						case Direction.Dir.East:
							go.GetComponent<Direction>().direction = Direction.Dir.South;
							break;
						case Direction.Dir.West:
							go.GetComponent<Direction>().direction = Direction.Dir.North;
							break;
					}
					break;
			}
			go.GetComponent<Script>().currentAction++;
			if(go.GetComponent<Script>().currentAction >= go.GetComponent<Script>().actions.Count)
			go.GetComponent<Script>().currentAction = 0;
		}
		}
		else
			cooldown -= Time.deltaTime;

	}
}