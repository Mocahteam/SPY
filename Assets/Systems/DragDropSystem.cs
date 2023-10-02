using FYFY;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// Ce systéme permet la gestion du drag and drop des différents éléments de construction de la séquence d'action.
/// Il gére entre autre :
///		Le drag and drop d'un élément du panel librairie vers une séquence d'action
///		Le drag and drop d'un élément d'une séquence d'action vers une séquence d'action (la même ou autre)
///		Le drag and drop d'un élément (bibliothèque ou sequence d'action) vers l'extérieur (pour le supprimer)
///		Le clic droit sur un élément dans une sequence d'action pour le supprimer
///		Le double click sur un élément de la bibliothèque pour l'ajouter sur la dernière dropzone utilisée
/// 
/// <summary>
/// beginDragElementFromLibrary
///		Pour le début du drag and drop d'un élément venant de la bibliothèque
/// beginDragElementFromEditableScript
///		Pour le début du drag and drop d'un élément venant de la séquence d'action en construction
/// dragElement
///		Pendant le drag d'un élément
/// endDragElement
///		A la fin d'un drag and drop si l'élément n'est pas lâché dans un container pour la création d'une séquence
/// deleteElement
///		Destruction d'une block d'action
/// </summary>

public class DragDropSystem : FSystem
{
	// Les familles
	private Family f_dropZone = FamilyManager.getFamily(new AllOfComponents(typeof(DropZone))); // Les drops zones
	private Family f_dropZoneEnabled = FamilyManager.getFamily(new AllOfComponents(typeof(DropZone)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY)); // Les drops zones visibles
	private Family f_dropArea = FamilyManager.getFamily(new AnyOfComponents(typeof(DropZone), typeof(ReplacementSlot))); // Les drops zones et les replacement slots
	private Family f_operators = FamilyManager.getFamily(new AllOfComponents(typeof(BaseOperator)));
	private Family f_elementToDelete = FamilyManager.getFamily(new AllOfComponents(typeof(NeedToDelete)));
	private Family f_elementToRefresh = FamilyManager.getFamily(new AllOfComponents(typeof(NeedRefreshHierarchy)));
	private Family f_defaultDropZone = FamilyManager.getFamily(new AllOfComponents(typeof(Selected)));

	private Family f_playMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family f_editMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

	private Family f_replacementSlot = FamilyManager.getFamily(new AllOfComponents(typeof(Outline), typeof(ReplacementSlot)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family f_newEnd = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));

	private Family f_dragging = FamilyManager.getFamily(new AllOfComponents(typeof(Dragging)));

	// Les variables
	private GameData gameData;
	private GameObject itemDragged; // L'item (ici bloc d'action) en cours de drag
	private Coroutine viewLastDropZone = null;
	public GameObject mainCanvas; // Le canvas principal
	private GameObject lastDropZoneUsed; // La dernière dropzone utilisée
	public AudioSource audioSource; // Pour le son d'ajout de bloc
	//Pour la gestion du double clic
	private float lastClickTime;
	public float catchTime;
	public RectTransform editableContainers;

	private EventSystem eventSystem;
	private GameObject lastSelectedGameObject = null;

	// L'instance
	public static DragDropSystem instance;

	public DragDropSystem()
    {
		instance = this;
	}

    protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();

		eventSystem = EventSystem.current;

		f_elementToDelete.addEntryCallback(deleteElement);
		f_defaultDropZone.addEntryCallback(selectNewDefaultDropZone);
		f_elementToRefresh.addEntryCallback(delegate (GameObject go)
		{
			refreshHierarchyContainers(go);
			foreach (NeedRefreshHierarchy ntr in go.GetComponents<NeedRefreshHierarchy>())
				GameObjectManager.removeComponent(ntr);
		});
		f_playMode.addEntryCallback(delegate {
			Pause = true;
			string scriptsContent = "";
			foreach (Transform viewportScriptContainer in editableContainers)
				scriptsContent += exportEditableScriptToString(viewportScriptContainer.Find("ScriptContainer"), null);

			// générer une trace seulement sur la scene principale
			if (SceneManager.GetActiveScene().name == "MainScene")
				GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
				{
					verb = "executed",
					objectType = "program",
					activityExtensions = new Dictionary<string, string>() {
					{ "content", scriptsContent }
				}
				});
		});
		f_editMode.addEntryCallback(delegate {
			if (f_newEnd.Count == 0)
				Pause = false;
		});

		f_newEnd.addEntryCallback(delegate { levelFinished(true); });
		f_newEnd.addExitCallback(delegate { levelFinished(false); });
	}

	protected override void onProcess(int familiesUpdateCount)
	{
		if (Input.GetKeyDown(KeyCode.Escape) && f_dragging.Count > 0)
		{
			setDropZoneState(false);
			undoDrop();
			// be sure all replacement outline are disabled
			foreach (GameObject replacementSlot in f_replacementSlot)
				replacementSlot.GetComponent<Outline>().enabled = false;
		}

		if (Input.GetKeyDown(KeyCode.Return) && f_editMode.Count > 0 && gameData.dragDropEnabled)
		{
			if (itemDragged == null)
			{
				GameObject current = eventSystem.currentSelectedGameObject;
				if (current.GetComponent<ElementToDrag>())
				{
					initBlockFromLibrary(current);
					itemDragged.transform.position = new Vector3(itemDragged.transform.position.x + 20, itemDragged.transform.position.y - 20, itemDragged.transform.position.z);
					eventSystem.SetSelectedGameObject(itemDragged);
				}
				else if (current.GetComponent<LibraryItemRef>())
                {
					initBlockFromEditableScript(current);
					itemDragged.transform.position = new Vector3(itemDragged.transform.position.x + 20, itemDragged.transform.position.y - 20, itemDragged.transform.position.z);
					eventSystem.SetSelectedGameObject(itemDragged);
				}
			}
            else
            {
				endDragElement();
			}
		}

		if (Input.GetKey(KeyCode.LeftArrow) && itemDragged != null)
			moveItemDragged(-Time.deltaTime*100, 0);
		if (Input.GetKey(KeyCode.RightArrow) && itemDragged != null)
			moveItemDragged(Time.deltaTime * 100, 0);
		if (Input.GetKey(KeyCode.UpArrow) && itemDragged != null)
			moveItemDragged(0, Time.deltaTime * 100);
		if (Input.GetKey(KeyCode.DownArrow) && itemDragged != null)
			moveItemDragged(0, -Time.deltaTime * 100);

		// press up, no item dragged and last selection was an element in script
		if (Input.GetKeyDown(KeyCode.UpArrow) && itemDragged == null && lastSelectedGameObject != null && lastSelectedGameObject.GetComponentInParent<LibraryItemRef>() != null)
		{
			// do nothing if last selected object is a focused inputfield (case of for blocks)
			TMP_InputField input = lastSelectedGameObject.GetComponent<TMP_InputField>();
			if (input == null || !input.isFocused)
			{
				// get root container
				List<Selectable> selectables = new List<Selectable>(lastSelectedGameObject.GetComponentInParent<UIRootContainer>().GetComponentsInChildren<Selectable>());
				// get all child selectable (automatically sorted top down) and get id of last selected object
				int id = selectables.IndexOf(lastSelectedGameObject.GetComponent<Selectable>());
				if (id > 0)
				{
					// get previous one
					GameObject newSelected = selectables[id - 1].gameObject;
					// set as new selected
					eventSystem.SetSelectedGameObject(newSelected);
					// get drop area
					if (newSelected.GetComponentInChildren<DropZone>(true) != null)
						lastDropZoneUsed = newSelected.GetComponentInChildren<DropZone>(true).gameObject;
					else if (newSelected.GetComponentInChildren<ReplacementSlot>() != null)
						lastDropZoneUsed = newSelected.GetComponentInChildren<ReplacementSlot>().gameObject;
					// auto focus on this droparea
					MainLoop.instance.StartCoroutine(focusOnLastDropZoneUsed());
				}
			}
		}

		// press down, no item dragged and last selection was an element in script
		if (Input.GetKeyDown(KeyCode.DownArrow) && itemDragged == null && lastSelectedGameObject != null && lastSelectedGameObject.GetComponentInParent<LibraryItemRef>() != null)
		{
			// do nothing if last selected object is a focused inputfield (case of for blocks)
			TMP_InputField input = lastSelectedGameObject.GetComponent<TMP_InputField>();
			if (input == null || !input.isFocused)
			{
				// get root container
				List<Selectable> selectables = new List<Selectable>(lastSelectedGameObject.GetComponentInParent<UIRootContainer>().GetComponentsInChildren<Selectable>());
				// get all child selectable (automatically sorted top down) and get id of last selected object
				int id = selectables.IndexOf(lastSelectedGameObject.GetComponent<Selectable>());
				if (id < selectables.Count - 1)
				{
					// get previous one
					GameObject newSelected = selectables[id + 1].gameObject;
					// set as new selected
					eventSystem.SetSelectedGameObject(newSelected);
					// get drop area
					if (newSelected.GetComponentInChildren<DropZone>(true) != null)
						lastDropZoneUsed = newSelected.GetComponentInChildren<DropZone>(true).gameObject;
					else if (newSelected.GetComponentInChildren<ReplacementSlot>() != null)
						lastDropZoneUsed = newSelected.GetComponentInChildren<ReplacementSlot>().gameObject;
					// auto focus on this droparea
					MainLoop.instance.StartCoroutine(focusOnLastDropZoneUsed());
				}
			}
		}

		lastSelectedGameObject = eventSystem.currentSelectedGameObject;
	}

	private void moveItemDragged(float x, float y)
    {
		itemDragged.transform.position = new Vector3(itemDragged.transform.position.x + x, itemDragged.transform.position.y + y, itemDragged.transform.position.z);
		eventSystem.SetSelectedGameObject(itemDragged);
		// check if we overlap drop zone
		foreach(GameObject dropZone in f_dropZoneEnabled)
        {
			RectTransform rectDropZone = dropZone.transform as RectTransform;
			RectTransform rectTrigger = rectDropZone.Find("DropZoneTrigger") as RectTransform;
			Vector3 itemPosInDropZone = rectDropZone.InverseTransformPoint(itemDragged.transform.position);

			// check if raycast is enabled on this dropzone and dragged item overlap dropzone
			bool raycastEnabled = dropZone.GetComponentInChildren<RaycastOnDrag>().GetComponent<Image>().raycastTarget;
			if (raycastEnabled && Math.Abs(itemPosInDropZone.x) < rectTrigger.rect.width / 2 && Math.Abs(itemPosInDropZone.y) < rectTrigger.rect.height / 2)
				checkHighlightDropArea(dropZone);
			else
				GameObjectManager.setGameObjectState(dropZone.transform.Find("PositionBar").gameObject, false);
		}
		// check if we overlap replacement slot
		foreach (GameObject replacementSlot in f_replacementSlot)
		{
			RectTransform rectRepSlot = replacementSlot.transform as RectTransform;
			RectTransform rectTrigger = rectRepSlot.Find("EventManager") as RectTransform;
			Vector3 itemPosInDropZone = rectRepSlot.InverseTransformPoint(itemDragged.transform.position);

			// check if raycast is enabled on this dropzone and dragged item overlap dropzone
			bool raycastEnabled = replacementSlot.GetComponentInChildren<RaycastOnDrag>().GetComponent<Image>().raycastTarget;
			if (raycastEnabled && Math.Abs(itemPosInDropZone.x) < rectTrigger.rect.width / 2 && Math.Abs(itemPosInDropZone.y) < rectTrigger.rect.height / 2)
				checkHighlightDropArea(replacementSlot);
			else
				replacementSlot.GetComponent<Outline>().enabled = false;
		}
	}

    // Si fin de niveau désactive le drag&drop
    private void levelFinished(bool state)
	{
		Pause = state;
	}

	// toggle toutes les dropzones
	private void setDropZoneState(bool value)
	{
		foreach (GameObject Dp in f_dropZone)
		{
			GameObjectManager.setGameObjectState(Dp.transform.gameObject, value);
			Dp.transform.GetChild(0).gameObject.SetActive(false); // Be sure the drop zone is disabled
		}

		// enable eventManager on each operator
		foreach (GameObject op in f_operators)
        {
			// be sure eventManager is disabled on this operator
			Transform eventManager = op.transform.Find("EventManager");
			if (eventManager != null)
			{
				if (op != itemDragged)
					eventManager.gameObject.SetActive(value);
				else
					eventManager.gameObject.SetActive(false); // means object dragged is this => always disable eventManager
			}
        }
	}

	private string exportEditableScriptToString(Transform scriptContainer, GameObject focusedArea)
	{
		string scriptsContent = scriptContainer.Find("Header").GetComponentInChildren<TMP_InputField>().text + " {";
		// on ignore les fils sans Highlightable
		for (int i = 0; i < scriptContainer.childCount; i++)
		{
			if (scriptContainer.GetChild(i).GetComponent<Highlightable>())
				scriptsContent += " " + Utility.exportBlockToString(scriptContainer.GetChild(i).GetComponent<Highlightable>(), focusedArea);
		}
		if (scriptContainer.GetChild(scriptContainer.childCount - 1).gameObject == focusedArea)
			scriptsContent += " ####";
		scriptsContent += " }\n";
		return scriptsContent;
	}

	// used by prefabs (Captors, boolean operators and drop areas)
	public void checkHighlightDropArea(GameObject dropArea)
    {
		if (itemDragged != null) {
			// First case => the dropArea is a drop zone and item dragged is a base element, we enable child red bar of the drop zone
			if (dropArea.GetComponent<DropZone>() && itemDragged.GetComponent<BaseElement>())
				GameObjectManager.setGameObjectState(dropArea.transform.GetChild(0).gameObject, true);
			else
			{ // Second case => the drop area is a replacement slot, we have to manage base element and condition element
				ReplacementSlot repSlot = dropArea.GetComponent<ReplacementSlot>();
				// Check if the replacement area and the item dragged are the same type
				if (repSlot && ((repSlot.slotType == ReplacementSlot.SlotType.BaseElement && itemDragged.GetComponent<BaseElement>()) ||
								(repSlot.slotType == ReplacementSlot.SlotType.BaseCondition && itemDragged.GetComponent<BaseCondition>())))
				{
					// Be sure to not outline the drop area before looking for enabled outline in parents and childs
					repSlot.GetComponent<Outline>().enabled = false;
					// First remove outline from parents, indeed with nested conditions all the hierarchy is outlined
					foreach (Outline outline in repSlot.GetComponentsInParent<Outline>(true))
						outline.enabled = false;
					// check if a child is not already outlined
					foreach (Outline outline in repSlot.GetComponentsInChildren<Outline>(true))
						if (outline.enabled)
							return;
					// no child is already outline => then we can outline this drop area
					repSlot.GetComponent<Outline>().enabled = true;
				}
			}
		}
    }

	// used by prefabs on ReplacementSlot
	public void unhighlightDropArea(GameObject dropArea)
	{
		if (itemDragged != null && itemDragged.GetComponent<BaseCondition>() && !dropArea.transform.IsChildOf(itemDragged.transform))
		{
			Outline[] outlines = dropArea.GetComponentsInParent<Outline>(); // the first is this
			
			// if we found an outline in parent, we enable the first parent that is the second item in the list (the first is the outline of the current drop area)
			if (outlines.Length >= 2)
				outlines[1].enabled = true;
			// then disable outline of current dropArea
			dropArea.GetComponent<Outline>().enabled = false;
		}
	}

	// Lors de la selection (début d'un drag) d'un bloc de la librairie (voir inspector)
	public void beginDragElementFromLibrary(BaseEventData element)
    {
		// On verifie si c'est un évènement généré par le bouton gauche de la souris
		if (!Pause && gameData.dragDropEnabled && (element as PointerEventData).button == PointerEventData.InputButton.Left && element.selectedObject != null)
			initBlockFromLibrary(element.selectedObject);
	}

	private void initBlockFromLibrary(GameObject model)
    {
		// On active les dropzones
		setDropZoneState(true);
		// On crée le bloc action associé à l'élément
		itemDragged = Utility.createEditableBlockFromLibrary(model, mainCanvas);
		// On l'ajoute aux familles de FYFY
		GameObjectManager.bind(itemDragged);
		GameObjectManager.addComponent<Dragging>(itemDragged);
		// exclude all UI elements that can disturb the drag from the EventSystem
		foreach (RaycastOnDrag child in itemDragged.GetComponentsInChildren<RaycastOnDrag>(true))
			child.GetComponent<Image>().raycastTarget = false;
	}

	// Lors de la selection (début d'un drag) d'un bloc dans la zone d'édition
	public void beginDragElementFromEditableScript(BaseEventData element)
    {
		// On verifie si c'est un évènement généré par le bouton gauche de la souris
		if (!Pause && gameData.dragDropEnabled && (element as PointerEventData).button == PointerEventData.InputButton.Left && element.selectedObject != null)
			initBlockFromEditableScript(element.selectedObject);
	}

	private void initBlockFromEditableScript(GameObject model)
    {
		itemDragged = model;
		GameObjectManager.addComponent<Dragging>(itemDragged);
		Transform parent = itemDragged.transform.parent;

		string content = Utility.exportBlockToString(itemDragged.GetComponent<Highlightable>());

		GameObject removedFromArea = null;
		Transform nextBrother = itemDragged.transform.parent.GetChild(itemDragged.transform.GetSiblingIndex() + 1);
		if (nextBrother.GetComponentInChildren<DropZone>(true))
			removedFromArea = nextBrother.GetComponentInChildren<DropZone>(true).gameObject;
		else // means next brother is a replacement slot
			removedFromArea = nextBrother.GetComponentInChildren<ReplacementSlot>(true).gameObject;

		// On active les dropzones 
		setDropZoneState(true);

		// Update empty zone if required
		Utility.manageEmptyZone(itemDragged);

		// On l'associe (temporairement) au Canvas Main
		itemDragged.transform.SetParent(mainCanvas.transform, true); // We need to perform it immediatelly to write change in statement
		itemDragged.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		GameObjectManager.refresh(itemDragged);

		// compute context after removing item dragged
		string context = exportEditableScriptToString(nextBrother.GetComponentInParent<UIRootContainer>().transform, removedFromArea);

		// exclude all UI elements that can disturb the drag from the EventSystem
		foreach (RaycastOnDrag child in itemDragged.GetComponentsInChildren<RaycastOnDrag>(true))
			child.GetComponent<Image>().raycastTarget = false;
		// Restore action and subactions to inventory
		foreach (BaseElement actChild in itemDragged.GetComponentsInChildren<BaseElement>(true))
			GameObjectManager.addComponent<AddOne>(actChild.GetComponent<LibraryItemRef>().linkedTo);
		// Restore conditions to inventory
		foreach (BaseCondition condChild in itemDragged.GetComponentsInChildren<BaseCondition>(true))
			GameObjectManager.addComponent<AddOne>(condChild.GetComponent<LibraryItemRef>().linkedTo);

		// Rend le bouton d'execution actif (ou non)
		GameObjectManager.addComponent<NeedRefreshPlayButton>(MainLoop.instance.gameObject);

		refreshHierarchyContainers(parent.gameObject);

		// générer une trace seulement sur la scene principale
		if (SceneManager.GetActiveScene().name == "MainScene")
			GameObjectManager.addComponent<ActionPerformedForLRS>(itemDragged, new
			{
				verb = "deleted",
				objectType = "block",
				activityExtensions = new Dictionary<string, string>() {
						{ "content", content },
						{ "context", context }
					}
			});
	}



	// Pendant le drag d'un bloc, permet de lui faire suivre le mouvement de la souris
	public void dragElement()
	{
		if(!Pause && gameData.dragDropEnabled && itemDragged != null) {
			itemDragged.transform.position = Input.mousePosition;
		}
	}

	private GameObject getFocusedDropArea()
    {
		foreach (GameObject go in f_dropZoneEnabled)
			if (go.transform.GetChild(0).gameObject.activeSelf)
				return go;
		foreach (GameObject go in f_replacementSlot)
			if (go.GetComponent<Outline>().enabled)
				return go;
		return null;
    }

	// Determine si l'element associé à l'événement EndDrag se trouve dans une zone de container ou non
	// Détruire l'objet si lâché hors d'un container
	public void endDragElement()
	{
		if (!Pause && gameData.dragDropEnabled && itemDragged != null)
		{
			// On commence par regarder s'il n'y a pas de container pointé, dans ce cas on supprime l'objet drag
			GameObject dropArea = getFocusedDropArea();
			// On désactive les dropzones
			setDropZoneState(false);
			if (dropArea == null)
			{
				undoDrop();
				return;
			}
			else // sinon on ajoute l'élément au container pointé
			{
				if (dropArea.transform.IsChildOf(itemDragged.transform))
				{
					undoDrop();
					return;
				}

				if (addDraggedItemOnDropZone(dropArea))
				{
					// We restore all UI elements inside the EventSystem
					foreach (RaycastOnDrag child in itemDragged.GetComponentsInChildren<RaycastOnDrag>(true))
						child.GetComponent<Image>().raycastTarget = true;
					MainLoop.instance.StartCoroutine(Utility.pulseItem(itemDragged));
					eventSystem.SetSelectedGameObject(itemDragged);
				}
				else
					eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
				
				if (itemDragged.GetComponent<Dragging>())
					GameObjectManager.removeComponent<Dragging>(itemDragged);
			}
			// Rafraichissement de l'UI
			GameObjectManager.addComponent<NeedRefreshPlayButton>(MainLoop.instance.gameObject);

			itemDragged = null;
		}
	}

	// Add the dragged item on the drop area
	private bool addDraggedItemOnDropZone (GameObject dropArea)
    {
		string content = Utility.exportBlockToString(itemDragged.GetComponent<Highlightable>());
		string context = exportEditableScriptToString(dropArea.GetComponentInParent<UIRootContainer>().transform, dropArea);

		if (!Utility.addItemOnDropArea(itemDragged, dropArea))
		{
			undoDrop();
			return false;
		}

		GameObjectManager.addComponent<NeedRefreshPlayButton>(MainLoop.instance.gameObject);

		// update limit bloc
		foreach (BaseElement actChild in itemDragged.GetComponentsInChildren<BaseElement>(true))
			GameObjectManager.addComponent<Dropped>(actChild.gameObject);
		foreach (BaseCondition condChild in itemDragged.GetComponentsInChildren<BaseCondition>(true))
			GameObjectManager.addComponent<Dropped>(condChild.gameObject);

		// refresh all the hierarchy of parent containers
		refreshHierarchyContainers(itemDragged);
		// Update size of parent GameObject
		GameObjectManager.addComponent<RefreshSizeOfEditableContainer>(MainLoop.instance.gameObject);

		// Lance le son de dépôt du block d'action
		audioSource.Play();

		// générer une trace seulement sur la scene principale
		if (SceneManager.GetActiveScene().name == "MainScene")
			GameObjectManager.addComponent<ActionPerformedForLRS>(itemDragged, new
			{
				verb = "inserted",
				objectType = "block",
				activityExtensions = new Dictionary<string, string>() {
					{ "content", content },
					{ "context", context }
				}
			});

		return true;
	}

	// suppression de l'objet en cours de drag
	private void undoDrop()
	{
		// Suppression des familles de FYFY
		GameObjectManager.unbind(itemDragged);
		// Déstruction du bloc
		GameObject.Destroy(itemDragged, 0.1f); // L'objet est réellement supprimé à la fin de l'update par Unity donc si l'appel est fait dans la phase de gestion des inputs c'est ok (itemDragged sera toujours vivant lors de la mise à jour de Fyfy avant la phase d'update et les éventuelles action empilées dans le GameObjectManager pourront être dépilées). Mais si l'appel est fait dans l'update alors à la fin de cette phase l'objet est réellement détruit donc à la prochaine mise à jour de Fyfy (au LateUpdate) les éventuelles actions empilées ne pourront pas être exécutées. Donc ont ajoute un délai à la suppression du gameobject.
		itemDragged = null;
		eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
	}

	// Déclenche la suppression de l'élément
	public void deleteElement(GameObject elementToDelete)
	{
		// On vérifie qu'il y a bien un objet pointé pour la suppression
		if (!Pause && gameData.dragDropEnabled && elementToDelete != null)
		{
			string content = Utility.exportBlockToString(elementToDelete.GetComponent<Highlightable>());

			GameObject removedFromArea = null;
			Transform nextBrother = elementToDelete.transform.parent.GetChild(elementToDelete.transform.GetSiblingIndex() + 1);
			if (nextBrother.GetComponentInChildren<DropZone>(true))
				removedFromArea = nextBrother.GetComponentInChildren<DropZone>(true).gameObject;
			else // means next brother is a replacement slot
				removedFromArea = nextBrother.GetComponentInChildren<ReplacementSlot>(true).gameObject;

			// Réactivation d'une EmptyZone si nécessaire
			Utility.manageEmptyZone(elementToDelete);

			// refresh all the hierarchy of parent containers
			refreshHierarchyContainers(elementToDelete);

			// On l'associe (temporairement) au Canvas Main
			elementToDelete.transform.SetParent(mainCanvas.transform, false); // We need to perform it immediatelly to write change in statement
			elementToDelete.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
			GameObjectManager.refresh(elementToDelete);
			// compute context after removing item dragged
			string context = exportEditableScriptToString(nextBrother.gameObject.GetComponentInParent<UIRootContainer>(true).transform, removedFromArea);

			//On associe à l'élément le component ResetBlocLimit pour déclancher le script de destruction de l'élément
			GameObjectManager.addComponent<ResetBlocLimit>(elementToDelete);
			GameObjectManager.addComponent<NeedRefreshPlayButton>(MainLoop.instance.gameObject);

			NeedToDelete ntd = elementToDelete.GetComponent<NeedToDelete>();
			// générer une trace seulement sur la scene principale
			if ((ntd == null || !ntd.silent) && SceneManager.GetActiveScene().name == "MainScene")
			{
				GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
				{
					verb = "deleted",
					objectType = "block",
					activityExtensions = new Dictionary<string, string>() {
						{ "content", content },
						{ "context", context }
					}
				});
			}

			eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
		}
	}

	private void selectNewDefaultDropZone(GameObject newDropZone)
    {
		// define this new drop zone as the default
		lastDropZoneUsed = newDropZone.transform.gameObject;
		// remove old trigger component
		foreach (Selected selectedDZ in newDropZone.GetComponents<Selected>())
			GameObjectManager.removeComponent(selectedDZ);
	}

	// Refresh the hierarchy (parent by parent) from elementToRefresh. Used in Control prefabs (If, For, ...)
	public void refreshHierarchyContainers(GameObject elementToRefresh)
    {
		// refresh all the hierarchy of parent containers
		Transform parent = elementToRefresh.transform.parent;
		while (parent is RectTransform)
		{
			MainLoop.instance.StartCoroutine(forceUIRefresh((RectTransform)parent));
			parent = parent.parent;
		}
	}

	// Besoin d'attendre l'update pour effectuer le recalcul de la taille des containers
	private IEnumerator forceUIRefresh(RectTransform bloc)
	{
		yield return null;
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(bloc);
	}

	// Si double clic sur l'élément de la bibliothèque (voir l'inspector), ajoute le bloc d'action au dernier container utilisé
	public void checkDoubleClick(BaseEventData element)
    {
		if (!Pause && gameData.dragDropEnabled && doubleClick() && !itemDragged && f_dropArea.Count > 0)
		{
			// if no drop zone used, try to get the last
			if (lastDropZoneUsed == null)
				lastDropZoneUsed = f_dropArea.getAt(f_dropArea.Count-1);
			// be sure the lastDropZone is defined
			if (lastDropZoneUsed != null)
			{
				// On crée le bloc action
				itemDragged = Utility.createEditableBlockFromLibrary(element.selectedObject, mainCanvas);
				// On l'ajoute aux familles de FYFY
				GameObjectManager.bind(itemDragged);
				// On l'envoie sur la dernière dropzone utilisée
				addDraggedItemOnDropZone(lastDropZoneUsed);
				// refresh all the hierarchy of parent containers
				refreshHierarchyContainers(lastDropZoneUsed);

				if (viewLastDropZone != null)
					MainLoop.instance.StopCoroutine(viewLastDropZone);
				MainLoop.instance.StartCoroutine(focusOnLastDropZoneUsed());
				MainLoop.instance.StartCoroutine(Utility.pulseItem(itemDragged));

				// Rafraichissement de l'UI
				GameObjectManager.addComponent<NeedRefreshPlayButton>(MainLoop.instance.gameObject);
				itemDragged = null;
			}
		}
	}

	private IEnumerator focusOnLastDropZoneUsed()
    {
		yield return new WaitForSeconds(.25f);

		// get last drop zone reference (depends if last drop zone is a replacement slot, a drop zone of an action or a drop zone of a control block
		RectTransform lastDropRef = null;
		if (lastDropZoneUsed.GetComponent<DropZone>())
		{
			if (lastDropZoneUsed.transform.parent.GetComponent<BaseElement>()) // BasicAction
				lastDropRef = lastDropZoneUsed.transform.parent as RectTransform; // target is the parent
			else if (lastDropZoneUsed.transform.parent.parent.GetComponent<ControlElement>() && lastDropZoneUsed.transform.parent.GetSiblingIndex() == 1) // the dropArea of the first child of a Control block
				lastDropRef = lastDropZoneUsed.transform.parent.parent as RectTransform; // target is the grandparent
		}
		else
			lastDropRef = lastDropZoneUsed.transform as RectTransform; // This is a replacement slot

		RectTransform editableCanvas = editableContainers.parent as RectTransform;

		float lastDropZoneUsedY = Mathf.Abs(editableContainers.InverseTransformPoint(lastDropRef.transform.position).y);
		float lastDropZoneUsedX = Mathf.Abs(editableContainers.InverseTransformPoint(lastDropRef.transform.position).x);

		Vector2 targetAnchoredPosition = new Vector2(editableContainers.anchoredPosition.x, editableContainers.anchoredPosition.y);
		// we auto focus on last drop zone used only if it is not visible
		if (lastDropZoneUsedY - editableContainers.anchoredPosition.y < 0 || lastDropZoneUsedY - editableContainers.anchoredPosition.y > editableCanvas.rect.height)
		{
			// check if last drop zone used is too high
			if (lastDropZoneUsedY - editableContainers.anchoredPosition.y < 0)
			{
				targetAnchoredPosition = new Vector2(
					targetAnchoredPosition.x,
					lastDropZoneUsedY - (lastDropRef.rect.height * 2f) // move view a little bit higher than last drop zone position
				);
			}
			// check if last drop zone used is too low
			else if (lastDropZoneUsedY - editableContainers.anchoredPosition.y > editableCanvas.rect.height)
			{
				targetAnchoredPosition = new Vector2(
					targetAnchoredPosition.x,
					-editableCanvas.rect.height + lastDropZoneUsedY + lastDropRef.rect.height / 2
				);
			}

			// same for x pos
			if (lastDropZoneUsedX + editableContainers.anchoredPosition.x < 0)
			{
				targetAnchoredPosition = new Vector2(
					-lastDropZoneUsedX + lastDropRef.rect.width,
					targetAnchoredPosition.y
				);
			}
			else if (lastDropZoneUsedX + editableContainers.anchoredPosition.x > editableCanvas.rect.width)
			{
				targetAnchoredPosition = new Vector2(
					editableCanvas.rect.width - lastDropZoneUsedX - lastDropRef.rect.width,
					targetAnchoredPosition.y
				);
			}

			float distance = Vector2.Distance(editableContainers.anchoredPosition, targetAnchoredPosition);
			while (Vector2.Distance(editableContainers.anchoredPosition, targetAnchoredPosition) > 0.1f)
			{
				editableContainers.anchoredPosition = Vector2.MoveTowards(editableContainers.anchoredPosition, targetAnchoredPosition, distance / 10);
				yield return null;
			}
		}
	}

	// Vérifie si le double click a eu lieu
	private bool doubleClick()
	{
		// check double click
		// On met à jour le timer du dernier click
		// et on retourne la réponse
		if (Time.time - lastClickTime < catchTime)
        {
			lastClickTime = Time.time-catchTime;
			return true;
		}
        else
        {
			lastClickTime = Time.time;
			return false;
		}
	}

	// see inputFiels in ForBloc prefab in inspector
	public void onlyPositiveInteger(GameObject forBlock, string newValue)
	{
		int oldValue = forBlock.GetComponent<ForControl>().nbFor;
		Transform input = forBlock.transform.Find("Header");
		int res;
		bool success = Int32.TryParse(newValue, out res);
		if (!success || (success && Int32.Parse(newValue) <= 0))
		{
			input.GetComponentInChildren<TMP_InputField>().text = "0";
			res = 0;
		}
		forBlock.GetComponent<ForControl>().nbFor = res;
		// compute context
		string context = exportEditableScriptToString(forBlock.GetComponentInParent<UIRootContainer>().transform, forBlock);

		// générer une trace seulement sur la scene principale
		if (res != oldValue && SceneManager.GetActiveScene().name == "MainScene")
		{
			GameObjectManager.addComponent<ActionPerformedForLRS>(forBlock, new
			{
				verb = "modified",
				objectType = "block",
				activityExtensions = new Dictionary<string, string>() {
				{ "context", context },
				{ "oldValue", oldValue.ToString()},
				{ "value", res.ToString()}
			}
			});
		}
	}
}