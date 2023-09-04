using UnityEngine;
using FYFY;
using System.Collections.Generic;
using System.Xml;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Read XML Scripts
/// </summary>
public class ScriptGenerator : FSystem {

	public static ScriptGenerator instance;

	private Family f_drone = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef)), new AnyOfTags("Drone")); // On récupére les agents pouvant être édités
	private Family f_draggableElement = FamilyManager.getFamily(new AnyOfComponents(typeof(ElementToDrag)));
	private Family f_decodeXMLScript = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptToLoad)));
	private Family f_ScriptEditorCanvas = FamilyManager.getFamily(new AnyOfTags("ScriptEditorCanvas"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	private HashSet<string> scriptNameUsed = new HashSet<string>();
	private GameData gameData;

	public GameObject mainCanvas;

	public ScriptGenerator()
	{
		instance = this;
	}

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();

		f_ScriptEditorCanvas.addEntryCallback(delegate (GameObject go)
		{
			GameObjectManager.addComponent<RefreshSizeOfEditableContainer>(go);
		});
	}

    protected override void onProcess(int familiesUpdateCount)
    {
        foreach(GameObject go in f_decodeXMLScript)
        {
			ScriptToLoad [] scriptsToLoad = go.GetComponents<ScriptToLoad>();
			foreach (ScriptToLoad stl in scriptsToLoad)
			{
				readXMLScript(stl.scriptNode, stl.scriptName, stl.editMode, stl.type);
				GameObjectManager.removeComponent(stl);
			}
        }
    }

    // Lit le XML d'un script est génère les game objects des instructions
    private void readXMLScript(XmlNode scriptNode, string name, UIRootContainer.EditMode editMode, UIRootContainer.SolutionType type)
	{
		if (scriptNode != null)
		{
			// Look for another script with the same name. If one already exists, we don't create one more.
			if (!scriptNameUsed.Contains(name))
			{
				// Rechercher un drone associé à ce script
				bool droneFound = false;
				foreach (GameObject drone in f_drone)
				{
					ScriptRef scriptRef = drone.GetComponent<ScriptRef>();
					if (scriptRef.executablePanel.transform.Find("Header").Find("agentName").GetComponent<TMP_InputField>().text == name)
					{
						List<GameObject> script = new List<GameObject>();
						foreach (XmlNode actionNode in scriptNode.ChildNodes)
							script.Add(readXMLInstruction(actionNode));
						GameObject tmpContainer = GameObject.Instantiate(scriptRef.executableScript);
						foreach (GameObject go in script)
							go.transform.SetParent(tmpContainer.transform, false); //add actions to container
						Utility.fillExecutablePanel(tmpContainer, scriptRef.executableScript, drone.tag);
						// bind all child
						foreach (Transform child in scriptRef.executableScript.transform)
							GameObjectManager.bind(child.gameObject);
						// On fait apparaitre le panneau du robot
						scriptRef.executablePanel.transform.Find("Header").Find("Toggle").GetComponent<Toggle>().isOn = true;
						GameObjectManager.setGameObjectState(scriptRef.executablePanel, true);
						GameObject.Destroy(tmpContainer);
						droneFound = true;
					}
				}
				if (!droneFound)
				{
					List<GameObject> script = new List<GameObject>();
					foreach (XmlNode actionNode in scriptNode.ChildNodes)
						script.Add(readXMLInstruction(actionNode));
					GameObjectManager.addComponent<AddSpecificContainer>(MainLoop.instance.gameObject, new { title = name, editState = editMode, typeState = type, script = script });
				}
			}
			else
			{
				Debug.LogWarning("Script \"" + name + "\" not created because another one already exists. Only one script with the same name is possible.");
			}
		}
	}

	private GameObject getLibraryItemByName(string itemName)
	{
		foreach (GameObject item in f_draggableElement)
			if (item.name == itemName)
				return item;
		return null;
	}

	private void processXMLInstruction(Transform gameContainer, XmlNode xmlContainer)
	{
		// The first child of a control container is an emptySolt
		GameObject emptySlot = gameContainer.GetChild(0).gameObject;
		foreach (XmlNode eleNode in xmlContainer.ChildNodes)
			Utility.addItemOnDropArea(readXMLInstruction(eleNode), emptySlot);
	}

	// Transforme le noeud d'action XML en gameObject élément/opérator
	private GameObject readXMLCondition(XmlNode conditionNode)
	{
		GameObject obj = null;
		ReplacementSlot[] slots = null;
		switch (conditionNode.Name)
		{
			case "and":
				obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName("AndOperator"), mainCanvas);
				slots = obj.GetComponentsInChildren<ReplacementSlot>(true);
				if (conditionNode.HasChildNodes)
				{
					GameObject emptyZone = null;
					foreach (XmlNode andNode in conditionNode.ChildNodes)
					{
						if (andNode.Name == "conditionLeft")
							// The Left slot is the second ReplacementSlot (first is the And operator)
							emptyZone = slots[1].gameObject;
						if (andNode.Name == "conditionRight")
							// The Right slot is the third ReplacementSlot
							emptyZone = slots[2].gameObject;
						if (emptyZone != null && andNode.HasChildNodes)
						{
							// Parse xml condition
							GameObject child = readXMLCondition(andNode.FirstChild);
							// Add child to empty zone
							Utility.addItemOnDropArea(child, emptyZone);
						}
						emptyZone = null;
					}
				}
				break;

			case "or":
				obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName("OrOperator"), mainCanvas);
				slots = obj.GetComponentsInChildren<ReplacementSlot>(true);
				if (conditionNode.HasChildNodes)
				{
					GameObject emptyZone = null;
					foreach (XmlNode orNode in conditionNode.ChildNodes)
					{
						if (orNode.Name == "conditionLeft")
							// The Left slot is the second ReplacementSlot (first is the And operator)
							emptyZone = slots[1].gameObject;
						if (orNode.Name == "conditionRight")
							// The Right slot is the third ReplacementSlot
							emptyZone = slots[2].gameObject;
						if (emptyZone != null && orNode.HasChildNodes)
						{
							// Parse xml condition
							GameObject child = readXMLCondition(orNode.FirstChild);
							// Add child to empty zone
							Utility.addItemOnDropArea(child, emptyZone);
						}
						emptyZone = null;
					}
				}
				break;

			case "not":
				obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName("NotOperator"), mainCanvas);
				if (conditionNode.HasChildNodes)
				{
					GameObject emptyZone = obj.transform.Find("Container").GetChild(1).gameObject;
					GameObject child = readXMLCondition(conditionNode.FirstChild);
					// Add child to empty zone
					Utility.addItemOnDropArea(child, emptyZone);
				}
				break;
			case "captor":
				obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName(conditionNode.Attributes.GetNamedItem("type").Value), mainCanvas);
				break;
		}

		if (!gameData.dragDropEnabled)
		{
			Selectable sel = obj.GetComponent<Selectable>();
			sel.interactable = false;
			Color disabledColor = sel.colors.disabledColor;

			if (obj.GetComponent<BaseOperator>())
				foreach (Transform child in obj.gameObject.transform)
				{
					Image childImg = child.GetComponent<Image>();
					if (child.name != "3DEffect" && childImg != null)
						childImg.color = disabledColor;
				}
		}

		return obj;
	}

	// Transforme le noeud d'action XML en gameObject
	private GameObject readXMLInstruction(XmlNode actionNode)
	{
		GameObject obj = null;
		Transform conditionContainer = null;
		Transform firstContainerBloc = null;
		Transform secondContainerBloc = null;
		switch (actionNode.Name)
		{
			case "if":
				obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName("IfThen"), mainCanvas);

				conditionContainer = obj.transform.Find("ConditionContainer");
				firstContainerBloc = obj.transform.Find("Container");

				// On ajoute les éléments enfants dans les bons containers
				foreach (XmlNode containerNode in actionNode.ChildNodes)
				{
					// Ajout des conditions
					if (containerNode.Name == "condition")
					{
						if (containerNode.HasChildNodes)
						{
							// The first child of the conditional container of a If action contains the ReplacementSlot
							GameObject emptyZone = conditionContainer.GetChild(0).gameObject;
							// Parse xml condition
							GameObject child = readXMLCondition(containerNode.FirstChild);
							// Add child to empty zone
							Utility.addItemOnDropArea(child, emptyZone);
						}
					}
					else if (containerNode.Name == "container")
					{
						if (containerNode.HasChildNodes)
							processXMLInstruction(firstContainerBloc, containerNode);
					}
				}
				break;

			case "ifElse":
				obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName("IfElse"), mainCanvas);
				conditionContainer = obj.transform.Find("ConditionContainer");
				firstContainerBloc = obj.transform.Find("Container");
				secondContainerBloc = obj.transform.Find("ElseContainer");

				// On ajoute les éléments enfants dans les bons containers
				foreach (XmlNode containerNode in actionNode.ChildNodes)
				{
					// Ajout des conditions
					if (containerNode.Name == "condition")
					{
						if (containerNode.HasChildNodes)
						{
							// The first child of the conditional container of a IfElse action contains the ReplacementSlot
							GameObject emptyZone = conditionContainer.GetChild(0).gameObject;
							// Parse xml condition
							GameObject child = readXMLCondition(containerNode.FirstChild);
							// Add child to empty zone
							Utility.addItemOnDropArea(child, emptyZone);
						}
					}
					else if (containerNode.Name == "thenContainer")
					{
						if (containerNode.HasChildNodes)
							processXMLInstruction(firstContainerBloc, containerNode);
					}
					else if (containerNode.Name == "elseContainer")
					{
						if (containerNode.HasChildNodes)
							processXMLInstruction(secondContainerBloc, containerNode);
					}
				}
				break;

			case "for":
				obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName("ForLoop"), mainCanvas);
				firstContainerBloc = obj.transform.Find("Container");
				BaseElement action = obj.GetComponent<ForControl>();

				((ForControl)action).nbFor = int.Parse(actionNode.Attributes.GetNamedItem("nbFor").Value);
				obj.transform.GetComponentInChildren<TMP_InputField>().text = ((ForControl)action).nbFor.ToString();

				if (actionNode.HasChildNodes)
					processXMLInstruction(firstContainerBloc, actionNode);
				break;

			case "while":
				obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName("While"), mainCanvas);
				firstContainerBloc = obj.transform.Find("Container");
				conditionContainer = obj.transform.Find("ConditionContainer");

				// On ajoute les éléments enfants dans les bons containers
				foreach (XmlNode containerNode in actionNode.ChildNodes)
				{
					// Ajout des conditions
					if (containerNode.Name == "condition")
					{
						if (containerNode.HasChildNodes)
						{
							// The first child of the conditional container of a While action contains the ReplacementSlot
							GameObject emptyZone = conditionContainer.GetChild(0).gameObject;
							// Parse xml condition
							GameObject child = readXMLCondition(containerNode.FirstChild);
							// Add child to empty zone
							Utility.addItemOnDropArea(child, emptyZone);
						}
					}
					else if (containerNode.Name == "container")
					{
						if (containerNode.HasChildNodes)
							processXMLInstruction(firstContainerBloc, containerNode);
					}
				}
				break;

			case "forever":
				obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName("Forever"), mainCanvas);
				firstContainerBloc = obj.transform.Find("Container");

				if (actionNode.HasChildNodes)
					processXMLInstruction(firstContainerBloc, actionNode);
				break;
			case "action":
				obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName(actionNode.Attributes.GetNamedItem("type").Value), mainCanvas);
				break;
		}

		if (!gameData.dragDropEnabled)
		{
			Selectable sel = obj.GetComponent<Selectable>();
			sel.interactable = false;
			Color disabledColor = sel.colors.disabledColor;

			if (obj.GetComponent<ControlElement>())
				foreach (Transform child in obj.gameObject.transform)
				{
					Image childImg = child.GetComponent<Image>();
					if (child.name != "3DEffect" && childImg != null)
						childImg.color = disabledColor;
				}
		}

		return obj;
	}
}
