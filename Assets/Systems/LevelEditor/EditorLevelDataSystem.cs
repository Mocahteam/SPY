using UnityEngine;
using FYFY;
using UnityEngine.UI;
using System.Xml;
using System.Collections.Generic;
using TMPro;

public class EditorLevelDataSystem : FSystem {
	public Family f_editorblocks = FamilyManager.getFamily(new AllOfComponents(typeof(EditorBlockData)));
	private Family f_newLoading = FamilyManager.getFamily(new AllOfComponents(typeof(NewLevelToLoad)));

	public static EditorLevelDataSystem instance;
	
	public LevelData levelData;
	public GameObject scrollViewContent;
	public GameObject executionLimitContainer;
	public Toggle dragAndDropToggle;
	public Toggle fogToggle;
	public Toggle hideExitsToggle;
	public TMP_InputField score2Input;
	public TMP_InputField score3Input;

	private GameData gameData;

	public EditorLevelDataSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart(){

		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();
		
		resetMetaData();

		f_newLoading.addEntryCallback(loadLevel);
	}

	public void resetMetaData()
	{
		executionLimitContainer.GetComponentInChildren<Toggle>(true).isOn = false;
		dragAndDropToggle.isOn = true;
		fogToggle.isOn = false;
		hideExitsToggle.isOn = false;

		foreach(GameObject go in f_editorblocks)
        {
			go.transform.Find("HideToggle").GetComponent<Toggle>().isOn = true;
		}
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
	}

	private void loadLevel(GameObject go)
	{
		resetMetaData();

		string levelKey = go.GetComponent<NewLevelToLoad>().levelKey;
		XmlDocument doc = gameData.levels[levelKey].OwnerDocument;
		// remove comments
		Utility.removeComments(doc);
		XmlNode root = doc.ChildNodes[1];

		// check if dragdropDisabled node exists and set levelData accordingly
		dragAndDropToggle.isOn = doc.GetElementsByTagName("dragdropDisabled").Count == 0;
		fogToggle.isOn = doc.GetElementsByTagName("fog").Count > 0;
		hideExitsToggle.isOn = doc.GetElementsByTagName("hideExits").Count > 0;

		foreach (XmlNode child in root.ChildNodes)
		{
			switch (child.Name)
			{
				case "dialogs":
					// dialogs are defined in the scenario editor
					break;
				case "executionLimit":
					executionLimitContainer.GetComponentInChildren<Toggle>(true).isOn = true;
					executionLimitContainer.GetComponentInChildren<TMP_InputField>(true).text = child.Attributes.GetNamedItem("amount")?.Value;
					break;
				case "blockLimits":
					// stack all EditorBlockData by name
					Dictionary<string, EditorBlockData> name2EditorBlockData = new Dictionary<string, EditorBlockData>();
					foreach (GameObject editorBlock in f_editorblocks)
					{
						EditorBlockData component = editorBlock.GetComponent<EditorBlockData>();
						name2EditorBlockData[component.blockName] = component;
					}
					// set loaded data to game objects
					foreach (XmlNode limitNode in child.ChildNodes)
					{
						string blockName = limitNode.Attributes.GetNamedItem("blockType").Value;
						int blockAmount = int.Parse(limitNode.Attributes.GetNamedItem("limit").Value);
						EditorBlockData blockData = name2EditorBlockData[blockName];
						if (blockAmount != 0)
						{
							// blockAmount != 0 => means block is not hiden
							blockData.transform.Find("HideToggle").GetComponent<Toggle>().isOn = false;
							// enable LimitBlock
							blockData.transform.Find("LimitToggle").GetComponent<Toggle>().interactable = true;
							// sync LimitBlock with data
							blockData.transform.Find("LimitToggle").GetComponent<Toggle>().isOn = blockAmount <= 0;
							// enable LimitField depending on LimitBlock state
							blockData.transform.Find("LimitField").GetComponent<TMP_InputField>().interactable = blockAmount > 0;
						} // else nothing to do default data is hiden block

						// in all cases set the inputfield to the file data
						blockData.transform.Find("LimitField").GetComponent<TMP_InputField>().text = blockAmount.ToString();
					}
					break;
				case "score":
					score2Input.text = child.Attributes.GetNamedItem("twoStars")?.Value;
					score3Input.text = child.Attributes.GetNamedItem("threeStars")?.Value;
					break;
			}
		}

		/*
		if (File.Exists(levelData.filePath))
		{
			var xmlScriptGenerator = new XmlScriptGenerator(mainCanvas);
			var xmlFile = XDocument.Load(levelData.filePath);
			var scripts = new List<XElement>();
			var robots = new Dictionary<string, Tuple<int, int>>();
			foreach (var element in xmlFile.Descendants("level").Elements())
			{
				Tuple<int, int> position;
				ObjectDirection orientation;
				int slotId;
				string associatedScriptName;
				switch (element.Name.LocalName)
				{
					
					case "script": => à passer dans le système qui gère les scripts ???
						scripts.Add(element);
						var scriptName = element.Attribute("outputLine").Value;
						var editMode = ScriptEditMode.Locked;
						var scriptType = ScriptType.Undefined;
						if (element.Attribute("editMode") != null)
						{
							editMode = (ScriptEditMode) int.Parse(element.Attribute("editMode").Value);
						}
						if (element.Attribute("type") != null)
						{
							scriptType = (ScriptType) int.Parse(element.Attribute("type").Value);
						}
						Robot.setParams(scriptName, scriptType, editMode);
						
						break;
					default:
						Debug.Log($"Ignored unexpected node type: {element.Name}");
						
						break;
				}
			}

			foreach (var scriptElement in scripts)
			{
				var node = new XmlDocument().ReadNode(scriptElement.CreateReader());
				xmlScriptGenerator.readXMLScript(node, node.Attributes.GetNamedItem("outputLine").Value, UIRootContainer.EditMode.Editable, UIRootContainer.SolutionType.Undefined);
			}

			levelData.requireRefresh = true;
		}
		else
		{
			initGrid();
		}

		levelData.isReady = true;
		GameObjectManager.setGameObjectState(paintableGrid.gameObject, true);*/
	}

	// See editBlockPrefab
	public void hideToggleChanged(GameObject source, bool newState)
	{
		// control unlimited before input field
		Toggle unlimitedToggle = source.transform.Find("LimitToggle").GetComponent<Toggle>();
		unlimitedToggle.isOn = false;
		unlimitedToggle.interactable = !newState;

		TMP_InputField inputField = source.GetComponentInChildren<TMP_InputField>();
		inputField.interactable = !newState;
		if (newState)
		{
			inputField.text = "0";
			source.GetComponent<Image>().color = source.GetComponent<EditorBlockData>().hideColor;
		}
		else
		{
			inputField.text = "1";
			source.GetComponent<Image>().color = source.GetComponent<EditorBlockData>().limitedColor;
		}
	}

	// See editBlockPrefab
	public void limitToggleChanged(GameObject source, bool newState)
	{
		TMP_InputField inputField = source.GetComponentInChildren<TMP_InputField>();
		inputField.interactable = !newState;
		if (newState)
		{
			inputField.text = "-1";
			source.GetComponent<Image>().color = source.GetComponent<EditorBlockData>().unlimitedColor;
		}
		else
		{
			inputField.text = "1";
			source.GetComponent<Image>().color = source.GetComponent<EditorBlockData>().limitedColor;
		}
	}

	// See DragDropDisabled panel
	public void onDragDropToggled(bool newState){
		// if drag&drop is disabled => hide all blocks
		if (!newState)
        {
			foreach (GameObject go in f_editorblocks)
			{
				go.transform.Find("HideToggle").GetComponent<Toggle>().isOn = true;
			}
		}
	}
	
	// See executionLimit panel
	public void preventMinusSign(TMP_InputField input)
	{
		if (input.text.StartsWith("-"))
			input.text = input.text.Trim('-');
	}

	// see executionLimit panel
	public void executionLimitChanged(bool newState)
	{
		executionLimitContainer.GetComponentInChildren<TMP_InputField>(true).interactable = newState;
	}

	public void scoreTwoStarsExit(string newData)
	{
		if (string.IsNullOrEmpty(newData))
			score2Input.text = "0";

		int twoStarsScore = int.Parse(newData);
		int threeStarsScore = int.TryParse(score3Input.text, out int x) ? x : 0;
		if (twoStarsScore > threeStarsScore)
			score3Input.text = newData;
	}

	public void scoreThreeStarsExit(string newData)
	{
		if (string.IsNullOrEmpty(newData))
			score3Input.text = "0";

		int threeStarsScore = int.Parse(newData);
		int twoStarsScore = int.TryParse(score2Input.text, out int x) ? x : 0;
		if (twoStarsScore > threeStarsScore)
			score2Input.text = newData;
	}
}

public enum BlockCategory
{
	Action = 0,
	Control = 1,
	Operator = 2,
	Sensor = 3
}