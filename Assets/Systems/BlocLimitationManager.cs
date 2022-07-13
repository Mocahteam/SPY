using UnityEngine;
using FYFY;
using TMPro;

/// <summary>
/// This system manages blocs limitation in inventory
/// </summary>
public class BlocLimitationManager : FSystem {
	private Family droppedActions = FamilyManager.getFamily(new AllOfComponents(typeof(Dropped), typeof(LibraryItemRef)));
	private Family deletedActions = FamilyManager.getFamily(new AllOfComponents(typeof(AddOne), typeof(ElementToDrag)));
	private Family draggableElement = FamilyManager.getFamily(new AllOfComponents(typeof(ElementToDrag)));
	private GameData gameData;

	protected override void onStart()
	{
		GameObject gd = GameObject.Find("GameData");
		if (gd != null)
		{
			gameData = gd.GetComponent<GameData>();
			// init limitation counters for each draggable elements
			foreach (GameObject go in draggableElement)
			{
				// default => hide go
				GameObjectManager.setGameObjectState(go, false);
				// update counter et active les block necessaire
				updateBlocLimit(go);
			}
		}
		droppedActions.addEntryCallback(useAction);
		deletedActions.addEntryCallback(unuseAction);
	}

	// Met à jour la limite du nombre de fois ou l'on peux utiliser un bloc (si il y a une limite)
	// Le désactive si la limite est atteinte
	// Met à jour le compteur
	private void updateBlocLimit(GameObject draggableGO){
		if (gameData.actionBlocLimit.ContainsKey(draggableGO.name))
		{
			bool isActive = gameData.actionBlocLimit[draggableGO.name] != 0; // negative means no limit
			GameObjectManager.setGameObjectState(draggableGO, isActive);
			if (isActive)
			{
				if (gameData.actionBlocLimit[draggableGO.name] < 0)
					// unlimited action => hide counter
					GameObjectManager.setGameObjectState(draggableGO.transform.GetChild(1).gameObject, false);
				else
				{
					// limited action => init and show counter
					GameObject counterText = draggableGO.transform.GetChild(1).gameObject;
					counterText.GetComponent<TextMeshProUGUI>().text = "Reste " + gameData.actionBlocLimit[draggableGO.name].ToString();
					GameObjectManager.setGameObjectState(counterText, true);
				}
			}
		}
	}

	private void useAction(GameObject go){
		LibraryItemRef lir = go.GetComponent<LibraryItemRef>();
		string actionKey = lir.linkedTo.name;
		if(actionKey != null && gameData.actionBlocLimit.ContainsKey(actionKey))
		{
			if (gameData.actionBlocLimit[actionKey] > 0)
				gameData.actionBlocLimit[actionKey] -= 1;
			updateBlocLimit(lir.linkedTo);		
		}
		GameObjectManager.removeComponent<Dropped>(go);
	}
	
	private void unuseAction(GameObject go){
		AddOne[] addOnes =  go.GetComponents<AddOne>();
		if(gameData.actionBlocLimit.ContainsKey(go.name)){
			if (gameData.actionBlocLimit[go.name] >= 0)
				gameData.actionBlocLimit[go.name] += addOnes.Length;
			updateBlocLimit(go);
		}
		foreach(AddOne a in addOnes){
			GameObjectManager.removeComponent(a);	
		}
	}
}