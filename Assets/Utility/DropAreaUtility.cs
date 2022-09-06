using FYFY;
using UnityEngine;
using UnityEngine.UI;

public static class DropAreaUtility
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

		return true;
	}
}