using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections.Generic;
using TMPro;
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

	// We create an editable block from a library item (without binded it to FYFY, depending on context the object has to be binded or not)
	public static GameObject createEditableBlockFromLibrary(GameObject element, GameObject targetCanvas)
	{
		// On récupére le prefab associé à l'action de la librairie
		GameObject prefab = element.GetComponent<ElementToDrag>().actionPrefab;
		// Create a dragged GameObject
		GameObject newItem = UnityEngine.Object.Instantiate<GameObject>(prefab, element.transform);
		// Ajout d'un TooltipContent identique à celui de l'inventaire
		if (element.GetComponent<TooltipContent>()) {
			TooltipContent tooltip = newItem.AddComponent<TooltipContent>();
			tooltip.text = element.GetComponent<TooltipContent>().text;
		}
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

	// Copy an editable script to the container of an agent
	public static void fillExecutablePanel(GameObject srcScript, GameObject targetContainer, string agentTag)
	{
		// On va copier la sequence créé par le joueur dans le container de la fenêtre du robot
		// On commence par créer une copie du container ou se trouve la sequence
		GameObject containerCopy = CopyActionsFromAndInitFirstChild(srcScript, false, agentTag);
		// On copie les actions dedans 
		for (int i = 0; i < containerCopy.transform.childCount; i++)
		{
			// On ne conserve que les BaseElement et on les nettoie
			if (containerCopy.transform.GetChild(i).GetComponent<BaseElement>())
			{
				Transform child = UnityEngine.GameObject.Instantiate(containerCopy.transform.GetChild(i)); ;
				// Si c'est un block special (for, if...)
				if (child.GetComponent<ControlElement>())
					CleanControlBlock(child);
				child.SetParent(targetContainer.transform, false);
			}
		}
		// Va linker les blocs ensemble
		// C'est à dire qu'il va définir pour chaque bloc, qu'elle est le suivant à exécuter
		computeNext(targetContainer);
		// On détruit la copy de la sequence d'action
		UnityEngine.Object.Destroy(containerCopy);
	}

	/**
	 * On copie le container qui contient la sequence d'actions et on initialise les firstChild
	 * Param:
	 *	Container (GameObject) : Le container qui contient le script à copier
	 *	isInteractable (bool) : Si le script copié peut contenir des éléments interactable (sinon l'interaction sera desactivé)
	 *	agent (GameObject) : L'agent sur qui l'on va copier la sequence (pour définir la couleur)
	 * 
	 **/
	private static GameObject CopyActionsFromAndInitFirstChild(GameObject container, bool isInteractable, string agentTag)
	{
		// On va travailler avec une copy du container
		GameObject copyGO = GameObject.Instantiate(container);
		//Pour tous les élément interactible, on va les désactiver/activer selon le paramétrage
		foreach (TMP_Dropdown drop in copyGO.GetComponentsInChildren<TMP_Dropdown>())
		{
			drop.interactable = isInteractable;
		}
		foreach (TMP_InputField input in copyGO.GetComponentsInChildren<TMP_InputField>())
		{
			input.interactable = isInteractable;
		}

		// Pour chaque bloc for
		foreach (ForControl forAct in copyGO.GetComponentsInChildren<ForControl>())
		{
			// Si activé, on note le nombre de tour de boucle à faire
			if (!isInteractable && !forAct.gameObject.GetComponent<WhileControl>())
			{
				forAct.nbFor = int.Parse(forAct.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text);
				forAct.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
			}// Sinon on met tout à 0
			else if (isInteractable && !forAct.gameObject.GetComponent<WhileControl>())
			{
				forAct.currentFor = 0;
				forAct.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = forAct.nbFor.ToString();
			}
			else if (forAct is WhileControl)
			{
				// On traduit la condition en string
				((WhileControl)forAct).condition = new List<string>();
				conditionToStrings(forAct.gameObject.transform.Find("ConditionContainer").GetChild(0).gameObject, ((WhileControl)forAct).condition);

			}
			// On parcourt les éléments présent dans le block action
			foreach (BaseElement act in forAct.GetComponentsInChildren<BaseElement>())
			{
				// Si ce n'est pas un bloc action alors on le note comme premier élément puis on arrête le parcourt des éléments
				if (!act.Equals(forAct))
				{
					forAct.firstChild = act.gameObject;
					break;
				}
			}
		}
		// Pour chaque block de boucle infini
		foreach (ForeverControl loopAct in copyGO.GetComponentsInChildren<ForeverControl>())
		{
			foreach (BaseElement act in loopAct.GetComponentsInChildren<BaseElement>())
			{
				if (!act.Equals(loopAct))
				{
					loopAct.firstChild = act.gameObject;
					break;
				}
			}
		}
		// Pour chaque block if
		foreach (IfControl ifAct in copyGO.GetComponentsInChildren<IfControl>())
		{
			// On traduit la condition en string
			ifAct.condition = new List<string>();
			conditionToStrings(ifAct.gameObject.transform.Find("ConditionContainer").GetChild(0).gameObject, ifAct.condition);

			GameObject thenContainer = ifAct.transform.Find("Container").gameObject;
			BaseElement firstThen = thenContainer.GetComponentInChildren<BaseElement>();
			if (firstThen)
				ifAct.firstChild = firstThen.gameObject;
			//Si c'est un elseAction
			if (ifAct is IfElseControl)
			{
				GameObject elseContainer = ifAct.transform.Find("ElseContainer").gameObject;
				BaseElement firstElse = elseContainer.GetComponentInChildren<BaseElement>();
				if (firstElse)
					((IfElseControl)ifAct).elseFirstChild = firstElse.gameObject;
			}
		}

		foreach (PointerSensitive pointerSensitive in copyGO.GetComponentsInChildren<PointerSensitive>())
			pointerSensitive.enabled = isInteractable;

		foreach (Selectable selectable in copyGO.GetComponentsInChildren<Selectable>())
			selectable.interactable = isInteractable;

		// On défini la couleur de l'action selon l'agent à qui appartiendra la script
		Color actionColor;
		switch (agentTag)
		{
			case "Player":
				actionColor = MainLoop.instance.GetComponent<AgentColor>().playerAction;
				break;
			case "Drone":
				actionColor = MainLoop.instance.GetComponent<AgentColor>().droneAction;
				break;
			default: // agent by default = robot
				actionColor = MainLoop.instance.GetComponent<AgentColor>().playerAction;
				break;
		}

		foreach (BasicAction act in copyGO.GetComponentsInChildren<BasicAction>())
		{
			act.gameObject.GetComponent<Image>().color = actionColor;
		}

		return copyGO;
	}

	/**
	 * Nettoie le bloc de controle (On supprime les end-zones, on met les conditions sous forme d'un seul bloc)
	 * Param:
	 *	specialBlock (GameObject) : Container qu'il faut nettoyer
	 * 
	 **/
	public static void CleanControlBlock(Transform specialBlock)
	{
		// Vérifier que c'est bien un block de controle
		if (specialBlock.GetComponent<ControlElement>())
		{
			// Récupérer le container des actions
			Transform container = specialBlock.transform.Find("Container");
			// remove the last child, the emptyZone
			GameObject emptySlot = container.GetChild(container.childCount - 1).gameObject;
			if (GameObjectManager.isBound(emptySlot))
				GameObjectManager.unbind(emptySlot);
			emptySlot.transform.SetParent(null);
			GameObject.Destroy(emptySlot);
			// remove the new last child, the dropzone
			GameObject dropZone = container.GetChild(container.childCount - 1).gameObject;
			if (GameObjectManager.isBound(emptySlot))
				GameObjectManager.unbind(dropZone);
			dropZone.transform.SetParent(null);
			GameObject.Destroy(dropZone);

			// Si c'est un block if on garde le container des actions (sans le emptyslot et la dropzone) mais la condition est traduite dans IfAction
			if (specialBlock.GetComponent<IfElseControl>())
			{
				// get else container
				Transform elseContainer = specialBlock.transform.Find("ElseContainer");
				// remove the last child, the emptyZone
				emptySlot = elseContainer.GetChild(elseContainer.childCount - 1).gameObject;
				if (GameObjectManager.isBound(emptySlot))
					GameObjectManager.unbind(emptySlot);
				emptySlot.transform.SetParent(null);
				GameObject.Destroy(emptySlot);
				// remove the new last child, the dropzone
				dropZone = elseContainer.GetChild(elseContainer.childCount - 1).gameObject;
				if (GameObjectManager.isBound(emptySlot))
					GameObjectManager.unbind(dropZone);
				dropZone.transform.SetParent(null);
				GameObject.Destroy(dropZone);
				// On parcourt les blocks qui composent le ElseContainer afin de les nettoyer également
				foreach (Transform block in elseContainer)
					// Si c'est le cas on fait un appel récursif
					if (block.GetComponent<ControlElement>())
						CleanControlBlock(block);
			}

			// On parcourt les blocks qui composent le container afin de les nettoyer également
			foreach (Transform block in container)
				// Si c'est le cas on fait un appel récursif
				if (block.GetComponent<ControlElement>())
					CleanControlBlock(block);
		}
	}

	// Transforme une sequence de condition en une chaine de caractére
	private static void conditionToStrings(GameObject condition, List<string> chaine)
	{
		// Check if condition is a BaseCondition
		if (condition.GetComponent<BaseCondition>())
		{
			// On regarde si la condition reçue est un élément ou bien un opérator
			// Si c'est un élément, on le traduit en string et on le renvoie 
			if (condition.GetComponent<BaseCaptor>())
				chaine.Add("" + condition.GetComponent<BaseCaptor>().captorType);
			else
			{
				BaseOperator bo;
				if (condition.TryGetComponent<BaseOperator>(out bo))
				{
					Transform conditionContainer = bo.transform.GetChild(0);
					// Si c'est une négation on met "!" puis on fait une récursive sur le container et on renvoie le tous traduit en string
					if (bo.operatorType == BaseOperator.OperatorType.NotOperator)
					{
						// On vérifie qu'il y a bien un élément présent, son container doit contenir 3 enfants (icone, une BaseCondition et le ReplacementSlot)
						if (conditionContainer.childCount == 3)
						{
							chaine.Add("NOT");
							conditionToStrings(conditionContainer.GetComponentInChildren<BaseCondition>().gameObject, chaine);
						}
						else
						{
							GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.BadCondition });
						}
					}
					else if (bo.operatorType == BaseOperator.OperatorType.AndOperator)
					{
						// Si les côtés de l'opérateur sont remplis, alors il compte 5 childs (2 ReplacementSlots, 2 BaseCondition et 1 icone), sinon cela veux dire que il manque des conditions
						if (conditionContainer.childCount == 5)
						{
							chaine.Add("(");
							conditionToStrings(conditionContainer.GetChild(0).gameObject, chaine);
							chaine.Add("AND");
							conditionToStrings(conditionContainer.GetChild(3).gameObject, chaine);
							chaine.Add(")");
						}
						else
						{
							GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.BadCondition });
						}
					}
					else if (bo.operatorType == BaseOperator.OperatorType.OrOperator)
					{
						// Si les côtés de l'opérateur sont remplis, alors il compte 5 childs, sinon cela veux dire que il manque des conditions
						if (conditionContainer.childCount == 5)
						{
							chaine.Add("(");
							conditionToStrings(conditionContainer.GetChild(0).gameObject, chaine);
							chaine.Add("OR");
							conditionToStrings(conditionContainer.GetChild(3).gameObject, chaine);
							chaine.Add(")");
						}
						else
						{
							GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.BadCondition });
						}
					}
				}
				else
				{
					Debug.LogError("Unknown BaseCondition!!!");
				}
			}
		}
		else
			GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.BadCondition });
	}
	
	// link actions together => define next property
	 // Associe à chaque bloc le bloc qui sera executé aprés
	public static void computeNext(GameObject container)
	{
		// parcourir tous les enfants jusqu'à l'avant dernier
		for (int i = 0; i < container.transform.childCount - 1; i++)
		{
			Transform child = container.transform.GetChild(i);
			child.GetComponent<BaseElement>().next = container.transform.GetChild(i + 1).gameObject;
		}
		// traitement de la dernière instruction
		if (container.transform.childCount > 0)
		{
			Transform lastChild = container.transform.GetChild(container.transform.childCount - 1);
			// On cherche un parent qui serait une boucle, si tel est le cas le next est cette boucle
			Transform parent = container.transform.parent;
			while (parent != null && parent.GetComponent<ForControl>() == null && parent.GetComponent<ForeverControl>() == null)
				parent = parent.parent;
			if (parent != null)
				lastChild.GetComponent<BaseElement>().next = parent.gameObject;
			// Sinon on ne fait rien et fin de la sequence
		}

		// parcourir tous les enfants jusqu'au dernier cette fois ci pour déclencher des appel récursif pour les structure de contrôle
		for (int i = 0; i < container.transform.childCount; i++)
		{
			Transform child = container.transform.GetChild(i);
			// Si le fils est un contrôle, appel résursif sur leurs containers
			if (child.GetComponent<ControlElement>())
			{
				computeNext(child.transform.Find("Container").gameObject);
				// Si c'est un else il ne faut pas oublier le container else
				if (child.GetComponent<IfElseControl>())
					computeNext(child.transform.Find("ElseContainer").gameObject);
			}
		}
	}
}