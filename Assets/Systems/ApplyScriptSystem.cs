using UnityEngine;
using FYFY;
using System.Collections.Generic;
public class ApplyScriptSystem : FSystem {

	private float cooldown = 2;
	private Family controllableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Script)));
	private Family entityGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position), typeof(Entity)));
	private bool initialized = false;
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

		if(!initialized){
			initializeTest();
			initialized=true;
		}

		if(cooldown <= 0){
		cooldown = 2;
		foreach( GameObject go in controllableGO){

			if(endOfScript(go))
				break;
			Action action = getCurrentAction(go);
			
			switch (action.actionType){
				case Action.ActionType.Forward:
					switch (go.GetComponent<Direction>().direction){
						case Direction.Dir.North:
							if(!checkObstacle(go.GetComponent<Position>().x,go.GetComponent<Position>().z + 1)){
								go.GetComponent<MoveTarget>().x = go.GetComponent<Position>().x;
								go.GetComponent<MoveTarget>().z = go.GetComponent<Position>().z + 1;
							}
							break;
						case Direction.Dir.South:
							if(!checkObstacle(go.GetComponent<Position>().x,go.GetComponent<Position>().z - 1)){
								go.GetComponent<MoveTarget>().x = go.GetComponent<Position>().x;
								go.GetComponent<MoveTarget>().z = go.GetComponent<Position>().z - 1;
							}
							break;
						case Direction.Dir.East:
							if(!checkObstacle(go.GetComponent<Position>().x + 1,go.GetComponent<Position>().z)){
								go.GetComponent<MoveTarget>().x = go.GetComponent<Position>().x + 1;
								go.GetComponent<MoveTarget>().z = go.GetComponent<Position>().z;
							}
							break;
						case Direction.Dir.West:
							if(!checkObstacle(go.GetComponent<Position>().x - 1,go.GetComponent<Position>().z)){
								go.GetComponent<MoveTarget>().x = go.GetComponent<Position>().x - 1;
								go.GetComponent<MoveTarget>().z = go.GetComponent<Position>().z;
							}
							break;
					}
					break;

				case Action.ActionType.TurnLeft:
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

				case Action.ActionType.TurnRight:
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
			incrementActionScript(go.GetComponent<Script>());
		}
		}
		else
			cooldown -= Time.deltaTime;

	}

	private Action getCurrentAction(GameObject go) {
		Action action = go.GetComponent<Script>().actions[go.GetComponent<Script>().currentAction]; 
		//end when a pure action is found
		while(!(action.actionType == Action.ActionType.Forward || action.actionType == Action.ActionType.TurnLeft || action.actionType == Action.ActionType.TurnRight)){
			//Case For
			if(action.actionType == Action.ActionType.For){
				action = action.actions[action.currentAction];
			}
		}

		return action;
	}

	private void incrementActionScript(Script script){
		if(incrementAction(script.actions[script.currentAction]))
			script.currentAction++;

	}
	private bool incrementAction(Action act){
		if(act.actionType == Action.ActionType.Forward || act.actionType == Action.ActionType.TurnLeft || act.actionType == Action.ActionType.TurnRight)
			return true;
		//Case For
		else if(act.actionType == Action.ActionType.For){
			if(incrementAction(act.actions[act.currentAction]))
				act.currentAction++;

			if(act.currentAction >= act.actions.Count){
				act.currentAction = 0;
				act.currentFor++;
				//End of for
				if(act.currentFor >= act.nbFor){
					act.currentAction = 0;
					act.currentFor++;
					return true;
				}
			}
		}
		
		return false;
	}

	private bool endOfScript(GameObject go){
		return go.GetComponent<Script>().currentAction >= go.GetComponent<Script>().actions.Count;
	}


	private bool checkObstacle(int x, int z){
		foreach( GameObject go in entityGO){
			if(go.GetComponent<Position>().x == x && go.GetComponent<Position>().z == z && go.GetComponent<Entity>().type == Entity.Type.Wall)
				return true;
		}
		return false;
	}
	private void initializeTest(){
		foreach( GameObject go in controllableGO){
			go.GetComponent<Script>().actions = new List<Action>();
			go.GetComponent<Script>().actions.Add(new Action());
			go.GetComponent<Script>().currentAction = 0;
			Debug.Log(go.GetComponent<Script>().actions.Count);
			go.GetComponent<Script>().actions[0].actions = new List<Action>();
			go.GetComponent<Script>().actions[0].actions.Add(new Action());
			go.GetComponent<Script>().actions[0].actions[0].actionType = Action.ActionType.Forward;
			go.GetComponent<Script>().actions[0].actionType = Action.ActionType.For;
			go.GetComponent<Script>().actions[0].nbFor = 3;
			go.GetComponent<Script>().actions[0].currentAction = 0;
		}
	}
}


