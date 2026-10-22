using UnityEngine;
using FYFY;
using TMPro;
using FYFY_plugins.PointerManager;
using System.Collections;

/// <summary>
/// This system manages blocs limitation in inventory
/// </summary>
public class BlocLimitationManager : FSystem
{
	private readonly Family f_actions = FamilyManager.getFamily(new AllOfComponents(typeof(PointerSensitive), typeof(LibraryItemRef)));
	private readonly Family f_droppedActions = FamilyManager.getFamily(new AllOfComponents(typeof(Dropped), typeof(LibraryItemRef)));
	private readonly Family f_deletedActions = FamilyManager.getFamily(new AllOfComponents(typeof(AddOne), typeof(ElementToDrag)));
	private readonly Family f_draggableElement = FamilyManager.getFamily(new AllOfComponents(typeof(ElementToDrag)));
	private readonly Family f_resetBlocLimit = FamilyManager.getFamily(new AllOfComponents(typeof(ResetBlocLimit)));
	private readonly Family f_gameLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded), typeof(MainLoop)));

	private GameData gameData;

	protected override void onStart()
	{
		GameObject gd = GameObject.Find("GameData");
		if (gd != null)
			gameData = gd.GetComponent<GameData>();

		f_resetBlocLimit.addEntryCallback(delegate (GameObject go) {
			DestroyScript(go);
			GameObjectManager.unbind(go);
			UnityEngine.Object.Destroy(go);
		});

		f_actions.addEntryCallback(LinkTo);

		f_gameLoaded.addEntryCallback(delegate {
			// init limitation counters for each draggable elements
			foreach (GameObject go in f_draggableElement)
			{
				// default => hide go
				go.SetActive(false);
				// update counter and enable required blocks
				UpdateBlocLimit(go);
			}
		});

		f_droppedActions.addEntryCallback(UseAction);
		f_deletedActions.addEntryCallback(UnuseAction);

		Pause = true;
	}

	//Recursive script destroyer
	private void DestroyScript(GameObject go)
	{
		if (go.GetComponent<LibraryItemRef>())
			GameObjectManager.addComponent<AddOne>(go.GetComponent<LibraryItemRef>().linkedTo);

		foreach (Transform child in go.transform)
			DestroyScript(child.gameObject);
	}

	// Find item in library to hook to this GameObject
	private void LinkTo(GameObject go)
	{
		if (go.GetComponent<LibraryItemRef>().linkedTo == null)
		{
			if (go.GetComponent<BasicAction>())
				go.GetComponent<LibraryItemRef>().linkedTo = GetLibraryItemByName(go.GetComponent<BasicAction>().actionType.ToString());
			else if (go.GetComponent<BaseCaptor>())
				go.GetComponent<LibraryItemRef>().linkedTo = GetLibraryItemByName(go.GetComponent<BaseCaptor>().captorType.ToString());
			else if (go.GetComponent<BaseOperator>())
				go.GetComponent<LibraryItemRef>().linkedTo = GetLibraryItemByName(go.GetComponent<BaseOperator>().operatorType.ToString());
			else if (go.GetComponent<WhileControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = GetLibraryItemByName("While");
			else if (go.GetComponent<ForeverControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = GetLibraryItemByName("Forever");
			else if (go.GetComponent<ForControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = GetLibraryItemByName("ForLoop");
			else if (go.GetComponent<IfElseControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = GetLibraryItemByName("IfElse");
			else if (go.GetComponent<IfControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = GetLibraryItemByName("IfThen");
		}
	}

	private GameObject GetLibraryItemByName(string name)
	{
		foreach (GameObject item in f_draggableElement)
			if (item.name == name)
				return item;
		return null;
	}

	// Met à jour la limite du nombre de fois où l'on peut utiliser un bloc (si il y a une limite)
	// Le désactive si la limite est atteinte
	// Met à jour le compteur
	private void UpdateBlocLimit(GameObject draggableGO)
	{
		if (gameData.actionBlockLimit.ContainsKey(draggableGO.name))
		{
			bool isActive = gameData.actionBlockLimit[draggableGO.name] != 0; // negative means no limit
			draggableGO.SetActive(isActive);
			if (isActive)
			{
				if (gameData.actionBlockLimit[draggableGO.name] < 0)
					// unlimited action => hide counter
					draggableGO.transform.GetChild(1).gameObject.SetActive(false);
				else
				{
					// limited action => init and show counter
					draggableGO.GetComponentInChildren<TextMeshProUGUI>(true).text = gameData.actionBlockLimit[draggableGO.name].ToString();
					draggableGO.transform.GetChild(1).gameObject.SetActive(true);
				}
			}
        }
        Transform grid = draggableGO.transform.parent;
        int childCount = 0;
        foreach (Transform child in grid)
            childCount += child.gameObject.activeSelf ? 1 : 0;
        grid.parent.parent.gameObject.SetActive(childCount > 0);
	}

	// Remove one item from library
	private void UseAction(GameObject go){
		LibraryItemRef lir = go.GetComponent<LibraryItemRef>();
		string actionKey = lir.linkedTo.name;

        if (actionKey != null && gameData.actionBlockLimit.ContainsKey(actionKey))
		{
			if (gameData.actionBlockLimit[actionKey] > 0)
				gameData.actionBlockLimit[actionKey] -= 1;
			UpdateBlocLimit(lir.linkedTo);		
		}
		GameObjectManager.removeComponent<Dropped>(go);
		gameData.totalActionBlocUsed++;
	}
	
	// Restore item in library
	private void UnuseAction(GameObject go){
		AddOne[] addOnes =  go.GetComponents<AddOne>();
		if(gameData.actionBlockLimit.ContainsKey(go.name)){
			if (gameData.actionBlockLimit[go.name] >= 0)
				gameData.actionBlockLimit[go.name] += addOnes.Length;
			UpdateBlocLimit(go);
			gameData.totalActionBlocUsed -= addOnes.Length;
		}
		foreach(AddOne a in addOnes){
			GameObjectManager.removeComponent(a);	
		}
	}
}