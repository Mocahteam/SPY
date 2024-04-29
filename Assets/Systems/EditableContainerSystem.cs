using UnityEngine;
using FYFY;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// Ce systéme gére tous les éléments d'édition des agents par l'utilisateur.
/// Il gére entre autre:
///		Le changement de nom du robot
///		Le changement automatique (si activé) du nom du container associé (si container associé)
///		Le changement automatique (si activé) du nom du robot lorsque l'on change le nom dans le container associé (si container associé)
/// 
/// <summary>
/// 
/// agentSelect
///		Pour enregistrer sur quel agent le systéme va travailler
///	modificationAgent
///		Pour les appels extérieurs, permet de trouver l'agent (et le considérer comme selectionné) en fonction de son nom
///		Renvoie True si trouvé, sinon false
/// setAgentName
///		Pour changer le nom d'un agent
///	majDisplayCardAgent
///		Met à jour l'affichage des info de l'agent dans sa fiche
///		
/// </summary>

public class EditableContainerSystem : FSystem 
{
	// Les familles
	private Family f_agent = FamilyManager.getFamily(new AllOfComponents(typeof(AgentEdit), typeof(ScriptRef))); // On récupére les agents pouvant être édités
	private Family f_scriptContainer = FamilyManager.getFamily(new AllOfComponents(typeof(UIRootContainer)), new AnyOfTags("ScriptConstructor")); // Les containers de scripts
	private Family f_refreshSize = FamilyManager.getFamily(new AllOfComponents(typeof(RefreshSizeOfEditableContainer)));
	private Family f_addSpecificContainer = FamilyManager.getFamily(new AllOfComponents(typeof(AddSpecificContainer)));
	private Family f_gameLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded)));
	private Family f_forceRemoveContainer = FamilyManager.getFamily(new AllOfComponents(typeof(ForceRemoveContainer)));
	private Family f_newEnd = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));

	// Les variables
	private UIRootContainer containerSelected; // Le container selectionné
	public GameObject EditableCanvas;
	public GameObject prefabViewportScriptContainer;
	public Button addContainerButton;
	public int maxWidth;

	private bool isEditorContext;

	private GameData gameData;

	// L'instance
	public static EditableContainerSystem instance;

	public EditableContainerSystem()
	{
		instance = this;
	}
	protected override void onStart()
	{

		isEditorContext = SceneManager.GetActiveScene().name == "EditorScene";

		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();

		if (!isEditorContext)
		{
			MainLoop.instance.StartCoroutine(tcheckLinkName());
			f_gameLoaded.addEntryCallback(delegate
			{
				if (!gameData.dragDropEnabled)
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
			f_newEnd.addEntryCallback(delegate
			{
				foreach (GameObject container in f_scriptContainer)
				{
					Transform header = container.transform.Find("Header");
					header.Find("ResetButton").GetComponent<Button>().interactable = false;
					header.Find("RemoveButton").GetComponent<Button>().interactable = false;
					header.Find("ContainerName").GetComponent<TMP_InputField>().interactable = false;
				}
				addContainerButton.interactable = false;
			});
			f_newEnd.addExitCallback(delegate
			{
				foreach (GameObject container in f_scriptContainer)
				{
					Transform header = container.transform.Find("Header");
					header.Find("ResetButton").GetComponent<Button>().interactable = true;

					if (container.GetComponentInChildren<UIRootContainer>().editState != UIRootContainer.EditMode.Locked && gameData.actionsHistory == null)
					{
						Transform containerName = header.Find("ContainerName");
						containerName.GetComponent<TMP_InputField>().interactable = true;
						containerName.GetComponent<TooltipContent>().text = "Indiquez ici le nom du robot<br>à qui envoyer le programme.";
						if (gameData.dragDropEnabled)
							header.Find("RemoveButton").GetComponent<Button>().interactable = true;
					}
				}

				if (gameData.actionsHistory == null && gameData.dragDropEnabled)
					addContainerButton.interactable = true;
			});
		}

		f_forceRemoveContainer.addEntryCallback(delegate (GameObject go)
		{
			removeContainer(go, true);
		});
	}

    protected override void onProcess(int familiesUpdateCount)
    {
        if (f_refreshSize.Count > 0) // better to process like this than callback on family (here we are sure to process all components
        {
			// Update size of parent GameObject
			MainLoop.instance.StartCoroutine(setEditableSize(false));
			foreach(GameObject go in f_refreshSize)
				foreach (RefreshSizeOfEditableContainer trigger in go.GetComponents<RefreshSizeOfEditableContainer>())
					GameObjectManager.removeComponent(trigger);
		}
		foreach (GameObject go in f_addSpecificContainer)
			foreach (AddSpecificContainer asc in go.GetComponents<AddSpecificContainer>())
			{
				addSpecificContainer(asc.title, asc.editState, asc.typeState, asc.script);
				GameObjectManager.removeComponent(asc);
			}
    }

	// utilisé sur le OnSelect du ContainerName dans le prefab ViewportScriptContainer
    public void selectContainer(UIRootContainer container)
	{
		containerSelected = container;
	}

	// used on + button (see in Unity editor)
	public void addContainer()
	{
		string newName = addSpecificContainer();
		// générer une trace seulement sur la scene principale
		if (SceneManager.GetActiveScene().name == "MainScene")
			GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
			{
				verb = "created",
				objectType = "script",
				activityExtensions = new Dictionary<string, string>() {
				{ "value", newName }
			}
			});
	}

	// Ajouter un container à la scéne retourne son nom définitif
	private string addSpecificContainer(string name = "", UIRootContainer.EditMode editState = UIRootContainer.EditMode.Editable, UIRootContainer.SolutionType typeState = UIRootContainer.SolutionType.Undefined, List<GameObject> script = null)
	{
		if (!nameContainerUsed(name))
		{
			// On clone le prefab
			GameObject cloneContainer = Object.Instantiate(prefabViewportScriptContainer);
			Transform editableContainers = EditableCanvas.transform.Find("EditableContainers");
			// On l'ajoute à l'éditableContainer
			cloneContainer.transform.SetParent(editableContainers, false);
			// We secure the scale
			cloneContainer.transform.localScale = new Vector3(1, 1, 1);
			// On regarde combien de viewport container contient l'éditable pour mettre le nouveau viewport à la bonne position
			cloneContainer.transform.SetSiblingIndex(EditableCanvas.GetComponent<EditableCanvacComponent>().nbViewportContainer);
			// Puis on imcrémente le nombre de viewport contenue dans l'éditable
			EditableCanvas.GetComponent<EditableCanvacComponent>().nbViewportContainer += 1;

			// Affiche le bon nom
			if (name != "")
			{
				// On définie son nom à celui de l'agent
				cloneContainer.GetComponentInChildren<UIRootContainer>().scriptName = name;
				// On affiche le bon nom sur le container
				cloneContainer.GetComponentInChildren<TMP_InputField>().text = name;
			}
			else
			{
				bool nameOk = false;
				for (int i = EditableCanvas.GetComponent<EditableCanvacComponent>().nbViewportContainer; !nameOk; i++)
				{
					// Si le nom n'est pas déjà utilisé on nomme le nouveau container de cette façon
					if (!nameContainerUsed("Script" + i))
					{
						cloneContainer.GetComponentInChildren<UIRootContainer>().scriptName = "Script" + i;
						// On affiche le bon nom sur le container
						cloneContainer.GetComponentInChildren<TMP_InputField>().text = "Script" + i;
						name = "Script" + i;
						nameOk = true;
					}
				}
			}
			MainLoop.instance.StartCoroutine(tcheckLinkName());

			// on paramètre le mode et le type différemment si on est dans l'éditeur ou dans le player
			Transform panel = cloneContainer.transform.Find("ScriptContainer").Find("LevelEditorPanel");
			if (!isEditorContext)
			{
				// si on est dans le player on cache l'UI permettant de configurer le mode et le type
				panel.gameObject.SetActive(false);
				// et si on est en mode Lock, on bloque l'édition et on interdit de supprimer le script
				if (editState == UIRootContainer.EditMode.Locked)
				{
					Transform header = cloneContainer.transform.Find("ScriptContainer").Find("Header");
					header.Find("RemoveButton").GetComponent<Button>().interactable = false;
					Transform containerName = header.Find("ContainerName");
					containerName.GetComponent<TooltipContent>().text = "Ce programme sera envoyé à " + name + ".<br><i>Vous ne pouvez pas le changer</i>.";
					containerName.GetComponent<TMP_InputField>().interactable = false;
				}
				// si le drag&drop n'est pas activé on bloque la balayette et la suppression du script
				if (!gameData.dragDropEnabled)
				{
					Transform header = cloneContainer.transform.Find("ScriptContainer").Find("Header");
					header.Find("RemoveButton").GetComponent<Button>().interactable = false;
					header.Find("ResetButton").GetComponent<Button>().interactable = false;
				}
			}
            else
            {
				// si on est dans l'éditeur on affiche l'UI permettant de configurer le mode et le type
				panel.gameObject.SetActive(true);
				panel.Find("EditMode_Dropdown").GetComponent<TMP_Dropdown>().value = (int)editState;
				panel.Find("ProgType_Dropdown").GetComponent<TMP_Dropdown>().value = (int)typeState;
			}
				
			cloneContainer.GetComponentInChildren<UIRootContainer>().editState = editState;

			cloneContainer.GetComponentInChildren<UIRootContainer>().type = typeState;

			// ajout du script par défaut
			GameObject dropArea = cloneContainer.GetComponentInChildren<ReplacementSlot>(true).gameObject;
			if (script != null && dropArea != null)
			{
				for (int k = 0; k < script.Count; k++)
				{
					Utility.addItemOnDropArea(script[k], dropArea);
					// On compte le nombre de bloc utilisé pour l'initialisation
					gameData.totalActionBlocUsed += script[k].GetComponentsInChildren<BaseElement>(true).Length;
					gameData.totalActionBlocUsed += script[k].GetComponentsInChildren<BaseCondition>(true).Length;
				}
				GameObjectManager.addComponent<NeedRefreshPlayButton>(MainLoop.instance.gameObject);
			}

			// On ajoute le nouveau viewport container à FYFY
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
			MainLoop.instance.StartCoroutine(setEditableSize(true));
			return name;
		}
		else
			return "";
	}

	private IEnumerator setEditableSize(bool autoScroll)
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

		if (autoScroll)
		{
			yield return null;
			// move scroll bar on the last added container
			ScrollRect scroll = EditableCanvas.GetComponentInParent<ScrollRect>(true);
			scroll.verticalScrollbar.value = 1;
			scroll.horizontalScrollbar.value = 1;
		}
	}

	// Empty the script window
	// See ResetButton in ViewportScriptContainer prefab in editor
	public void resetScriptContainer(GameObject scriptContainer)
	{
		// générer une trace seulement sur la scene principale
		if (SceneManager.GetActiveScene().name == "MainScene")
			GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
			{
				verb = "cleaned",
				objectType = "script",
				activityExtensions = new Dictionary<string, string>() {
				{ "value", scriptContainer.transform.Find("Header").Find("ContainerName").GetComponent<TMP_InputField>().text }
			}
			});

		deleteContent(scriptContainer);
	}

	// Remove the script window
	// See RemoveButton in ViewportScriptContainer prefab in editor
	public void removeContainer(GameObject container, bool silent)
	{
		GameObject scriptContainerPointer = container.transform.GetChild(0).gameObject;

		// générer une trace seulement sur la scene principale
		if (!silent && SceneManager.GetActiveScene().name == "MainScene")
		{
			GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
			{
				verb = "deleted",
				objectType = "script",
				activityExtensions = new Dictionary<string, string>() {
					{ "value", scriptContainerPointer.transform.Find("Header").Find("ContainerName").GetComponent<TMP_InputField>().text }
				}
			});
		}

		deleteContent(scriptContainerPointer);
		MainLoop.instance.StartCoroutine(realDelete(container));

		EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
	}

	private void deleteContent (GameObject container)
    {
		// On parcourt le script container pour détruire toutes les instructions
		for (int i = container.transform.childCount - 1; i >= 0; i--)
			if (container.transform.GetChild(i).GetComponent<BaseElement>())
				GameObjectManager.addComponent<NeedToDelete>(container.transform.GetChild(i).gameObject, new { silent = true });
	}

	private IEnumerator realDelete(GameObject container)
	{
		yield return null;
		GameObjectManager.unbind(container);
		Object.Destroy(container);
		// Update size of parent GameObject
		MainLoop.instance.StartCoroutine(setEditableSize(true));
	}

	// Rename the script window
	// See ContainerName in ViewportScriptContainer prefab in editor
	public void newNameContainer(string newName)
	{
		string oldName = containerSelected.scriptName;
		if (oldName != newName)
		{
			// Si le nom n'est pas utilisé et que le mode n'est pas locked
			if (!nameContainerUsed(newName) && containerSelected.editState != UIRootContainer.EditMode.Locked)
			{
				// Si le container est en mode synch, rechercher le ou les agents associés
				if (containerSelected.editState == UIRootContainer.EditMode.Synch)
				{
					// On met à jour le nom de tous les agents qui auraient le même nom pour garder l'association avec le container editable
					foreach (GameObject agent in f_agent)
						if (agent.GetComponent<AgentEdit>().associatedScriptName.ToLower() == oldName.ToLower())
						{
							agent.GetComponent<AgentEdit>().associatedScriptName = newName;
							agent.GetComponent<ScriptRef>().executablePanel.GetComponentInChildren<TMP_InputField>().text = newName;
						}
				}
				// On change pour son nouveau nom
				containerSelected.scriptName = newName;
				containerSelected.transform.Find("Header").Find("ContainerName").GetComponent<TMP_InputField>().text = newName;

				// générer une trace seulement sur la scene principale
				if (SceneManager.GetActiveScene().name == "MainScene")
					GameObjectManager.addComponent<ActionPerformedForLRS>(containerSelected.gameObject, new
					{
						verb = "renamed",
						objectType = "script",
						activityExtensions = new Dictionary<string, string>() {
						{ "oldValue", oldName },
						{ "value", newName }
					}
					});
			}
			else
			{ // Sinon on annule le changement
				containerSelected.transform.Find("Header").Find("ContainerName").GetComponent<TMP_InputField>().text = oldName;
			}
		}
		MainLoop.instance.StartCoroutine(tcheckLinkName());
	}

	// Vérifie si le nom proposé existe déjà ou non pour un script container
	private bool nameContainerUsed(string nameTested)
	{
		Transform editableContainers = EditableCanvas.transform.Find("EditableContainers");
		foreach (Transform container in editableContainers)
			if (container.GetComponentInChildren<UIRootContainer>().scriptName.ToLower() == nameTested.ToLower())
				return true;

		return false;
	}


	// Vérifie si les noms des containers correspond à un agent et vice-versa
	// Si non, fait apparaitre le nom en rouge
	private IEnumerator tcheckLinkName()
	{
		yield return null;

		// On parcourt les containers et si aucun nom ne correspond alors on met leur nom en gras rouge
		foreach (GameObject container in f_scriptContainer)
		{
			bool nameSame = false;
			foreach (GameObject agent in f_agent)
				if (container.GetComponent<UIRootContainer>().scriptName.ToLower() == agent.GetComponent<AgentEdit>().associatedScriptName.ToLower())
					nameSame = true;

			// Si même nom trouvé on met l'arriére plan blanc
			if (nameSame)
				container.transform.Find("Header").Find("ContainerName").GetComponent<TMP_InputField>().image.color = Color.white;
			else // sinon rouge 
				container.transform.Find("Header").Find("ContainerName").GetComponent<TMP_InputField>().image.color = new Color(1f, 0.4f, 0.28f, 1f);
		}

		// On fait la même chose pour les agents
		foreach (GameObject agent in f_agent)
		{
			bool nameSame = false;
			foreach (GameObject container in f_scriptContainer)
				if (container.GetComponent<UIRootContainer>().scriptName.ToLower() == agent.GetComponent<AgentEdit>().associatedScriptName.ToLower())
					nameSame = true;

			// Si même nom trouvé on met l'arriére transparent
			if (nameSame)
				agent.GetComponent<ScriptRef>().executablePanel.GetComponentInChildren<TMP_InputField>(true).image.color = new Color(1f, 1f, 1f, 1f);
			else // sinon rouge 
				agent.GetComponent<ScriptRef>().executablePanel.GetComponentInChildren<TMP_InputField>(true).image.color = new Color(1f, 0.4f, 0.28f, 1f);
		}
	}
}