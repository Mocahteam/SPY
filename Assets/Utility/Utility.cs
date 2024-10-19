using FYFY;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public static class Utility
{

	public static string testFromScenarioEditor = "testFromScenarioEditor";
	public static string testFromLevelEditor = "testFromLevelEditor";
	public static string testFromUrl = "testFromUrl";
	public static string editingScenario = "editingScenario";

	[Serializable]
	public class RawConstraint
	{
		public string attribute;
		public string constraint;
		public string value;
		public string tag2;
		public string attribute2;
	}

	[Serializable]
	public class RawFilter
	{
		public string label;
		public string tag;
		public RawConstraint[] constraints;
	}
	// Add an item on a drop area
	// return true if the item was added and false otherwise
	public static bool addItemOnDropArea(GameObject item, GameObject dropArea)
	{
		if (dropArea.GetComponent<DropZone>())
		{
			// if item is not a BaseElement (BasicAction or ControlElement) cancel action, undo drop
			if (!item.GetComponent<BaseElement>())
			{
				return false;
			}

			// the item is compatible with dropZone
			Transform targetContainer = null;
			int siblingIndex = 0;
			if (dropArea.transform.parent.GetComponent<BaseElement>()) // BasicAction
			{
				targetContainer = dropArea.transform.parent.parent; // target is the grandparent
				siblingIndex = dropArea.transform.parent.GetSiblingIndex();
			}
			else if (dropArea.transform.parent.parent.GetComponent<ControlElement>() && dropArea.transform.parent.GetSiblingIndex() == 1) // the dropArea of the first child of a Control block
			{
				targetContainer = dropArea.transform.parent.parent.parent; // target is the grandgrandparent
				siblingIndex = dropArea.transform.parent.parent.GetSiblingIndex();
			}
			else
			{
				Debug.LogError("Warning! Unknown case: the drop zone is not in the correct context");
				return false;
			}
			// if binded set this drop area as default
			if (GameObjectManager.isBound(dropArea) && !dropArea.GetComponent<Selected>())
				GameObjectManager.addComponent<Selected>(dropArea);
			// On associe l'element au container
			item.transform.SetParent(targetContainer);
			// On met l'élément à la position voulue
			item.transform.SetSiblingIndex(siblingIndex);
		}
		else if (dropArea.GetComponent<ReplacementSlot>()) // we replace the replacementSlot by the item
		{
			ReplacementSlot repSlot = dropArea.GetComponent<ReplacementSlot>();
			// if replacement slot is for base element => insert item just before replacement slot
			if (repSlot.slotType == ReplacementSlot.SlotType.BaseElement)
			{
				// On associe l'element au container
				item.transform.SetParent(repSlot.transform.parent);
				// On met l'élément à la position voulue
				item.transform.SetSiblingIndex(repSlot.transform.GetSiblingIndex()); 
				// disable empty slot
				repSlot.GetComponent<Outline>().enabled = false;

				// if binded set this empty slot as default
				if (GameObjectManager.isBound(repSlot.gameObject) && !repSlot.GetComponent<Selected>())
					GameObjectManager.addComponent<Selected>(repSlot.gameObject);
			}
			// if replacement slot is for base condition => two case fill an empty zone or replace existing condition
			else if (repSlot.slotType == ReplacementSlot.SlotType.BaseCondition)
			{
				// On associe l'element au container
				item.transform.SetParent(repSlot.transform.parent);
				// On met l'élément à la position voulue
				item.transform.SetSiblingIndex(repSlot.transform.GetSiblingIndex());
				// check if the replacement slot is an empty zone (doesn't contain a condition)
				if (!repSlot.GetComponent<BaseCondition>())
				{
					// disable empty slot
					repSlot.GetComponent<Outline>().enabled = false;

					// Because this function can be call for binded GO or not
					if (GameObjectManager.isBound(repSlot.gameObject)) GameObjectManager.setGameObjectState(repSlot.transform.gameObject, false);
					else repSlot.transform.gameObject.SetActive(false);
				}
				else
				{
					// ResetBlocLimit will restore library and remove dropArea and children
					GameObjectManager.addComponent<ResetBlocLimit>(repSlot.gameObject);
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
			// enable the next last child of the container
			GameObjectManager.setGameObjectState(elementToDelete.transform.parent.GetChild(elementToDelete.transform.GetSiblingIndex() + 1).gameObject, true);
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
				Transform child = UnityEngine.GameObject.Instantiate(containerCopy.transform.GetChild(i));

				// remove drop zones
				foreach (DropZone dropZone in child.GetComponentsInChildren<DropZone>(true))
                {
					if (GameObjectManager.isBound(dropZone.gameObject))
						GameObjectManager.unbind(dropZone.gameObject);
					dropZone.transform.SetParent(null);
					GameObject.Destroy(dropZone.gameObject);
				}
				//remove empty zones for BaseElements
				foreach (ReplacementSlot emptyZone in child.GetComponentsInChildren<ReplacementSlot>(true))
				{
					if (emptyZone.slotType == ReplacementSlot.SlotType.BaseElement) {
						if (GameObjectManager.isBound(emptyZone.gameObject))
							GameObjectManager.unbind(emptyZone.gameObject);
						emptyZone.transform.SetParent(null);
						GameObject.Destroy(emptyZone.gameObject);
					}
				}
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
		foreach (TMP_Dropdown drop in copyGO.GetComponentsInChildren<TMP_Dropdown>(true))
		{
			drop.interactable = isInteractable;
		}
		foreach (TMP_InputField input in copyGO.GetComponentsInChildren<TMP_InputField>(true))
		{
			input.interactable = isInteractable;
		}

		// Pour chaque bloc for
		foreach (ForControl forAct in copyGO.GetComponentsInChildren<ForControl>(true))
		{
			// Si activé, on note le nombre de tour de boucle à faire
			if (!isInteractable && !forAct.gameObject.GetComponent<WhileControl>())
			{
				try
				{
					forAct.nbFor = int.Parse(forAct.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text);
				} catch{
					forAct.nbFor = 0;
				}
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
				((WhileControl)forAct).condition = new List<ConditionItem>();
				conditionToStrings(forAct.gameObject.transform.Find("ConditionContainer").GetChild(0).gameObject, ((WhileControl)forAct).condition);

			}
			// On parcourt les éléments présent dans le block action
			foreach (BaseElement act in forAct.GetComponentsInChildren<BaseElement>(true))
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
		foreach (ForeverControl loopAct in copyGO.GetComponentsInChildren<ForeverControl>(true))
		{
			foreach (BaseElement act in loopAct.GetComponentsInChildren<BaseElement>(true))
			{
				if (!act.Equals(loopAct))
				{
					loopAct.firstChild = act.gameObject;
					break;
				}
			}
		}
		// Pour chaque block if
		foreach (IfControl ifAct in copyGO.GetComponentsInChildren<IfControl>(true))
		{
			// On traduit la condition en string
			ifAct.condition = new List<ConditionItem> ();
			conditionToStrings(ifAct.gameObject.transform.Find("ConditionContainer").GetChild(0).gameObject, ifAct.condition);

			GameObject thenContainer = ifAct.transform.Find("Container").gameObject;
			BaseElement firstThen = thenContainer.GetComponentInChildren<BaseElement>(true);
			if (firstThen)
				ifAct.firstChild = firstThen.gameObject;
			//Si c'est un elseAction
			if (ifAct is IfElseControl)
			{
				GameObject elseContainer = ifAct.transform.Find("ElseContainer").gameObject;
				BaseElement firstElse = elseContainer.GetComponentInChildren<BaseElement>(true);
				if (firstElse)
					((IfElseControl)ifAct).elseFirstChild = firstElse.gameObject;
			}
		}

		foreach (EventTrigger eventTrigger in copyGO.GetComponentsInChildren<EventTrigger>(true))
			Component.Destroy(eventTrigger);

		// On défini la couleur de l'action selon l'agent à qui appartiendra le script
		if (agentTag == "Drone") {
			foreach (BaseElement act in copyGO.GetComponentsInChildren<BaseElement>(true))
			{
				Selectable sel = act.GetComponent<Selectable>();
				sel.interactable = false;
				Color disabledColor = sel.colors.disabledColor;

				if (act.GetComponent<ControlElement>())
					foreach (Transform child in act.gameObject.transform)
					{
						Image childImg = child.GetComponent<Image>();
						if (child.name != "3DEffect" && childImg != null)
							childImg.color = disabledColor;
					}
			}
			foreach (BaseCondition act in copyGO.GetComponentsInChildren<BaseCondition>(true))
			{
				Selectable sel = act.GetComponent<Selectable>();
				sel.interactable = false;
				Color disabledColor = sel.colors.disabledColor;
				if (act.GetComponent<BaseOperator>())
					foreach (Transform child in act.gameObject.transform)
					{
						Image childImg = child.GetComponent<Image>();
						if (child.name != "3DEffect" && childImg != null)
							childImg.color = disabledColor;
					}
			}
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

			// Si c'est un block if on garde le container des actions (sans le emptyslot) mais la condition est traduite dans IfAction
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
	private static void conditionToStrings(GameObject condition, List<ConditionItem> chaine)
	{
		// Check if condition is a BaseCondition
		if (condition.GetComponent<BaseCondition>())
		{
			// On regarde si la condition reçue est un élément ou bien un opérator
			// Si c'est un élément, on le traduit en string et on le renvoie 
			if (condition.GetComponent<BaseCaptor>())
				chaine.Add(new ConditionItem("" + condition.GetComponent<BaseCaptor>().captorType, condition));
			else
			{
				BaseOperator bo;
				if (condition.TryGetComponent<BaseOperator>(out bo))
				{
					Transform conditionContainer = bo.transform.GetChild(1);
					// Si c'est une négation on met "!" puis on fait une récursive sur le container et on renvoie le tous traduit en string
					if (bo.operatorType == BaseOperator.OperatorType.NotOperator)
					{
						// On vérifie qu'il y a bien un élément présent, son container doit contenir 3 enfants (icone, une BaseCondition et le ReplacementSlot)
						if (conditionContainer.childCount == 3)
						{
							chaine.Add(new ConditionItem("NOT", bo.gameObject));
							conditionToStrings(conditionContainer.GetComponentInChildren<BaseCondition>(true).gameObject, chaine);
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
							chaine.Add(new ConditionItem("(", null));
							conditionToStrings(conditionContainer.GetChild(0).gameObject, chaine);
							chaine.Add(new ConditionItem("AND", bo.gameObject));
							conditionToStrings(conditionContainer.GetChild(3).gameObject, chaine);
							chaine.Add(new ConditionItem(")", null));
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
							chaine.Add(new ConditionItem("(", null));
							conditionToStrings(conditionContainer.GetChild(0).gameObject, chaine);
							chaine.Add(new ConditionItem("OR", bo.gameObject));
							conditionToStrings(conditionContainer.GetChild(3).gameObject, chaine);
							chaine.Add(new ConditionItem(")", null));
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
			// Pour la dernière instruction le next dépend du parent
			Transform parent = container.transform.parent;
			if (parent != null && parent.GetComponent<BaseElement>() != null) {
				if (parent.GetComponent<ForControl>() != null || parent.GetComponent<ForeverControl>() != null)
					lastChild.GetComponent<BaseElement>().next = parent.gameObject;
				else
					lastChild.GetComponent<BaseElement>().next = parent.GetComponent<BaseElement>().next;
			}
			// Sinon on ne fait rien et fin de la sequence
		}

		// parcourir tous les enfants jusqu'au dernier cette fois ci pour déclencher des appel récursifs pour les structures de contrôle
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

	public static void removeComments(XmlNode node)
	{
		for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
		{
			XmlNode child = node.ChildNodes[i];
			if (child.NodeType == XmlNodeType.Comment)
				node.RemoveChild(child);
			else
				removeComments(child);
		}
	}

	public enum ExportType
    {
		PseudoCode,
		XML
	}

	private static string indent(int level)
	{
		string result = "";
		for (int i = 0; i < level; i++)
			result += "\t";
		return result;
	}

	public static string exportBlockToString(Highlightable script, GameObject focusedArea = null, ExportType exportType = ExportType.PseudoCode, int indentLevel = 0)
	{
		if (script == null)
			return "";
		else
		{
			string export = "";
			// Cas d'une ACTION
			if (script is BasicAction)
			{
				DropZone dz = script.GetComponentInChildren<DropZone>(true);
				if (dz != null && dz.gameObject == focusedArea)
					export += exportType == ExportType.PseudoCode ? "#### " : indent(indentLevel) + "<!--####-->\n";

				if (exportType == ExportType.PseudoCode)
					export += (script.GetComponent<CurrentAction>() ? "* " : "") + (script as BasicAction).actionType.ToString() + ";";
				else
					export += indent(indentLevel) + "<action type=\"" + (script as BasicAction).actionType + "\"/>"+(script.GetComponent<CurrentAction>() ? "<!-- Current Action -->" : "") +"\n";
			}
			// Cas d'un CAPTOR
			else if (script is BaseCaptor)
			{
				ReplacementSlot localRS = script.GetComponent<ReplacementSlot>();
				if (localRS.gameObject == focusedArea)
					export += exportType == ExportType.PseudoCode ? "##" : indent(indentLevel) + "<!--##-->\n";

				if (exportType == ExportType.PseudoCode)
					export += (script as BaseCaptor).captorType.ToString();
				else
					export += indent(indentLevel) + "<captor type=\"" + (script as BaseCaptor).captorType + "\"/>\n";

				if (localRS.gameObject == focusedArea)
					export += exportType == ExportType.PseudoCode ? "##" : indent(indentLevel) + "<!--##-->\n";
			}
			// Cas d'un OPERATOR
			else if (script is BaseOperator)
			{
				ReplacementSlot localRS = script.GetComponent<ReplacementSlot>();
				if (localRS.gameObject == focusedArea)
					export += exportType == ExportType.PseudoCode ? "##" : indent(indentLevel) + "<!--##-->\n";
				BaseOperator ope = script as BaseOperator;
				Transform container = script.transform.Find("Container");
				// Cas du NOT
				if (ope.operatorType == BaseOperator.OperatorType.NotOperator)
				{
					export += exportType == ExportType.PseudoCode ? "NOT (" : indent(indentLevel) + "<not>\n";

					if (container.Find("EmptyConditionalSlot").GetComponent<ReplacementSlot>().gameObject == focusedArea)
						export += exportType == ExportType.PseudoCode ? "####" : indent(indentLevel+1) + "<!--####-->\n";
					else
						export += exportBlockToString(container.GetComponentInChildren<BaseCondition>(true), focusedArea, exportType, indentLevel + 1);

					export += exportType == ExportType.PseudoCode ? ")" : indent(indentLevel) + "</not>\n";
				}
				// Cas du AND et du OR
				else if (ope.operatorType == BaseOperator.OperatorType.AndOperator || ope.operatorType == BaseOperator.OperatorType.OrOperator)
				{
					export += exportType == ExportType.PseudoCode ? "(" : indent(indentLevel) + "<"+(ope.operatorType == BaseOperator.OperatorType.AndOperator ? "and" : "or") +">\n" + indent(indentLevel+1) + "<conditionLeft>\n" ;

					if (container.Find("EmptyConditionalSlot1").GetComponent<ReplacementSlot>().gameObject == focusedArea)
						export += exportType == ExportType.PseudoCode ? "####" : indent(indentLevel + 2) + "<!--####-->\n";
					else
						export += exportBlockToString(container.GetChild(0).GetComponentInChildren<BaseCondition>(true), focusedArea, exportType, indentLevel+2);

					export += exportType == ExportType.PseudoCode ? ") " + (ope.operatorType == BaseOperator.OperatorType.AndOperator ? "AND" : "OR") + " (" : indent(indentLevel + 1) + "</conditionLeft>\n" + indent(indentLevel + 1) + "<conditionRight>\n";

					if (container.Find("EmptyConditionalSlot2").GetComponent<ReplacementSlot>().gameObject == focusedArea)
						export += exportType == ExportType.PseudoCode ? "####" : indent(indentLevel + 2) + "<!--####-->\n";
					else
						export += exportBlockToString(container.GetChild(container.childCount - 2).GetComponentInChildren<BaseCondition>(true), focusedArea, exportType, indentLevel + 2);

					export += exportType == ExportType.PseudoCode ? ")" : indent(indentLevel + 1) + "</conditionRight>\n" + indent(indentLevel) + "</" + (ope.operatorType == BaseOperator.OperatorType.AndOperator ? "and" : "or") + ">\n";
				}
				if (localRS.gameObject == focusedArea)
					export += exportType == ExportType.PseudoCode ? "##" : indent(indentLevel) + "<!--##-->\n";
			}
			// Cas des structures de contrôle
			else if (script is ControlElement)
			{
				DropZone dz = script.transform.Find("Header").GetComponentInChildren<DropZone>(true);
				if (dz != null && dz.gameObject == focusedArea)
					export += exportType == ExportType.PseudoCode ? "#### " : indent(indentLevel) + "<!--####-->\n";
				ControlElement control = script as ControlElement;
				// Cas du WHILE
				if (script is WhileControl)
				{
					export += exportType == ExportType.PseudoCode ? "WHILE (" : indent(indentLevel) + "<while>\n" + indent(indentLevel+1) + "<condition>\n";

					if (script.transform.Find("ConditionContainer").Find("EmptyConditionalSlot").GetComponent<ReplacementSlot>().gameObject == focusedArea)
						export += exportType == ExportType.PseudoCode ? "####" : indent(indentLevel + 2) + "<!--####-->\n";
					else
						export += exportBlockToString(script.transform.Find("ConditionContainer").GetComponentInChildren<BaseCondition>(true), focusedArea, exportType, indentLevel + 2);

					export += exportType == ExportType.PseudoCode ? ") {" : indent(indentLevel + 1) + "</condition>\n" + indent(indentLevel + 1) + "<container>\n";
					indentLevel++; // on a un niveau de plus pour le While
				}
				// Cas du FOR
				else if (script is ForControl)
				{
					export += exportType == ExportType.PseudoCode ? "REPEAT ("+(script.gameObject == focusedArea ? "##" : "") : indent(indentLevel) + "<for nbFor=\"";

					export += (script as ForControl).nbFor;

					export += exportType == ExportType.PseudoCode ? (script.gameObject == focusedArea ? "##" : "") + ") {" : ("\">"+ (script.gameObject == focusedArea ? "<!--####-->" : "") + "\n");
				}
				// Cas du FOREVER
				else if (script is ForeverControl)
					export += exportType == ExportType.PseudoCode ? "FOREVER {" : indent(indentLevel) + "<forever>\n";
				// Cas du IF et du IF/ELSE
				else if (script is IfControl)
				{
					export += exportType == ExportType.PseudoCode ? "IF (" : indent(indentLevel) + (script is IfElseControl ? "<ifElse>\n" : "<if>\n") + indent(indentLevel+1) + "<condition>\n";

					if (script.transform.Find("ConditionContainer").Find("EmptyConditionalSlot").GetComponent<ReplacementSlot>().gameObject == focusedArea)
						export += exportType == ExportType.PseudoCode ? "####" : indent(indentLevel + 2) + "<!--####-->\n";
					else
						export += exportBlockToString(script.transform.Find("ConditionContainer").GetComponentInChildren<BaseCondition>(true), focusedArea, exportType, indentLevel + 2);

					export += exportType == ExportType.PseudoCode ? ") {" : indent(indentLevel + 1) + "</condition>\n" + indent(indentLevel + 1) + (script is IfElseControl ? "<thenContainer>\n" : "<container>\n");
					indentLevel++; // on a un niveau de plus pour le If et le IfElse
				}

				Transform container = script.transform.Find("Container");
				// parcourir tous les enfants et exclure les zone de drop
				for (int i = 0; i < container.childCount; i++)
					if (container.GetChild(i).GetComponent<ReplacementSlot>() == null)
						export += (exportType == ExportType.PseudoCode ? " " : "") + exportBlockToString(container.GetChild(i).GetComponent<BaseElement>(), focusedArea, exportType, indentLevel + 1);
				if (container.GetChild(container.childCount - 1).gameObject == focusedArea)
					export += exportType == ExportType.PseudoCode ? " ####" : indent(indentLevel + 1) + "<!--####-->\n";

				export += exportType == ExportType.PseudoCode ? " }" : script switch {
					WhileControl wc => indent(indentLevel) + "</container>\n" + indent(indentLevel-1) + "</while>\n",
					ForControl loop => indent(indentLevel) + "</for>\n",
					ForeverControl forever => indent(indentLevel) + "</forever>\n",
					IfElseControl ifelse => indent(indentLevel) + "</thenContainer>\n",
					IfControl ic => indent(indentLevel) + "</container>\n" + indent(indentLevel-1) + "</if>\n",
					_ => ""
				};

				if (script is IfElseControl)
				{
					export += exportType == ExportType.PseudoCode ? " ELSE {" : indent(indentLevel) + "<elseContainer>\n";

					Transform containerElse = script.transform.Find("ElseContainer");
					// parcourir tous les enfants et exclure les zone de drop
					for (int i = 0; i < containerElse.childCount; i++)
						if (containerElse.GetChild(i).GetComponent<ReplacementSlot>() == null)
							export += (exportType == ExportType.PseudoCode ? " " : "") + exportBlockToString(containerElse.GetChild(i).GetComponent<BaseElement>(), focusedArea, exportType, indentLevel+1);
					if (containerElse.GetChild(containerElse.childCount - 1).gameObject == focusedArea)
						export += exportType == ExportType.PseudoCode ? " ####" : indent(indentLevel + 1) + "<!--####-->\n";

					export += exportType == ExportType.PseudoCode ? " }" : indent(indentLevel) + "</elseContainer>\n" + indent(indentLevel - 1) + "</ifElse>\n";
				}
			}
			return export;
		}
	}

	public static void readXMLDialogs(XmlNode dialogs, List<Dialog> target)
	{
		foreach (XmlNode dialogXML in dialogs.ChildNodes)
		{
			Dialog dialog = new Dialog();
			if (dialogXML.Attributes.GetNamedItem("text") != null)
				dialog.text = dialogXML.Attributes.GetNamedItem("text").Value;
			if (dialogXML.Attributes.GetNamedItem("img") != null)
				dialog.img = dialogXML.Attributes.GetNamedItem("img").Value;
			if (dialogXML.Attributes.GetNamedItem("imgHeight") != null)
				dialog.imgHeight = float.Parse(dialogXML.Attributes.GetNamedItem("imgHeight").Value);
			if (dialogXML.Attributes.GetNamedItem("camX") != null)
				dialog.camX = int.Parse(dialogXML.Attributes.GetNamedItem("camX").Value);
			if (dialogXML.Attributes.GetNamedItem("camY") != null)
				dialog.camY = int.Parse(dialogXML.Attributes.GetNamedItem("camY").Value);
			if (dialogXML.Attributes.GetNamedItem("sound") != null)
				dialog.sound = dialogXML.Attributes.GetNamedItem("sound").Value;
			if (dialogXML.Attributes.GetNamedItem("video") != null)
				dialog.video = dialogXML.Attributes.GetNamedItem("video").Value;
			if (dialogXML.Attributes.GetNamedItem("enableInteraction") != null)
				dialog.enableInteraction = int.Parse(dialogXML.Attributes.GetNamedItem("enableInteraction").Value) == 1;
			if (dialogXML.Attributes.GetNamedItem("briefingType") != null)
				dialog.briefingType = int.Parse(dialogXML.Attributes.GetNamedItem("briefingType").Value);
			target.Add(dialog);
		}
	}

	public static IEnumerator pulseItem(GameObject newItem)
	{
		newItem.transform.localScale = new Vector3(1, 1, 1);
		float initScaleX = newItem.transform.localScale.x;
		newItem.transform.localScale = new Vector3(newItem.transform.localScale.x + 0.3f, newItem.transform.localScale.y, newItem.transform.localScale.z);
		while (newItem != null  && newItem.transform.localScale.x > initScaleX) // newItem peut être nul si le GameObject est supprimé avant la fin de la coroutine
		{
			newItem.transform.localScale = new Vector3(newItem.transform.localScale.x - 0.01f, newItem.transform.localScale.y, newItem.transform.localScale.z);
			yield return null;
		}
		if (newItem != null) // newItem peut être nul si le GameObject est supprimé avant la fin de la coroutine
			newItem.transform.localScale = new Vector3(1, 1, 1);
	}

	public static bool isCompetencyMatchWithLevel(Competency competency, XmlDocument level)
	{
		// check all filters of the competency
		Dictionary<string, List<XmlNode>> filtersState = new Dictionary<string, List<XmlNode>>();
		foreach (RawFilter filter in competency.filters)
		{

			if (filtersState.ContainsKey(filter.label))
			{
				// if a filter with this label is defined and no XmlNode identified, useless to check this new one
				if (filtersState[filter.label].Count == 0)
					continue;
			}
			else
			{
				// init this filter with all XmlNode of required tag
				List<XmlNode> tagList = new List<XmlNode>();
				foreach (XmlNode tag in level.GetElementsByTagName(filter.tag))
					tagList.Add(tag);
				filtersState.Add(filter.label, tagList);
			}

			// check if this filter is true
			List<XmlNode> tags = filtersState[filter.label];
			foreach (RawConstraint constraint in filter.constraints)
			{
				int levelAttrValue;
				switch (constraint.constraint)
				{
					// Check if the value of an attribute of the tag is equal to a given value
					case "=":
						for (int t = tags.Count - 1; t >= 0; t--)
							if (tags[t].Attributes.GetNamedItem(constraint.attribute) == null || tags[t].Attributes.GetNamedItem(constraint.attribute).Value != constraint.value)
								tags.RemoveAt(t);
						break;
					// Check if the value of an attribute of the tag is not equal to a given value
					case "<>":
						for (int t = tags.Count - 1; t >= 0; t--)
							if (tags[t].Attributes.GetNamedItem(constraint.attribute) == null || tags[t].Attributes.GetNamedItem(constraint.attribute).Value == constraint.value)
								tags.RemoveAt(t);
						break;
					// Check if the value of an attribute of the tag is greater than a given value (for limit attribute consider -1 as infinite value)
					case ">":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(constraint.attribute) == null)
								tags.RemoveAt(t);
							else
							{
								try
								{
									levelAttrValue = int.Parse(tags[t].Attributes.GetNamedItem(constraint.attribute).Value);
									if (levelAttrValue <= int.Parse(constraint.value) && (constraint.attribute != "limit" || levelAttrValue != -1)) // because -1 means infinity for block limit
										tags.RemoveAt(t);
								}
								catch
								{
									tags.RemoveAt(t);
								}
							}
						}
						break;
					// Check if the value of an attribute of the tag is smaller than a given value (for limit attribute consider -1 as infinite value)
					case "<":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(constraint.attribute) == null)
								tags.RemoveAt(t);
							else
							{
								try
								{
									levelAttrValue = int.Parse(tags[t].Attributes.GetNamedItem(constraint.attribute).Value);
									if (levelAttrValue >= int.Parse(constraint.value) || (constraint.attribute == "limit" && levelAttrValue == -1)) // because -1 means infinity for block limit
										tags.RemoveAt(t);
								}
								catch
								{
									tags.RemoveAt(t);
								}
							}
						}
						break;
					// Check if the value of an attribute of the tag is greater than or equal a given value (for limit attribute consider -1 as infinite value)
					case ">=":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(constraint.attribute) == null)
								tags.RemoveAt(t);
							else
							{
								try
								{
									levelAttrValue = int.Parse(tags[t].Attributes.GetNamedItem(constraint.attribute).Value);
									if (levelAttrValue < int.Parse(constraint.value) && (constraint.attribute != "limit" || levelAttrValue != -1)) // because -1 means infinity for block limit
										tags.RemoveAt(t);
								}
								catch
								{
									tags.RemoveAt(t);
								}
							}
						}
						break;
					// Check if the value of an attribute of the tag is smaller than or equal a given value (for limit attribute consider -1 as infinite value)
					case "<=":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(constraint.attribute) == null)
								tags.RemoveAt(t);
							else
							{
								try
								{
									levelAttrValue = int.Parse(tags[t].Attributes.GetNamedItem(constraint.attribute).Value);
									if (levelAttrValue > int.Parse(constraint.value) || (constraint.attribute == "limit" && levelAttrValue == -1)) // because -1 means infinity for block limit
										tags.RemoveAt(t);
								}
								catch
								{
									tags.RemoveAt(t);
								}
							}
						}
						break;
					// Check if the attribute of the tag is included inside a given value
					case "isIncludedIn":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(constraint.attribute) == null || !constraint.value.Contains(tags[t].Attributes.GetNamedItem(constraint.attribute).Value))
								tags.RemoveAt(t);
						}
						break;
					// Check if the value of an attribute of a tag is equal to the value of an attribute of another tag
					case "sameValue":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (tags[t].Attributes.GetNamedItem(constraint.attribute) == null)
								tags.RemoveAt(t);
							else
							{
								bool found = false;
								foreach (XmlNode node in tags[t].OwnerDocument.GetElementsByTagName(constraint.tag2))
								{
									if (node != tags[t] && node.Attributes.GetNamedItem(constraint.attribute2) != null && node.Attributes.GetNamedItem(constraint.attribute2).Value == tags[t].Attributes.GetNamedItem(constraint.attribute).Value)
									{
										found = true;
										break;
									}
								}
								if (!found)
									tags.RemoveAt(t);
							}
						}
						break;
					// Check if a tag contains at least one child
					case "hasChild":
						for (int t = tags.Count - 1; t >= 0; t--)
						{
							if (!tags[t].HasChildNodes)
								tags.RemoveAt(t);
						}
						break;
				}
			}
		}
		// check the rule (combination of filters)
		string rule = competency.rule;
		foreach (string key in filtersState.Keys)
		{
			rule = rule.Replace(key, "" + filtersState[key].Count);
		}
		DataTable dt = new DataTable();
		if (rule != "")
			return (bool)dt.Compute(rule, "");
		else
			return false;
	}

	// used for localization process to integrate inside expression some data
	public static string getFormatedText(string expression, params object[] data)
	{
		for (int i = 0; i < data.Length; i++)
			expression = expression.Replace("#" + i + "#", data[i].ToString());
		return expression;
	}

	public static string extractLocale(string content)
    {
		if (content == null) return "";
		string localKey = LocalizationSettings.Instance.GetSelectedLocale().Identifier.Code;
		if (content.Contains("[" + localKey + "]") && content.Contains("[/" + localKey + "]"))
        {
			int start = content.IndexOf("[" + localKey + "]") + localKey.Length + 2;
			int length = content.IndexOf("[/" + localKey + "]") - start;
			return content.Substring(start, length);
		}
		else
			return content;
	}

	/// <summary>
	/// Called when trying to save
	/// </summary>
	public static bool CheckSaveNameValidity(string nameCandidate)
	{
		bool isValid = nameCandidate != "";

		if (isValid)
		{
			char[] chars = Path.GetInvalidFileNameChars();

			foreach (char c in chars)
				if (nameCandidate.IndexOf(c) != -1)
				{
					isValid = false;
					break;
				}
		}
		return isValid;
	}

	public static string extractFileName(string uri)
    {
		if (uri.Contains(new Uri(Application.persistentDataPath + "/").AbsoluteUri))
			return uri.Replace(new Uri(Application.persistentDataPath + "/").AbsoluteUri, "");
		else if(uri.Contains(new Uri(Application.streamingAssetsPath + "/").AbsoluteUri))
			return uri.Replace(new Uri(Application.streamingAssetsPath + "/").AbsoluteUri, "");
		else 
			return uri;
	}
}