using FYFY;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manage history to accumulate player attempt when resolving the level in several steps.
/// Manage undo/redo of the editable containers.
/// History is displayed at the end of the level
/// </summary>
public class HistoryManager : FSystem
{
	private Family f_askToSaveHistory = FamilyManager.getFamily(new AllOfComponents(typeof(AskToSaveHistory)));
	private Family f_newEnd = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
	private Family f_agent = FamilyManager.getFamily(new AllOfComponents(typeof(AgentEdit), typeof(ScriptRef))); // On récupére les agents pouvant être édité
	private Family f_removeButton = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("RemoveButton"));
	private Family f_addSpecificContainer = FamilyManager.getFamily(new AllOfComponents(typeof(AddSpecificContainer)));
    private Family f_gameLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded)));
    private Family f_playMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
    private Family f_undoable = FamilyManager.getFamily(new AllOfComponents(typeof(Undoable)));

    private GameData gameData;
    private Transform UndoRedoStack;
    private int stackPos;

    public RectTransform EditableContainers;
	public GameObject libraryFor;
	public GameObject libraryWait;
	public GameObject canvas;
	public GameObject buttonAddEditableContainer;
	public GameObject buttonExecute;
    public Button buttonUndo;
    public Button buttonRedo;

    // L'instance
    public static HistoryManager instance;

    public HistoryManager()
    {
        instance = this;
    }

    protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();

        if (EditableContainers != null)
        {
            f_askToSaveHistory.addEntryCallback(delegate (GameObject go)
            {
                saveHistory();
                GameObjectManager.removeComponent<AskToSaveHistory>(go);
            });

            f_newEnd.addEntryCallback(levelFinished);

            MainLoop.instance.StartCoroutine(delayLoadHistory());
            UndoRedoStack = EditableContainers.parent.Find("UndoRedoStack");
            // suppression des événements Undoable qui pourraient rester dans la pile d'annulation
            removeLastsUndoable(UndoRedoStack.childCount);
            // enregistrer l'état initial des zones d'édition
            f_gameLoaded.addEntryCallback(delegate {
                GameObject copy = GameObject.Instantiate(EditableContainers.gameObject, UndoRedoStack, false);
                stackPos = 0;
            });
            // vider la pile d'annulation si on quitte le mode Play
            f_playMode.addEntryCallback(delegate { removeLastsUndoable(UndoRedoStack.childCount); });
        }

        buttonUndo.interactable = false;
        buttonRedo.interactable = false;
    }

    protected override void onProcess(int familiesUpdateCount)
    {
        if (f_undoable.Count > 0)
        {
            // nettoyage des Undoable
            foreach (GameObject go in f_undoable)
                GameObjectManager.removeComponent<Undoable>(go);
            // Si on est remonté dans la pile d'annulation, on supprime les éléments qui sont au dessus de la position courante
            if (stackPos < UndoRedoStack.childCount - 1)
                removeLastsUndoable(UndoRedoStack.childCount - 1 - stackPos);
            // Stack de l'état actuel des zones d'édition
            GameObject copy = GameObject.Instantiate(EditableContainers.gameObject, UndoRedoStack);

            // Il faut un peu supprimer la copy de composants qui trainent car supprimés via le GameObjectManager, il seront réellement supprimés à la fin de la frame, donc il sont présents dans la copy, il faut donc nettoyer tout ça
            // suppression des Undoable
            foreach (Undoable u in copy.GetComponentsInChildren<Undoable>(true))
                Object.Destroy(u);
            // suppression des Dropped
            foreach (Dropped d in copy.GetComponentsInChildren<Dropped>(true))
                Object.Destroy(d);
            // suppression des ActionPerformedForLRS
            foreach (ActionPerformedForLRS a in copy.GetComponentsInChildren<ActionPerformedForLRS>(true))
                Object.Destroy(a);

            // We don't bind the history to FYFY
            stackPos++;
            buttonUndo.interactable = true;
            buttonRedo.interactable = false;
        }
    }

    private void removeLastsUndoable(int count)
    {
        for (int i = 0; i < count && UndoRedoStack.childCount > 0; i++)
        {
            GameObject child = UndoRedoStack.GetChild(UndoRedoStack.childCount - 1).gameObject;
            child.transform.SetParent(null);
            GameObject.Destroy(child);
        }
        if (stackPos > UndoRedoStack.childCount - 1)
            stackPos = UndoRedoStack.childCount - 1;
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
	// See MainScene => EndPanel => ReloadLevel button
	public void saveHistory()
	{
        if (gameData.actionsHistory == null)
        {
            // set history as a copy of editable canvas
            gameData.actionsHistory = GameObject.Instantiate(EditableContainers).gameObject;
            gameData.actionsHistory.SetActive(false); // keep this gameObject as a ghost
            // We don't bind the history to FYFY
        }
        else
        {
            // parse all containers inside editable canvas
            for (int containerCpt = 0; containerCpt < EditableContainers.childCount; containerCpt++)
            {
                Transform viewportForEditableContainer = EditableContainers.GetChild(containerCpt);
                // the first child is the script container that contains script elements
                foreach (Transform child in viewportForEditableContainer.GetChild(0))
                {
                    if (child.GetComponent<BaseElement>())
                    {
                        // copy this child inside the appropriate script
                        GameObject.Instantiate(child, gameData.actionsHistory.transform.GetChild(containerCpt).GetChild(0));
                        // We don't bind this copy to FYFY
                    }
                }
            }
        }

        // Suppression du contenu de chaque zone d'édition pour que la prochaine phase de programme commence sur des zones d'édition vides
        // Ici on ne cherche pas à restaurer les blocs d'actions dans la bibliothèque car ils ont bien été consomés lors de la phase d'exécution du programme
        foreach (Transform viewportForEditableContainer in EditableContainers)
        {
            for (int i = viewportForEditableContainer.GetChild(0).childCount - 1; i >= 0; i--)
            {
                Transform child = viewportForEditableContainer.GetChild(0).GetChild(i);
                if (child.GetComponent<BaseElement>())
                {
                    UtilityGame.manageEmptyZone(child.gameObject);
                    GameObjectManager.unbind(child.gameObject);
                    child.SetParent(null); // because destroying is not immediate
                    GameObject.Destroy(child.gameObject);
                }
            }
        }
        EditableContainers.anchoredPosition = new Vector2(0, 0);

        // Add Wait action for each inaction
        for (int containerCpt = 0; containerCpt < EditableContainers.childCount; containerCpt++)
		{
			// look for associated agent
			string associatedAgent = EditableContainers.GetChild(containerCpt).GetComponentInChildren<UIRootContainer>().scriptName;
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
				if (minNbOfInaction == 1 || !gameData.actionBlockLimit.ContainsKey("ForLoop") || gameData.actionBlockLimit["ForLoop"] != -1)
				{
					for (int i = 0; i < minNbOfInaction; i++)
					{
						GameObject newWait = UtilityGame.createEditableBlockFromLibrary(libraryWait, canvas);
						newWait.transform.SetParent(gameData.actionsHistory.transform.GetChild(containerCpt).GetChild(0), false);
						newWait.transform.SetAsLastSibling();
						gameData.totalActionBlocUsed++;
					}
				}
				else if (minNbOfInaction > 1)
				{
					// Create for control
					ForControl forCont = UtilityGame.createEditableBlockFromLibrary(libraryFor, canvas).GetComponent<ForControl>();
					forCont.currentFor = 0;
					forCont.nbFor = minNbOfInaction;
					forCont.transform.GetComponentInChildren<TMP_InputField>(true).text = forCont.nbFor.ToString();
					forCont.transform.SetParent(gameData.actionsHistory.transform.GetChild(containerCpt).GetChild(0), false);
					// Create Wait action
					Transform forContainer = forCont.transform.Find("Container");
					GameObject newWait = UtilityGame.createEditableBlockFromLibrary(libraryWait, canvas);
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

		buttonAddEditableContainer.GetComponent<TooltipContent>().text = buttonAddEditableContainer.GetComponentInParent<Localization>(true).localization[1];

		//Disable remove container buttons and naming input field
		foreach (GameObject trash in f_removeButton)
		{
			trash.GetComponent<Button>().interactable = false;
			TMP_InputField name_input = trash.transform.parent.Find("ContainerName").GetComponent<TMP_InputField>();
			name_input.interactable = false;
			name_input.GetComponent<TooltipContent>().text = Utility.getFormatedText(name_input.GetComponentInParent<Localization>(true).localization[3], name_input.text);
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
            yield return removeEditableContainers();

            gameData.totalActionBlocUsed = 0;

            // Restore history
            yield return restoreEditableContainers(gameData.actionsHistory.transform, false);

            //destroy history
            GameObject.Destroy(gameData.actionsHistory);
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

    private IEnumerator removeEditableContainers()
	{
        foreach (Transform viewportForEditableContainer in EditableContainers)
            GameObjectManager.addComponent<ForceRemoveContainer>(viewportForEditableContainer.gameObject);

        while (EditableContainers.childCount > 0)
            yield return null;
    }

    // Restore saved scripts in history inside editable script containers
    // copy: if true, copy the saved scripts, else move them
    private IEnumerator restoreEditableContainers(Transform src, bool copy)
	{
        // Restore src contents into editable containers
        for (int i = 0; i < src.childCount; i++)
        {
            Transform saved_EditableContainer = src.GetChild(i).GetChild(0);
            UIRootContainer uiRC = saved_EditableContainer.GetComponent<UIRootContainer>();
            List<GameObject> script = new List<GameObject>();
            foreach (Transform saved_child in saved_EditableContainer)
                if (saved_child.GetComponent<BaseElement>())
                    script.Add(copy ? GameObject.Instantiate(saved_child.gameObject) : saved_child.gameObject);
            GameObjectManager.addComponent<AddSpecificContainer>(MainLoop.instance.gameObject, new { title = uiRC.scriptName, editState = uiRC.editState, typeState = uiRC.type, script = script });
        }

        // Wait that AddSpecificContainer are created
        yield return null;
        yield return null;
        // Wait that canvas are created
        while (f_addSpecificContainer.Count > 0)
            yield return null;
        // Count used elements
        foreach (Transform viewportForEditableContainer in EditableContainers)
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
    }

    public void undo()
    {
        if (stackPos > 0)
            MainLoop.instance.StartCoroutine(processUndo());
    }

    private IEnumerator processUndo()
    {
        stackPos--;
        if (stackPos == 0)
            buttonUndo.interactable = false;
        buttonRedo.interactable = true;

        yield return removeEditableContainers();
        yield return restoreEditableContainers(UndoRedoStack.GetChild(stackPos), true);

        string scriptsContent = "";
        foreach (Transform viewportScriptContainer in EditableContainers)
            scriptsContent += UtilityGame.exportEditableScriptToString(viewportScriptContainer.Find("ScriptContainer"), null);
        GameObjectManager.addComponent<ActionPerformedForLRS>(EditableContainers.gameObject, new
        {
            verb = "undone",
            objectType = "script",
            activityExtensions = new Dictionary<string, string>() {
                { "content", scriptsContent }
            }
        });
    }

    public void redo()
    {
        if (stackPos < UndoRedoStack.childCount - 1)
            MainLoop.instance.StartCoroutine(processRedo());
    }

    private IEnumerator processRedo()
    {
        stackPos++;
        if (stackPos == UndoRedoStack.childCount - 1)
            buttonRedo.interactable = false;
        buttonUndo.interactable = true;

        yield return removeEditableContainers();
        yield return restoreEditableContainers(UndoRedoStack.GetChild(stackPos), true);

        string scriptsContent = "";
        foreach (Transform viewportScriptContainer in EditableContainers)
            scriptsContent += UtilityGame.exportEditableScriptToString(viewportScriptContainer.Find("ScriptContainer"), null);
        GameObjectManager.addComponent<ActionPerformedForLRS>(EditableContainers.gameObject, new
        {
            verb = "redone",
            objectType = "script",
            activityExtensions = new Dictionary<string, string>() {
                { "content", scriptsContent }
            }
        });
    }
}