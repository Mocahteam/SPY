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

	protected override void onStart()
	{
		GameObject gd = GameObject.Find("GameData");
		if (gd != null)
		{
			gameData = gd.GetComponent<GameData>();
			// init limitation counters for each draggable elements
			// Initialisation des block à afficher dans l'inventaire
			foreach (GameObject go in draggableElement)
			{
				// get prefab associated to this draggable element
				// On récupére le préfab de l'élément
				GameObject prefab = go.GetComponent<ElementToDrag>().actionPrefab;
                // get action key depending on prefab type
                // Si c'est un bloc action
				string key = getActionKey(prefab.GetComponent<Highlightable>());
				// Si c'est un bloc pour les conditions

				// update counter et active les block necessaire
				updateBlocLimit(key, go);
			}
		}
		droppedActions.addEntryCallback(useAction);
		deletedActions.addEntryCallback(unuseAction);
	}

	// Retourne l'action key du bloc
	private string getActionKey(Highlightable action){
		if (action is BasicAction)
			return ((BasicAction)action).actionType.ToString();
		else if (action is IfAction)
			return "If";
		else if (action is ForAction)
			return "For";
		else if (action is BaseCondition)
			return ((BaseCondition)action).conditionType.ToString();
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

	// Met à jour la limite du nombre de fois ou l'on peux utiliser un bloc (si il y a une limite)
	// Le désactive si la limite est atteinte
	// Met à jour le compteur
	private void updateBlocLimit(string keyName, GameObject draggableGO){
		Debug.Log("Name bloc : " + draggableGO.name + " " + keyName);
		Debug.Log("Value : " + gameData.actionBlocLimit[keyName]);
		bool isActive = gameData.actionBlocLimit[keyName] != 0; // negative means no limit
		Debug.Log("Active : " + isActive);
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