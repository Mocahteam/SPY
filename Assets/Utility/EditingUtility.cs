using FYFY;
using UnityEngine;
using UnityEngine.UI;

public static class EditingUtility
{
	// Add an item on a drop area
	// return true if the item was added and false otherwise
	public static bool addItemOnDropArea(GameObject item, GameObject dropArea)
	{
		if (dropArea.GetComponent<DropZone>())
		{
			// if item is not a BaseElement (BasicAction or ControlElement) cancel action, undo drop
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
			else if (dropArea.transform.parent.parent.GetComponent<ControlElement>() && dropArea.transform.parent.GetSiblingIndex() == 1) // the dropArea of the first child of a Control block
			{
				targetContainer = dropArea.transform.parent.parent.parent; // target is the grandgrandparent
				siblingIndex = dropArea.transform.parent.parent.GetSiblingIndex();
			}
			else if (dropArea.transform.parent.parent.GetComponent<ControlElement>() && dropArea.transform.parent.GetSiblingIndex() > 1) // the dropArea of another child of a Control block
			{
				targetContainer = dropArea.transform.parent; // target is the parent
				siblingIndex = dropArea.transform.GetSiblingIndex();
			}
			else
			{
				Debug.LogError("Warning! Unknown case: the drop zone is not in the correct context");
				return false;
			}
			// if binded set this drop area as default
			if (GameObjectManager.isBound(dropArea))
				GameObjectManager.addComponent<Selected>(dropArea);
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
				GameObject dropzone = dropArea.transform.parent.GetChild(dropArea.transform.GetSiblingIndex() - 1).gameObject;

				// Because this function can be call for binded GO or not
				if (GameObjectManager.isBound(dropzone)) GameObjectManager.setGameObjectState(dropzone, true);
				else dropzone.SetActive(true);

				// if binded set this drop zone as default
				if (GameObjectManager.isBound(dropzone))
					GameObjectManager.addComponent<Selected>(dropzone);
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
					// ResetBlocLimit will restore library and remove dropArea and children
					GameObjectManager.addComponent<ResetBlocLimit>(dropArea);
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

		return true;
	}

	// We create the an editable block from a library item (without binded it to FYFY, depending on context the object has to be binded or not)
	public static GameObject createEditableBlockFromLibrary(GameObject element, GameObject targetCanvas)
	{
		// On récupére le prefab associé à l'action de la librairie
		GameObject prefab = element.GetComponent<ElementToDrag>().actionPrefab;
		// Create a dragged GameObject
		GameObject newItem = UnityEngine.Object.Instantiate<GameObject>(prefab, element.transform);
		// On l'attache au canvas pour le drag ou l'on veux
		newItem.transform.SetParent(targetCanvas.transform);
		// link with library
		if (newItem.GetComponent<LibraryItemRef>())
			newItem.GetComponent<LibraryItemRef>().linkedTo = element;
		return newItem;
	}

	// elementToDelete will be deleted then manage empty zone accordingly
	public static void manageEmptyZone(GameObject elementToDelete)
	{
		if (elementToDelete.GetComponent<BaseCondition>())
		{
			// enable the next last child of the container
			GameObjectManager.setGameObjectState(elementToDelete.transform.parent.GetChild(elementToDelete.transform.GetSiblingIndex() + 1).gameObject, true);
		}
		else if (elementToDelete.GetComponent<BaseElement>())
		{
			// We have to disable dropZone and enable empty zone if no other BaseElement exists inside the container
			// We count the number of brother (including this) that is a BaseElement
			int cpt = 0;
			foreach (Transform brother in elementToDelete.transform.parent)
				if (brother.GetComponent<BaseElement>())
					cpt++;
			// if the container contains only 1 child (the element we are removing) => enable EmptyZone and disable dropZone
			if (cpt <= 1)
			{
				// enable EmptyZone
				GameObjectManager.setGameObjectState(elementToDelete.transform.parent.GetChild(elementToDelete.transform.parent.childCount - 1).gameObject, true);
				// disable DropZone
				GameObjectManager.setGameObjectState(elementToDelete.transform.parent.GetChild(elementToDelete.transform.parent.childCount - 2).gameObject, false);
			}
		}
	}
}