using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CurrentActionSystem : FSystem {
    private Family newStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private Family currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(BasicAction),typeof(UIActionType), typeof(CurrentAction)));
	//private Family editableScriptContainer = FamilyManager.getFamily(new AllOfComponents(typeof(VerticalLayoutGroup), typeof(CanvasRenderer), typeof(PointerSensitive)));
	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));
	private Family wallGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"));
	private Family droneGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Drone"));
	private Family doorGO = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
	private Family redDetectorGO = FamilyManager.getFamily(new AllOfComponents(typeof(Rigidbody), typeof(Detector), typeof(Position)));
	private Family coinGO = FamilyManager.getFamily(new AllOfComponents(typeof(CapsuleCollider), typeof(Position), typeof(ParticleSystem)), new AnyOfTags("Coin"));
	private Family activableConsoleGO = FamilyManager.getFamily(new AllOfComponents(typeof(Activable),typeof(Position),typeof(AudioSource)));
	private Family firstStep = FamilyManager.getFamily(new AllOfComponents(typeof(FirstStep))); 
	private Family scriptIsRunning = FamilyManager.getFamily(new AllOfComponents(typeof(PlayerIsMoving)));

	public CurrentActionSystem(){
		newStep_f.addEntryCallback(onNewStep);
		firstStep.addEntryCallback(initFirstActions);
		scriptIsRunning.addExitCallback(removePlayersCurrentActions);
	}

	private IEnumerator delayAddCurrentAction(GameObject nextAction, GameObject agent){
		yield return null;
		GameObjectManager.addComponent<CurrentAction>(nextAction, new{agent = agent});
	}

	private void removePlayersCurrentActions(int unused){
		foreach(GameObject currentAction in currentActions){
			if(currentAction.GetComponent<CurrentAction>().agent.CompareTag("Player"))
				GameObjectManager.removeComponent<CurrentAction>(currentAction);
		}
	}
	
	private void onNewStep(GameObject unused){
		if(unused == null || scriptIsRunning.Count != 0){ // player has next action
			Debug.Log("on new step");
			GameObject nextAction;
			CurrentAction current;
			foreach(GameObject currentAction in currentActions){
				current = currentAction.GetComponent<CurrentAction>();
				if(current != null){ //current not in gameData.actionsHistory
					nextAction = getNextAction(currentAction, current.agent);
					Debug.Log("nextAction = "+nextAction);
					/*
					//loop drone script
					if(nextAction == null && current.agent.CompareTag("Drone")){
						nextAction = getFirstActionOf(current.agent.GetComponent<ScriptRef>().scriptContainer.transform.GetChild(0).gameObject, current.agent);
					}
					*/
					if(nextAction != null){
						//parent = for & first loop and first child, currentfor = 0 -> currentfor = 1
						if(nextAction.transform.parent.GetComponent<ForAction>() && nextAction.transform.parent.GetComponent<ForAction>().currentFor == 0 && 
						nextAction.Equals(nextAction.transform.parent.GetComponent<ForAction>().firstChild)){
							ForAction forAct = nextAction.transform.parent.GetComponent<ForAction>();
							forAct.currentFor++;
							forAct.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
						}
						//ask to add CurrentAction on next frame
						MainLoop.instance.StartCoroutine(delayAddCurrentAction(nextAction, current.agent));	
					}

					GameObjectManager.removeComponent<CurrentAction>(currentAction);				
				}
			}
		}
		
	}

	public GameObject getNextAction(GameObject currentAction, GameObject agent){
		if(currentAction.GetComponent<BasicAction>()){
			if(currentAction.GetComponent<BasicAction>().next == null || currentAction.GetComponent<BasicAction>().next.GetComponent<BasicAction>()){
				return currentAction.GetComponent<BasicAction>().next;
			}
			else{
				return getNextAction(currentAction.GetComponent<BasicAction>().next, agent);
			}
		}

		else if(currentAction.GetComponent<ForAction>()){
			ForAction forAct = currentAction.GetComponent<ForAction>();
			//do next action
			if(currentAction.GetComponent<ForAction>().currentFor == currentAction.GetComponent<ForAction>().nbFor){
				if(currentAction.GetComponent<CurrentAction>())
					GameObjectManager.removeComponent<CurrentAction>(currentAction);
				if(currentAction.GetComponent<ForAction>().next == null || currentAction.GetComponent<ForAction>().next.GetComponent<BasicAction>()){
					return currentAction.GetComponent<ForAction>().next;
				}
				else{
					return getNextAction(currentAction.GetComponent<ForAction>().next , agent);
				}
			}
			//loop
			else{
				if(!currentAction.GetComponent<CurrentAction>())
					GameObjectManager.addComponent<CurrentAction>(currentAction, new{agent = agent});
				forAct.currentFor++;
				forAct.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
				if(currentAction.GetComponent<ForAction>().firstChild == null || currentAction.GetComponent<ForAction>().firstChild.GetComponent<BasicAction>())
					return currentAction.GetComponent<ForAction>().firstChild;
				else
					return getNextAction(currentAction.GetComponent<ForAction>().firstChild, agent);
			}
		}
		else if(currentAction.GetComponent<IfAction>()){
			if(currentAction.GetComponent<IfAction>().firstChild != null && ifValid(currentAction.GetComponent<IfAction>(), agent)){ 
				if(currentAction.GetComponent<IfAction>().firstChild.GetComponent<BasicAction>())
					return currentAction.GetComponent<IfAction>().firstChild;
				else
					return getNextAction(currentAction.GetComponent<IfAction>().firstChild, agent);				
			}
			else{
				if(currentAction.GetComponent<IfAction>().next == null || currentAction.GetComponent<IfAction>().next.GetComponent<BasicAction>()){
					return currentAction.GetComponent<IfAction>().next;
				}
				else{
					return getNextAction(currentAction.GetComponent<IfAction>().next , agent);
				}				
			}

		}
		else if(currentAction.GetComponent<LoopAction>()){
			//loop
			if(!currentAction.GetComponent<CurrentAction>())
				GameObjectManager.addComponent<CurrentAction>(currentAction, new{agent = agent});
			
			if(currentAction.GetComponent<LoopAction>().firstChild == null || currentAction.GetComponent<LoopAction>().firstChild.GetComponent<BasicAction>())
				return currentAction.GetComponent<LoopAction>().firstChild;
			
			else
				return getNextAction(currentAction.GetComponent<LoopAction>().firstChild, agent);
		}

		return null;
	}

	private IEnumerator delayInit(){
		yield return null;
		GameObject firstAction = null;
		foreach(GameObject robot in playerGO){
			if(robot.GetComponent<ScriptRef>().scriptContainer.transform.childCount > 0){
				firstAction = getFirstActionOf(robot.GetComponent<ScriptRef>().scriptContainer.transform.GetChild(0).gameObject, robot);
			}
			Debug.Log("firstAction = "+firstAction);
			if(firstAction != null){
				GameObjectManager.addComponent<CurrentAction>(firstAction, new{agent = robot});

				//Debug.Log("firstAction robot = "+firstAction.name);
				if(firstAction.transform.parent.GetComponent<ForAction>()){
					ForAction forAct = firstAction.transform.parent.GetComponent<ForAction>();
					GameObjectManager.addComponent<CurrentAction>(firstAction.transform.parent.gameObject, new{agent = robot});
					forAct.currentFor++;
					forAct.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
				}
				else if (firstAction.transform.parent.GetComponent<LoopAction>())
					GameObjectManager.addComponent<CurrentAction>(firstAction.transform.parent.gameObject, new{agent = robot});
			}
			
			else{
				GameObjectManager.addComponent<EmptyExecution>(MainLoop.instance.gameObject);
			}
			

		}
		if(firstAction != null){ //not an empty execution
			//current action drone(s)
			//Debug.Log("drones "+droneGO.Count);
			firstAction = null;
			foreach(GameObject drone in droneGO){
				if(!drone.GetComponent<ScriptRef>().scriptContainer.GetComponentInChildren<CurrentAction>()){
					if(drone.GetComponent<ScriptRef>().scriptContainer.transform.childCount > 0){
						firstAction = getFirstActionOf(drone.GetComponent<ScriptRef>().scriptContainer.transform.GetChild(0).gameObject, drone);
						//Debug.Log("firstAction drone = "+firstAction.name);
					}
					//Debug.Log(firstAction);
					if(firstAction != null){
						GameObjectManager.addComponent<CurrentAction>(firstAction, new{agent = drone});
						Debug.Log("firstAction drone = "+firstAction.name);
						if(firstAction.transform.parent.GetComponent<ForAction>()){
							ForAction forAct = firstAction.transform.parent.GetComponent<ForAction>();
							GameObjectManager.addComponent<CurrentAction>(firstAction.transform.parent.gameObject, new{agent = drone});
							forAct.currentFor++;
							forAct.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
							
						}
						else if (firstAction.transform.parent.GetComponent<LoopAction>())
							GameObjectManager.addComponent<CurrentAction>(firstAction.transform.parent.gameObject, new{agent = drone});
					}			
				}
				else{
					onNewStep(null);
				}
			}
		}

	}

	private void initFirstActions(GameObject unused){
		MainLoop.instance.StartCoroutine(delayInit());
	}

	public void firstAction(GameObject buttonStop){
		if(!buttonStop.activeInHierarchy){
			GameObjectManager.addComponent<FirstStep>(MainLoop.instance.gameObject);
			//MainLoop.instance.StartCoroutine(delayFirstAction());
		}
	}

	public GameObject getFirstActionOf (GameObject go, GameObject agent){
		Debug.Log("getFirstAction "+agent.name);
		if(go == null)
			return null;
		if(go.GetComponent<BasicAction>()){
			Debug.Log("basic action = "+go.name);
			return go;
		}
		else{
			//For
			if (go.GetComponent<ForAction>()){
				//Debug.Log("foraction");
				if(go.GetComponent<ForAction>().firstChild != null && go.GetComponent<ForAction>().nbFor != 0){
					//Debug.Log("nbfor != 0 & firstchild");
					return getFirstActionOf(go.GetComponent<ForAction>().firstChild , agent);
				}
				else{
					return getFirstActionOf(go.GetComponent<ForAction>().next , agent);
				}

			}
			//If
			else if (go.GetComponent<IfAction>()){
				if(go.GetComponent<IfAction>().firstChild != null && ifValid(go.GetComponent<IfAction>(), agent)){
					return getFirstActionOf(go.GetComponent<IfAction>().firstChild, agent);
				}
				else{
					return getFirstActionOf(go.GetComponent<IfAction>().next , agent);
				}
							
			}
			//Loop
			else if(go.GetComponent<LoopAction>()){
				if(go.GetComponent<LoopAction>().firstChild != null){
					//Debug.Log("nbfor != 0 & firstchild");
					return getFirstActionOf(go.GetComponent<LoopAction>().firstChild , agent);
				}
				else{
					return getFirstActionOf(go.GetComponent<LoopAction>().next , agent);
				}
			}
			
		}
		return null;
	}

	public bool ifValid(IfAction nextIf, GameObject scripted){
		bool ifok = nextIf.ifNot;
		Debug.Log("ifvalid "+ifok);
		Vector2 vec = new Vector2();
		switch(getDirection(scripted.GetComponent<Direction>().direction,nextIf.ifDirection)){
			case Direction.Dir.North:
				vec = new Vector2(0,1);
				break;
			case Direction.Dir.South:
				vec = new Vector2(0,-1);
				break;
			case Direction.Dir.East:
				vec = new Vector2(1,0);
				break;
			case Direction.Dir.West:
				vec = new Vector2(-1,0);
				break;
		}
		switch(nextIf.ifEntityType){
			case 0:
				foreach( GameObject go in wallGO){
					if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * nextIf.range &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * nextIf.range){
						ifok = !nextIf.ifNot;
					}
				}
				break;
			case 1:
				foreach( GameObject go in doorGO){
					if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * nextIf.range &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * nextIf.range){
						ifok = !nextIf.ifNot;
					}
				}
				break;
			case 2:
				foreach( GameObject go in droneGO){
					if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * nextIf.range &&
						go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * nextIf.range){
						ifok = !nextIf.ifNot;
					}
				}
				break;
			case 3:
				foreach( GameObject go in playerGO){
					if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * nextIf.range &&
						go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * nextIf.range){
						ifok = !nextIf.ifNot;
					}
				}
				break;
			case 4:
				foreach( GameObject go in activableConsoleGO){
					if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * nextIf.range &&
						go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * nextIf.range){
						ifok = !nextIf.ifNot;
					}
				}
				break;
			case 5:
				foreach( GameObject go in redDetectorGO){
					if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * nextIf.range &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * nextIf.range){
						ifok = !nextIf.ifNot;
					}
				}
				break;
			case 6:
				foreach( GameObject go in coinGO){
					if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * nextIf.range &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * nextIf.range){
						ifok = !nextIf.ifNot;
					}
				}
				break;				
		}
		return ifok;
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

}