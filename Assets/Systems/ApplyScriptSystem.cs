using UnityEngine;
using FYFY;
using System.Collections.Generic;
using UnityEngine.UI;
public class ApplyScriptSystem : FSystem {

	private Family controllableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Script)));
	private Family entityGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position), typeof(Entity)));
	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(Script)), new AnyOfTags("Player"));
	private GameObject scriptComposer;
	private GameData gameData;

	public ApplyScriptSystem(){
		scriptComposer = GameObject.Find("ScriptContainer");
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
	} 
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

		if(gameData.step){
			foreach( GameObject go in controllableGO){
				
				if(ActionManipulator.endOfScript(go)){
					break;
				}
				Action action = ActionManipulator.getCurrentAction(go);
				
				switch (action.actionType){
					case Action.ActionType.Forward:
						ApplyForward(go);
						break;

					case Action.ActionType.TurnLeft:
						ApplyTurnLeft(go);
						break;

					case Action.ActionType.TurnRight:
						ApplyTurnRight(go);
						break;
					case Action.ActionType.Wait:
						break;
				}
				ActionManipulator.incrementActionScript(go.GetComponent<Script>());
			}

			foreach( GameObject go in playerGO){
				if(ActionManipulator.endOfScript(go)){
					ActionManipulator.restartScript(go.GetComponent<Script>());
				}
			}
		}

	}

	private void ApplyForward(GameObject go){
		switch (go.GetComponent<Direction>().direction){
			case Direction.Dir.North:
				if(!checkObstacle(go.GetComponent<Position>().x,go.GetComponent<Position>().z + 1)){
					go.GetComponent<Position>().x = go.GetComponent<Position>().x;
					go.GetComponent<Position>().z = go.GetComponent<Position>().z + 1;
				}
				break;
			case Direction.Dir.South:
				if(!checkObstacle(go.GetComponent<Position>().x,go.GetComponent<Position>().z - 1)){
					go.GetComponent<Position>().x = go.GetComponent<Position>().x;
					go.GetComponent<Position>().z = go.GetComponent<Position>().z - 1;
				}
				break;
			case Direction.Dir.East:
				if(!checkObstacle(go.GetComponent<Position>().x + 1,go.GetComponent<Position>().z)){
					go.GetComponent<Position>().x = go.GetComponent<Position>().x + 1;
					go.GetComponent<Position>().z = go.GetComponent<Position>().z;
				}
				break;
			case Direction.Dir.West:
				if(!checkObstacle(go.GetComponent<Position>().x - 1,go.GetComponent<Position>().z)){
					go.GetComponent<Position>().x = go.GetComponent<Position>().x - 1;
					go.GetComponent<Position>().z = go.GetComponent<Position>().z;
				}
				break;
		}
	}

	private void ApplyTurnLeft(GameObject go){
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
	}

	private void ApplyTurnRight(GameObject go){
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
	}


	private bool checkObstacle(int x, int z){
		foreach( GameObject go in entityGO){
			if(go.GetComponent<Position>().x == x && go.GetComponent<Position>().z == z && go.GetComponent<Entity>().type == Entity.Type.Wall)
				return true;
		}
		return false;
	}

	public void applyScriptToPlayer(){
		foreach( GameObject go in playerGO){
			ActionManipulator.resetScript(go.GetComponent<Script>());
			go.GetComponent<Script>().actions = ActionManipulator.ScriptContainerToActionList(scriptComposer);
			gameData.nbStep = ActionManipulator.getNbStep(go.GetComponent<Script>());
		}
	}

	
}


