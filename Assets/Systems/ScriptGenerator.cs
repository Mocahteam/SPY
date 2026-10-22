using UnityEngine;
using FYFY;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Read XML Scripts
/// </summary>
public class ScriptGenerator : FSystem {

	public static ScriptGenerator instance;

	private readonly Family f_drone = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef)), new AnyOfTags("Drone")); // On récupére les agents pouvant être édités
	private readonly Family f_draggableElement = FamilyManager.getFamily(new AnyOfComponents(typeof(ElementToDrag)));
	private readonly Family f_decodeXMLScript = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptToLoad)));

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
	}

    protected override void onProcess(int familiesUpdateCount)
    {
        foreach(GameObject go in f_decodeXMLScript)
        {
			ScriptToLoad [] scriptsToLoad = go.GetComponents<ScriptToLoad>();
			foreach (ScriptToLoad stl in scriptsToLoad)
			{
				ReadXMLScript(stl.scriptNode, stl.scriptName, stl.editMode, stl.type);
				GameObjectManager.removeComponent(stl);
			}
        }
    }

    // Lit le XML d'un script est génère les game objects des instructions
    private void ReadXMLScript(XmlNode scriptNode, string name, UIRootContainer.EditMode editMode, UIRootContainer.SolutionType type)
	{
		if (scriptNode != null)
		{
			// Rechercher un drone associé à ce script
			bool droneFound = false;
			foreach (GameObject drone in f_drone)
			{
				ScriptRef scriptRef = drone.GetComponent<ScriptRef>();
				if (scriptRef.executablePanel.GetComponentInChildren<UIRootExecutor>(true).scriptName == name)
				{
					List<GameObject> script = new List<GameObject>();
					foreach (XmlNode actionNode in scriptNode.ChildNodes)
						script.Add(ReadXMLInstruction(actionNode));
					GameObject tmpContainer = GameObject.Instantiate(scriptRef.executableScript);
					foreach (GameObject go in script)
						go.transform.SetParent(tmpContainer.transform, false); //add actions to container
					UtilityGame.fillExecutablePanel(tmpContainer, scriptRef.executableScript, drone.tag);
					// bind all child (except the first "header")
					for (int i = 1; i < scriptRef.executableScript.transform.childCount; i++)
						GameObjectManager.bind(scriptRef.executableScript.transform.GetChild(i).gameObject);
					scriptRef.executablePanel.SetActive(true);
					GameObject.Destroy(tmpContainer);
					droneFound = true;
				}
			}
			if (!droneFound)
			{
				List<GameObject> script = new List<GameObject>();
				foreach (XmlNode actionNode in scriptNode.ChildNodes)
					script.Add(ReadXMLInstruction(actionNode));
				GameObjectManager.addComponent<AddSpecificContainer>(MainLoop.instance.gameObject, new { title = name, editState = editMode, typeState = type, script = script });
			}
		}
	}

	private GameObject GetLibraryItemByName(string itemName)
	{
		foreach (GameObject item in f_draggableElement)
			if (item.name == itemName)
				return item;
		return null;
	}

	private void ProcessXMLInstruction(Transform gameContainer, XmlNode xmlContainer)
	{
		// The first child of a control container is an emptySolt
		GameObject emptySlot = gameContainer.GetChild(0).gameObject;
		foreach (XmlNode eleNode in xmlContainer.ChildNodes)
			UtilityGame.addItemOnDropArea(ReadXMLInstruction(eleNode), emptySlot);
	}

	// Transforme le noeud d'action XML en gameObject élément/opérator
	private GameObject ReadXMLCondition(XmlNode conditionNode)
	{
		ReplacementSlot[] slots;

		string libraryId = conditionNode.Name switch
		{
			"and" => "AndOperator",
			"or" => "OrOperator",
			"not" => "NotOperator",
			"captor" => conditionNode.Attributes.GetNamedItem("type").Value,
			_ => "Undef"
		};

		GameObject obj = PreprocessXML(libraryId, conditionNode);

        switch (conditionNode.Name)
		{
			case "and":
			case "or":
				slots = obj.GetComponentsInChildren<ReplacementSlot>(true);
				if (conditionNode.HasChildNodes)
				{
					GameObject emptyZone = null;
					foreach (XmlNode node in conditionNode.ChildNodes)
					{
						if (node.Name == "conditionLeft")
							// The Left slot is the second ReplacementSlot (first is the And/Or operator)
							emptyZone = slots[1].gameObject;
						if (node.Name == "conditionRight")
							// The Right slot is the third ReplacementSlot
							emptyZone = slots[2].gameObject;
						if (emptyZone != null && node.HasChildNodes)
						{
							// Parse xml condition
							GameObject child = ReadXMLCondition(node.FirstChild);
							// Add child to empty zone
							UtilityGame.addItemOnDropArea(child, emptyZone);
						}
						emptyZone = null;
					}
				}
				break;

			case "not":
				if (conditionNode.HasChildNodes)
				{
					GameObject emptyZone = obj.transform.Find("Container").GetChild(1).gameObject;
					GameObject child = ReadXMLCondition(conditionNode.FirstChild);
					// Add child to empty zone
					UtilityGame.addItemOnDropArea(child, emptyZone);
				}
				break;
        }

		return obj;
	}

	// Transforme le noeud d'action XML en gameObject
	private GameObject ReadXMLInstruction(XmlNode actionNode)
	{
		Transform conditionContainer;
		Transform firstContainerBloc;
		Transform secondContainerBloc;

		string libraryId = actionNode.Name switch{
			"if" => "IfThen",
			"ifElse" => "IfElse",
			"for" => "ForLoop",
			"while" => "While",
			"forever" => "Forever",
			"action" => actionNode.Attributes.GetNamedItem("type").Value,
			_ => "Undef"
		};

        GameObject obj = PreprocessXML(libraryId, actionNode);

        switch (actionNode.Name)
		{
			case "if":
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
							GameObject child = ReadXMLCondition(containerNode.FirstChild);
							// Add child to empty zone
							UtilityGame.addItemOnDropArea(child, emptyZone);
						}
					}
					else if (containerNode.Name == "container")
					{
						if (containerNode.HasChildNodes)
							ProcessXMLInstruction(firstContainerBloc, containerNode);
					}
				}
				break;

			case "ifElse":
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
							GameObject child = ReadXMLCondition(containerNode.FirstChild);
							// Add child to empty zone
							UtilityGame.addItemOnDropArea(child, emptyZone);
						}
					}
					else if (containerNode.Name == "thenContainer")
					{
						if (containerNode.HasChildNodes)
							ProcessXMLInstruction(firstContainerBloc, containerNode);
					}
					else if (containerNode.Name == "elseContainer")
					{
						if (containerNode.HasChildNodes)
							ProcessXMLInstruction(secondContainerBloc, containerNode);
					}
				}
				break;

			case "for":
				firstContainerBloc = obj.transform.Find("Container");
				BaseElement action = obj.GetComponent<ForControl>();

				((ForControl)action).nbFor = int.Parse(actionNode.Attributes.GetNamedItem("nbFor").Value);
				TMP_InputField input = obj.GetComponentInChildren<TMP_InputField>(true);
				input.text = ((ForControl)action).nbFor.ToString();

                Locker inputLocker = input.GetComponent<Locker>();
                inputLocker.locked = actionNode.Attributes.GetNamedItem("inputLocked") != null && actionNode.Attributes.GetNamedItem("inputLocked").Value == "True";
				UtilityGame.syncLockerUI(inputLocker);
				input.interactable = !inputLocker.locked;

                if (actionNode.HasChildNodes)
					ProcessXMLInstruction(firstContainerBloc, actionNode);
				break;

			case "while":
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
							GameObject child = ReadXMLCondition(containerNode.FirstChild);
							// Add child to empty zone
							UtilityGame.addItemOnDropArea(child, emptyZone);
						}
					}
					else if (containerNode.Name == "container")
					{
						if (containerNode.HasChildNodes)
							ProcessXMLInstruction(firstContainerBloc, containerNode);
					}
				}
				break;

			case "forever":
				firstContainerBloc = obj.transform.Find("Container");

				if (actionNode.HasChildNodes)
					ProcessXMLInstruction(firstContainerBloc, actionNode);
				break;
		}

		return obj;
	}

	private GameObject PreprocessXML(string libraryId, XmlNode node)
	{
		GameObject obj = null;
        // Vérifier que ce node est connu pour la gestion des blocs disponibles, si non le définir
        if (!gameData.actionBlockLimit.ContainsKey(libraryId))
            gameData.actionBlockLimit[libraryId] = 0;

		if (libraryId != "Undef")
		{
			// créer le bloc et définir s'il est interactable
			bool isInteractable = (node.Attributes.GetNamedItem("locked") == null || node.Attributes.GetNamedItem("locked").Value == "False") && (gameData.dragDropEnabled || SceneManager.GetActiveScene().name == "MissionEditor");
            obj = UtilityGame.createEditableBlockFromLibrary(GetLibraryItemByName(libraryId), mainCanvas, isInteractable);
		}

        return obj;
    }
}
