using UnityEngine;
using FYFY;
using UnityEngine.UI;
using FYFY_plugins.TriggerManager;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using FYFY_plugins.PointerManager;
public class ApplyScriptSystem : FSystem {
	private Family wallGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"));
	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));
	private Family activableConsoleGO = FamilyManager.getFamily(new AllOfComponents(typeof(Activable),typeof(Position),typeof(AudioSource)));
    private Family newCurrentAction_f = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction), typeof(BasicAction)));
	//private Family playerScriptContainer_f = FamilyManager.getFamily(new AllOfComponents(typeof(UITypeContainer)), new AnyOfTags("ScriptConstructor"));
	//private Family scriptedGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position), typeof(Direction)));
	private Family exitGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position), typeof(AudioSource)), new AnyOfTags("Exit"));
    private Family endpanel_f = FamilyManager.getFamily(new AllOfComponents(typeof(Image), typeof(AudioSource)), new AnyOfTags("endpanel"));
    private Family robotcollision_f = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D)), new AnyOfTags("Player"));
	//private Family droneGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Drone"));
	//private Family redDetectorGO = FamilyManager.getFamily(new AllOfComponents(typeof(Rigidbody), typeof(Detector), typeof(Position)));
	//private Family coinGO = FamilyManager.getFamily(new AllOfComponents(typeof(CapsuleCollider), typeof(Position), typeof(ParticleSystem)), new AnyOfTags("Coin"));
	//private Family highlightedItems = FamilyManager.getFamily(new AllOfComponents(typeof(UIActionType), typeof(HighLight)));
	//private Family highlightedItems = FamilyManager.getFamily(new AllOfComponents(typeof(BasicAction), typeof(UIActionType), typeof(CurrentAction)));
	//private Family editableScriptContainer = FamilyManager.getFamily(new AllOfComponents(typeof(VerticalLayoutGroup), typeof(CanvasRenderer), typeof(PointerSensitive)));
	//private Family agentCanvas = FamilyManager.getFamily(new AllOfComponents(typeof(HorizontalLayoutGroup), typeof(CanvasRenderer)), new NoneOfComponents(typeof(Image)));
	private GameObject endPanel;
	private GameData gameData;
	//private bool activeRedDetector;
	//private static Action previousAction;

	public ApplyScriptSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
        newCurrentAction_f.addEntryCallback(onNewCurrentAction);
        endPanel = endpanel_f.First();
        GameObjectManager.setGameObjectState(endPanel, false);

    }

	/*
    private void onNewCollision(GameObject robot){
		if(activeRedDetector){
			Triggered3D trigger = robot.GetComponent<Triggered3D>();
			foreach(GameObject target in trigger.Targets){
				//Check if the player collide with a detection cell
				if (target.GetComponent<Detector>() != null){
					//end level
					Debug.Log("Repéré !");
					GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.Detected });
				}
				else if(target.CompareTag("Coin")){

				}
			}			
		}
    }

	public void detectCollision(bool on){
		activeRedDetector = on;
	}
	*/

	//Return the current action
    public static Transform getCurrentAction(GameObject go) {
		Transform action = go.GetComponent<ScriptRef>().container.transform.GetChild(go.GetComponent<ScriptRef>().currentAction); 
		//end when a pure action is found
		while(!(action.GetComponent<BasicAction>())){
			//Case For / If
			if(action.GetComponent<ForAction>() || action.GetComponent<IfAction>()){
				if(action.childCount != 0)
					action = action.GetChild(action.GetComponent<BaseElement>().currentAction);
				else
					action = go.GetComponent<ScriptRef>().container.transform.GetChild(go.GetComponent<ScriptRef>().currentAction+1); 
			}
		}
		return action;
	}

	// Use to process your families.
	private void onNewCurrentAction(GameObject currentAction) {
		CurrentAction ca = currentAction.GetComponent<CurrentAction>();	
		Debug.Log("on current action "+ca.name);
		if(ca.agent.CompareTag("Player")){
			if(!MainLoop.instance.gameObject.GetComponent<PlayerIsMoving>())
				GameObjectManager.addComponent<PlayerIsMoving>(MainLoop.instance.gameObject);
		}

		switch (currentAction.GetComponent<BasicAction>().actionType){
			case BasicAction.ActionType.Forward:
				ApplyForward(ca.agent);
				break;

			case BasicAction.ActionType.TurnLeft:
				ApplyTurnLeft(ca.agent);
				break;

			case BasicAction.ActionType.TurnRight:
				ApplyTurnRight(ca.agent);
				break;
			case BasicAction.ActionType.TurnBack:
				ApplyTurnBack(ca.agent);
				break;
			case BasicAction.ActionType.Wait:
				break;
			case BasicAction.ActionType.Activate:
				foreach( GameObject actGo in activableConsoleGO){
					if(actGo.GetComponent<Position>().x == ca.agent.GetComponent<Position>().x && actGo.GetComponent<Position>().z == ca.agent.GetComponent<Position>().z){
						actGo.GetComponent<AudioSource>().Play();
						actGo.GetComponent<Activable>().isActivated = true;
					}
				}
				break;
		}

        //Check if the player is on the end of the level
        int nbEnd = 0;
        foreach (GameObject player in playerGO)
        {
            foreach (GameObject exit in exitGO)
            {
                if (player.GetComponent<Position>().x == exit.GetComponent<Position>().x && player.GetComponent<Position>().z == exit.GetComponent<Position>().z)
                {
                    nbEnd++;
                    //end level
                    if (nbEnd >= playerGO.Count)
                    {
                        Debug.Log("Fin du niveau");
                        GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.Win });
                    }
                }
            }
        }



        //applyIfEntityType();
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

	private void ApplyTurnBack(GameObject go){
		switch (go.GetComponent<Direction>().direction){
			case Direction.Dir.North:
				go.GetComponent<Direction>().direction = Direction.Dir.South;
				break;
			case Direction.Dir.South:
				go.GetComponent<Direction>().direction = Direction.Dir.North;
				break;
			case Direction.Dir.East:
				go.GetComponent<Direction>().direction = Direction.Dir.West;
				break;
			case Direction.Dir.West:
				go.GetComponent<Direction>().direction = Direction.Dir.East;
				break;
		}
	}

	private bool checkObstacle(int x, int z){
		foreach( GameObject go in wallGO){
			if(go.GetComponent<Position>().x == x && go.GetComponent<Position>().z == z)
				return true;
		}
		return false;
	}

/*
	public void applyIfEntityType(){
		//Check if If actions are valid
		//int nbStepToAdd = 0;
		foreach( GameObject scripted in scriptedGO){
			int nbStepPlayer = 0;
			//invalidAllIf(scripted.GetComponent<ScriptRef>());
			Transform nextIfAction = getCurrentIf(scripted);
			IfAction nextIf = nextIfAction.gameObject.GetComponent<IfAction>();
			while(nextIf != null && !endOfScript(scripted)){
				//Check if ok
				if(ifValid(nextIf, scripted)){
					//nextIf.ifValid = true;
					if(scripted.tag == "Player"){
						nbStepPlayer += getNbStep(nextIfAction, true);
					}
				}
				else{
					nextIf.currentAction = nextIfAction.transform.childCount-1;
					//incrementActionScript(scripted.GetComponent<ScriptRef>(), highlightedItems);
				}
				nextIfAction = getCurrentIf(scripted);
			}
		/*
			if(nbStepPlayer > nbStepToAdd){
				nbStepToAdd = nbStepPlayer;
			}
		*/
		//}
		//gameData.nbStep += nbStepToAdd;

	//}


	//Return true if the script is at the end
    public static bool endOfScript(GameObject go){
		return go.GetComponent<ScriptRef>().currentAction >= go.GetComponent<ScriptRef>().container.transform.childCount;
	}
/*
	public Transform getCurrentIf(GameObject go){
		if(go.GetComponent<ScriptRef>().container.transform.childCount == 0 ||
		go.GetComponent<ScriptRef>().currentAction >= go.GetComponent<ScriptRef>().container.transform.childCount){
			return null;
		}
		Transform action = go.GetComponent<ScriptRef>().container.transform.GetChild(go.GetComponent<ScriptRef>().currentAction); 
		//end when a pure action is found
		while(!(action.gameObject.GetComponent<BasicAction>())){
			//Case For / If
			if(action.gameObject.GetComponent<ForAction>()){
				if(action.gameObject.GetComponent<ForAction>().currentAction >= action.transform.childCount){
					return null;
				}
				action = action.transform.GetChild(action.gameObject.GetComponent<ForAction>().currentAction);
			}
			if(action.gameObject.GetComponent<IfAction>()){
				if(action.transform.childCount != 0 && action.gameObject.GetComponent<IfAction>().currentAction == 0
				&& !ifValid(action.gameObject.GetComponent<IfAction>(), go)){

					return action;
				}
				else{
					if(action.gameObject.GetComponent<IfAction>().currentAction >= action.transform.childCount){
						return null;
					}
					action = action.transform.GetChild(action.gameObject.GetComponent<IfAction>().currentAction);
				}
			}
		}

		return null;
	}
*/
	//increment the iterator of the action script
	public static void incrementActionScript(ScriptRef script, Family highlightedItems){
		//remove highlight of previous action
		/*
		foreach(GameObject highlightedGO in highlightedItems){
			if (highlightedGO != null && highlightedGO.GetComponent<HighLight>() != null){
				Debug.Log("remove");
				GameObjectManager.removeComponent<HighLight>(highlightedGO);
			}
		}*/


		Transform action = script.container.transform.GetChild(script.currentAction);
		
		if(incrementAction(action)){
			Debug.Log("increment action");
			/*
			if (action.target != null && action.target.GetComponent<UIActionType>().type != Action.ActionType.For &&
			action.target.GetComponent<UIActionType>().type != Action.ActionType.If){
				//add highlight to current action
				if(action.target != null && action.target.GetComponent<HighLight>() == null){
					Debug.Log(action.target.GetComponent<UIActionType>().type);
					GameObjectManager.addComponent<HighLight>(action.target);

				}
			}*/

			Debug.Log("++");
			script.currentAction++;

		}
		/*
		if(script.currentAction >= script.transform.childCount && script.repeat){
			script.currentAction = 0;
			Debug.Log("= 0");
		}*/

	}

    public static bool incrementAction(Transform act){
		/*
		Debug.Log("previousAction = "+ ((previousAction == null)? "null":previousAction.target.name));
		//remove highlight of previous action if previous action = for or if
		if(previousAction != null && previousAction.target.GetComponent<HighLight>() != null){
			Debug.Log("remove highlight previous action");
			GameObjectManager.removeComponent<HighLight>(previousAction.target);					
		}
		*/
		if(act.gameObject.GetComponent<BasicAction>()){
			/*
			if(act.target != null && act.target.GetComponent<HighLight>() != null){
				Debug.Log("target : " + act.target.name);
				Debug.Log("remove highlight2");
				GameObjectManager.removeComponent(act.target.GetComponent<HighLight>());
			}*/
			return true;
		}
		//case for&if
		/*
		if (act.currentAction != 0 && act.actions.Count != 0){
			if (act.actions[act.currentAction-1].actionType == Action.ActionType.For || act.actions[act.currentAction-1].actionType == Action.ActionType.If){
				Action lastAction = act.actions[act.currentAction-1];
				if(lastAction.actions.Count != 0)
					previousAction = lastAction.actions[lastAction.actions.Count-1];
			}
		}*/		

		//Case For
		else if(act.gameObject.GetComponent<ForAction>()){
			/*
			if (act.actions.Count != 0 && act.currentAction != 0){
				previousAction = act.actions[act.currentAction-1];
			}*/

			if(act.childCount != 0 && incrementAction(act.GetChild(act.gameObject.GetComponent<ForAction>().currentAction))){
				//if not end of for
				if(act.GetComponent<ForAction>().currentFor < act.GetComponent<ForAction>().nbFor && act.GetComponent<ForAction>().target != null){
					//new loop display
					act.GetComponent<ForAction>().target.transform.GetComponentInChildren<TMP_InputField>().text =
					(act.GetComponent<ForAction>().currentFor +1).ToString() + " / " + act.GetComponent<ForAction>().nbFor.ToString();
				}
				/*
				string type = act.GetChild(act.GetComponent<ForAction>().currentAction).GetComponent<ForAction>().target.GetComponent<UIActionType>().action.GetType().ToString();
				if(act.GetChild(act.GetComponent<ForAction>().currentAction).GetComponent<ForAction>().target != null &&
				 !type.Equals("ForAction") && !type.Equals("IfAction"))

				{
					//add highlight to current action in current loop
					if(act.actions[act.currentAction].target != null && act.actions[act.currentAction].target.GetComponent<HighLight>() == null){
						GameObjectManager.addComponent<HighLight>(act.actions[act.currentAction].target);
						//previousAction = act.actions[act.currentAction];
					}					
				}*/

				act.gameObject.GetComponent<ForAction>().currentAction++;
			}
			//another loop
			if(act.gameObject.GetComponent<ForAction>().currentAction >= act.childCount){
				act.gameObject.GetComponent<ForAction>().currentAction = 0;
				act.gameObject.GetComponent<ForAction>().currentFor++;


				//End of for
				if(act.gameObject.GetComponent<ForAction>().currentFor >= act.gameObject.GetComponent<ForAction>().nbFor){
					act.gameObject.GetComponent<ForAction>().currentAction = 0;
					act.gameObject.GetComponent<ForAction>().currentFor = act.gameObject.GetComponent<ForAction>().nbFor-1;
					/*
					Debug.Log("end for -- ");
					if(act.actions.Count > 0 && act.actions[act.actions.Count-1].target != null && act.actions[act.actions.Count-1].target.GetComponent<HighLight>() != null){
						Debug.Log("target : " + act.actions[act.actions.Count-1].target.name);
						Debug.Log("remove highlight3 end for");
						GameObjectManager.removeComponent(act.actions[act.actions.Count-1].target.GetComponent<HighLight>());
					}*/
					//Debug.Log("true");
					return true;
				}
				
				/*
				Debug.Log("---------------end loop");
				Debug.Log(act.currentAction);
				Debug.Log(act.actions[act.currentAction-1].target);
				//Debug.Log(act.actions[act.currentAction-1].target.GetComponent<HighLight>());
				//remove highlight on last action of for
				/*
				if(act.currentAction > 0 && act.actions[act.currentAction-1].target != null){
					Debug.Log("-remove highlight22");
					GameObjectManager.removeComponent<HighLight>(act.actions[act.currentAction-1].target);
				}/*
				if(act.actions[act.actions.Count-1].target != null && act.actions[act.actions.Count-1].target.GetComponent<HighLight>() != null){
					Debug.Log("TEST");
					GameObjectManager.removeComponent<HighLight>(act.actions[act.actions.Count-1].target);
				}*/
			}
		}
		else if(act.gameObject.GetComponent<IfAction>()){
			/*
			if (act.actions.Count != 0 && act.currentAction != 0){
				previousAction = act.actions[act.currentAction-1];
			}*/		
			if(act.childCount != 0 && incrementAction(act.GetChild(act.gameObject.GetComponent<IfAction>().currentAction))){
				/*
				//remove highlight of previous action in current loop				
				if(act.currentAction > 0 && act.actions[act.currentAction-1].target != null && act.actions[act.currentAction-1].target.GetComponent<HighLight>() != null){
					GameObjectManager.removeComponent<HighLight>(act.actions[act.currentAction-1].target);
				}*/
				/*
				if (act.actions[act.currentAction].target != null && act.actions[act.currentAction].target.GetComponent<UIActionType>().type != Action.ActionType.For &&
				act.actions[act.currentAction].target.GetComponent<UIActionType>().type != Action.ActionType.If){
					//add highlight to current action in current loop}
					if(act.actions[act.currentAction].target != null && act.actions[act.currentAction].target.GetComponent<HighLight>() == null){
						GameObjectManager.addComponent<HighLight>(act.actions[act.currentAction].target);
						//previousAction = act.actions[act.currentAction];
					}
				}*/
				act.gameObject.GetComponent<IfAction>().currentAction++;
			}
				
			
			if(act.gameObject.GetComponent<IfAction>().currentAction >= act.childCount){
				act.gameObject.GetComponent<IfAction>().currentAction = 0;
				//if(act.actions.Count != 0 && act.actions[act.currentAction].target != null && act.actions[act.currentAction].target.GetComponent<HighLight>() != null){
					/*
					Debug.Log("target : " + act.actions[act.currentAction].target.name);
					Debug.Log("remove highlight4 end if");
					GameObjectManager.removeComponent(act.actions[act.currentAction].target.GetComponent<HighLight>());
					*/
				//}
					
				return true;
			}
		}
		/*
		if(act.actions[act.currentAction].target != null && act.actions[act.currentAction].target.GetComponent<HighLight>() == null){
			Debug.Log("highlight target : " + act.actions[act.currentAction].target.name);
			Debug.Log("add highlight2");
			GameObjectManager.addComponent<HighLight>(act.actions[act.currentAction].target);
		}*/
		return false;
	}

	//Return the lenght of the script
	public static int getNbStep(ScriptRef script){
		int nb = 0;
		foreach(Transform act in script.container.transform){
			nb += getNbStep(act);
		}

		return nb;
	}

	public static int getNbStep(Transform action, bool ignoreIf = false){
		if(action.gameObject.GetComponent<ForAction>()){
			int nb = 0;
			foreach(Transform act in action.transform){
				nb += getNbStep(act) * action.GetComponent<ForAction>().nbFor;
			}
			return nb;
		}
		else if(action.gameObject.GetComponent<IfAction>() && !ignoreIf){
			return 0;
		}
		else if(action.gameObject.GetComponent<IfAction>() && ignoreIf){
			int nb = 0;
			foreach(Transform act in action.transform){
				nb += getNbStep(act);
			}
			return nb;
		}
		else
			return 1;
	}

	//0 Forward, 1 Backward, 2 Left, 3 Right
	public static Direction.Dir getDirection(Direction.Dir dirEntity, int relativeDir){
		if(relativeDir == 0)
			return dirEntity;
		switch(dirEntity){
			case Direction.Dir.North:
				switch(relativeDir){
					case 1:
						return Direction.Dir.South;
					case 2:
						return Direction.Dir.West;
					case 3:
						return Direction.Dir.East;
				}
				break;
			case Direction.Dir.West:
				switch(relativeDir){
					case 1:
						return Direction.Dir.East;
					case 2:
						return Direction.Dir.South;
					case 3:
						return Direction.Dir.North;
				}
				break;
			case Direction.Dir.East:
				switch(relativeDir){
					case 1:
						return Direction.Dir.West;
					case 2:
						return Direction.Dir.North;
					case 3:
						return Direction.Dir.South;
				}
				break;
			case Direction.Dir.South:
				switch(relativeDir){
					case 1:
						return Direction.Dir.North;
					case 2:
						return Direction.Dir.East;
					case 3:
						return Direction.Dir.West;
				}
				break;
		}
		return dirEntity;
	}
	/*
	public static void invalidAllIf(Script script){
		foreach(Action act in script.actions){
			if(act.actionType == Action.ActionType.If)
				act.ifValid = false;
			invalidAllIf(act);
		}
	}
	public static void invalidAllIf(Action action){
		if(action.actions != null){
			foreach(Action act in action.actions){
				if(act.actionType == Action.ActionType.If)
					act.ifValid = false;
				invalidAllIf(act);
			}
		}
	}
	*/

}


