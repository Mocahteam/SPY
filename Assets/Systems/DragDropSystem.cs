using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// Ce syst�me permet la gestion du drag and drop des diff�rents �l�ments de construction de la s�quence d'action.
/// Il g�re entre autre :
///		Le drag and drop d'un �l�ment du panel librairie vers une s�quence d'action
///		Le drag and drop d'un �l�ment d'une s�quence d'action vers une s�quence d'action (la m�me ou autre)
///		Le drag and drop d'un �l�ment (biblioth�que ou sequence d'action) vers l'ext�rieur (pour le supprimer)
///		Le clic droit sur un �l�ment dans une sequence d'action pour le supprimer
///		Le double click sur un �l�ment de la biblioth�que pour l'ajouter sur la derni�re dropzone utilis�e
/// 
/// <summary>
/// beginDragElementFromLibrary
///		Pour le d�but du drag and drop d'un �l�ment venant de la biblioth�que
/// beginDragElementFromEditableScript
///		Pour le d�but du drag and drop d'un �l�ment venant de la s�quence d'action en construction
/// dragElement
///		Pendant le drag d'un �l�ment
/// endDragElement
///		A la fin d'un drag and drop si l'�l�ment n'est pas l�ch� dans un container pour la cr�ation d'une s�quence
/// deleteElement
///		Destruction d'une block d'action
/// </summary>

public class DragDropSystem : FSystem
{
	// Les familles
    private Family f_viewportContainerPointed = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(ViewportContainer))); // Les container contenant les containers �ditables
	private Family f_dropZone = FamilyManager.getFamily(new AllOfComponents(typeof(DropZone))); // Les drops zones
	private Family f_dropArea = FamilyManager.getFamily(new AnyOfComponents(typeof(DropZone), typeof(ReplacementSlot))); // Les drops zones et les replacement slots
	private Family f_operators = FamilyManager.getFamily(new AllOfComponents(typeof(BaseOperator)));
	private Family f_focusedDropArea = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver)), new AnyOfComponents(typeof(ReplacementSlot), typeof(DropZone)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY)); // the drop area under mouse cursor
	private Family f_elementToDelete = FamilyManager.getFamily(new AllOfComponents(typeof(NeedToDelete)));
	private Family f_elementToRefresh = FamilyManager.getFamily(new AllOfComponents(typeof(NeedRefreshHierarchy)));
	private Family f_defaultDropZone = FamilyManager.getFamily(new AllOfComponents(typeof(Selected)));

	private Family f_playMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family f_editMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

	// Les variables
	private GameData gameData;
	private GameObject itemDragged; // L'item (ici bloc d'action) en cours de drag
	public GameObject mainCanvas; // Le canvas principal
	public GameObject lastDropZoneUsed; // La derni�re dropzone utilis�e
	public AudioSource audioSource; // Pour le son d'ajout de bloc
	//Pour la gestion du double clic
	private float lastClickTime;
	public float catchTime;

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
		});
		f_editMode.addEntryCallback(delegate {
			Pause = false;
		});
	}

	// toggle toutes les dropzones
	private void setDropZoneState(bool value)
	{
		foreach (GameObject Dp in f_dropZone)
		{
			GameObjectManager.setGameObjectState(Dp.transform.gameObject, value);
			Dp.transform.GetChild(0).gameObject.SetActive(false); // Be sure the drop zone is disabled
		}

		// enable eventManager of each operator
		foreach (GameObject op in f_operators)
        {
			// be sure Outline is disabled on all operator
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

	// Lors de la selection (d�but d'un drag) d'un bloc de la librairie (voir inspector)
	public void beginDragElementFromLibrary(BaseEventData element)
    {
		// On verifie si c'est un �v�nement g�n�r� par le bouton gauche de la souris
		if (!Pause && gameData.dragDropEnabled && (element as PointerEventData).button == PointerEventData.InputButton.Left && element.selectedObject != null)
		{
			// On active les dropzones
			setDropZoneState(true);
			// On cr�e le bloc action associ� � l'�l�ment
			itemDragged = EditingUtility.createEditableBlockFromLibrary(element.selectedObject, mainCanvas);
			// On l'ajoute aux familles de FYFY
			GameObjectManager.bind(itemDragged);
			// exclude all UI elements that can disturb the drag from the EventSystem
			foreach (RaycastOnDrag child in itemDragged.GetComponentsInChildren<RaycastOnDrag>(true))
				child.GetComponent<Image>().raycastTarget = false;
		}
	}


	// Lors de la selection (d�but d'un drag) d'un bloc dans la zone d'�dition
	public void beginDragElementFromEditableScript(BaseEventData element)
    {
		// On verifie si c'est un �v�nement g�n�r� par le bouton gauche de la souris
		if (!Pause && gameData.dragDropEnabled && (element as PointerEventData).button == PointerEventData.InputButton.Left && element.selectedObject != null)
		{
			itemDragged = element.selectedObject;
			Transform parent = itemDragged.transform.parent;

			// On active les dropzones 
			setDropZoneState(true);

			// Update empty zone if required
			EditingUtility.manageEmptyZone(itemDragged);

			// On l'associe (temporairement) au Canvas Main
			GameObjectManager.setGameObjectParent(itemDragged, mainCanvas, true);
			itemDragged.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
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
		}
	}


	// Pendant le drag d'un bloc, permet de lui faire suivre le mouvement de la souris
	public void dragElement()
	{
		if(!Pause && gameData.dragDropEnabled && itemDragged != null) {
			itemDragged.transform.position = Input.mousePosition;
		}
	}


	// Determine si l'element associ� � l'�v�nement EndDrag se trouve dans une zone de container ou non
	// D�truire l'objet si l�ch� hors d'un container
	public void endDragElement()
	{
		if (!Pause && gameData.dragDropEnabled && itemDragged != null)
		{
			// On d�sactive les dropzones
			setDropZoneState(false);

			// On commence par regarder s'il n'y a pas de container point�, dans ce cas on supprime l'objet drag
			if (f_viewportContainerPointed.Count <= 0 && f_focusedDropArea.Count <= 0)
			{
				undoDrop();
				return;
			}
			else // sinon on ajoute l'�l�ment au container point�
			{
				GameObject dropArea = null;
				// If a drop area is pointer over, we select it
				if (f_focusedDropArea.Count > 0)
				{
					// The family can contains several GameObject in particular with nested condition (ie: (A AND B) OR C => in this case if we point A the following element will be pointerover: A, AND, OR and the order is not predictable) => we have to found the deepest Condition
					GameObject deepestArea = null;
					foreach (GameObject pointed in f_focusedDropArea)
						if (deepestArea == null || pointed.transform.IsChildOf(deepestArea.transform))
							deepestArea = pointed;
					dropArea = deepestArea;
				}
				else // means user drop item not directely on drop area but focus the root container => require to find a target area
				{
					// Find the replacement slot if enabled or else the drop zone
					GameObject rootContainer = f_viewportContainerPointed.First().GetComponentInChildren<UIRootContainer>().gameObject;
					foreach (Transform child in rootContainer.transform)
					{
						// if we found a drop zone we store it
						if (child.GetComponent<DropZone>())
							dropArea = child.gameObject;
						// but if we found an enabled replacement slot we override previous choice and we stop loop
						if (child.GetComponent<ReplacementSlot>() && child.gameObject.activeSelf)
						{
							dropArea = child.gameObject;
							break;
						}
					}
				}
				if (dropArea == null)
				{
					Debug.LogError("Warning! Unknown case: the drop area can't be found");
					undoDrop();
					return;
				}
				// check that dropArea is not a child of the dragged element (can appear if we move the mouse fast)
				else if (dropArea.transform.IsChildOf(itemDragged.transform))
                {
					undoDrop();
					return;
				}

				if (addDraggedItemOnDropZone(dropArea))
				{
					// We restore all UI elements inside the EventSystem
					foreach (RaycastOnDrag child in itemDragged.GetComponentsInChildren<RaycastOnDrag>(true))
						child.GetComponent<Image>().raycastTarget = true;
				}
			}
			// Rafraichissement de l'UI
			GameObjectManager.addComponent<NeedRefreshPlayButton>(MainLoop.instance.gameObject);

			itemDragged = null;
		}
	}

	// Add the dragged item on the drop area
	private bool addDraggedItemOnDropZone (GameObject dropArea)
    {
		if (!EditingUtility.addItemOnDropArea(itemDragged, dropArea))
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

		// Lance le son de d�p�t du block d'action
		audioSource.Play();
		return true;
	}

	// suppression de l'objet en cours de drag
	private void undoDrop()
    {
		// Suppression des familles de FYFY
		GameObjectManager.unbind(itemDragged);
		// D�struction du bloc
		UnityEngine.Object.Destroy(itemDragged);
		itemDragged = null;
	}

	// D�clenche la suppression de l'�l�ment
	public void deleteElement(GameObject elementToDelete)
	{
		// On v�rifie qu'il y a bien un objet point� pour la suppression
		if(!Pause && gameData.dragDropEnabled && elementToDelete != null)
        {
			// R�activation d'une EmptyZone si n�cessaire
			EditingUtility.manageEmptyZone(elementToDelete);
			//On associe � l'�l�ment le component ResetBlocLimit pour d�clancher le script de destruction de l'�l�ment
			GameObjectManager.addComponent<ResetBlocLimit>(elementToDelete);
			GameObjectManager.addComponent<NeedRefreshPlayButton>(MainLoop.instance.gameObject);
			// refresh all the hierarchy of parent containers
			refreshHierarchyContainers(elementToDelete);
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

	// Si double clic sur l'�l�ment de la biblioth�que (voir l'inspector), ajoute le bloc d'action au dernier container utilis�
	public void checkDoubleClick(BaseEventData element)
    {
		if (!Pause && gameData.dragDropEnabled && doubleClick() && !itemDragged)
		{
			// if no drop zone used, try to get the last
			if (lastDropZoneUsed == null)
				lastDropZoneUsed = f_dropArea.getAt(f_dropArea.Count-1);
			// be sure the lastDropZone is defined
			if (lastDropZoneUsed != null)
			{
				// On cr�e le bloc action
				itemDragged = EditingUtility.createEditableBlockFromLibrary(element.selectedObject, mainCanvas);
				// On l'ajoute aux familles de FYFY
				GameObjectManager.bind(itemDragged);
				// On l'envoie sur la derni�re dropzone utilis�e
				addDraggedItemOnDropZone(lastDropZoneUsed);
				// refresh all the hierarchy of parent containers
				refreshHierarchyContainers(lastDropZoneUsed);
				// Rafraichissement de l'UI
				GameObjectManager.addComponent<NeedRefreshPlayButton>(MainLoop.instance.gameObject);
				itemDragged = null;
			}
		}
	}


	// V�rifie si le double click a eu lieu
	private bool doubleClick()
	{
		// check double click
		// On met � jour le timer du dernier click
		// et on retourne la r�ponse
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
}