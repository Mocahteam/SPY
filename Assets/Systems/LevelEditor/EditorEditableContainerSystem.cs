using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using TMPro;
using UnityEngine.UI;

public class EditorEditableContainerSystem : FSystem 
{
	// Les familles
	private Family f_agent = FamilyManager.getFamily(new AllOfComponents(typeof(AgentEdit), typeof(ScriptRef))); // On r�cup�re les agents pouvant �tre �dit�s
	private Family f_viewportContainerPointed = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(ViewportContainer))); // Les containers contenant les containers �ditables
	private Family f_scriptContainer = FamilyManager.getFamily(new AllOfComponents(typeof(UIRootContainer)), new AnyOfTags("ScriptConstructor")); // Les containers de scripts
	private Family f_refreshSize = FamilyManager.getFamily(new AllOfComponents(typeof(RefreshSizeOfEditableContainer)));
	private Family f_addSpecificContainer = FamilyManager.getFamily(new AllOfComponents(typeof(AddSpecificContainer)));
	private Family f_gameLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded)));
	public LevelData levelData;
	public PaintableGrid paintableGrid;

	// Les variables
	public GameObject agentSelected = null;
	private UIRootContainer containerSelected; // Le container selectionn�
	public GameObject EditableCanvas;
	public GameObject prefabViewportScriptContainer;
	public Button addContainerButton;
	public int maxWidth;

	private GameData gameData;

	// L'instance
	public static EditorEditableContainerSystem instance;

	public EditorEditableContainerSystem()
	{
		instance = this;
	}
	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();

		f_gameLoaded.addEntryCallback(delegate {
			GameObject gameDataGO = GameObject.Find("GameData");
			if (gameDataGO != null && !gameDataGO.GetComponent<GameData>().dragDropEnabled)
			{
				foreach (GameObject container in f_scriptContainer)
				{
					Transform header = container.transform.Find("Header");
					header.Find("ResetButton").GetComponent<Button>().interactable = false;
					header.Find("RemoveButton").GetComponent<Button>().interactable = false;
					GameObjectManager.removeComponent<TooltipContent>(header.Find("ProgramText").gameObject);
				}
				addContainerButton.interactable = false;
			}
		});

		MainLoop.instance.StartCoroutine(waitForLevelReady());
	}

    protected override void onProcess(int familiesUpdateCount)
    {
        if (f_refreshSize.Count > 0) // better to process like this than callback on family (here we are sure to process all components
        {
			// Update size of parent GameObject
			MainLoop.instance.StartCoroutine(setEditableSize());
			foreach(GameObject go in f_refreshSize)
				foreach (RefreshSizeOfEditableContainer trigger in go.GetComponents<RefreshSizeOfEditableContainer>())
					GameObjectManager.removeComponent(trigger);
		}
		if (f_addSpecificContainer.Count > 0)
			foreach (GameObject go in f_addSpecificContainer)
				foreach (AddSpecificContainer asc in go.GetComponents<AddSpecificContainer>())
				{
					addSpecificContainer(asc.title, asc.editState, asc.typeState, asc.script);
					GameObjectManager.removeComponent(asc);
				}
    }

	// utilis� sur le OnSelect du ContainerName dans le prefab ViewportScriptContainer
    public void selectContainer(UIRootContainer container)
	{
		containerSelected = container;
	}

	// used on + button (see in Unity editor)
	public void addContainer()
	{
		addSpecificContainer();
		MainLoop.instance.StartCoroutine(syncEditableScrollBars());
	}

	private IEnumerator waitForLevelReady()
	{
		while (!levelData.isReady)
			yield return null;
		
		// TODO: Fix levels with existing scripts not showing up in the editor until a new script is added
	}

	// Move editable view on the last editable container
	private IEnumerator syncEditableScrollBars()
	{
		// delay three times because we have to wait addSpecificContainer end (that call setEditableSize coroutine)
		yield return null;
		yield return null;
		yield return null;
		// move scroll bar on the last added container
		EditableCanvas.GetComponentInParent<ScrollRect>().verticalScrollbar.value = 1;
		EditableCanvas.GetComponentInParent<ScrollRect>().horizontalScrollbar.value = 1;
	}

	// Ajouter un container � la sc�ne
	private void addSpecificContainer(string name = "", UIRootContainer.EditMode editState = UIRootContainer.EditMode.Editable, UIRootContainer.SolutionType typeState = UIRootContainer.SolutionType.Undefined, List<GameObject> script = null)
	{
		if (!nameContainerUsed(name))
		{
			// On clone le prefab
			GameObject cloneContainer = Object.Instantiate(prefabViewportScriptContainer);
			Transform editableContainers = EditableCanvas.transform.Find("EditableContainers");
			// On l'ajoute � l'�ditableContainer
			cloneContainer.transform.SetParent(editableContainers, false);
			// We secure the scale
			cloneContainer.transform.localScale = new Vector3(1, 1, 1);
			// On regarde combien de viewport container contient l'�ditable pour mettre le nouveau viewport � la bonne position
			cloneContainer.transform.SetSiblingIndex(EditableCanvas.GetComponent<EditableCanvacComponent>().nbViewportContainer);
			// Puis on imcr�mente le nombre de viewport contenue dans l'�ditable
			EditableCanvas.GetComponent<EditableCanvacComponent>().nbViewportContainer += 1;

			// Affiche le bon nom
			if (name != "")
			{
				// On d�finie son nom � celui de l'agent
				cloneContainer.GetComponentInChildren<UIRootContainer>().scriptName = name;
				// On affiche le bon nom sur le container
				cloneContainer.GetComponentInChildren<TMP_InputField>().text = name;
			}
			else
			{
				bool nameOk = false;
				for (int i = EditableCanvas.GetComponent<EditableCanvacComponent>().nbViewportContainer; !nameOk; i++)
				{
					// Si le nom n'est pas d�j� utilis� on nomme le nouveau container de cette fa�on
					if (!nameContainerUsed("Script" + i))
					{
						cloneContainer.GetComponentInChildren<UIRootContainer>().scriptName = "Script" + i;
						// On affiche le bon nom sur le container
						cloneContainer.GetComponentInChildren<TMP_InputField>().text = "Script" + i;
						nameOk = true;
					}
				}
			}
			MainLoop.instance.StartCoroutine(tcheckLinkName());

			// Si on est en mode Lock, on bloque l'�dition et on interdit de supprimer le script
			if (editState == UIRootContainer.EditMode.Locked)
			{
				cloneContainer.GetComponentInChildren<TMP_InputField>().interactable = false;
				Transform header = cloneContainer.transform.Find("ScriptContainer").Find("Header");
				header.Find("RemoveButton").GetComponent<Button>().interactable = false;
				header.Find("ContainerName").GetComponent<TooltipContent>().text = "Ce programme sera envoy� � " + name + ".<br><i>Vous ne pouvez pas le changer</i>.";
			}
			cloneContainer.GetComponentInChildren<UIRootContainer>().editState = editState;

			cloneContainer.GetComponentInChildren<UIRootContainer>().type = typeState;

			// ajout du script par d�faut
			GameObject dropArea = cloneContainer.GetComponentInChildren<ReplacementSlot>(true).gameObject;
			if (script != null && dropArea != null)
			{
				for (int k = 0; k < script.Count; k++)
				{
					Utility.addItemOnDropArea(script[k], dropArea);
					// On compte le nombre de bloc utilis� pour l'initialisation
					gameData.totalActionBlocUsed += script[k].GetComponentsInChildren<BaseElement>(true).Length;
					gameData.totalActionBlocUsed += script[k].GetComponentsInChildren<BaseCondition>(true).Length;
				}
				GameObjectManager.addComponent<NeedRefreshPlayButton>(MainLoop.instance.gameObject);
			}

			// On ajoute le nouveau viewport container � FYFY
			GameObjectManager.bind(cloneContainer);

			// if drag&drop diabled => hide all replacement slots that are not BaseCondition
			if (!gameData.dragDropEnabled)
				foreach (ReplacementSlot slot in cloneContainer.GetComponentsInChildren<ReplacementSlot>(true))
					if (slot.slotType != ReplacementSlot.SlotType.BaseCondition)
						GameObjectManager.setGameObjectState(slot.gameObject, false);

			if (script != null && dropArea != null)
				// refresh all the hierarchy of parent containers
				GameObjectManager.addComponent<NeedRefreshHierarchy>(dropArea);

			// Update size of parent GameObject
			MainLoop.instance.StartCoroutine(setEditableSize());
		}
	}

	private IEnumerator setEditableSize()
	{
		yield return null;
		yield return null;
		RectTransform editableContainers = (RectTransform)EditableCanvas.transform.Find("EditableContainers");
		// Resolve bug when creating the first editable component, it is the child of the verticalLayout but not included inside!!!
		// We just disable and enable it and force update rect
		if (editableContainers.childCount > 0)
		{
			editableContainers.GetChild(0).gameObject.SetActive(false);
			editableContainers.GetChild(0).gameObject.SetActive(true);
		}
		editableContainers.ForceUpdateRectTransforms();
		yield return null;
		// compute new size
		((RectTransform)EditableCanvas.transform.parent).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(maxWidth, editableContainers.rect.width));
	}

	// Empty the script window
	// See ResetButton in ViewportScriptContainer prefab in editor
	public void resetScriptContainer()
	{
		// On r�cup�re le contenair point� lors du clic de la balayette
		GameObject scriptContainerPointer = f_viewportContainerPointed.First().transform.Find("ScriptContainer").gameObject;

		deleteContent(scriptContainerPointer);
	}

	// Remove the script window
	// See RemoveButton in ViewportScriptContainer prefab in editor
	public void removeContainer(GameObject container)
	{
		deleteContent(container.transform.GetChild(0).gameObject);
		MainLoop.instance.StartCoroutine(realDelete(container));
	}

	private void deleteContent (GameObject container)
    {
		// On parcourt le script container pour d�truire toutes les instructions
		for (int i = container.transform.childCount - 1; i >= 0; i--)
			if (container.transform.GetChild(i).GetComponent<BaseElement>())
				GameObjectManager.addComponent<NeedToDelete>(container.transform.GetChild(i).gameObject);
	}

	private IEnumerator realDelete(GameObject container)
	{
		yield return null;
		GameObjectManager.unbind(container);
		Object.Destroy(container);
		// Update size of parent GameObject
		MainLoop.instance.StartCoroutine(setEditableSize());
	}

	// Rename the script window
	// See ContainerName in ViewportScriptContainer prefab in editor
	public void newNameContainer(string newName)
	{
		string oldName = containerSelected.scriptName;
		if (oldName != newName)
		{
			// Si le nom n'est pas utilis� et que le mode n'est pas locked
			if (!nameContainerUsed(newName) && containerSelected.editState != UIRootContainer.EditMode.Locked)
			{
				// Si le container est en mode synch, rechercher le ou les agents associ�s
				if (containerSelected.editState == UIRootContainer.EditMode.Synch)
				{
					// On met � jour le nom de tous les agents qui auraient le m�me nom pour garder l'association avec le container editable
					foreach (GameObject agent in f_agent)
						if (agent.GetComponent<AgentEdit>().associatedScriptName == oldName)
						{
							agent.GetComponent<AgentEdit>().associatedScriptName = newName;
							agent.GetComponent<ScriptRef>().executablePanel.GetComponentInChildren<TMP_InputField>().text = newName;
						}
				}
				// On change pour son nouveau nom
				containerSelected.scriptName = newName;
				containerSelected.transform.Find("Header").Find("ContainerName").GetComponent<TMP_InputField>().text = newName;
			}
			else
			{ // Sinon on annule le changement
				containerSelected.transform.Find("Header").Find("ContainerName").GetComponent<TMP_InputField>().text = oldName;
			}
		}
		MainLoop.instance.StartCoroutine(tcheckLinkName());
	}

	// V�rifie si le nom propos� existe d�j� ou non pour un script container
	private bool nameContainerUsed(string nameTested)
	{
		Transform editableContainers = EditableCanvas.transform.Find("EditableContainers");
		foreach (Transform container in editableContainers)
			if (container.GetComponentInChildren<UIRootContainer>().scriptName == nameTested)
				return true;

		return false;
	}


	// V�rifie si les noms des containers correspond � un agent et vice-versa
	// Si non, fait apparaitre le nom en rouge
	private IEnumerator tcheckLinkName()
	{
		yield return null;

		var validNames = new HashSet<string>();
		foreach (var rob in paintableGrid.floorObjects.Values.Where(fo => fo.type == Cell.Player || fo.type == Cell.Enemy).Cast<Robot>())
		{
			validNames.Add(rob.inputLine);
		}

		// On parcourt les containers et si aucun nom ne correspond alors on met leur nom en gras rouge
		foreach (GameObject container in f_scriptContainer)
		{
			var nameSame = validNames.Any(s => s == container.GetComponent<UIRootContainer>().scriptName);

			// Si m�me nom trouv� on met l'arri�re plan blanc
			if (nameSame)
				container.transform.Find("Header").Find("ContainerName").GetComponent<TMP_InputField>().image.color = Color.white;
			else // sinon rouge 
				container.transform.Find("Header").Find("ContainerName").GetComponent<TMP_InputField>().image.color = new Color(1f, 0.4f, 0.28f, 1f);
		}
	}
}
