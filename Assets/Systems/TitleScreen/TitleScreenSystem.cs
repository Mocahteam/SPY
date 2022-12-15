using UnityEngine;
using FYFY;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System.Xml;
using System.Collections;
using UnityEngine.Networking;
using System;
using UnityEngine.Events;
using System.Runtime.InteropServices;

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
	public GameObject loadingScenarioContent;
	public GameObject scenarioContent;
	public GameObject quitButton;

	private GameObject selectedScenario;
	private Dictionary<string, List<string>> defaultCampaigns; // List of levels for each default campaign
	private UnityAction localCallback;

	private string loadLevelWithURL = "";
	private int webGL_levelLoaded = 0;

	[DllImport("__Internal")]
	private static extern void ShowHtmlButtons(); // call javascript

	[Serializable]
	public class WebGlDataLevels
    {
		public string scenarioName;
		public List<string> levelPath;
    }

	[Serializable]
	public class WebGlScenarioList
    {
		public List<WebGlDataLevels> scenarios;
    }


	// L'instance
	public static TitleScreenSystem instance;

	public TitleScreenSystem()
	{
		instance = this;
	}

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
		selectedScenario = null;

		GameObjectManager.setGameObjectState(campagneMenu, false);
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			ShowHtmlButtons();
			MainLoop.instance.StartCoroutine(GetScenarioWebRequest());
			MainLoop.instance.StartCoroutine(GetLevelsWebRequest());
			GameObjectManager.setGameObjectState(quitButton, false);
		}
	}

	private IEnumerator GetScenarioWebRequest()
	{
		UnityWebRequest www = UnityWebRequest.Get(Application.streamingAssetsPath + "/WebGlData/ScenarioList.json");
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Erreur lors de l'accès au document " + Application.streamingAssetsPath + "/WebGlData/ScenarioList.json : " + www.error, OkButton = "", CancelButton = "OK", call = localCallback });
		}
		else
		{
			string scenarioJson = www.downloadHandler.text;

			WebGlScenarioList scenarioListRaw = JsonUtility.FromJson<WebGlScenarioList>(scenarioJson);
			// try to load all scenarios
			foreach (WebGlDataLevels scenarioRaw in scenarioListRaw.scenarios)
			{
				defaultCampaigns[scenarioRaw.scenarioName] = new List<string>();
				foreach (string levelPath in scenarioRaw.levelPath)
					defaultCampaigns[scenarioRaw.scenarioName].Add(Application.streamingAssetsPath + "/" + levelPath);
			}
			createScenarioButtons();
		}
	}

	private IEnumerator GetLevelsWebRequest()
	{
		UnityWebRequest www = UnityWebRequest.Get(Application.streamingAssetsPath + "/WebGlData/LevelsList.json");
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Erreur lors de l'accès au document " + Application.streamingAssetsPath + "/WebGlData/LevelsList.json : " + www.error, OkButton = "", CancelButton = "OK", call = localCallback });
		}
		else
		{
			string levelsJson = www.downloadHandler.text;
			WebGlDataLevels levelsListRaw = JsonUtility.FromJson<WebGlDataLevels>(levelsJson);
			// try to load all levels
			foreach (string levelRaw in levelsListRaw.levelPath)
				MainLoop.instance.StartCoroutine(GetLevelWebRequest(Application.streamingAssetsPath + "/" + levelRaw));
			// wait level loading
			while (webGL_levelLoaded < levelsListRaw.levelPath.Count)
				yield return null;
			// Now, if require, we can load requested level by URL
			if (loadLevelWithURL != "")
				testLevelPath(loadLevelWithURL);
		}
	}

	private IEnumerator GetLevelWebRequest(string levelUri)
	{
		UnityWebRequest www = UnityWebRequest.Get(levelUri);
		yield return www.SendWebRequest();

		webGL_levelLoaded++;
		if (www.result != UnityWebRequest.Result.Success)
		{
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Erreur lors de l'accès au document " + levelUri + " : " + www.error, OkButton = "", CancelButton = "OK", call = localCallback });
		}
		else
		{
			string levelXML = www.downloadHandler.text;
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(levelXML);
			EditingUtility.removeComments(doc);
			// a valid level must have only one tag "level"
			if (doc.GetElementsByTagName("level").Count == 1)
				gameData.levels.Add(levelUri, doc.GetElementsByTagName("level")[0]);
		}
	}

	private class JavaScriptData
	{
		public string name;
		public string content;
	}

	// Fonction appelée depuis le javascript (voir Assets/WebGLTemplates/Custom/index.html) via le Wrapper du Système
	public void importScenario(string content)
	{
		JavaScriptData jsd = JsonUtility.FromJson<JavaScriptData>(content);
		try
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(jsd.content);
			EditingUtility.removeComments(doc);
			extractLevelListFromScenario(jsd.name, doc);
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Scénario chargé", OkButton = "", CancelButton = "OK", call = localCallback });
		}
		catch (Exception e) {
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Erreur lors du chargement du scénario : "+e.Message, OkButton = "", CancelButton = "OK", call = localCallback });
		}
	}

	private void extractLevelListFromScenario(string scenarioName, XmlDocument doc)
	{
		// a valid scenario must have only one tag "scenario"
		if (doc.GetElementsByTagName("scenario").Count == 1)
		{
			List<string> levelList = new List<string>();
			foreach (XmlNode child in doc.GetElementsByTagName("scenario")[0])
				if (child.Name.Equals("level"))
					levelList.Add(Application.streamingAssetsPath + "/" + (child.Attributes.GetNamedItem("name").Value));
			defaultCampaigns[Path.GetFileName(scenarioName)] = levelList; //key = directory name
		}
	}

	public void updateScenarioList()
	{
		if (Application.platform != RuntimePlatform.WebGLPlayer)
		{
			loadLevelsAndScenarios(Application.streamingAssetsPath);
			loadLevelsAndScenarios(Application.persistentDataPath);
		}
		createScenarioButtons();
	}

	private void createScenarioButtons()
	{
		// remove all old scenario
		foreach(Transform child in listOfCampaigns.transform)
        {
			GameObjectManager.unbind(child.gameObject);
			GameObject.Destroy(child.gameObject);
        }

		//create level directory buttons
		foreach (string key in defaultCampaigns.Keys)
		{
			GameObject directoryButton = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/Button") as GameObject, listOfCampaigns.transform);
			directoryButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Path.GetFileNameWithoutExtension(key);
			GameObjectManager.bind(directoryButton);
			// add on click
			directoryButton.GetComponent<Button>().onClick.AddListener(delegate { showLevels(key); });
		}
	}

	private void loadLevelsAndScenarios(string path)
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
					gameData.levels.Add(fileName.Replace("\\", "/"), doc.GetElementsByTagName("level")[0]);
				// try to extract scenario
				extractLevelListFromScenario(fileName, doc);
			}
			catch { }
		}

		// explore subdirectories
		foreach (string directory in Directory.GetDirectories(path))
			loadLevelsAndScenarios(directory);
	}

	private void showLevels(string campaignKey) {
		GameObjectManager.setGameObjectState(mainCanvas.transform.Find("SPYMenu").Find("MenuCampaigns").gameObject, false);
		GameObjectManager.setGameObjectState(mainCanvas.transform.Find("SPYMenu").Find("MenuLevels").gameObject, true);
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
			GameObject button = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/LevelButton") as GameObject, listOfLevels.transform);
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

	//Used on scenario editing window (see button ButtonTestLevel)
	public void testLevel(TMP_Text levelToLoad)
	{
		testLevelPath(levelToLoad.text);
	}
	private void testLevelPath(string levelToLoad)
	{
		gameData.scenarioName = "testLevel";
		gameData.scenario = new List<string>();
		gameData.levelToLoad = Application.streamingAssetsPath + "/" + levelToLoad;
		gameData.scenario.Add(gameData.levelToLoad);
		GameObjectManager.loadScene("MainScene");
	}

	// Fonction appelée depuis le javascript (voir Assets/WebGLTemplates/Custom/index.html) via le Wrapper du Système
	public void askToLoadLevel(string levelToLoad)
    {
		loadLevelWithURL = levelToLoad;
	}

	// See Quitter button in editor
	public void quitGame(){
		Application.Quit();
	}

	// See ButtonLoadScenario
	public void displayLoadingPanel()
	{
		selectedScenario = null;
		GameObjectManager.setGameObjectState(mainCanvas.transform.Find("LoadingPanel").gameObject, true);
		// remove all old scenario
		foreach (Transform child in loadingScenarioContent.transform)
		{
			GameObjectManager.unbind(child.gameObject);
			GameObject.Destroy(child.gameObject);
		}

		//create level directory buttons
		foreach (string key in defaultCampaigns.Keys)
		{
			GameObject scenarioItem = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/ScenarioAvailable") as GameObject, loadingScenarioContent.transform);
			scenarioItem.GetComponent<TextMeshProUGUI>().text = key;
			GameObjectManager.bind(scenarioItem);
		}
	}

	public void onScenarioSelected(GameObject go)
    {
		selectedScenario = go;
    }

	public void loadScenario()
    {
		if (selectedScenario != null && defaultCampaigns.ContainsKey(selectedScenario.GetComponentInChildren<TMP_Text>().text))
		{
			//remove all old scenario
			foreach (Transform child in scenarioContent.transform)
            {
				GameObjectManager.unbind(child.gameObject);
				GameObject.Destroy(child.gameObject);
            }

			foreach (string levelPath in defaultCampaigns[selectedScenario.GetComponentInChildren<TMP_Text>().text])
			{
				GameObject newLevel = GameObject.Instantiate(Resources.Load("Prefabs/deletableElement") as GameObject, scenarioContent.transform);
				newLevel.GetComponentInChildren<TMP_Text>().text = levelPath.Replace(Application.streamingAssetsPath + "/", "");
				LayoutRebuilder.ForceRebuildLayoutImmediate(newLevel.transform as RectTransform);
				GameObjectManager.bind(newLevel);
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(scenarioContent.transform as RectTransform);
		}
    }
}