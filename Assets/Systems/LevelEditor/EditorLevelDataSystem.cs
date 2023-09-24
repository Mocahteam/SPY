using UnityEngine;
using FYFY;
using UnityEngine.UI;
using System.Xml;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class EditorLevelDataSystem : FSystem {
	public Family f_editorblocks = FamilyManager.getFamily(new AllOfComponents(typeof(EditorBlockData)));
	private Family f_newLoading = FamilyManager.getFamily(new AllOfComponents(typeof(NewLevelToLoad)));

	public static EditorLevelDataSystem instance;
	
	public GameObject executionLimitContainer;
	public Toggle dragAndDropToggle;
	public Toggle fogToggle;
	public Toggle hideExitsToggle;
	public TMP_InputField score2Input;
	public TMP_InputField score3Input;
	public Transform editableContainers;

	public Color hideColor;
	public Color limitedColor;
	public Color unlimitedColor;

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

		// all blocks are unlimited inside level editor
		gameData.actionBlockLimit = new Dictionary<string, int>();
		foreach(GameObject block in f_editorblocks)
			gameData.actionBlockLimit[block.GetComponent<EditorBlockData>().name] = -1;

		resetMetaData();

		f_newLoading.addEntryCallback(loadLevel);
	}

	public void resetMetaData()
	{
		executionLimitContainer.GetComponentInChildren<Toggle>(true).isOn = false;
		dragAndDropToggle.isOn = true;
		fogToggle.isOn = false;
		hideExitsToggle.isOn = false;
		score2Input.text = "";
		score3Input.text = "";

		// Remove all existing editable area
		foreach (Transform viewportForEditableContainer in editableContainers)
			GameObjectManager.addComponent<ForceRemoveContainer>(viewportForEditableContainer.gameObject);

		foreach (GameObject go in f_editorblocks)
        {
			go.transform.Find("HideToggle").GetComponent<Toggle>().isOn = true;
		}
	}

	private void loadLevel(GameObject go)
	{
		resetMetaData();

		string levelKey = go.GetComponent<NewLevelToLoad>().levelKey;
		XmlDocument doc = gameData.levels[levelKey].OwnerDocument;
		Utility.removeComments(doc);
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
					try
					{
						executionLimitContainer.GetComponentInChildren<Toggle>(true).isOn = true;
						executionLimitContainer.GetComponentInChildren<TMP_InputField>(true).text = child.Attributes.GetNamedItem("amount")?.Value;
					}
					catch
					{
						Debug.Log("Warning: Skipped executionLimit from file " + levelKey + ". Wrong data!");
					}
					break;
				case "blockLimits":
					try
					{
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
					}
					catch
					{
						Debug.Log("Warning: Skipped blockLimits from file " + levelKey + ". Wrong data!");
					}
					break;
				case "score":
					try
					{
						score2Input.text = child.Attributes.GetNamedItem("twoStars")?.Value;
						score3Input.text = child.Attributes.GetNamedItem("threeStars")?.Value;
					}
					catch
					{
						Debug.Log("Warning: Skipped score from file " + levelKey + ". Wrong data!");
					}
					break;
				case "script":
					try
					{
						UIRootContainer.EditMode editModeByUser = UIRootContainer.EditMode.Locked;
						XmlNode editMode = child.Attributes.GetNamedItem("editMode");
						int tmpValue;
						if (editMode != null && int.TryParse(editMode.Value, out tmpValue))
							editModeByUser = (UIRootContainer.EditMode)tmpValue;
						UIRootContainer.SolutionType typeByUser = UIRootContainer.SolutionType.Undefined;
						XmlNode typeNode = child.Attributes.GetNamedItem("type");
						if (typeNode != null && int.TryParse(typeNode.Value, out tmpValue))
							typeByUser = (UIRootContainer.SolutionType)tmpValue;
						XmlNode name = child.Attributes.GetNamedItem("outputLine");
						if (name == null)
							name = child.Attributes.GetNamedItem("name"); // for retrocompatibility

						// Ask to create script
						/* We can't used :
						GameObjectManager.addComponent<ScriptToLoad>(MainLoop.instance.gameObject, new
						{
							scriptNode = child,
							scriptName = Utility.extractLocale(name.Value),
							editMode = editModeByUser,
							type = typeByUser
						});
						because we get exception "not IConvertible" when we try to get the component !!! something wrong with XmlNode attribute of ScriptToLoad component...
						Solution: we add ScriptToLoad component to the MainLoop immediately, we init each of its attributes and we ask to refresh the MainLoop to update families*/
						ScriptToLoad stl = MainLoop.instance.gameObject.AddComponent<ScriptToLoad>();
						stl.scriptNode = child;
						stl.scriptName = Utility.extractLocale(name.Value);
						stl.editMode = editModeByUser;
						stl.type = typeByUser;
						MainLoop.instance.StartCoroutine(delayRefreshMainLoop());
					}
					catch
					{
						Debug.Log("Warning: Skipped script from file " + levelKey + ". Wrong data!");
					}
					break;
			}
		}
	}

	private IEnumerator delayRefreshMainLoop()
    {
		yield return null;
		GameObjectManager.refresh(MainLoop.instance.gameObject);
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
			source.GetComponent<Image>().color = hideColor;
		}
		else
		{
			inputField.text = "1";
			source.GetComponent<Image>().color = limitedColor;
		}
	}

	// See editBlockPrefab => HideToggle
	public void limitToggleChanged(GameObject source, bool newState)
	{
		TMP_InputField inputField = source.GetComponentInChildren<TMP_InputField>();
		inputField.interactable = !newState;
		if (newState)
		{
			inputField.text = "-1";
			source.GetComponent<Image>().color = unlimitedColor;
		}
		else
		{
			inputField.text = "1";
			source.GetComponent<Image>().color = limitedColor;
		}
	}

	// see childs of executionLimit GO
	public void preventMinusSign(TMP_InputField input)
	{
		if (input.text.StartsWith("-"))
			input.text = input.text.Trim('-');
	}

	// see childs of executionLimit GO
	public void executionLimitChanged(bool newState)
	{
		executionLimitContainer.GetComponentInChildren<TMP_InputField>(true).interactable = newState;
	}

	// see childs of score2Stars GO
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