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
	private Family exitGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position), typeof(AudioSource)), new AnyOfTags("Exit"));
    private Family endpanel_f = FamilyManager.getFamily(new AllOfComponents(typeof(Image), typeof(AudioSource)), new AnyOfTags("endpanel"));
<<<<<<< HEAD
    //private Family robotcollision_f = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D)), new AnyOfTags("Player"));
=======
    private Family robotcollision_f = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D)), new AnyOfTags("Player"));
	private Family droneGO = FamilyManager.getFamily(new AllOfComponents(typeof(Script), typeof(Position)), new AnyOfTags("Drone"));
	private Family doorGO = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family redDetectorGO = FamilyManager.getFamily(new AllOfComponents(typeof(Rigidbody), typeof(Detector), typeof(Position)));
	private Family coinGO = FamilyManager.getFamily(new AllOfComponents(typeof(CapsuleCollider), typeof(Position), typeof(ParticleSystem)), new AnyOfTags("Coin"));
	//private Family highlightedItems = FamilyManager.getFamily(new AllOfComponents(typeof(UIActionType), typeof(HighLight)));
	private Family highlightedItems = FamilyManager.getFamily(new AllOfComponents(typeof(UIActionType), typeof(HighLight)));

>>>>>>> d279ca19856d0c78f795c23fcc180dd5d9d63441
	private GameObject endPanel;
	private GameData gameData;

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
		Transform action = go.GetComponent<ScriptRef>().scriptContainer.transform.GetChild(go.GetComponent<ScriptRef>().currentAction); 
		//end when a pure action is found
		while(!(action.GetComponent<BasicAction>())){
			//Case For / If
			if(action.GetComponent<ForAction>() || action.GetComponent<IfAction>()){
				if(action.childCount != 0)
					action = action.GetChild(action.GetComponent<BaseElement>().currentAction);
				else
					action = go.GetComponent<ScriptRef>().scriptContainer.transform.GetChild(go.GetComponent<ScriptRef>().currentAction+1); 
			}
		}
		return action;
	}

	// Use to process your families.
	private void onNewCurrentAction(GameObject currentAction) {
		CurrentAction ca = currentAction.GetComponent<CurrentAction>();	
		//Debug.Log("on current action "+ca.name);
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
                        //Debug.Log("Fin du niveau");
                        GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.Win });
                    }
                }
            }
        }

	}


<<<<<<< HEAD
=======
		yield return new WaitForSeconds(0.3f);

		go.GetComponent<Renderer>().enabled = false;
		go.GetComponent<AudioSource>().Play();
		
		yield return new WaitForSeconds(0.5f);
        GameObjectManager.setGameObjectState(go, false);
		//GameObjectManager.unbind(go);
		//Object.Destroy(go);
	}
>>>>>>> d279ca19856d0c78f795c23fcc180dd5d9d63441
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

}


