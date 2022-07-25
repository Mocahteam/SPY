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
///		Le drag and drop d'un élément (libraie ou sequence d'action) vers l'extérieur (pour le supprimer)
///		Le clique droit sur un élément dans une sequence d'action pour le supprimer
///		Le double click sur un élément pour l'ajouter à la derniére séquence d'action utilisé
/// 
/// <summary>
/// beginDragElementFromLibrary
///		Pour le début du drag and drop d'un élément venant de la librairie
/// beginDragElementFromEditableScript
///		Pour le début du drag and drop d'un élément venant de la séquence d'action en construction
/// dragElement
///		Pendant le drag d'un élément
/// endDragElement
///		A la fin d'un drag and drop si l'élément n'est pas laché dans un container pour la création d'une séquence
/// creationActionBlock
///		Création d'un block d'action lors de la selection de l'element correspondant dans la librairie
/// deleteElement
///		Destruction d'une block d'action
/// </summary>

public class DragDropSystem : FSystem
{
	// Les familles
    private Family viewportContainerPointed_f = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(ViewportContainer))); // Les container contenant les container éditable
	private Family dropZone_f = FamilyManager.getFamily(new AllOfComponents(typeof(DropZone))); // Les drops zones
	private Family focusedDropArea_f = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver)), new AnyOfComponents(typeof(ReplacementSlot), typeof(DropZone)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY)); // the drop area under mouse cursor

	// Les variables
	private GameObject itemDragged; // L'item (ici block d'action) en cours de drag
	public GameObject mainCanvas; // Le canvas principal
	public GameObject lastDropZoneUsed; // La dernière dropzone utilisée
	public AudioSource audioSource; // Pour le son d'ajout de block
	public GameObject buttonPlay;
	//Pour la gestion du double click
	private float lastClickTime;
	public float catchTime;

	// L'instance
	public static DragDropSystem instance;

	public DragDropSystem()
    {
		instance = this;
	}


	// Besoin d'attendre l'update pour effectuer le recalcul de la taille des container
	public IEnumerator forceUIRefresh(RectTransform bloc)
	{
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(bloc);
	}

	// On active toutes les drop zone qui n'ont pas de voisin ReplacementSlot
	private void setDropZoneState(bool value)
	{
		foreach (GameObject Dp in dropZone_f)
		{
			// if a drop zone is not a neighbor of an empty slot => toggle it
			GameObject neighbor = Dp.transform.parent.GetChild(Mathf.Min(Dp.transform.GetSiblingIndex()+1, Dp.transform.parent.childCount-1)).gameObject;
			if (neighbor != null && !neighbor.GetComponent<ReplacementSlot>())
			{
				GameObjectManager.setGameObjectState(Dp, value);
				Dp.transform.GetChild(0).gameObject.SetActive(false); // Be sure the red bar is disabled
			}
		}
	}

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
					foreach (Outline outline in repSlot.GetComponentsInParent<Outline>())
						outline.enabled = false;
					// check if a child is not already outlined
					foreach (Outline outline in repSlot.GetComponentsInChildren<Outline>())
						if (outline.enabled)
							return;
					// no child is already outline => then we can outline this drop area
					repSlot.GetComponent<Outline>().enabled = true;
				}
			}
		}
    }

	// Only called on ReplacementSlot
	public void unhighlightDropArea(GameObject dropArea)
	{
		if (itemDragged != null && itemDragged.GetComponent<BaseCondition>())
		{
			Outline[] outlines = dropArea.GetComponentsInParent<Outline>(); // the first is this
			// if we found an outline in parent, we enable the first parent that is the second item in the list (the first is the outline of the current drop area)
			if (outlines.Length >= 2)
				outlines[1].enabled = true;
			// then disable outline of current dropArea
			dropArea.GetComponent<Outline>().enabled = false;

		}
	}

	// Lors de la selection (début d'un drag) d'un block de la librairie
	// Crée un game object action = à l'action selectionné dans librairie pour ensuite pouvoir le manipuler (durant le drag et le drop)
	public void beginDragElementFromLibrary(BaseEventData element)
    {
		// On verifie si c'est un évènement généré par le bouton gauche de la souris
		if ((element as PointerEventData).button == PointerEventData.InputButton.Left && element.selectedObject != null)
		{
			// On active les drops zone 
			setDropZoneState(true);
			// On créer le block action associé à l'élément
			itemDragged = createEditableBlockFromLibrary(element.selectedObject);
			// On l'ajoute au famille de FYFY
			GameObjectManager.bind(itemDragged);
			// exclude this GameObject from the EventSystem
			itemDragged.GetComponent<Image>().raycastTarget = false;
			// and all his child who can disturb the drag
			foreach (Image child in itemDragged.GetComponentsInChildren<Image>())
				child.raycastTarget = false;
			foreach (TMP_Text child in itemDragged.GetComponentsInChildren<TMP_Text>())
				child.raycastTarget = false;
		}

	}


	// Lors de la selection (début d'un drag) d'un block de la sequence
	// l'enlever de la hiérarchie de la sequence d'action 
	public void beginDragElementFromEditableScript(BaseEventData element)
    {
		// On verifie si c'est un évènement généré par le bouton gauche de la souris
		if ((element as PointerEventData).button == PointerEventData.InputButton.Left && element.selectedObject != null)
		{
			itemDragged = element.selectedObject;
			Transform parent = itemDragged.transform.parent;

			// On active les drops zone 
			setDropZoneState(true);

			// Update empty zone if required
			manageEmptyZone(itemDragged);

			// On l'associe (temporairement) au Canvas Main
			GameObjectManager.setGameObjectParent(itemDragged, mainCanvas, true);
			itemDragged.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
			// exclude this GameObject from the EventSystem
			itemDragged.GetComponent<Image>().raycastTarget = false;
			// and all his child who can disturb the drag
			foreach (Image child in itemDragged.GetComponentsInChildren<Image>())
				child.raycastTarget = false;
			foreach (TMP_Text child in itemDragged.GetComponentsInChildren<TMP_Text>())
				child.raycastTarget = false;
			// Restore action and subactions to inventory
			foreach (BaseElement actChild in itemDragged.GetComponentsInChildren<BaseElement>())
				GameObjectManager.addComponent<AddOne>(actChild.gameObject);
			// Restore conditions to inventory
			foreach (BaseCondition condChild in itemDragged.GetComponentsInChildren<BaseCondition>())
				GameObjectManager.addComponent<AddOne>(condChild.gameObject);

			// Rend le bouton d'execution actif (ou non)
			UISystem.instance.startUpdatePlayButton();

			refreshHierarchyContainers(parent.gameObject);
		}
	}


	// Pendant le drag d'un block, permet de lui faire suivre le mouvement de la souris
	public void dragElement()
	{
		if(itemDragged != null) {
			itemDragged.transform.position = Input.mousePosition;
		}
	}


	// Determine si l'element associé à l'événement EndDrag se trouve dans une zone de container ou non
	// Détruire l'objet si lâché hors d'un container
	public void endDragElement()
	{
		// On désactive les drop zone
		setDropZoneState(false);

		if (itemDragged != null)
		{
			// On commence par regarder s'il n'y a pas de container pointé, dans ce cas on supprime l'objet drag
			if (viewportContainerPointed_f.Count <= 0 && focusedDropArea_f.Count <= 0)
			{
				undoDrop();
				return;
			}
			else // sinon on ajoute l'élément au container pointé
			{
				GameObject dropArea = null;
				// If a drop area is pointer over, we select it
				if (focusedDropArea_f.Count > 0)
				{
					// The family can contains several GameObject in particular with nested condition (ie: (A AND B) OR C => in this case if we point A the following element will be pointerover: A, AND, OR and the order is not predictable) => we have to found the deepest Condition
					GameObject deepestArea = null;
					foreach (GameObject pointed in focusedDropArea_f)
						if (deepestArea == null || pointed.transform.IsChildOf(deepestArea.transform))
							deepestArea = pointed;
					dropArea = deepestArea;
				}
				else // means user drop item not directely on drop area but focus the root container => require to find a target area
				{
					// Find the replacement slot if enabled or else the drop zone
					GameObject rootContainer = viewportContainerPointed_f.First().GetComponentInChildren<UIRootContainer>().gameObject;
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

				if (addDraggedItemOnDropZone(dropArea))
				{
					// We restore this GameObject inside the EventSystem
					itemDragged.GetComponent<Image>().raycastTarget = true;
					// and its childrens
					foreach (Image child in itemDragged.GetComponentsInChildren<Image>())
						child.raycastTarget = true;
					foreach (TMP_Text child in itemDragged.GetComponentsInChildren<TMP_Text>())
						child.raycastTarget = true;
				}
			}
			// Rafraichissement de l'UI
			UISystem.instance.startUpdatePlayButton();

			itemDragged = null;
		}
	}

	// Add the dragged item on the drop area
	public bool addDraggedItemOnDropZone (GameObject dropArea)
    {
		if (!addItemOnDropArea(itemDragged, dropArea))
		{
			undoDrop();
			return false;
		}

		// update limit bloc
		foreach (BaseElement actChild in itemDragged.GetComponentsInChildren<BaseElement>())
			GameObjectManager.addComponent<Dropped>(actChild.gameObject);
		foreach (BaseCondition condChild in itemDragged.GetComponentsInChildren<BaseCondition>())
			GameObjectManager.addComponent<Dropped>(condChild.gameObject);

		// refresh all the hierarchy of parent containers
		refreshHierarchyContainers(itemDragged);
		// Update size of parent GameObject
		MainLoop.instance.StartCoroutine(UISystem.instance.setEditableSize());

		// Lance le son de dépôt du block d'action
		audioSource.Play();
		return true;
	}

	// Add an item on a drop area
	// return true if the item was added and false otherwise
	public bool addItemOnDropArea(GameObject item, GameObject dropArea)
	{
		if (dropArea.GetComponent<DropZone>())
		{
			// if item is not a BaseElement (BasicAction or ControlElement) cancel actionundo drop
			if (!item.GetComponent<BaseElement>())
				return false;

			// the item is compatible with dropZone
			Transform targetContainer = null;
			int siblingIndex = 0;
			if (dropArea.transform.parent.GetComponent<UIRootContainer>()) // The main container (the one associated to the agent)
			{
				targetContainer = dropArea.transform.parent; // target is the parent
				siblingIndex = dropArea.transform.GetSiblingIndex();
			}
			else if (dropArea.transform.parent.GetComponent<BaseElement>()) // BasicAction
			{
				targetContainer = dropArea.transform.parent.parent; // target is the grandparent
				siblingIndex = dropArea.transform.parent.GetSiblingIndex();
			}
			else if (dropArea.transform.parent.parent.GetComponent<ControlElement>() && dropArea.transform.parent.GetSiblingIndex() == 0) // the dropArea of the first child of a Control block
			{
				targetContainer = dropArea.transform.parent.parent.parent; // target is the grandgrandparent
				siblingIndex = dropArea.transform.parent.parent.GetSiblingIndex();
			}
			else if (dropArea.transform.parent.parent.GetComponent<ControlElement>() && dropArea.transform.parent.GetSiblingIndex() != 0) // the dropArea of another child of a Control block
			{
				targetContainer = dropArea.transform.parent; // target is the parent
				siblingIndex = dropArea.transform.GetSiblingIndex();
			}
			else
			{
				Debug.LogError("Warning! Unknown case: the drop zone is not in the correct context");
				return false;
			}
			lastDropZoneUsed = dropArea;
			// On associe l'element au container
			item.transform.SetParent(targetContainer);
			// On met l'élément à la position voulue
			item.transform.SetSiblingIndex(siblingIndex);
		}
		else if (dropArea.GetComponent<ReplacementSlot>()) // we replace the replacementSlot by the item
		{
			// If replacement slot is not in the same type of item => cancel action
			ReplacementSlot repSlot = dropArea.GetComponent<ReplacementSlot>();
			if ((repSlot.slotType == ReplacementSlot.SlotType.BaseElement && !item.GetComponent<BaseElement>()) ||
				(repSlot.slotType == ReplacementSlot.SlotType.BaseCondition && !item.GetComponent<BaseCondition>()))
				return false;
			// if replacement slot is for base element => insert item, hide replacement slot and enable dropZone
			if (repSlot.slotType == ReplacementSlot.SlotType.BaseElement)
			{
				// On associe l'element au container
				item.transform.SetParent(dropArea.transform.parent);
				// On met l'élément à la position voulue
				item.transform.SetSiblingIndex(dropArea.transform.GetSiblingIndex() - 1); // the empty zone is preceded by the drop zone, so we add the item at the position of the drop zone (reason of -1)	
				// disable empty slot
				dropArea.GetComponent<Outline>().enabled = false;

				// Because this function can be call for binded GO or not
				if (GameObjectManager.isBound(dropArea)) GameObjectManager.setGameObjectState(dropArea, false);
				else dropArea.SetActive(false);

				// define last drop zone to the drop zone associated to this replacement slot
				lastDropZoneUsed = dropArea.transform.parent.GetChild(dropArea.transform.GetSiblingIndex() - 1).gameObject;

				// Because this function can be call for binded GO or not
				if (GameObjectManager.isBound(lastDropZoneUsed)) GameObjectManager.setGameObjectState(lastDropZoneUsed, true);
				else lastDropZoneUsed.SetActive(true);
			}
			// if replacement slot is for base condition => two case fill an empty zone or replace existing condition
			else if (repSlot.slotType == ReplacementSlot.SlotType.BaseCondition)
			{
				// On associe l'element au container
				item.transform.SetParent(dropArea.transform.parent);
				// On met l'élément à la position voulue
				item.transform.SetSiblingIndex(dropArea.transform.GetSiblingIndex());
				// check if the replacement slot is an empty zone (doesn't contain a condition)
				if (!repSlot.GetComponent<BaseCondition>())
				{
					// disable empty slot
					dropArea.GetComponent<Outline>().enabled = false;

					// Because this function can be call for binded GO or not
					if (GameObjectManager.isBound(dropArea)) GameObjectManager.setGameObjectState(dropArea, false);
					else dropArea.SetActive(false);
				}
				else
				{
					// Because this function can be call for binded GO or not
					if (GameObjectManager.isBound(dropArea))
						// Remove old condition from FYFY
						GameObjectManager.unbind(dropArea);

					// Destroy it
					UnityEngine.Object.Destroy(dropArea);
				}
			}
		}
		else
		{
			Debug.LogError("Warning! Unknown case: the drop area is not a drop zone or a replacement zone");
			return false;
		}
		// We secure the scale
		item.transform.localScale = new Vector3(1, 1, 1);

		UISystem.instance.startUpdatePlayButton();
		return true;
	}

	private void undoDrop()
    {
		// Suppresion des familles de FYFY
		GameObjectManager.unbind(itemDragged);
		// Déstruction du block
		UnityEngine.Object.Destroy(itemDragged);
		itemDragged = null;
	}

	// On crée l'objet dragged à partir d'un item de la bibliothèque sans le bind à FYFY. C'est en fonction du contexte d'appel que l'objet doit être bind ou pas
	public GameObject createEditableBlockFromLibrary(GameObject element)
    {
		// On récupére le prefab associé à l'action de la librairie
		GameObject prefab = element.GetComponent<ElementToDrag>().actionPrefab;
		// Create a dragged GameObject
		GameObject newItem = UnityEngine.Object.Instantiate<GameObject>(prefab, element.transform);
		//On l'attache au canvas pour le drag ou l'on veux
		newItem.transform.SetParent(mainCanvas.transform);
		// Si c'est un basic action
		if(newItem.GetComponent<Highlightable>() is BasicAction)
        {
			BaseElement action = newItem.GetComponent<BaseElement>();
			newItem.GetComponent<LibraryItemRef>().linkedTo = element;
		}
		return newItem;
	}


	// Supprime l'element
	public void deleteElement(GameObject elementToDelete)
	{
		// On vérifie qu'il y a bien un objet pointé pour la suppression
		if(elementToDelete != null)
        {
			GameObject conditionContainer = null;
			GameObject containerAction = null;
			GameObject elseContainer = null;

			// On regarde si l'objet contien des enfants, si oui, on parcourt les enfants pour les supprimer aussi
            if (elementToDelete.GetComponent<IfControl>())
            {
				conditionContainer = elementToDelete.transform.Find("ConditionContainer").gameObject;
				containerAction = elementToDelete.transform.Find("Container").gameObject;
				if (elementToDelete.GetComponent<IfElseControl>())
                {
					elseContainer = elementToDelete.transform.Find("ElseContainer").gameObject;
				}
			}
			else if (elementToDelete.GetComponent<ForControl>())
            {
				containerAction = elementToDelete.transform.Find("Container").gameObject;
				if (elementToDelete.GetComponent<WhileControl>())
				{
					conditionContainer = elementToDelete.transform.Find("ConditionContainer").gameObject;
				}
			}

			if(conditionContainer != null)
            {
				foreach(Transform child in conditionContainer.transform)
                {
					Debug.Log("Child name : " + child.name);
					if(child.name != "EmptyConditionalSlot")
                    {
						Debug.Log("Delete : " + child.name);
						deleteElement(child.gameObject);
					}
                }
			}
			if(containerAction != null)
            {

            }
			if(elseContainer != null)
            {

            }

			// Réactivation d'une EmptyZone si nécessaire
			manageEmptyZone(elementToDelete);
			GameObjectManager.addComponent<AddOne>(elementToDelete);
			//On associe à l'élément le component ResetBlocLimit pour déclancher le script de destruction de l'élément
			GameObjectManager.addComponent<ResetBlocLimit>(elementToDelete);
			UISystem.instance.startUpdatePlayButton();
			// refresh all the hierarchy of parent containers
			refreshHierarchyContainers(elementToDelete);
		}
	}

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

	// Si double click sur l'élément, ajoute le block d'action au dernier container utilisé
	public void checkDoubleClick(BaseEventData element)
    {
		if (doubleClick() && lastDropZoneUsed != null)
		{

			// if last drop zone used is the neighbor of enabled replacement slot => disable this replacement slot
			GameObject neighbor = lastDropZoneUsed.transform.parent.GetChild(Mathf.Min(lastDropZoneUsed.transform.GetSiblingIndex() + 1, lastDropZoneUsed.transform.parent.childCount - 1)).gameObject;
			if (neighbor != null && neighbor.activeInHierarchy && neighbor.GetComponent<ReplacementSlot>())
				GameObjectManager.setGameObjectState(neighbor, false);
			// On crée le block action
			itemDragged = createEditableBlockFromLibrary(element.selectedObject);
			// On l'ajoute au famille de FYFY
			GameObjectManager.bind(itemDragged);
			// On l'envoie sur la dernière dropzone utilisée
			addDraggedItemOnDropZone(lastDropZoneUsed);
			// refresh all the hierarchy of parent containers
			refreshHierarchyContainers(lastDropZoneUsed);
			// Rafraichissement de l'UI
			UISystem.instance.startUpdatePlayButton();
			itemDragged = null;
		}
	}


	// Vérifie si le double click a eu lieu
	private bool doubleClick()
	{
		// check double click
		// On met à jours le timer du dernier click
		// et on retourne la réponse
		if (Time.time - lastClickTime < catchTime)
        {
			lastClickTime = Time.time;
			return true;
		}
        else
        {
			lastClickTime = Time.time;
			return false;
		}

	}

	// Réactive une end zone si besoin
	private void manageEmptyZone(GameObject ele)
    {
		if (ele.GetComponent<BaseCondition>())
        {
			// enable the next last child of the container
			GameObjectManager.setGameObjectState(ele.transform.parent.GetChild(ele.transform.GetSiblingIndex()+1).gameObject, true);
        }
		else if (ele.GetComponent<BaseElement>())
        {
			// We have to disable dropZone and enable empty zone if no other BaseElement exists inside the container
			// We count the number of brother (including this) that is a BaseElement
			int cpt = 0;
			foreach (Transform brother in ele.transform.parent)
				if (brother.GetComponent<BaseElement>())
					cpt++;
			// if the container contains only 1 child (the element we are removing) => enable EmptyZone and disable dropZone
			if (cpt <= 1)
            {
				// enable EmptyZone
				GameObjectManager.setGameObjectState(ele.transform.parent.GetChild(ele.transform.parent.childCount - 1).gameObject, true);
				// disable DropZone
				GameObjectManager.setGameObjectState(ele.transform.parent.GetChild(ele.transform.parent.childCount - 2).gameObject, false);
			}
        }
	}
}