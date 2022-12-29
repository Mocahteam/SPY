using UnityEngine;
using FYFY;
using System.Collections;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manage history to accumulate player attempt when resolving the level in several steps.
/// History is displayed at the end of the level
/// </summary>
public class HistoryManager : FSystem
{
	private Family f_askToSaveHistory = FamilyManager.getFamily(new AllOfComponents(typeof(AskToSaveHistory)));
	private Family f_newEnd = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
	private Family f_agent = FamilyManager.getFamily(new AllOfComponents(typeof(AgentEdit), typeof(ScriptRef))); // On récupére les agents pouvant être édité
	private Family f_removeButton = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("RemoveButton"));

	private GameData gameData;

	public GameObject EditableCanvas;
	public GameObject libraryFor;
	public GameObject libraryWait;
	public GameObject canvas;
	public GameObject buttonAddEditableContainer;
	public GameObject buttonExecute;

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();

		f_askToSaveHistory.addEntryCallback(delegate (GameObject go)
			{
				saveHistory();
				GameObjectManager.removeComponent<AskToSaveHistory>(go);
			});

		f_newEnd.addEntryCallback(levelFinished);

		MainLoop.instance.StartCoroutine(delayLoadHistory());
	}

	// check if player win the game and if true, load history
	private void levelFinished(GameObject go)
	{
		// En cas de fin de niveau
		if (go.GetComponent<NewEnd>().endType == NewEnd.Win)
		{
			// Affichage de l'historique de l'ensemble des actions exécutées
			saveHistory();
			MainLoop.instance.StartCoroutine(delayLoadHistory());
		}
		// for other end type, nothing to do more
	}


	private IEnumerator delayLoadHistory()
	{
		// delay three frame, time the editable container will be created
		yield return null;
		yield return null;
		yield return null;
		loadHistory();
	}


	// Add the executed scripts to the containers history
	public void saveHistory()
	{
		if (gameData.actionsHistory == null)
		{
			// set history as a copy of editable canvas
			gameData.actionsHistory = GameObject.Instantiate(EditableCanvas.transform.GetChild(0).transform).gameObject;
			gameData.actionsHistory.SetActive(false); // keep this gameObject as a ghost
			// We don't bind the history to FYFY
		}
		else
		{
			// parse all containers inside editable canvas
			for (int containerCpt = 0; containerCpt < EditableCanvas.transform.GetChild(0).childCount; containerCpt++)
			{
				Transform viewportForEditableContainer = EditableCanvas.transform.GetChild(0).GetChild(containerCpt);
				// the first child is the script container that contains script elements
				foreach (Transform child in viewportForEditableContainer.GetChild(0))
				{
					if (child.GetComponent<BaseElement>())
					{
						// copy this child inside the appropriate history
						GameObject.Instantiate(child, gameData.actionsHistory.transform.GetChild(containerCpt).GetChild(0));
						// We don't bind the history to FYFY
					}
				}
			}
		}
		// Erase all editable containers
		foreach (Transform viewportForEditableContainer in EditableCanvas.transform.GetChild(0))
		{
			for (int i = viewportForEditableContainer.GetChild(0).childCount - 1; i >= 0; i--)
			{
				Transform child = viewportForEditableContainer.GetChild(0).GetChild(i);
				if (child.GetComponent<BaseElement>())
				{
					EditingUtility.manageEmptyZone(child.gameObject);
					GameObjectManager.unbind(child.gameObject);
					child.SetParent(null); // because destroying is not immediate
					GameObject.Destroy(child.gameObject);
				}
			}
		}
		(EditableCanvas.transform.GetChild(0).transform as RectTransform).anchoredPosition = new Vector2(0, 0);

		// Add Wait action for each inaction
		for (int containerCpt = 0; containerCpt < EditableCanvas.transform.GetChild(0).childCount; containerCpt++)
		{
			// look for associated agent
			string associatedAgent = EditableCanvas.transform.GetChild(0).GetChild(containerCpt).GetComponentInChildren<UIRootContainer>().scriptName;
			GameObject agentSelected = null;
			int minNbOfInaction = int.MaxValue;
			foreach (GameObject agent in f_agent)
				if (associatedAgent.ToLower() == agent.GetComponent<AgentEdit>().associatedScriptName.ToLower())
				{
					ScriptRef sr = agent.GetComponent<ScriptRef>();
					if (sr.nbOfInactions < minNbOfInaction)
					{
						agentSelected = agent;
						minNbOfInaction = sr.nbOfInactions;
					}
					sr.nbOfInactions = 0;
				}
			if (agentSelected != null)
            {
				if (minNbOfInaction == 1)
				{
					GameObject newWait = EditingUtility.createEditableBlockFromLibrary(libraryWait, canvas);
					newWait.transform.SetParent(gameData.actionsHistory.transform.GetChild(containerCpt).GetChild(0), false);
					newWait.transform.SetAsLastSibling();
				}
				else if (minNbOfInaction > 1)
				{
					// Create for control
					ForControl forCont = EditingUtility.createEditableBlockFromLibrary(libraryFor, canvas).GetComponent<ForControl>();
					forCont.currentFor = 0;
					forCont.nbFor = minNbOfInaction;
					forCont.transform.GetComponentInChildren<TMP_InputField>(true).text = forCont.nbFor.ToString();
					forCont.transform.SetParent(gameData.actionsHistory.transform.GetChild(containerCpt).GetChild(0), false);
					// Create Wait action
					Transform forContainer = forCont.transform.Find("Container");
					GameObject newWait = EditingUtility.createEditableBlockFromLibrary(libraryWait, canvas);
					newWait.transform.SetParent(forContainer, false);
					newWait.transform.SetAsFirstSibling();
					// Set drop/empty zone
					forContainer.GetChild(forContainer.childCount - 2).gameObject.SetActive(true); // enable drop zone
					forContainer.GetChild(forContainer.childCount - 1).gameObject.SetActive(false); // disable empty zone
				}
			}
		}

		// Disable add container button
		buttonAddEditableContainer.GetComponent<Button>().interactable = false;
		buttonAddEditableContainer.GetComponent<TooltipContent>().text = "Ajout impossible après avoir\ncommencé à résoudre le niveau";

		//Disable remove container buttons
		foreach (GameObject trash in f_removeButton)
			trash.GetComponent<Button>().interactable = false;
	}

	// Restore saved scripts in history inside editable script containers
	private void loadHistory()
	{
		if (gameData != null && gameData.actionsHistory != null)
		{
			// For security, erase all editable containers
			foreach (Transform viewportForEditableContainer in EditableCanvas.transform.GetChild(0))
				for (int i = viewportForEditableContainer.GetChild(0).childCount - 1; i >= 0; i--)
				{
					Transform child = viewportForEditableContainer.GetChild(0).GetChild(i);
					if (child.GetComponent<BaseElement>())
					{
						gameData.totalActionBlocUsed -= child.GetComponentsInChildren<BaseElement>(true).Length;
						gameData.totalActionBlocUsed -= child.GetComponentsInChildren<BaseCondition>(true).Length;
						GameObjectManager.unbind(child.gameObject);
						child.transform.SetParent(null); // because destroying is not immediate, we remove this child from its parent, then Unity can take the time he wants to destroy GameObject
						GameObject.Destroy(child.gameObject);
					}
				}
			// Restore history
			for (int i = 0; i < gameData.actionsHistory.transform.childCount; i++)
			{
				Transform history_viewportForEditableContainer = gameData.actionsHistory.transform.GetChild(i);
				// the first child is the script container that contains script elements
				foreach (Transform history_child in history_viewportForEditableContainer.GetChild(0))
				{
					if (history_child.GetComponent<BaseElement>())
					{
						// copy this child inside the appropriate editable container
						Transform history_childCopy = GameObject.Instantiate(history_child, EditableCanvas.transform.GetChild(0).GetChild(i).GetChild(0));
						// Place this child copy at the end of the container
						history_childCopy.SetAsFirstSibling();
						history_childCopy.SetSiblingIndex(history_childCopy.parent.childCount - 2);
						GameObjectManager.bind(history_childCopy.gameObject);
					}
				}
			}
			// Count used elements
			foreach (Transform viewportForEditableContainer in EditableCanvas.transform.GetChild(0))
			{
				foreach (BaseElement act in viewportForEditableContainer.GetComponentsInChildren<BaseElement>(true))
					GameObjectManager.addComponent<Dropped>(act.gameObject);
				foreach (BaseCondition act in viewportForEditableContainer.GetComponentsInChildren<BaseCondition>(true))
					GameObjectManager.addComponent<Dropped>(act.gameObject);
			}
			//destroy history
			GameObject.Destroy(gameData.actionsHistory);
			LayoutRebuilder.ForceRebuildLayoutImmediate(EditableCanvas.GetComponent<RectTransform>());
			//enable Play button
			buttonExecute.GetComponent<Button>().interactable = true;
		}
	}
}