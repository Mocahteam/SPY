using UnityEngine;
using FYFY;
using System.Collections.Generic;
using UnityEngine.UI;
public class ApplyScriptSystem : FSystem {

	private float cooldown = 2;
	private Family controllableGO = FamilyManager.getFamily(new AllOfComponents(typeof(Script)));
	private Family entityGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position), typeof(Entity)));
	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(Script)), new AnyOfTags("Player"));
	private GameObject scriptComposer;
	private bool initialized = false;

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

		if(!initialized){
			initializeTest();
			initialized=true;
		}

		if(gameData.scriptRunning){
			if(cooldown <= 0){
				cooldown = 2;
				foreach( GameObject go in controllableGO){

					if(endOfScript(go))
						break;
					Action action = getCurrentAction(go);
					
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
					}
					incrementActionScript(go.GetComponent<Script>());
				}

				foreach( GameObject go in playerGO){
					if(endOfScript(go)){
						gameData.scriptRunning = false;
						restartScript(go.GetComponent<Script>());
					}
				}

			}
			else
				cooldown -= Time.deltaTime;
		}

	}

	private void ApplyForward(GameObject go){
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
		if(script.currentAction >= script.actions.Count && script.repeat)
			script.currentAction = 0;

	}

	private void restartScript(Script script){
		script.currentAction = 0;
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
					act.currentFor = 0;
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

	public void addAction(Script script, Action actionToAdd){
		if(script.actions == null)
			script.actions = new List<Action>();

		script.actions.Add(actionToAdd);
	}

	public void addAction(Action action, Action actionToAdd){
		if(action.actions == null)
			action.actions = new List<Action>();

		action.actions.Add(actionToAdd);
	}

	public Action createAction(Action.ActionType type, int nbFor = 0){
		Action action = new Action();
		action.actionType = type;
		action.currentAction = 0;
		action.currentFor = 0;
		action.nbFor = nbFor;

		if(type == Action.ActionType.For || type == Action.ActionType.If || type == Action.ActionType.IfElse || type == Action.ActionType.While)
			action.actions = new List<Action>();

		return action;
	}

	public void resetScript(Script script){
		script.actions = new List<Action>();
		script.currentAction = 0;
	}


	private void initializeTest(){
		foreach( GameObject go in controllableGO){

			resetScript(go.GetComponent<Script>());
			//Action forTurnForward = createAction(Action.ActionType.For, 4);
			//addAction(forTurnForward, createAction(Action.ActionType.Forward));
			//addAction(forTurnForward, createAction(Action.ActionType.TurnLeft));
			//addAction(go.GetComponent<Script>(), forTurnForward);
		}
	}

	public void applyScriptToPlayer(){
		foreach( GameObject go in playerGO){
			resetScript(go.GetComponent<Script>());
			go.GetComponent<Script>().actions = ScriptContainerToActionList();
		}

		gameData.scriptRunning = true;
	}

	private List<Action> ScriptContainerToActionList(){
		List<Action> l = new List<Action>();

		for(int i = 0; i< scriptComposer.transform.childCount; i++){
			GameObject child = scriptComposer.transform.GetChild(i).gameObject;
			if(child.GetComponent<UIActionType>().type == Action.ActionType.For){
				Action forAct = createAction(child.GetComponent<UIActionType>().type);
				forAct.nbFor = int.Parse(child.transform.GetChild(0).transform.GetChild(1).GetComponent<InputField>().text);
				if(forAct.nbFor > 0 && child.transform.childCount > 1 && ContainerToActionList(forAct, child))
					l.Add(forAct);

			}
			else{
				l.Add(createAction(child.GetComponent<UIActionType>().type));
			}
		}
		return l;
	}

	private bool ContainerToActionList(Action act, GameObject obj){

		bool nonEmpty = false;
		for(int i = 1; i < obj.transform.childCount; i++){
			GameObject child = obj.transform.GetChild(i).gameObject;
			if(child.GetComponent<UIActionType>().type == Action.ActionType.For){
				Action forAct = createAction(child.GetComponent<UIActionType>().type);
				forAct.nbFor = int.Parse(child.transform.GetChild(0).transform.GetChild(1).GetComponent<InputField>().text);
				if(forAct.nbFor > 0 && child.transform.childCount > 1 && ContainerToActionList(forAct, child)){
					addAction(act, forAct);
					nonEmpty = true;
				}
			}
			else{
				addAction(act, createAction(child.GetComponent<UIActionType>().type));
				nonEmpty = true;
			}
		}
		return nonEmpty;
	}
}


