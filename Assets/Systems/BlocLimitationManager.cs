using UnityEngine;
using FYFY;
using TMPro;

/// <summary>
/// This system manages blocs limitation in inventory
/// </summary>
public class BlocLimitationManager : FSystem {
	private Family droppedActions = FamilyManager.getFamily(new AllOfComponents(typeof(Dropped), typeof(UIActionType)));
	private Family deletedActions = FamilyManager.getFamily(new AllOfComponents(typeof(AddOne)));
	private Family draggableElement = FamilyManager.getFamily(new AllOfComponents(typeof(ElementToDrag)));
	private GameData gameData;

	public BlocLimitationManager(){
		if (Application.isPlaying)
		{
			gameData = GameObject.Find("GameData").GetComponent<GameData>();
			// init limitation counters for each draggable elements
			foreach (GameObject go in draggableElement)
			{
				// get prefab associated to this draggable element
				GameObject prefab = go.GetComponent<ElementToDrag>().actionPrefab;
				// get action key depending on prefab type
				string key = getActionKey(prefab.GetComponent<BaseElement>());
				// update counter
				updateBlocLimit(key, go);
			}
			droppedActions.addEntryCallback(useAction);
			deletedActions.addEntryCallback(unuseAction);
		}
	}

	private string getActionKey(BaseElement action){
		if (action is BasicAction)
			return ((BasicAction)action).actionType.ToString();
		else if (action is IfAction)
			return "If";
		else if (action is ForAction)
			return "For";
		else
			return null;
	}

	private GameObject getDraggableElement (string name){
		foreach(GameObject go in draggableElement){
			if (go.name.Equals(name)){
				return go;
			}
		}
		return null;
	}

	private void updateBlocLimit(string keyName, GameObject draggableGO){
		bool isActive = gameData.actionBlocLimit[keyName] != 0; // negative means no limit
		GameObjectManager.setGameObjectState(draggableGO, isActive);
		if(isActive){
			if(gameData.actionBlocLimit[keyName] < 0)
				// unlimited action => hide counter
				GameObjectManager.setGameObjectState(draggableGO.transform.GetChild(1).gameObject, false);
			else{
				// limited action => init and show counter
				GameObject counterText = draggableGO.transform.GetChild(1).gameObject;
				counterText.GetComponent<TextMeshProUGUI>().text = "Reste " + gameData.actionBlocLimit[keyName].ToString();
				GameObjectManager.setGameObjectState(counterText, true);
			}
		}		
	}

	private void useAction(GameObject go){
		string actionKey = getActionKey(go.GetComponent<BaseElement>());
		if(actionKey != null){
			gameData.actionBlocLimit[actionKey] -= 1;
			GameObject draggableModel = getDraggableElement(actionKey);
			updateBlocLimit(actionKey, draggableModel);		
		}
		GameObjectManager.removeComponent<Dropped>(go);
	}
	
	private void unuseAction(GameObject go){
		BaseElement action;
		if(go.GetComponent<ElementToDrag>())
			action = go.GetComponent<ElementToDrag>().actionPrefab.GetComponent<BaseElement>();
		else
			action = go.GetComponent<BaseElement>();

		string actionKey = getActionKey(action);

		AddOne[] addOnes =  go.GetComponents<AddOne>();
		if(actionKey != null){
			gameData.actionBlocLimit[actionKey] += addOnes.Length;
			GameObject draggableModel = getDraggableElement(actionKey);
			updateBlocLimit(actionKey, draggableModel);
		}
		foreach(AddOne a in addOnes){
			GameObjectManager.removeComponent(a);	
		}
	}
}