using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

public class CurrentActionSystem : FSystem {
    private Family newStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private Family currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(UIActionType), typeof(CurrentAction)));
	private Family editableScriptContainer = FamilyManager.getFamily(new AllOfComponents(typeof(VerticalLayoutGroup), typeof(CanvasRenderer), typeof(PointerSensitive)));
	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));
	private Family wallGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"));
	private Family droneGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Drone"));
	private Family doorGO = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
	private Family redDetectorGO = FamilyManager.getFamily(new AllOfComponents(typeof(Rigidbody), typeof(Detector), typeof(Position)));
	private Family coinGO = FamilyManager.getFamily(new AllOfComponents(typeof(CapsuleCollider), typeof(Position), typeof(ParticleSystem)), new AnyOfTags("Coin"));
	private Family activableConsoleGO = FamilyManager.getFamily(new AllOfComponents(typeof(Activable),typeof(Position),typeof(AudioSource)));
	
	public CurrentActionSystem(){
		newStep_f.addEntryCallback(onNewStep);
	}

	private void onNewStep(GameObject unused){
		GameObject nextAction;
		CurrentAction current;
		foreach(GameObject currentAction in currentActions){
			if(currentAction.GetComponent<BasicAction>()){
				current = currentAction.GetComponent<CurrentAction>();
				nextAction = getNextAction(currentAction, current.agent);
				Debug.Log("nextAction = "+nextAction);
				
				if(nextAction != null){
					GameObjectManager.addComponent<CurrentAction>(nextAction, new{agent = current.agent});				
				}
				GameObjectManager.removeComponent<CurrentAction>(currentAction);
			}
		}
	}

	public GameObject getNextAction(GameObject currentAction, GameObject agent){
		GameObject nextAction = null;
		Debug.Log("getnextaction - currentAction = "+currentAction.name);
		if(currentAction != null && currentAction.GetComponent<BaseElement>() && currentAction.GetComponent<BaseElement>().next != null){
			//parent = for & last action of for
			if(currentAction.transform.parent.GetComponent<ForAction>() && currentAction.GetComponent<BasicAction>().next.Equals(currentAction.transform.parent.gameObject)){
				ForAction forAct = currentAction.transform.parent.GetComponent<ForAction>();
				//not last loop
				if(forAct.currentFor < forAct.nbFor){
					//repeat loop
					currentAction.transform.parent.GetComponent<ForAction>().currentFor++;
					nextAction = forAct.firstChild; 
				}
				//last loop
				else{
					GameObjectManager.removeComponent<CurrentAction>(forAct.next);
					nextAction = forAct.next;
				}
			}
			//next = if
			else if(currentAction.GetComponent<BaseElement>().next.GetComponent<IfAction>()){
				if(ifValid(currentAction.GetComponent<BaseElement>().next.GetComponent<IfAction>(), agent)){
					GameObject firstchild = currentAction.GetComponent<BaseElement>().next.GetComponent<IfAction>().firstChild;
					if (firstchild != null){
						Debug.Log("if firstchild not null");
						if(firstchild.GetComponent<BasicAction>()){
							Debug.Log("if firstchild basic action");
							nextAction = firstchild;
						}
						else{
							nextAction = getNextAction(firstchild, agent);
						}						
					}
					else if(currentAction.GetComponent<BaseElement>().next.GetComponent<IfAction>().next != null) {
						nextAction = getNextAction(currentAction.GetComponent<BaseElement>().next.GetComponent<IfAction>().next, agent);
					}
				}
				else if(currentAction.GetComponent<BaseElement>().next.GetComponent<IfAction>().next != null) {
					nextAction = getNextAction(currentAction.GetComponent<BaseElement>().next.GetComponent<IfAction>().next, agent);
				}
			}
			//next = for
			else if(currentAction.GetComponent<BaseElement>().next.GetComponent<ForAction>()){
				if(currentAction.GetComponent<BaseElement>().next.GetComponent<ForAction>().nbFor != 0){
					GameObject firstchild = currentAction.GetComponent<BaseElement>().next.GetComponent<ForAction>().firstChild;
					if(firstchild != null){
						if(firstchild.GetComponent<BasicAction>()){
							nextAction = firstchild;
						}
						else{
							nextAction = getNextAction(firstchild, agent);
						}
					}
					else if(currentAction.GetComponent<BaseElement>().next.GetComponent<ForAction>().next != null) {
						nextAction = getNextAction(currentAction.GetComponent<BaseElement>().next.GetComponent<ForAction>().next, agent);
					}
				}
				else if(currentAction.GetComponent<BaseElement>().next.GetComponent<ForAction>().next != null) {
					//currentAction = next;
					nextAction = getNextAction(currentAction.GetComponent<BaseElement>().next.GetComponent<ForAction>().next, agent);
				}

			}
			//next = BasicAction
			else if(currentAction.GetComponent<BaseElement>().next.GetComponent<BasicAction>()){
				nextAction = currentAction.GetComponent<BaseElement>().next;
			}

		}
		return nextAction;
	}

	public void firstAction(){
		Debug.Log("robots "+playerGO.Count);
		//current action robot(s)
		GameObject firstAction;
		foreach(GameObject robot in playerGO){
			firstAction = getFirstActionOf(robot.GetComponent<ScriptRef>().container, robot);
			Debug.Log(firstAction);
			if(firstAction != null){
				GameObjectManager.addComponent<CurrentAction>(firstAction, new{agent = robot});

				Debug.Log("firstAction robot = "+firstAction.name);
				if(firstAction.transform.parent.GetComponent<ForAction>()){
					Debug.Log("if");
					GameObjectManager.addComponent<CurrentAction>(firstAction.transform.parent.gameObject, new{agent = robot});
				}
			}

		}

		//current action drone(s)
		Debug.Log("drones "+droneGO.Count);
		foreach(GameObject drone in droneGO){
			if(!drone.GetComponent<ScriptRef>().container.GetComponentInChildren<CurrentAction>()){
			firstAction = getFirstActionOf(drone.GetComponent<ScriptRef>().container, drone);
			Debug.Log(firstAction);
				if(firstAction != null){
					GameObjectManager.addComponent<CurrentAction>(firstAction, new{agent = drone});
					Debug.Log("firstAction drone = "+firstAction.name);
					if(firstAction.transform.GetComponentInParent<ForAction>()){
						GameObjectManager.addComponent<CurrentAction>(firstAction.transform.parent.gameObject, new{agent = drone});
					}
				}				
			}
		}

	}

	public GameObject getFirstActionOf (GameObject container, GameObject agent){
		GameObject res = null;
		if (container.transform.childCount != 0){
			foreach(Transform go in container.transform){
				//BasicAction
				if(go.GetComponent<BasicAction>()){
					res = go.gameObject;
					break;				
				}
				//For
				else if (go.GetComponent<ForAction>() && go.GetComponent<ForAction>().nbFor != 0 && go.GetComponent<ForAction>().firstChild != null){
					if(go.GetComponent<ForAction>().firstChild.GetComponent<BasicAction>()){
						res = go.GetComponent<ForAction>().firstChild;
						break;						
					}
					else if (go.GetComponent<ForAction>().firstChild.GetComponent<ForAction>() || go.GetComponent<ForAction>().firstChild.GetComponent<IfAction>()){
						res = getFirstActionOf(go.GetComponent<ForAction>().firstChild, agent);
						break;
					}
				}
				//If
				else if (go.GetComponent<IfAction>() && go.GetComponent<IfAction>().firstChild != null && ifValid(go.GetComponent<IfAction>(), agent)){
					Debug.Log("if ok");
					if(go.GetComponent<IfAction>().firstChild.GetComponent<BasicAction>()){
						res = go.GetComponent<IfAction>().firstChild;
						break;						
					}
					else if (go.GetComponent<IfAction>().firstChild.GetComponent<ForAction>() || go.GetComponent<IfAction>().firstChild.GetComponent<IfAction>()){
						res = getFirstActionOf(go.GetComponent<IfAction>().firstChild, agent);
						break;
					}
				}
			}
		}
		return res;
	}

	public bool ifValid(IfAction nextIf, GameObject scripted){
		bool ifok = nextIf.ifNot;
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
				for(int i = 1; i <= nextIf.range; i++){
					foreach( GameObject go in wallGO){
						if(go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x * i && go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y * i){
							ifok = !nextIf.ifNot;
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