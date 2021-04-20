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
		foreach(GameObject currentAction in currentActions){
			updateCurrentAction(currentAction);
		}
	}

	public void updateCurrentAction(GameObject currentAction){
		if(currentAction != null && currentAction.GetComponent<BasicAction>()){
			GameObjectManager.removeComponent<CurrentAction>(currentAction);
			//parent = for & last action of for
			if(currentAction.transform.parent.GetComponent<ForAction>() && currentAction.GetComponent<BasicAction>().next.Equals(currentAction.transform.parent.gameObject)){
				ForAction forAct = currentAction.transform.parent.GetComponent<ForAction>();
				//not last loop
				if(forAct.currentFor < forAct.nbFor){
					//repeat loop
					currentAction.transform.parent.GetComponent<ForAction>().currentFor++;
					GameObjectManager.addComponent<CurrentAction>(forAct.firstChild); 
				}
				//last loop
				else{
					GameObjectManager.removeComponent<CurrentAction>(forAct.next);
					GameObjectManager.addComponent<CurrentAction>(forAct.next);
				}
			}
			//next = if
			else if(currentAction.GetComponent<BasicAction>().next.GetComponent<IfAction>()){
				GameObjectManager.addComponent<CurrentAction>(currentAction.GetComponent<BasicAction>().next.GetComponent<IfAction>().firstChild);
			}
			//next = for
			else if(currentAction.GetComponent<BasicAction>().next.GetComponent<ForAction>()){
				if(currentAction.GetComponent<BasicAction>().next.GetComponent<ForAction>().nbFor != 0){
					GameObjectManager.addComponent<CurrentAction>(currentAction.GetComponent<BasicAction>().next);
					GameObjectManager.addComponent<CurrentAction>(currentAction.GetComponent<BasicAction>().next.GetComponent<ForAction>().firstChild);
				}
				else{
					//currentAction = next;
					updateCurrentAction(currentAction.GetComponent<BasicAction>().next.GetComponent<ForAction>().next);
				}

			}
			//next = BasicAction
			else if(currentAction.GetComponent<BasicAction>().next.GetComponent<BasicAction>()){
				GameObjectManager.addComponent<CurrentAction>(currentAction.GetComponent<BasicAction>().next);
			}
			
		}		
	}

	public void firstAction(){
		Debug.Log("robots "+playerGO.Count);
		//current action robot(s)
		GameObject firstAction;
		foreach(GameObject robot in playerGO){
			firstAction = getFirstActionOf(robot.GetComponent<ScriptRef>().container, robot);
			Debug.Log(firstAction);
			if(firstAction != null){
				GameObjectManager.addComponent<CurrentAction>(firstAction);
				Debug.Log("firstAction robot = "+firstAction.name);
				if(firstAction.transform.parent.GetComponent<ForAction>()){
				//if(firstAction.GetComponentInParent<ForAction>()){
					Debug.Log("if");
					GameObjectManager.addComponent<CurrentAction>(firstAction.transform.parent.gameObject);
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
					GameObjectManager.addComponent<CurrentAction>(firstAction);
					Debug.Log("firstAction drone = "+firstAction.name);
					if(firstAction.transform.GetComponentInParent<ForAction>()){
						GameObjectManager.addComponent<CurrentAction>(firstAction.transform.parent.gameObject);
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