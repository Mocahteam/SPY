using UnityEngine;
using FYFY;
using UnityEngine.UI;
using System;
using TMPro;

public class ActionBlocSystem : FSystem {
	
	//available actions in panel
	private Family availableActions = FamilyManager.getFamily(new AllOfComponents(typeof(Available), typeof(ElementToDrag)));
	
	//dropped actions in playerscript container
	private Family droppedActions = FamilyManager.getFamily(new AllOfComponents(typeof(Dropped), typeof(UIActionType)));
	private Family deletedActions = FamilyManager.getFamily(new AllOfComponents(typeof(AddOne)));
	private GameData gameData;
	private Family draggableElement = FamilyManager.getFamily(new AllOfComponents(typeof(ElementToDrag)));

	public ActionBlocSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		//LimitTexts
		foreach(GameObject go in draggableElement){
			updateBlocLimit(getActionKey(go.GetComponent<ElementToDrag>().actionPrefab.GetComponent<BaseElement>()));
		}
		droppedActions.addEntryCallback(useAction);
		deletedActions.addEntryCallback(unuseAction);
	}

	private string getActionKey(BaseElement action){
		string actionKey = null;
		switch(action.GetType().ToString()){
			case "BasicAction":
				actionKey = ((BasicAction)action).actionType.ToString();
				break;
			case "IfAction":
				actionKey = "If";
				break;
			case "ForAction":
				actionKey = "For";
				break;
			default:
				break;
		}
		return actionKey;
	}

	private GameObject getDraggableElement (string name){
		foreach(GameObject go in draggableElement){
			if (go.name.Equals(name)){
				return go;
			}
		}
		return null;
	}

	private void updateBlocLimit(string keyName){
		bool isActive = gameData.actionBlocLimit[keyName] != 0; // negative means no limit
		GameObjectManager.setGameObjectState(getDraggableElement(keyName), isActive);
		if(isActive){
			GameObjectManager.addComponent<Available>(getDraggableElement(keyName));
			if(gameData.actionBlocLimit[keyName] < 0){
				GameObjectManager.setGameObjectState(getDraggableElement(keyName).transform.GetChild(1).gameObject, false);
			}
			else{
				getDraggableElement(keyName).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Reste " + gameData.actionBlocLimit[keyName].ToString();
				GameObjectManager.setGameObjectState(getDraggableElement(keyName).transform.GetChild(1).gameObject, true);
			}
		}		
	}

	private void useAction(GameObject go){
		Debug.Log("useaction");
		string actionKey = getActionKey(go.GetComponent<BaseElement>());
		if(actionKey != null){
			Debug.Log("-1");
			gameData.actionBlocLimit[actionKey] -= 1;
			updateBlocLimit(actionKey);		
		}
		GameObjectManager.removeComponent<Dropped>(go);
	}
	
	private void unuseAction(GameObject go){
		Debug.Log("unuse action");
		Debug.Log("deleted item = "+go.name);
		BaseElement action;
		if(go.GetComponent<ElementToDrag>()){
			action = go.GetComponent<ElementToDrag>().actionPrefab.GetComponent<BaseElement>();
		}
		else{
			action = go.GetComponent<BaseElement>();
		}
		string actionKey = getActionKey(action);

		AddOne[] addOnes =  go.GetComponents<AddOne>();
		if(actionKey != null){
			Debug.Log("+1");
			gameData.actionBlocLimit[actionKey] += addOnes.Length;
			updateBlocLimit(actionKey);

		}
		foreach(AddOne a in addOnes){
			GameObjectManager.removeComponent(a);	
		}
	}

}