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
	//private Family scriptIsRunning = FamilyManager.getFamily(new AllOfComponents(typeof(PlayerIsMoving)));

	public CurrentActionSystem(){
		newStep_f.addEntryCallback(onNewStep);
		firstStep.addEntryCallback(initFirstActions);
	}

	private IEnumerator delayAddCurrentAction(GameObject nextAction, GameObject agent){
		yield return null;
		GameObjectManager.addComponent<CurrentAction>(nextAction, new{agent = agent});
	}
	private void onNewStep(GameObject unused){
		Debug.Log("on new step");
		GameObject nextAction;
		CurrentAction current;
		//bool playerEnd = true;
		foreach(GameObject currentAction in currentActions){
			current = currentAction.GetComponent<CurrentAction>();
			if(current != null){ //current not in gameData.actionsHistory
				nextAction = getNextAction(currentAction, current.agent);
				Debug.Log("nextAction = "+nextAction);
				//loop drone script
				if(nextAction == null && current.agent.CompareTag("Drone")){
					nextAction = getFirstActionOf(current.agent.GetComponent<ScriptRef>().container.transform.GetChild(0).gameObject, current.agent);
				}
				if(nextAction != null){
					/*
					if(current.agent.CompareTag("Player")){
						playerEnd = false;
					}*/
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
		//execution finished
		/*
		if(playerEnd && MainLoop.instance.gameObject.GetComponent<PlayerIsMoving>()){
			Debug.Log("fin exec");
			GameObjectManager.removeComponent<PlayerIsMoving>(MainLoop.instance.gameObject);     
		}*/
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
		return null;
	}

	private IEnumerator delayInit(){
		yield return null;
		GameObject firstAction = null;
		foreach(GameObject robot in playerGO){
			if(robot.GetComponent<ScriptRef>().container.transform.childCount > 0){
				firstAction = getFirstActionOf(robot.GetComponent<ScriptRef>().container.transform.GetChild(0).gameObject, robot);
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
			}
			
			else{
				GameObjectManager.addComponent<EmptyExecution>(MainLoop.instance.gameObject);
			}
			

		}

		//current action drone(s)
		//Debug.Log("drones "+droneGO.Count);
		firstAction = null;
		foreach(GameObject drone in droneGO){
			if(!drone.GetComponent<ScriptRef>().container.GetComponentInChildren<CurrentAction>()){
				if(drone.GetComponent<ScriptRef>().container.transform.childCount > 0){
					firstAction = getFirstActionOf(drone.GetComponent<ScriptRef>().container.transform.GetChild(0).gameObject, drone);
				}
				//Debug.Log(firstAction);
				if(firstAction != null){
					GameObjectManager.addComponent<CurrentAction>(firstAction, new{agent = drone});
					//Debug.Log("firstAction drone = "+firstAction.name);
					if(firstAction.transform.parent.GetComponent<ForAction>()){
						ForAction forAct = firstAction.transform.parent.GetComponent<ForAction>();
						GameObjectManager.addComponent<CurrentAction>(firstAction.transform.parent.gameObject, new{agent = drone});
						forAct.currentFor++;
						forAct.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
						
					}
				}			
			}
			else{
				onNewStep(null);
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
		Debug.Log("A");
		if(go == null)
			return null;
		if(go.GetComponent<BasicAction>())
			return go; 
		else{
			Debug.Log("B");
			//For
			if (go.GetComponent<ForAction>()){
				Debug.Log("F");
				if(go.GetComponent<ForAction>().firstChild != null && go.GetComponent<ForAction>().nbFor != 0){
					return getFirstActionOf(go.GetComponent<ForAction>().firstChild , agent);
				}
				else{
					return getFirstActionOf(go.GetComponent<ForAction>().next , agent);
				}

					/*
					if(go.GetComponent<ForAction>().firstChild.GetComponent<BasicAction>()){
						Debug.Log("G");
						res = go.GetComponent<ForAction>().firstChild;
						break;						
					}
					else if (go.GetComponent<ForAction>().firstChild.GetComponent<ForAction>() || go.GetComponent<ForAction>().firstChild.GetComponent<IfAction>()){
						Debug.Log("H");
						res = getFirstActionOf(go.GetComponent<ForAction>().firstChild, agent);
						break;
					}
					*/
			}
			//If
			else if (go.GetComponent<IfAction>()){
				Debug.Log("I");
				if(go.GetComponent<IfAction>().firstChild != null && ifValid(go.GetComponent<IfAction>(), agent)){
					return getFirstActionOf(go.GetComponent<IfAction>().firstChild, agent);
				}
				else{
					return getFirstActionOf(go.GetComponent<IfAction>().next , agent);
				}

				/*
				//Debug.Log("if ok");
				if(go.GetComponent<IfAction>().firstChild.GetComponent<BasicAction>()){
					Debug.Log("J");
					res = go.GetComponent<IfAction>().firstChild;
					break;						
				}
				else if (go.GetComponent<IfAction>().firstChild.GetComponent<ForAction>() || go.GetComponent<IfAction>().firstChild.GetComponent<IfAction>()){
					Debug.Log("K");
					res = getFirstActionOf(go.GetComponent<IfAction>().firstChild, agent);
					break;
				}
				*/
							
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
				Debug.Log("wall");
				for(int i = 1; i <= nextIf.range; i++){
					foreach( GameObject go in wallGO){
						if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * i && go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * i){
							ifok = !nextIf.ifNot;
							Debug.Log("true");
						}
					}
				}
				break;
			case 1:
				for(int i = 1; i <= nextIf.range; i++){
					foreach( GameObject go in doorGO){
						if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * i && go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * i){
							ifok = !nextIf.ifNot;
						}
					}
				}
				break;
			case 2:
				for(int i = 1; i <= nextIf.range; i++){
					foreach( GameObject go in droneGO){
						if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * i && go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * i){
							ifok = !nextIf.ifNot;
						}
					}
				}
				break;
			case 3:
				for(int i = 1; i <= nextIf.range; i++){
					foreach( GameObject go in playerGO){
						if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * i && go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * i){
							ifok = !nextIf.ifNot;
						}
					}
				}
				break;
			case 4:
				for(int i = 1; i <= nextIf.range; i++){
					foreach( GameObject go in activableConsoleGO){
						if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * i && go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * i){
							ifok = !nextIf.ifNot;
						}
					}
				}
				break;
			case 5:
				for(int i = 1; i <= nextIf.range; i++){
					foreach( GameObject go in redDetectorGO){
						if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * i && go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * i){
							ifok = !nextIf.ifNot;
						}
					}
				}
				break;
			case 6:
				for(int i = 1; i <= nextIf.range; i++){
					foreach( GameObject go in coinGO){
						if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * i && go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * i){
							ifok = !nextIf.ifNot;
						}
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