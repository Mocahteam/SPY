using FYFY;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
	private Family f_scriptContainer = FamilyManager.getFamily(new AllOfComponents(typeof(UIRootContainer)), new AnyOfTags("ScriptConstructor")); // Les containers de scripts editable
	private Family f_activeScriptContainer = FamilyManager.getFamily(new AllOfComponents(typeof(UIRootContainer)), new AnyOfTags("ScriptConstructor"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY)); // Les containers de scripts editable actifs
	private Family f_refreshSize = FamilyManager.getFamily(new AnyOfComponents(typeof(ResetBlocLimit), typeof(Dropped))); // A chaque fois qu'un objet est ajouté ou supprimé la largeur du script peut changer donc on met à jour la taille du conteneur
	private Family f_addSpecificContainer = FamilyManager.getFamily(new AllOfComponents(typeof(AddSpecificContainer)));
	private Family f_gameLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded)));
	private Family f_forceRemoveContainer = FamilyManager.getFamily(new AllOfComponents(typeof(ForceRemoveContainer)));
	private Family f_newEnd = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd))); 
	private Family f_checkLinkName = FamilyManager.getFamily(new AllOfComponents(typeof(CheckLinkName)));

    // Les variables
    private UIRootContainer containerSelected; // Le container selectionné
	public GameObject EditableCanvas;
	public GameObject prefabViewportScriptContainer;
	public Button addContainerButton;
	public int maxWidth;

	public CurrentSettingsValues currentSettingsValues;

	private bool isEditorContext;
	private bool newScriptContainer = false;
    private InputAction doubleClick;

    private GameData gameData;

	// L'instance
	public static EditableContainerSystem instance;

	public EditableContainerSystem()
	{
		instance = this;
	}

	protected override void onStart()
	{

		isEditorContext = SceneManager.GetActiveScene().name == "MissionEditor";

		GameObject go = GameObject.Find("GameData");
		if (go != null)
		{
			gameData = go.GetComponent<GameData>();

            doubleClick = InputSystem.actions.FindAction("DoubleClick");

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
                        // Bloquer la possibilité de changer le nom du robot si on est en fin de partie
                        header.Find("Naming/RobotName_static/ButtonEditName").GetComponent<Button>().interactable = false;
					}
					addContainerButton.interactable = false;
				});
				f_newEnd.addExitCallback(delegate
				{
					foreach (GameObject container in f_scriptContainer)
					{
						Transform header = container.transform.Find("Header");
						header.Find("ResetButton").GetComponent<Button>().interactable = true;
						// Sur une sortie de fin de partie (ReloadState par exemple), si aucun historique n'est défini c'est qu'on est en début de partie donc on peut redonner la main sur l'édition du nom du robot
						if (container.GetComponentInChildren<UIRootContainer>().editState != UIRootContainer.EditMode.Locked && gameData.actionsHistory == null)
						{
							Transform buttonEditName = header.Find("Naming/RobotName_static/ButtonEditName");
							buttonEditName.GetComponent<Button>().interactable = true;
							buttonEditName.GetComponent<TooltipContent>().text = Utility.GetLocalizedString("UpdateRobotNameHere");
							if (gameData.dragDropEnabled)
								header.Find("RemoveButton").GetComponent<Button>().interactable = true;
						}
					}

					if (gameData.actionsHistory == null && gameData.dragDropEnabled)
						addContainerButton.interactable = true;
				});
				f_checkLinkName.addEntryCallback(delegate (GameObject go){
					MainLoop.instance.StartCoroutine(tcheckLinkName());
					foreach(CheckLinkName cln in go.GetComponents<CheckLinkName>())
						GameObjectManager.removeComponent(cln);
				});
			}
		}

		f_forceRemoveContainer.addEntryCallback(delegate (GameObject go)
		{
			removeContainer(go, true);
		});

		f_activeScriptContainer.addEntryCallback(delegate (GameObject go)
		{
			newScriptContainer = true;
		});
	}

    protected override void onProcess(int familiesUpdateCount)
    {
        if ((f_refreshSize.Count > 0 && f_activeScriptContainer.Count > 0) || newScriptContainer)
			MainLoop.instance.StartCoroutine(setEditableSize(newScriptContainer));

		foreach (GameObject go in f_addSpecificContainer)
			foreach (AddSpecificContainer asc in go.GetComponents<AddSpecificContainer>())
			{
				addSpecificContainer(asc.title, asc.editState, asc.typeState, asc.script);
				GameObjectManager.removeComponent(asc);
			}
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

        GameObjectManager.addComponent<Undoable>(MainLoop.instance.gameObject);
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
			cloneContainer.transform.SetSiblingIndex(EditableCanvas.GetComponent<EditableCanvasComponent>().nbViewportContainer);
			// Puis on imcrémente le nombre de viewport contenue dans l'éditable
			EditableCanvas.GetComponent<EditableCanvasComponent>().nbViewportContainer += 1;

			// Affiche le bon nom
			if (name != "")
			{
				// On définie son nom à celui de l'agent
				cloneContainer.GetComponentInChildren<UIRootContainer>().scriptName = name;
                // On affiche le bon nom sur le container
                TMP_InputField inputName = cloneContainer.GetComponentInChildren<TMP_InputField>(true);
                inputName.text = name;
				inputName.transform.parent.Find("RobotName_static/RobotName").GetComponent<TMP_Text>().text = name;
            }
			else
			{
				bool nameOk = false;
				for (int i = EditableCanvas.GetComponent<EditableCanvasComponent>().nbViewportContainer; !nameOk; i++)
				{
					// Si le nom n'est pas déjà utilisé on nomme le nouveau container de cette façon
					if (!nameContainerUsed("Script" + i))
                    {
                        name = "Script" + i;
                        cloneContainer.GetComponentInChildren<UIRootContainer>().scriptName = name;
						// On affiche le bon nom sur le container
						TMP_InputField inputName = cloneContainer.GetComponentInChildren<TMP_InputField>(true);
						inputName.text = name;
                        inputName.transform.parent.Find("RobotName_static/RobotName").GetComponent<TMP_Text>().text = name;
                        nameOk = true;
					}
				}
			}
			if (!isEditorContext)
				MainLoop.instance.StartCoroutine(tcheckLinkName());

			// on paramètre le mode et le type différemment si on est dans l'éditeur ou dans le player
			Transform panel = cloneContainer.transform.Find("ScriptContainer/LevelEditorPanel");
			if (!isEditorContext)
			{
				// si on est dans le player on cache l'UI permettant de configurer le mode et le type
				panel.gameObject.SetActive(false);
				Transform header = cloneContainer.transform.Find("ScriptContainer/Header");
				Transform buttonEditName = header.Find("Naming/RobotName_static/ButtonEditName");
				// et si on est en mode Lock, on bloque l'édition et on interdit de supprimer le script
				if (editState == UIRootContainer.EditMode.Locked)
				{
					header.Find("RemoveButton").GetComponent<Button>().interactable = false;
                    buttonEditName.GetComponent<Button>().interactable = false;
                    LocalizeStringEvent lse = buttonEditName.GetComponent<LocalizeStringEvent>();
                    lse.StringReference.TableEntryReference = "CalledBy";
                    (lse.StringReference["robotName"] as StringVariable).Value = buttonEditName.parent.Find("RobotName").GetComponent<TMP_Text>().text;
				}
				else
					buttonEditName.GetComponent<TooltipContent>().text = Utility.GetLocalizedString("UpdateRobotNameHere");
				// si le drag&drop n'est pas activé on bloque la balayette et la suppression du script
				if (!gameData.dragDropEnabled)
				{
					header.Find("RemoveButton").GetComponent<Button>().interactable = false;
					header.Find("ResetButton").GetComponent<Button>().interactable = false;
				}
			}
            else
            {
				// si on est dans l'éditeur on affiche l'UI permettant de configurer le mode et le type
				panel.gameObject.SetActive(true);
				panel.Find("EditMode_Dropdown").GetComponentInChildren<TMP_Dropdown>(true).value = (int)editState;
				panel.Find("ProgType_Dropdown").GetComponentInChildren<TMP_Dropdown>(true).value = (int)typeState;
			}
				
			cloneContainer.GetComponentInChildren<UIRootContainer>().editState = editState;

			cloneContainer.GetComponentInChildren<UIRootContainer>().type = typeState;

			// ajout du script par défaut
			GameObject dropArea = cloneContainer.GetComponentInChildren<ReplacementSlot>(true).gameObject;
			if (script != null && dropArea != null)
			{
				for (int k = 0; k < script.Count; k++)
				{
					UtilityGame.addItemOnDropArea(script[k], dropArea);
					// On compte le nombre de bloc utilisé pour l'initialisation
					gameData.totalActionBlocUsed += script[k].GetComponentsInChildren<BaseElement>(true).Length;
					gameData.totalActionBlocUsed += script[k].GetComponentsInChildren<BaseCondition>(true).Length;
				}
				// Rafraichissement de l'UI uniquement sur la scene principale
				if (SceneManager.GetActiveScene().name == "MainScene")
					GameObjectManager.addComponent<NeedRefreshPlayButton>(MainLoop.instance.gameObject);
			}

			// On ajoute le nouveau viewport container à FYFY
			GameObjectManager.bind(cloneContainer);

			// if drag&drop disabled => hide all replacement slots that are not BaseCondition
			if (!gameData.dragDropEnabled && !isEditorContext)
				foreach (ReplacementSlot slot in cloneContainer.GetComponentsInChildren<ReplacementSlot>(true))
					if (slot.slotType != ReplacementSlot.SlotType.BaseCondition)
						slot.gameObject.SetActive(false);

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
		// compute new size including scroll bar
		((RectTransform)EditableCanvas.transform.parent).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(maxWidth, editableContainers.rect.width + (EditableCanvas.GetComponentInParent<ScrollRect>().verticalScrollbar.transform as RectTransform).rect.width));
		LayoutRebuilder.MarkLayoutForRebuild((RectTransform)EditableCanvas.transform.parent);

		if (autoScroll)
		{
			// move scroll bar on the last added container
			ScrollRect scroll = EditableCanvas.GetComponentInParent<ScrollRect>(true);
			scroll.verticalScrollbar.value = 1;
			scroll.horizontalScrollbar.value = 1;
		}

		newScriptContainer = false;
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
					{ "value", scriptContainer.GetComponent<UIRootContainer>().scriptName }
				}
			});

        GameObjectManager.addComponent<Undoable>(MainLoop.instance.gameObject);

        deleteContent(scriptContainer);
	}

	// Remove the script window
	// See RemoveButton in ViewportScriptContainer prefab in editor
	public void removeContainer(GameObject container, bool silent)
	{
		GameObject scriptContainerPointer = container.transform.GetChild(0).gameObject;
	
		if (!silent)
		{
            // générer une trace seulement sur la scene principale
            if (SceneManager.GetActiveScene().name == "MainScene")
			{
				GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
				{
					verb = "deleted",
					objectType = "script",
					activityExtensions = new Dictionary<string, string>() {
					{ "value", scriptContainerPointer.GetComponent<UIRootContainer>().scriptName }
				}
				});
			}
		}

        deleteContent(scriptContainerPointer);
		MainLoop.instance.StartCoroutine(realDelete(container, silent));
	}

	private void deleteContent (GameObject container)
    {
		// On parcourt le script container pour détruire toutes les instructions
		for (int i = container.transform.childCount - 1; i >= 0; i--)
			if (container.transform.GetChild(i).GetComponent<BaseElement>())
				GameObjectManager.addComponent<NeedToDelete>(container.transform.GetChild(i).gameObject, new { silent = true });
	}

	private IEnumerator realDelete(GameObject container, bool silent)
	{
		yield return null;
		GameObjectManager.unbind(container);
		container.transform.SetParent(null);
		Object.Destroy(container);
		if (f_activeScriptContainer.Count > 0)
			yield return setEditableSize(true);
		if (!silent)
            GameObjectManager.addComponent<Undoable>(MainLoop.instance.gameObject);
    }

    // See ButtonEditName in ViewportScriptContainer prefab in editor
    public void editRobotName(TMP_Text name)
	{
		// on vérifie que le bouton permettant d'éditer le nom du robot est bien actif (on peut arriver ici même s'il est désactivé sur un double clic ou un submit sur le TMP_Text)
		if (name.transform.parent.Find("ButtonEditName").GetComponent<Button>().IsInteractable())
		{
            // On désactive le nom statique du robot et on active le champ de saisie
            name.transform.parent.gameObject.SetActive(false);
            TMP_InputField input = name.transform.parent.parent.Find("RobotName_edit").GetComponent<TMP_InputField>();
			input.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(input.gameObject);
			containerSelected = name.GetComponentInParent<UIRootContainer>();
		}
    }

	// Vérifie un (double click) sur le nom du robot dans la zone éditable pour éditer son nom
	public void checkDoubleClick(BaseEventData element)
	{
		PointerEventData pointerData = element as PointerEventData;
        if (doubleClick.WasPerformedThisFrame())
			editRobotName(pointerData.pointerPress.GetComponent<TMP_Text>());
	}

    // Rename the script window
    // See RobotName_edit in ViewportScriptContainer prefab in editor
    public void newNameContainer(string newName)
	{
		string oldName = containerSelected.scriptName;
        TMP_InputField input = containerSelected.transform.Find("Header/Naming/RobotName_edit").GetComponent<TMP_InputField>();
		TMP_Text name = containerSelected.transform.Find("Header/Naming/RobotName_static/RobotName").GetComponent<TMP_Text>();
        if (oldName != newName)
		{
			// Si le nom n'est pas utilisé et que le mode n'est pas locked (ignorer ça si on est dans l'éditeur de mission)
			if (!nameContainerUsed(newName) && (containerSelected.editState != UIRootContainer.EditMode.Locked || isEditorContext))
			{
				// On change pour son nouveau nom
				containerSelected.scriptName = newName;
                input.text = newName;
                name.text = newName;

                // générer une trace seulement sur la scene principale
                if (!isEditorContext)
					GameObjectManager.addComponent<ActionPerformedForLRS>(containerSelected.gameObject, new
					{
						verb = "renamed",
						objectType = "script",
						activityExtensions = new Dictionary<string, string>() {
						{ "oldValue", oldName },
						{ "value", newName }
					}
					});

                GameObjectManager.addComponent<Undoable>(MainLoop.instance.gameObject);
            }
			else
			{ // Sinon on annule le changement
                input.text = oldName;
                name.text = oldName;

            }
		}
		// On vérifie l'association du nom uniquement sur la scène principale
		if (!isEditorContext)
			MainLoop.instance.StartCoroutine(tcheckLinkName());

		// on désactive le champ de saisie et on active le nom statique du robot
		input.gameObject.SetActive(false);
        name.transform.parent.gameObject.SetActive(true);
		// On laisse le UINavigation faire sa sélection par défaut avant de surcharegr la sélection ici
        MainLoop.instance.StartCoroutine(Utility.delayGOSelection(name.transform.parent.Find("ButtonEditName").gameObject));
    }

	// Vérifie si le nom proposé existe déjà ou non pour un script container
	private bool nameContainerUsed(string nameTested)
	{
		if (nameTested == "")
			return false;

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

		// On parcourt les containers et si aucun nom ne correspond alors on met leur nom en rouge
		foreach (GameObject container in f_scriptContainer)
		{
			bool nameSame = false;
			foreach (GameObject agent in f_agent)
				if (container.GetComponent<UIRootContainer>().scriptName.ToLower() == agent.GetComponent<AgentEdit>().associatedScriptName.ToLower())
					nameSame = true;

            Selectable name = container.transform.Find("Header/Naming/RobotName_static/RobotName").GetComponent<Selectable>();
			ColorBlock nameColor = name.colors;
			// Si même nom trouvé on met la couleur par défaut
			if (nameSame)
				nameColor.normalColor = currentSettingsValues.values.currentNormalColor_Text;
			else // sinon la couleur de mauvaise association 
				nameColor.normalColor = currentSettingsValues.values.currentWrongAssociationColor;
			name.colors = nameColor;
		}

		// On fait la même chose pour les agents
		foreach (GameObject agent in f_agent)
		{
			bool nameSame = false;
			foreach (GameObject container in f_scriptContainer)
				if (container.GetComponent<UIRootContainer>().scriptName.ToLower() == agent.GetComponent<AgentEdit>().associatedScriptName.ToLower())
					nameSame = true;

			Selectable agentName = agent.GetComponent<ScriptRef>().executablePanel.transform.Find("Header/agentName").GetComponent<Selectable>();
			ColorBlock nameColors = agentName.colors;
			// Si même nom trouvé on met la couleur par défaut
			if (nameSame)
				nameColors.normalColor = currentSettingsValues.values.currentNormalColor_Text;
			else // sinon la couleur de mauvaise association 
				nameColors.normalColor = currentSettingsValues.values.currentWrongAssociationColor;
		}
	}
}