using UnityEngine;
using FYFY;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System.Xml;
using Object = UnityEngine.Object;

/// <summary>
/// Manage main menu to launch a specific mission
/// </summary>
public class TitleScreenSystem : FSystem {
	private GameData gameData;
	public GameData prefabGameData;
	public GameObject mainCanvas;
	public GameObject campagneMenu;
	public GameObject compLevelButton;
	public GameObject listOfCampaigns;
	public GameObject listOfLevels;


	private Dictionary<string, List<string>> defaultCampaigns; // List of levels for each default campaign

	protected override void onStart()
	{
		if (!GameObject.Find("GameData"))
		{
			gameData = UnityEngine.Object.Instantiate(prefabGameData);
			gameData.name = "GameData";
			GameObjectManager.dontDestroyOnLoadAndRebind(gameData.gameObject);
		}
		else
		{
			gameData = GameObject.Find("GameData").GetComponent<GameData>();
		}

		gameData.levels = new Dictionary<string, XmlNode>();
		gameData.scenario = new List<string>();

		defaultCampaigns = new Dictionary<string, List<string>>();

		GameObjectManager.setGameObjectState(campagneMenu, false);
		string levelsPath;
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			for (int i = 1; i <= 22; i++)
				defaultCampaigns["Campagne infiltration"].Add(Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels" +
			Path.DirectorySeparatorChar + "Campagne infiltration" + Path.DirectorySeparatorChar +"Niveau" + i + ".xml");
			// Hide Competence button
			GameObjectManager.setGameObjectState(compLevelButton, false);
		}
		else
		{
			// Load all levels
			loadLevels(Application.streamingAssetsPath);
			// Load content of each campaign
			levelsPath = Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels";
			List<string> levels;
			foreach (string directory in Directory.GetDirectories(levelsPath))
			{
				levels = readScenario(directory);
				if (levels != null)
					defaultCampaigns[Path.GetFileName(directory)] = levels; //key = directory name
			}
		}

		//create level directory buttons
		foreach (string key in defaultCampaigns.Keys)
		{
			GameObject directoryButton = Object.Instantiate<GameObject>(Resources.Load("Prefabs/Button") as GameObject, listOfCampaigns.transform);
			directoryButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = key;
			GameObjectManager.bind(directoryButton);
			// add on click
			directoryButton.GetComponent<Button>().onClick.AddListener(delegate { showLevels(key); });
		}
	}

	private void loadLevels(string path)
	{
		// try to load all child files
		foreach (string fileName in Directory.GetFiles(path))
		{
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(fileName);
				EditingUtility.removeComments(doc);
				// a valid level must have only one tag "level"
				if (doc.GetElementsByTagName("level").Count == 1)
					gameData.levels.Add(fileName, doc.GetElementsByTagName("level")[0]);
			}
			catch { }
		}

		// explore subdirectories
		foreach (string directory in Directory.GetDirectories(path))
			loadLevels(directory);
	}

	private List<string> readScenario(string repositoryPath) {
		if (File.Exists(repositoryPath + Path.DirectorySeparatorChar + "Scenario.xml")) {
			List<string> levelList = new List<string>();
			XmlDocument doc = new XmlDocument();
			doc.Load(repositoryPath + Path.DirectorySeparatorChar + "Scenario.xml");
			XmlNode root = doc.ChildNodes[1]; //root = <scenario/>
			foreach (XmlNode child in root.ChildNodes) {
				if (child.Name.Equals("level")) {
					levelList.Add(repositoryPath + Path.DirectorySeparatorChar + (child.Attributes.GetNamedItem("name").Value));
				}
			}
			return levelList;
		}
		return null;
	}

	protected override void onProcess(int familiesUpdateCount) {
		if (Input.GetButtonDown("Cancel")) {
			Application.Quit();
		}
	}

	private void showLevels(string campaignKey) {
		GameObjectManager.setGameObjectState(mainCanvas.transform.Find("MenuCampaigns").gameObject, false);
		GameObjectManager.setGameObjectState(mainCanvas.transform.Find("MenuLevels").gameObject, true);
		// delete all old level buttons
		foreach (Transform child in listOfLevels.transform)
        {
			GameObjectManager.unbind(child.gameObject);
			GameObject.Destroy(child.gameObject);
        }

		// create level buttons for this campaign
		for (int i = 0; i < defaultCampaigns[campaignKey].Count; i++)
		{
			string levelKey = defaultCampaigns[campaignKey][i];
			GameObject button = Object.Instantiate<GameObject>(Resources.Load("Prefabs/LevelButton") as GameObject, listOfLevels.transform);
			button.transform.Find("Button").GetChild(0).GetComponent<TextMeshProUGUI>().text = Path.GetFileNameWithoutExtension(levelKey);
			button.transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate { launchLevel(campaignKey, levelKey); });
			GameObjectManager.bind(button);
			//locked levels
			if (i <= PlayerPrefs.GetInt(campaignKey, 0)) //by default first level of directory is the only unlocked level of directory
				button.GetComponentInChildren<Button>().interactable = true;
			//unlocked levels
			else
				button.GetComponentInChildren<Button>().interactable = false;
			//scores
			int scoredStars = PlayerPrefs.GetInt(levelKey + gameData.scoreKey, 0); //0 star by default
			Transform scoreCanvas = button.transform.Find("ScoreCanvas");
			for (int nbStar = 0; nbStar < 4; nbStar++)
			{
				if (nbStar == scoredStars)
					GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, true);
				else
					GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, false);
			}
		}
	}

	public void launchLevel(string campaignKey, string levelToLoad) {
		gameData.scenarioName = campaignKey;
		gameData.levelToLoad = levelToLoad;
		gameData.scenario = defaultCampaigns[campaignKey];
		GameObjectManager.loadScene("MainScene");
	}

	// See Quitter button in editor
	public void quitGame(){
		Application.Quit();
	}
}