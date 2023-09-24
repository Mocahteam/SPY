using System;
using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;
using System.Xml;
using System.Collections.Generic;

public class SaveFileSystem : FSystem
{
	private Family f_editorblocks = FamilyManager.getFamily(new AllOfComponents(typeof(EditorBlockData)));

	public TMP_InputField saveName;

	public Toggle DragDrop;
	public Toggle Fog;
	public Toggle ExecutionLimit;
	public Toggle HideExits;
	public TMP_InputField score2;
	public TMP_InputField score3;

	public GameObject editableContainer;
	public LevelData levelData;
	public PaintableGrid paintableGrid;

	public static SaveFileSystem instance;

	private GameData gameData;
	private UnityAction localCallback;

	[DllImport("__Internal")]
	private static extern void Save(string content, string defaultName); // call javascript

	public SaveFileSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if((Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S)) ||
		   Input.GetKey(KeyCode.LeftCommand) && Input.GetKeyDown(KeyCode.S))
			displaySavingPanel();
	}

	// See TestLevel GO (Button)
	public void testLevel()
    {
		string exportXML = buildLevelContent();
		XmlDocument doc = new XmlDocument();
		doc.LoadXml(exportXML);
		Utility.removeComments(doc);
		gameData.levels[Utility.testFromLevelEditor] = doc.GetElementsByTagName("level")[0];
		gameData.selectedScenario = Utility.testFromLevelEditor;
		WebGlScenario test = new WebGlScenario();
		test.levels = new List<DataLevel>();
		DataLevel dl = new DataLevel();
		dl.src = Utility.testFromLevelEditor;
		dl.name = Utility.testFromLevelEditor;
		test.levels.Add(dl);
		gameData.scenarios[Utility.testFromLevelEditor] = test;
		gameData.levelToLoad = 0;
		GameObjectManager.loadScene("MainScene");
	}

	// See Button Save
	public void displaySavingPanel()
	{
		if (levelData.levelName != null || levelData.levelName != "")
			saveName.text = Path.GetFileNameWithoutExtension(levelData.levelName);
		GameObjectManager.setGameObjectState(saveName.transform.parent.parent.gameObject, true);
		EventSystem.current.SetSelectedGameObject(saveName.transform.parent.Find("Buttons").Find("CancelButton").gameObject);
	}

	// see ValideMessageButton (only called in standalone context, not used for WebGL)
	public void saveXmlFile()
	{
		if (!Utility.CheckSaveNameValidity(saveName.text))
		{
			localCallback = null;
			string invalidChars = "";
			foreach (char someChar in Path.GetInvalidFileNameChars())
				if (Char.IsPunctuation(someChar) || Char.IsSymbol(someChar))
					invalidChars += someChar + " ";
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(gameData.localization[12], invalidChars), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
			// Be sure saving windows is enabled
			GameObjectManager.setGameObjectState(saveName.transform.parent.parent.gameObject, true);
		}
		else
		{
			// add file extension
			if (!saveName.text.EndsWith(".xml"))
				saveName.text += ".xml";

			if (Application.platform != RuntimePlatform.WebGLPlayer && File.Exists(Application.persistentDataPath + "/Levels/" + saveName.text))
			{
				localCallback = null;
				localCallback += delegate { saveToFile(); };
				GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(gameData.localization[13], saveName.text), OkButton = gameData.localization[3], CancelButton = gameData.localization[4], call = localCallback });
				// Be sure saving windows is enabled
				GameObjectManager.setGameObjectState(saveName.transform.parent.parent.gameObject, true);
			}
			else
				saveToFile();
		}
	}

	private void saveToFile()
	{
		string levelExport = buildLevelContent();

		// generate XML structure from string
		XmlDocument doc = new XmlDocument();
		doc.LoadXml(levelExport);
		Utility.removeComments(doc);

		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			Save(levelExport, saveName.text);
			// Add/Replace level content in memory
			string fakeUri = Application.streamingAssetsPath + "/Levels/LocalFiles/" + saveName.text;
			gameData.levels[new Uri(fakeUri).AbsoluteUri] = doc.GetElementsByTagName("level")[0];
		}
		else
		{
			try
			{
				// Create all necessary directories if they don't exist
				Directory.CreateDirectory(Application.persistentDataPath + "/Levels");
				string path = Application.persistentDataPath + "/Levels/" + saveName.text;
				// Write on disk
				File.WriteAllText(path, levelExport);
				levelData.levelName = saveName.text;
				levelData.filePath = new Uri(path).AbsoluteUri;
				// Add/Replace level content in memory
				gameData.levels[new Uri(path).AbsoluteUri] = doc.GetElementsByTagName("level")[0];

				localCallback = null;
				GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(gameData.localization[14], Application.persistentDataPath, "Levels", saveName.text), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
			}
			catch (Exception e)
			{
				localCallback = null;
				GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(gameData.localization[15], e.Message), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
			}
		}
		// Be sure saving windows is disabled
		GameObjectManager.setGameObjectState(saveName.transform.parent.parent.gameObject, false);
	}

	private string buildLevelContent()
	{
		string levelExport = "<?xml version=\"1.0\"?>\n";
		levelExport += "<level>\n";

		levelExport += "\t<map>\n";
		// crop unused map
		int minLine = int.MaxValue;
		int minCol = int.MaxValue;
		int maxLine = -1;
		int maxCol = -1;
		for (int line = 0; line < UtilityEditor.gridMaxSize; line++)
			for (int col = 0; col < UtilityEditor.gridMaxSize; col++)
				if (paintableGrid.grid[line, col] != Cell.Void)
                {
					minLine = line < minLine ? line : minLine;
					minCol = col < minCol ? col : minCol;
					maxLine = line > maxLine ? line : maxLine;
					maxCol = col > maxCol ? col : maxCol;
				}
		// process only map section used
		// Add a first line with void cells
		levelExport += "\t\t<line>";
		for (int col = minCol; col <= maxCol+2; col++)
			levelExport += "<cell value=\"" + (int)Cell.Void + "\" />";
		levelExport += "</line>\n";
		// parse the maps section used
		for (var line = minLine; line <= maxLine; line++)
		{
			levelExport += "\t\t<line>";
			// Add a first column with void cells
			levelExport += "<cell value=\"" + (int)Cell.Void + "\" />";
			for (int col = minCol; col <= maxCol; col++)
				levelExport += "<cell value=\""+ (int)paintableGrid.grid[line, col] + "\" />";
			// Add a last column with void cells
			levelExport += "<cell value=\"" + (int)Cell.Void + "\" />";
			levelExport += "</line>\n";
		}
		// Add a last line with void cells
		levelExport += "\t\t<line>";
		for (int col = minCol; col <= maxCol + 2; col++)
			levelExport += "<cell value=\"" + (int)Cell.Void + "\" />";
		levelExport += "</line>\n";
		levelExport += "\t</map>\n\n";

		if (!DragDrop.isOn)
			levelExport += "\t<dragdropDisabled />\n\n";

		if (ExecutionLimit.isOn)
		{
			string value = ExecutionLimit.transform.parent.GetComponentInChildren<TMP_InputField>(true).text;
			levelExport += "\t<executionLimit amount=\"" + (value == "" ? "0" : value) + "\" />\n\n";
		}

		if (Fog.isOn)
			levelExport += "\t<fog />\n\n";

		if (HideExits.isOn)
			levelExport += "\t<hideExits />\n\n";

		levelExport += "\t<score twoStars=\""+(score2.text == "" ? "0" : score2.text)+"\" threeStars=\""+ (score3.text == "" ? "0" : score3.text) + "\"/>\n\n";

		levelExport += "\t<blockLimits>\n";
		foreach (GameObject blockLimit in f_editorblocks)
		{
			EditorBlockData ebd = blockLimit.GetComponent<EditorBlockData>();
			levelExport += "\t\t<blockLimit blockType=\"" + ebd.blockName + "\" limit=\"" + ebd.GetComponentInChildren<TMP_InputField>(true).text + "\" />\n";
		}
		levelExport += "\t</blockLimits>\n\n";

		foreach (Tuple<int, int> foCoords in paintableGrid.floorObjects.Keys)
		{
			FloorObject fo = paintableGrid.floorObjects[foCoords];
			switch (fo)
			{
				case Console c:
					levelExport += "\t<console state=\""+ (c.state ? "1" : "0") + "\" posX=\"" + (c.col+1 - minCol) + "\" posY=\""+ (c.line+1 - minLine) + "\" direction=\""+ (int)c.orientation + "\">\n";
					// add each slot
					foreach (string slot in c.slots)
						levelExport += "\t\t<slot slotId=\""+ slot + "\" />\n";
					levelExport += "\t</console>\n\n";
					break;
				case Door d:
					levelExport += "\t<door posX=\"" + (d.col+1 - minCol) + "\" posY=\"" + (d.line+ 1 - minLine) + "\" slotId=\""+ d.slot + "\" direction=\""+ (int)d.orientation + "\" />\n\n";
					break;
				case PlayerRobot pr:
					levelExport += "\t<player inputLine=\""+ pr.inputLine + "\" posX=\"" + (pr.col + 1 - minCol) + "\" posY=\"" + (pr.line + 1 - minLine) + "\" direction=\"" + (int)pr.orientation + "\" />\n\n";
					break;
				case EnemyRobot er:
					levelExport += "\t<enemy inputLine=\"" + er.inputLine + "\" posX=\"" + (er.col + 1 - minCol) + "\" posY=\"" + (er.line + 1 - minLine) + "\" direction=\"" + (int)er.orientation + "\" range=\""+ er.range + "\" selfRange=\""+(er.selfRange ? "True" : "False") +"\" typeRange=\""+ (int)er.typeRange + "\" />\n\n";
					break;
				case DecorationObject deco:
					levelExport += "\t<decoration name=\""+ deco.path + "\" posX=\"" + (deco.col + 1 - minCol) + "\" posY=\"" + (deco.line + 1 - minLine) + "\" direction=\"" + (int)deco.orientation + "\" />\n\n";
					break;

				default:
					if (fo.type != Cell.Coin)
					{
						Debug.Log("Unexpected floor object type, object ignored: " + fo.type);
						break;
					}
					levelExport += "\t<coin posX=\"" + (fo.col + 1 - minCol) + "\" posY=\"" + (fo.line + 1 - minLine) + "\" />\n\n";
					break;
			}
		}

		for (int i = 0; i < editableContainer.transform.childCount; i++)
		{
			Transform editorViewportScriptContainer = editableContainer.transform.GetChild(i).Find("ScriptContainer");
			string scriptName = editorViewportScriptContainer.Find("Header").Find("ContainerName").GetComponent<TMP_InputField>().text;
			TMP_Dropdown editMode = editorViewportScriptContainer.Find("LevelEditorPanel").Find("EditMode_Dropdown").GetComponent<TMP_Dropdown>();
			TMP_Dropdown type = editorViewportScriptContainer.Find("LevelEditorPanel").Find("ProgType_Dropdown").GetComponent<TMP_Dropdown>();
			levelExport += "\t<script outputLine=\""+ scriptName + "\" editMode=\""+ editMode.value + "\" type=\""+ type.value + "\">\n";

			// on ignore les fils sans Highlightable
			for (int j = 0; j < editorViewportScriptContainer.childCount; j++)
			{
				Highlightable h = editorViewportScriptContainer.GetChild(j).GetComponent<Highlightable>();
				if (h != null)
					levelExport += Utility.exportBlockToString(h, null, Utility.ExportType.XML, 2);
			}
			levelExport += "\t</script>\n\n";
		}

		levelExport += "</level>";
		return levelExport;
	}
}