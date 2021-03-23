using UnityEngine;
using FYFY;

public class ActionBlocSystem : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	
	//available actions in panel
	private Family availableActions = FamilyManager.getFamily(new AllOfComponents(typeof(Available), typeof(ElementToDrag)));
	
	//dropped actions in playerscript container
	private Family droppedActions = FamilyManager.getFamily(new AllOfComponents(typeof(Dropped), typeof(UIActionType)));
	private GameData gameData;
	private GameObject droppedItem;
	public ActionBlocSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
	}
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		//availableActions.addExitCallback(hideActionBloc);
		droppedActions.addEntryCallback(useAction);
		droppedActions.addExitCallback(unuseAction);
	}

	/*
	private void hideActionBloc(int id){
		GameObject go = GameObject.Find(id.ToString());
		GameObjectManager.setGameObjectState(go, false);
	}
	*/
	private void useAction(GameObject go){
		droppedItem = go;
		Action.ActionType type = go.GetComponent<UIActionType>().type;
		int typeid = -1;
		switch(type){
			case Action.ActionType.Forward:
				typeid = 0;
				break;
			case Action.ActionType.TurnLeft:
				typeid = 1;
				break;
			case Action.ActionType.TurnRight:
				typeid = 2;
				break;
			case Action.ActionType.Wait:
				typeid = 3;
				break;
			case Action.ActionType.Activate:
				typeid = 4;
				break;
			case Action.ActionType.For:
				typeid = 5;
				break;
			case Action.ActionType.If:
				typeid = 6;
				break;
			case Action.ActionType.TurnBack:
				typeid = 7;
				break;
			default:
				break;
		}
		if(typeid != -1){
			gameData.actionBlocLimit[typeid] -= 1;
			if(gameData.actionBlocLimit[typeid] == 0){
				foreach(GameObject actionGO in availableActions){
					if (actionGO.GetComponent<ElementToDrag>().actionPrefab.name.Equals(go.name))
						GameObjectManager.removeComponent<Available>(actionGO);
						//Object.Destroy(go.GetComponent<Available>());
				}
				
			}
				
		}
	}
	
	private void unuseAction(int id){
		if(droppedItem != null){
			Action.ActionType type = droppedItem.GetComponent<UIActionType>().type;
		int typeid = -1;
		switch(type){
			case Action.ActionType.Forward:
				typeid = 0;
				break;
			case Action.ActionType.TurnLeft:
				typeid = 1;
				break;
			case Action.ActionType.TurnRight:
				typeid = 2;
				break;
			case Action.ActionType.Wait:
				typeid = 3;
				break;
			case Action.ActionType.Activate:
				typeid = 4;
				break;
			case Action.ActionType.For:
				typeid = 5;
				break;
			case Action.ActionType.If:
				typeid = 6;
				break;
			case Action.ActionType.TurnBack:
				typeid = 7;
				break;
			default:
				break;
		}
		if(typeid != -1){
			gameData.actionBlocLimit[typeid] += 1;
			if(gameData.actionBlocLimit[typeid] == 0){
				foreach(GameObject actionGO in availableActions){
					if (actionGO.GetComponent<ElementToDrag>().actionPrefab.name.Equals(droppedItem.name))
						GameObjectManager.addComponent<Available>(actionGO);
						//Object.Destroy(actionGO.GetComponent<Available>());
				}
				
			}
		}
		droppedItem = null;
		}
	}

}