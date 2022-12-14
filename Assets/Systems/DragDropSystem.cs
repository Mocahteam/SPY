using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private Family f_viewportContainerPointed = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(ViewportContainer))); // Les container contenant les containers éditables
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

	// Les variables
	private GameData gameData;
	private GameObject itemDragged; // L'item (ici bloc d'action) en cours de drag
	private Coroutine viewLastDropZone = null;
	public GameObject mainCanvas; // Le canvas principal
	public GameObject lastDropZoneUsed; // La dernière dropzone utilisée
	public AudioSource audioSource; // Pour le son d'ajout de bloc
	//Pour la gestion du double clic
	private float lastClickTime;
	public float catchTime;
	public RectTransform editableContainers;

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

	// Lors de la selection (début d'un drag) d'un bloc de la librairie (voir inspector)
	public void beginDragElementFromLibrary(BaseEventData element)
    {
		// On verifie si c'est un évènement généré par le bouton gauche de la souris
		if (!Pause && gameData.dragDropEnabled && (element as PointerEventData).button == PointerEventData.InputButton.Left && element.selectedObject != null)
		{
			// On active les dropzones
			setDropZoneState(true);
			// On crée le bloc action associé à l'élément
			itemDragged = EditingUtility.createEditableBlockFromLibrary(element.selectedObject, mainCanvas);
			// On l'ajoute aux familles de FYFY
			GameObjectManager.bind(itemDragged);
			// exclude all UI elements that can disturb the drag from the EventSystem
			foreach (RaycastOnDrag child in itemDragged.GetComponentsInChildren<RaycastOnDrag>(true))
				child.GetComponent<Image>().raycastTarget = false;
		}
	}


	// Lors de la selection (début d'un drag) d'un bloc dans la zone d'édition
	public void beginDragElementFromEditableScript(BaseEventData element)
    {
		// On verifie si c'est un évènement généré par le bouton gauche de la souris
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
					MainLoop.instance.StartCoroutine(pulseItem(itemDragged));
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

		// Lance le son de dépôt du block d'action
		audioSource.Play();
		return true;
	}

	// suppression de l'objet en cours de drag
	private void undoDrop()
    {
		// Suppression des familles de FYFY
		GameObjectManager.unbind(itemDragged);
		// Déstruction du bloc
		UnityEngine.Object.Destroy(itemDragged);
		itemDragged = null;
	}

	// Déclenche la suppression de l'élément
	public void deleteElement(GameObject elementToDelete)
	{
		// On vérifie qu'il y a bien un objet pointé pour la suppression
		if(!Pause && gameData.dragDropEnabled && elementToDelete != null)
        {
			// Réactivation d'une EmptyZone si nécessaire
			EditingUtility.manageEmptyZone(elementToDelete);
			//On associe à l'élément le component ResetBlocLimit pour déclancher le script de destruction de l'élément
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

	// Si double clic sur l'élément de la bibliothèque (voir l'inspector), ajoute le bloc d'action au dernier container utilisé
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
				// On crée le bloc action
				itemDragged = EditingUtility.createEditableBlockFromLibrary(element.selectedObject, mainCanvas);
				// On l'ajoute aux familles de FYFY
				GameObjectManager.bind(itemDragged);
				// On l'envoie sur la dernière dropzone utilisée
				addDraggedItemOnDropZone(lastDropZoneUsed);
				// refresh all the hierarchy of parent containers
				refreshHierarchyContainers(lastDropZoneUsed);

				if (viewLastDropZone != null)
					MainLoop.instance.StopCoroutine(viewLastDropZone);
				MainLoop.instance.StartCoroutine(focusOnLastDropZoneUsed());
				MainLoop.instance.StartCoroutine(pulseItem(itemDragged));

				// Rafraichissement de l'UI
				GameObjectManager.addComponent<NeedRefreshPlayButton>(MainLoop.instance.gameObject);
				itemDragged = null;
			}
		}
	}

	private IEnumerator pulseItem(GameObject newItem)
    {
		float initScaleX = newItem.transform.localScale.x;
		newItem.transform.localScale = new Vector3(newItem.transform.localScale.x + 0.3f, newItem.transform.localScale.y, newItem.transform.localScale.z);
		while (newItem.transform.localScale.x > initScaleX)
		{
			Debug.Log(newItem.transform.localScale.x);
			newItem.transform.localScale = new Vector3(newItem.transform.localScale.x-0.01f, newItem.transform.localScale.y, newItem.transform.localScale.z);
			yield return null;
		}
		Debug.Log(newItem.transform.localScale.x+" "+ initScaleX);
	}

	private IEnumerator focusOnLastDropZoneUsed()
    {
		yield return new WaitForSeconds(.25f);

		RectTransform editableCanvas = editableContainers.parent as RectTransform;
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
}