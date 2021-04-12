using UnityEngine;
using FYFY;
using UnityEngine.UI;

public class ActionBlocSystem : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	
	//available actions in panel
	private Family availableActions = FamilyManager.getFamily(new AllOfComponents(typeof(Available), typeof(ElementToDrag)));
	
	//dropped actions in playerscript container
	private Family droppedActions = FamilyManager.getFamily(new AllOfComponents(typeof(Dropped), typeof(UIActionType)));
	private Family deletedActions = FamilyManager.getFamily(new AllOfComponents(typeof(AddOne)));
	//private Family undroppedActions = FamilyManager.getFamily(new AllOfComponents(typeof(UIActionType)), new NoneOfComponents(typeof(Dropped)));
	private GameData gameData;
	//private GameObject droppedItemLinkedTo;
	//private Action.ActionType droppedItemType = Action.ActionType.Undefined;
	public ActionBlocSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		//availableActions.addExitCallback(hideActionBloc);
		droppedActions.addEntryCallback(useAction);
		//undroppedActions.addEntryCallback(unuseAction);
		//droppedActions.addExitCallback(unuseAction);
		deletedActions.addEntryCallback(unuseAction);
	}
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
	}

	/*
	private void hideActionBloc(int id){
		GameObject go = GameObject.Find(id.ToString());
		GameObjectManager.setGameObjectState(go, false);
	}
	*/
	private void useAction(GameObject go){
		Debug.Log("useaction");
		//droppedItem = go;
		BaseElement action = go.GetComponent<UIActionType>().action;
		//droppedItemType = type;
		//droppedItemLinkedTo = go.GetComponent<UIActionType>().linkedTo;
		int typeid = -1;
		switch(action.GetType().ToString()){
			case "BasicAction":
				typeid = (int)((BasicAction)action).actionType;
				break;
			case "IfAction":
				typeid = 6;
				break;
			case "ForAction":
				typeid = 7;
				break;
			default:
				break;
		}
		if(typeid != -1){
			Debug.Log("-1");
			gameData.actionBlocLimit[typeid] -= 1;
			if(gameData.actionBlocLimit[typeid] == 0){
				Debug.Log("action not available");
				GameObjectManager.removeComponent<Available>(go.GetComponent<UIActionType>().linkedTo);
				go.GetComponent<UIActionType>().linkedTo.GetComponent<Image>().raycastTarget = false;
				/*
				foreach(GameObject actionGO in availableActions){
					if (actionGO.GetComponent<ElementToDrag>().actionPrefab.name.Equals(go.name))
						GameObjectManager.removeComponent<Available>(actionGO);
						//Object.Destroy(go.GetComponent<Available>());
				}*/
				
			}
				
		}
	}
	
	private void unuseAction(GameObject go){
		Debug.Log("unuse action");
		//if(gameData.deletedItemLinkedTo != null){
			//Debug.Log("deleteditem = "+gameData.deletedItemLinkedTo.name);
			//foreach(GameObject go in deletedActions){
				Debug.Log("deleted item = "+go.name);
				BaseElement action = go.GetComponent<ElementToDrag>().actionPrefab.GetComponent<UIActionType>().action;
				int typeid = -1;
				switch(action.GetType().ToString()){
					case "BasicAction":
						typeid = (int)((BasicAction)action).actionType;
						break;
					case "IfAction":
						typeid = 6;
						break;
					case "ForAction":
						typeid = 7;
						break;
					default:
						break;
				}
				AddOne[] addOnes =  go.GetComponents<AddOne>();
				if(typeid != -1){
					Debug.Log("+1");
					gameData.actionBlocLimit[typeid] += addOnes.Length;
					if(gameData.actionBlocLimit[typeid] > 0){
						Debug.Log("action available");
						GameObjectManager.addComponent<Available>(go);
						go.GetComponent<Image>().raycastTarget = true;
						/*
						foreach(GameObject actionGO in availableActions){
							if (actionGO.GetComponent<ElementToDrag>().actionPrefab.name.Equals(droppedItem.name))
								GameObjectManager.addComponent<Available>(actionGO);
								//Object.Destroy(actionGO.GetComponent<Available>());
						}*/
						
					}
				}
				foreach(AddOne a in addOnes){
					GameObjectManager.removeComponent(a);
			//	}
				
			//}
			//gameData.deletedItemLinkedTo.Clear();
			
		}
	}

}