using UnityEngine;
using FYFY;
using UnityEngine.UI;
using System;
using TMPro;

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
	private Family draggableElement = FamilyManager.getFamily(new AllOfComponents(typeof(ElementToDrag)));

	//private GameObject droppedItemLinkedTo;
	//private Action.ActionType droppedItemType = Action.ActionType.Undefined;
	public ActionBlocSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		//LimitTexts
		for(int i = 0 ; i < draggableElement.Count ; i++){
			updateBlocLimit(i);
		}
		//availableActions.addExitCallback(hideActionBloc);
		droppedActions.addEntryCallback(useAction);
		//undroppedActions.addEntryCallback(unuseAction);
		//droppedActions.addExitCallback(unuseAction);
		deletedActions.addEntryCallback(unuseAction);
	}

	private void updateBlocLimit(int i){
		bool isActive = gameData.actionBlocLimit[i] != 0; // negative means no limit
		GameObjectManager.setGameObjectState(draggableElement.getAt(i), isActive);
		if(isActive){
			GameObjectManager.addComponent<Available>(draggableElement.getAt(i));
			if(gameData.actionBlocLimit[i] < 0){
				GameObjectManager.setGameObjectState(draggableElement.getAt(i).transform.GetChild(1).gameObject, false);
			}
			else{
				draggableElement.getAt(i).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Reste\n" + gameData.actionBlocLimit[i].ToString();
				GameObjectManager.setGameObjectState(draggableElement.getAt(i).transform.GetChild(1).gameObject, true);
			}
		}		
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
		BaseElement action = go.GetComponent<BaseElement>();
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
			updateBlocLimit(typeid);
			/*
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
				
			//}
				
		}
		GameObjectManager.removeComponent<Dropped>(go);
	}
	
	private void unuseAction(GameObject go){
		Debug.Log("unuse action");
		Debug.Log("deleted item = "+go.name);
		BaseElement action = go.GetComponent<ElementToDrag>().actionPrefab.GetComponent<BaseElement>();
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
		/*
		if(go.GetComponent<ElementToDrag>().actionPrefab.GetComponent<UITypeContainer>()){
			switch(go.GetComponent<ElementToDrag>().actionPrefab.GetComponent<UITypeContainer>().type){
				case UITypeContainer.Type.If:
				typeid = 6;
				break;
				case UITypeContainer.Type.For:
				typeid = 7;
				break;
			}
		}
		else{
			typeid = (int)(BasicAction.ActionType) Enum.Parse(typeof(BasicAction.ActionType), go.name, true);
		}*/
		AddOne[] addOnes =  go.GetComponents<AddOne>();
		if(typeid != -1){
			Debug.Log("+1");
			gameData.actionBlocLimit[typeid] += addOnes.Length;
			updateBlocLimit(typeid);
			/*
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
				
			//}
		}
		foreach(AddOne a in addOnes){
			GameObjectManager.removeComponent(a);	
		}
	}

}