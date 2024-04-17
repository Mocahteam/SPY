using UnityEngine;
using FYFY;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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
	private Family f_addSpecificContainer = FamilyManager.getFamily(new AllOfComponents(typeof(AddSpecificContainer)));

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
					Utility.manageEmptyZone(child.gameObject);
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
				// several agent could be linked to the same script, in this case we add the minimal number of wait
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
				// We add wait blocs if only one is required or if this level do not provide unlimited for loop blocs
				if (minNbOfInaction == 1 || gameData.actionBlockLimit["ForLoop"] != -1)
				{
					for (int i = 0; i < minNbOfInaction; i++)
					{
						GameObject newWait = Utility.createEditableBlockFromLibrary(libraryWait, canvas);
						newWait.transform.SetParent(gameData.actionsHistory.transform.GetChild(containerCpt).GetChild(0), false);
						newWait.transform.SetAsLastSibling();
						gameData.totalActionBlocUsed++;
					}
				}
				else if (minNbOfInaction > 1)
				{
					// Create for control
					ForControl forCont = Utility.createEditableBlockFromLibrary(libraryFor, canvas).GetComponent<ForControl>();
					forCont.currentFor = 0;
					forCont.nbFor = minNbOfInaction;
					forCont.transform.GetComponentInChildren<TMP_InputField>(true).text = forCont.nbFor.ToString();
					forCont.transform.SetParent(gameData.actionsHistory.transform.GetChild(containerCpt).GetChild(0), false);
					// Create Wait action
					Transform forContainer = forCont.transform.Find("Container");
					GameObject newWait = Utility.createEditableBlockFromLibrary(libraryWait, canvas);
					newWait.transform.SetParent(forContainer, false);
					newWait.transform.SetAsFirstSibling();
					// Set drop/empty zone
					forContainer.GetChild(forContainer.childCount - 2).gameObject.SetActive(true); // enable drop zone
					forContainer.GetChild(forContainer.childCount - 1).gameObject.SetActive(false); // disable empty zone
					gameData.totalActionBlocUsed = gameData.totalActionBlocUsed+2;
				}
			}
		}

		// Disable add container button
		buttonAddEditableContainer.GetComponent<Button>().interactable = false;
		
		buttonAddEditableContainer.GetComponent<TooltipContent>().text = gameData.localization[52];

		//Disable remove container buttons and naming input field
		foreach (GameObject trash in f_removeButton)
		{
			trash.GetComponent<Button>().interactable = false;
			TMP_InputField name_input = trash.transform.parent.Find("ContainerName").GetComponent<TMP_InputField>();
			name_input.interactable = false;
			name_input.GetComponent<TooltipContent>().text = Utility.getFormatedText(gameData.localization[53], name_input.text);
		}
	}


	// Restore saved scripts in history inside editable script containers
	private IEnumerator delayLoadHistory()
	{
		if (gameData != null && gameData.actionsHistory != null)
		{
			// Wait that AddSpecificContainer was created
			yield return null;
			yield return null;
			yield return null;

			// Wait that default editable canvas are created
			while (f_addSpecificContainer.Count > 0)
				yield return null;


			// Remove all default canvas and restore all blocs
			Transform editableContainers = EditableCanvas.transform.GetChild(0);
			foreach (Transform viewportForEditableContainer in editableContainers)
				GameObjectManager.addComponent<ForceRemoveContainer>(viewportForEditableContainer.gameObject);

			while (editableContainers.childCount > 0)
				yield return null;

			gameData.totalActionBlocUsed = 0;
			// Restore history
			for (int i = 0; i < gameData.actionsHistory.transform.childCount; i++)
			{
				Transform history_EditableContainer = gameData.actionsHistory.transform.GetChild(i).GetChild(0);
				UIRootContainer uiRC = history_EditableContainer.GetComponent<UIRootContainer>();
				List<GameObject> script = new List<GameObject>();
				foreach (Transform history_child in history_EditableContainer)
					if (history_child.GetComponent<BaseElement>())
						script.Add(history_child.gameObject);
				GameObjectManager.addComponent<AddSpecificContainer>(MainLoop.instance.gameObject, new { title = uiRC.scriptName, editState = uiRC.editState, typeState = uiRC.type, script = script });
			}

			// Wait that history AddSpecificContainer are created
			yield return null;
			yield return null;
			// Wait that history canvas are created
			while (f_addSpecificContainer.Count > 0)
				yield return null;
			// Count used elements
			foreach (Transform viewportForEditableContainer in EditableCanvas.transform.GetChild(0))
			{
				foreach (BaseElement act in viewportForEditableContainer.GetComponentsInChildren<BaseElement>(true))
				{
					GameObjectManager.addComponent<Dropped>(act.gameObject);
					gameData.totalActionBlocUsed--; // cancel this drop count, already count with AddSpecificContainer
				}
				foreach (BaseCondition act in viewportForEditableContainer.GetComponentsInChildren<BaseCondition>(true))
				{
					GameObjectManager.addComponent<Dropped>(act.gameObject);
					gameData.totalActionBlocUsed--; // cancel this drop count, already count with AddSpecificContainer
				}
			}
			//destroy history
			GameObject.Destroy(gameData.actionsHistory);
			LayoutRebuilder.ForceRebuildLayoutImmediate(EditableCanvas.GetComponent<RectTransform>());
			//enable Play button
			buttonExecute.GetComponent<Button>().interactable = true;
			// disable editable container if won
			if (f_newEnd.Count > 0 && f_newEnd.First().GetComponent<NewEnd>().endType == NewEnd.Win)
			{
				// Inactive of each editable panel
				foreach (GameObject brush in f_removeButton)
				{
					// Disable trash button
					brush.GetComponent<Button>().interactable = false;
					// Disable reset button
					brush.transform.parent.GetChild(brush.transform.GetSiblingIndex() - 1).GetComponent<Button>().interactable = false;
					// Disable naming TMP
					brush.transform.parent.GetComponentInChildren<TMPro.TMP_InputField>().interactable = false;
				}
			}
		}
	}
}