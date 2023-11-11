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
using DIG.GBLXAPI;
using Newtonsoft.Json;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

/// <summary>
/// Manage main menu to launch a specific mission
/// </summary>
public class TitleScreenSystem : FSystem {
	private Family f_sessionId = FamilyManager.getFamily(new AllOfComponents(typeof(TextMeshProUGUI)), new AnyOfTags("SessionId"));
	private Family f_competencies = FamilyManager.getFamily(new AllOfComponents(typeof(Competency))); // Les compétences
	private Family f_localizationLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(LocalizationLoaded)));

	private Family f_buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	private GameData gameData;
	private UserData userData;
	public GameObject prefabGameData;
	public GameObject mainCanvas;
	public GameObject mainMenu;
	public GameObject compLevelButton;
	public GameObject listOfCampaigns;
	public GameObject listOfLevels;
	public GameObject playButton;
	public GameObject quitButton;
	public GameObject levelEditorButton;
	public GameObject loadingScreen;
	public GameObject sessionIdPanel;
	public GameObject deletableElement;
	public TMP_InputField scenarioName;
	public TMP_InputField scenarioAbstract;
	public GameObject detailsCampaign;
	public GameObject virtualKeyboard;
	public TMP_Text progress;
	public TMP_Text logs;

	private UnityAction localCallback;

	private GameObject lastSelected;

	private string loadLevelWithURL = "";
	private int webGL_fileLoaded = 0;
	private int webGL_fileToLoad = 0;
	private bool webGL_askToEnableSendSystem = false;

	[DllImport("__Internal")]
	private static extern void ShowHtmlButtons(); // call javascript

	[Serializable]
	public class WebGlScenarioList
    {
		public List<WebGlScenario> scenarios;
    }


	// L'instance
	public static TitleScreenSystem instance;

	public TitleScreenSystem()
	{
		instance = this;
	}

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go == null)
		{
			go = UnityEngine.Object.Instantiate(prefabGameData);
			go.name = "GameData";
		}
		gameData = go.GetComponent<GameData>();
		userData = go.GetComponent<UserData>();
		if (webGL_askToEnableSendSystem)
			gameData.sendStatementEnabled = true;
		GameObjectManager.dontDestroyOnLoadAndRebind(gameData.gameObject);

		// Enable Loading screen
		GameObjectManager.setGameObjectState(loadingScreen, true);

		EventSystem.current.SetSelectedGameObject(playButton);

		if (!GameObject.Find("GBLXAPI"))
			initGBLXAPI();
		else
			foreach (GameObject sID in f_sessionId)
				sID.GetComponent<TMP_Text>().text = GBL_Interface.playerName;

		if (gameData.levels == null) // we have to load streaming assets
        {
			gameData.levels = new Dictionary<string, XmlNode>();
			gameData.scenarios = new Dictionary<string, WebGlScenario>();
			GetScenariosAndLevels();
		}
		else // means we come back from a playing session, streaming assets are already loaded
		{
			Transform spyMenu = mainCanvas.transform.Find("SPYMenu");
			if (gameData.selectedScenario == Utility.testFromScenarioEditor) // reload scenario editor
            {
                MainLoop.instance.StartCoroutine(delayOpeningScenarioEditor());
			}
			else if (gameData.selectedScenario == Utility.testFromLevelEditor) // reload level editor
            {
				launchLevelEditor();
			}
            else if (gameData.selectedScenario != "" && gameData.selectedScenario != Utility.testFromUrl)
            {
				// reload last opened scenario
				playButton.GetComponent<Button>().onClick.Invoke();
				showLevels(gameData.selectedScenario);
				GameObjectManager.setGameObjectState(spyMenu.Find("MenuCampaigns").gameObject, false); // be sure campaign menu is disabled
				GameObjectManager.setGameObjectState(spyMenu.Find("MenuLevels").gameObject, true); // enable levels menu
				EventSystem.current.SetSelectedGameObject(spyMenu.Find("MenuLevels").Find("Retour").gameObject);
			}
			
		}

		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			ShowHtmlButtons();
			GameObjectManager.setGameObjectState(quitButton, false);
		}

		// Disable MainMenu while loading
		GameObjectManager.setGameObjectState(mainMenu, false);
		// wait level loading
		MainLoop.instance.StartCoroutine(WaitLoadingData());
	}

	private IEnumerator delayOpeningScenarioEditor()
    {
		yield return new WaitForEndOfFrame();
		compLevelButton.GetComponent<Button>().onClick.Invoke();
	}

	private void initGBLXAPI()
	{
		if (!GBLXAPI.IsInit)
			GBLXAPI.Init(GBL_Interface.lrsAddresses);

		GBLXAPI.debugMode = false;

		webGL_fileToLoad++;
		MainLoop.instance.StartCoroutine(FindAvailableSessionId());
	}

	// see CloseSettings button in SettingsWindow in TitleScreen scene
	public void closeSettingsAndSelectNextFocusedButton(GameObject settingsWindows)
    {
		GameObjectManager.setGameObjectState(settingsWindows, false);
		if (sessionIdPanel.activeInHierarchy)
			EventSystem.current.SetSelectedGameObject(sessionIdPanel.transform.Find("ShowSessionId").Find("Settings").gameObject);
		else
			EventSystem.current.SetSelectedGameObject(playButton.transform.parent.Find("Parameters").gameObject);
    }

    protected override void onProcess(int familiesUpdateCount)
	{

		if (Input.GetKeyDown(KeyCode.Tab))
		{
			if (virtualKeyboard.activeInHierarchy)
				EventSystem.current.SetSelectedGameObject(virtualKeyboard.transform.Find("Panel").Find("Close").gameObject);
			else
				EventSystem.current.SetSelectedGameObject(f_buttons.getAt(f_buttons.Count - 1));
		}
		// Get the currently selected UI element from the event system.
		GameObject selected = EventSystem.current.currentSelectedGameObject;
		// Do nothing if there are none OR if the selected game object is not a child of a scroll rect OR if the selected game object is the same as it was last frame,
		// meaning we haven't to move.
		if (selected != null && selected.GetComponentInParent<ScrollRect>() != null && selected != lastSelected)
		{
			// Get the content
			RectTransform viewport = selected.GetComponentInParent<ScrollRect>().viewport;
			RectTransform contentPanel = selected.GetComponentInParent<ScrollRect>().content;

			float selectedInContent_Y = Mathf.Abs(contentPanel.InverseTransformPoint(selected.transform.position).y);

			Vector2 targetAnchoredPosition = new Vector2(contentPanel.anchoredPosition.x, contentPanel.anchoredPosition.y);
			// we auto focus on selected object only if it is not visible
			if (selectedInContent_Y - contentPanel.anchoredPosition.y < 0 || (selectedInContent_Y + (selected.transform as RectTransform).rect.height) - contentPanel.anchoredPosition.y > viewport.rect.height)
			{
				// check if selected object is too high
				if (selectedInContent_Y - contentPanel.anchoredPosition.y < 0)
				{
					targetAnchoredPosition = new Vector2(
						targetAnchoredPosition.x,
						selectedInContent_Y
					);
				}
				// selected object is too low
				else 
				{
					targetAnchoredPosition = new Vector2(
						targetAnchoredPosition.x,
						-viewport.rect.height + selectedInContent_Y + (selected.transform as RectTransform).rect.height
					);
				}
				
				contentPanel.anchoredPosition = targetAnchoredPosition;
			}
			lastSelected = selected;
		}
	}

    // See Ok button in SetClass panel in TitleScreen scene
    public void synchUserData(GameObject go)
    {
		if (userData.progression == null)
			userData.progression = new Dictionary<string, int>();
		if (userData.highScore == null)
			userData.highScore = new Dictionary<string, int>();
		userData.schoolClass = go.GetComponentInChildren<TMP_InputField>().text;
		userData.isTeacher = go.GetComponentInChildren<Toggle>().isOn;
		GameObjectManager.addComponent<SendUserData>(MainLoop.instance.gameObject);
	}

	// See Ok button in SetSessionId panel in TitleScreen scene
	public void GetProgression(TMP_InputField idSession)
    {
		webGL_fileToLoad = 1;
		webGL_fileLoaded = 0;
		string formatedString = idSession.text.ToUpper();
		formatedString = String.Concat(formatedString.Where(c => !Char.IsWhiteSpace(c)));
		
		GameObjectManager.setGameObjectState(loadingScreen, true);

		MainLoop.instance.StartCoroutine(GetProgressionWebRequest(formatedString));
		MainLoop.instance.StartCoroutine(WaitLoadingData());
	}

	private IEnumerator FindAvailableSessionId()
    {
		string sessionID = Environment.MachineName + "-" + DateTime.Now.ToString("yyyy.MM.dd.hh.mm.ss"); //Generate player name unique to each playing session (computer name + date)

		string formatedString = String.Format("{0:X}", sessionID.GetHashCode());

		// Make a request to check if this sessionId is already used
		UnityWebRequest www = UnityWebRequest.Get("https://spy.lip6.fr/ServerREST_LIP6/?idSession=" + formatedString);
		logs.text = "";
		progress.text = "0%";
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			logs.text = "<color=\"red\">" + Utility.getFormatedText(gameData.localization[21], formatedString) + "</color>\n" + logs.text;
			yield return new WaitForSeconds(0.5f);
			if (webGL_fileLoaded < webGL_fileToLoad) // recursive call while player does not cancel loading
			{
				logs.text = "<color=\"orange\">" + Utility.getFormatedText(gameData.localization[22], formatedString) + "</color>\n" + logs.text;
				MainLoop.instance.StartCoroutine(FindAvailableSessionId());
			}
			GameObjectManager.setGameObjectState(loadingScreen.transform.Find("ForceLaunch").gameObject, true);
        }
        else
		{
			// If content is "", means this sessionId is available (no progression data associated to this sessionId)
			if (www.downloadHandler.text == "")
			{
				webGL_fileLoaded++;
				GBL_Interface.playerName = formatedString;
				GBL_Interface.userUUID = formatedString;
				foreach (GameObject go in f_sessionId)
					go.GetComponent<TMP_Text>().text = formatedString;

				userData.progression = null;
				userData.highScore = null;

				if (gameData.sendStatementEnabled || !Application.isEditor)
				{
					GameObjectManager.setGameObjectState(sessionIdPanel, true);
					// select ok button
					EventSystem.current.SetSelectedGameObject(sessionIdPanel.transform.Find("ShowSessionId").Find("Buttons").Find("OkButton").gameObject);
				}
			}
			else {
				// means this sessionId is already used, try to find another
				MainLoop.instance.StartCoroutine(FindAvailableSessionId());
			}
		}
	}

	private IEnumerator GetProgressionWebRequest(string idSession)
    {
		UnityWebRequest www = UnityWebRequest.Get("https://spy.lip6.fr/ServerREST_LIP6/?idSession=" + idSession);
		logs.text = "";
		progress.text = "0%";
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			logs.text = "<color=\"red\">" + Utility.getFormatedText(gameData.localization[23], idSession) + "</color>\n" + logs.text;
			yield return new WaitForSeconds(0.5f);
			if (webGL_fileLoaded < webGL_fileToLoad) // recursive call while player does not cancel loading
			{
				logs.text = "<color=\"orange\">" + Utility.getFormatedText(gameData.localization[24], idSession) + "</color>\n" + logs.text;
				MainLoop.instance.StartCoroutine(GetProgressionWebRequest(idSession));
			}
			GameObjectManager.setGameObjectState(loadingScreen.transform.Find("ForceLaunch").gameObject, true);
		}
		else
		{
			webGL_fileLoaded++;
			if (www.downloadHandler.text == "")
			{
				localCallback = null;
				localCallback += delegate
				{
					GameObjectManager.setGameObjectState(sessionIdPanel, true);
					GameObjectManager.setGameObjectState(sessionIdPanel.transform.Find("ShowSessionId").gameObject, false);
					GameObjectManager.setGameObjectState(sessionIdPanel.transform.Find("SetSessionId").gameObject, true);
				};
				GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(gameData.localization[16], idSession), OkButton = gameData.localization[5], CancelButton = gameData.localization[0], call = localCallback });
			}
			else
			{
				string[] stringSeparators = new string[] { "#SEP#" };
				string[] tokens = www.downloadHandler.text.Split(stringSeparators, StringSplitOptions.None);
				Debug.Log(www.downloadHandler.text);
				if (tokens.Length != 4)
				{
					Debug.LogWarning(www.error);
					localCallback = null;
					localCallback += delegate
					{
						GameObjectManager.setGameObjectState(sessionIdPanel, true);
						GameObjectManager.setGameObjectState(sessionIdPanel.transform.Find("ShowSessionId").gameObject, false);
						GameObjectManager.setGameObjectState(sessionIdPanel.transform.Find("SetSessionId").gameObject, true);
					};
					GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = gameData.localization[17], OkButton = gameData.localization[5], CancelButton = gameData.localization[0], call = localCallback });
				}
				else
				{
					localCallback = null;
					GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(gameData.localization[18], idSession), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
					userData.progression = JsonConvert.DeserializeObject<Dictionary<string, int>>(tokens[0]);
					if (userData.progression == null)
						userData.progression = new Dictionary<string, int>();
					userData.highScore = JsonConvert.DeserializeObject<Dictionary<string, int>>(tokens[1]);
					if (userData.highScore == null)
						userData.highScore = new Dictionary<string, int>();
					userData.schoolClass = tokens[2];
					userData.isTeacher = tokens[3] == "1";
					Transform setClass = sessionIdPanel.transform.Find("SetClass");
					setClass.GetComponentInChildren<TMP_InputField>(true).text = userData.schoolClass;
					setClass.GetComponentInChildren<Toggle>().isOn = userData.isTeacher;
					GBL_Interface.playerName = idSession;
					GBL_Interface.userUUID = idSession;
				}
				foreach (GameObject go in f_sessionId)
					go.GetComponent<TMP_Text>().text = GBL_Interface.playerName;
			}
		}
	}

	private void GetScenariosAndLevels()
	{
		logs.text = "";
		progress.text = "0%";
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			// Load scenario and levels from server
			webGL_fileToLoad += 2;
			MainLoop.instance.StartCoroutine(GetScenarioWebRequest());
			MainLoop.instance.StartCoroutine(GetLevelsWebRequest());
		}
		else
		{
			// Load scenario and levels from disk
			// explore streaming asstets path
			MainLoop.instance.StartCoroutine(exploreLevelsAndScenarios(Application.streamingAssetsPath));
			// explore persistent data path
			MainLoop.instance.StartCoroutine(exploreLevelsAndScenarios(Application.persistentDataPath));
		}
	}

	private IEnumerator WaitLoadingData()
	{
		yield return new WaitForSeconds(1f);

		while (webGL_fileLoaded < webGL_fileToLoad || f_localizationLoaded.Count == 0)
			yield return null;

		// and, if require, we can load requested level by URL
		if (loadLevelWithURL != "")
			GameObjectManager.addComponent<AskToTestLevel>(MainLoop.instance.gameObject, new { url = loadLevelWithURL });
		// Disable Loading screen
		GameObjectManager.setGameObjectState(loadingScreen, false);
		// Enable MainMenu if we not come back from playing a scenario level
		if (gameData.selectedScenario == "" || gameData.selectedScenario == Utility.testFromUrl)
			GameObjectManager.setGameObjectState(mainMenu, true);
	}

	private IEnumerator GetScenarioWebRequest()
	{
		string uri = new Uri(Application.streamingAssetsPath + "/WebGlData/ScenarioList.json").AbsoluteUri;
		UnityWebRequest www = UnityWebRequest.Get(uri);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			logs.text = "<color=\"red\">("+gameData.localization[25]+") " + uri + "</color>\n" + logs.text;
			yield return new WaitForSeconds(0.5f);
			if (webGL_fileLoaded < webGL_fileToLoad) // recursive call while player does not force launching
			{
				logs.text = "<color=\"orange\">(" + gameData.localization[26] + ") " + uri + "</color>\n" + logs.text;
				MainLoop.instance.StartCoroutine(GetScenarioWebRequest());
			}
			GameObjectManager.setGameObjectState(loadingScreen.transform.Find("ForceLaunch").gameObject, true);
		}
		else
		{
			webGL_fileLoaded++;
			string scenarioJson = www.downloadHandler.text;
			WebGlScenarioList scenarioListRaw = JsonConvert.DeserializeObject<WebGlScenarioList>(scenarioJson);
			foreach (WebGlScenario scenarioRaw in scenarioListRaw.scenarios)
			{
				gameData.scenarios[scenarioRaw.key] = scenarioRaw;
				foreach (DataLevel levelPath in scenarioRaw.levels)
				{
					levelPath.src = new Uri(Application.streamingAssetsPath + "/" + levelPath.src).AbsoluteUri;
				}
			}
		}
	}

	private IEnumerator GetLevelsWebRequest()
	{
		string uri = new Uri(Application.streamingAssetsPath + "/WebGlData/LevelsList.json").AbsoluteUri;
		UnityWebRequest www = UnityWebRequest.Get(uri);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			logs.text = "<color=\"red\">(" + gameData.localization[25] + ") " + uri + "</color>\n" + logs.text;
			yield return new WaitForSeconds(0.5f);
			if (webGL_fileLoaded < webGL_fileToLoad) // recursive call while player does not force launching
			{
				logs.text = "<color=\"orange\">(" + gameData.localization[26] + ") " + uri + "</color>\n" + logs.text;
				MainLoop.instance.StartCoroutine(GetLevelsWebRequest());
			}
			GameObjectManager.setGameObjectState(loadingScreen.transform.Find("ForceLaunch").gameObject, true);
		}
		else
		{
			webGL_fileLoaded++;
			string levelsJson = www.downloadHandler.text;
			WebGlScenario levelsListRaw = JsonUtility.FromJson<WebGlScenario>(levelsJson);
			webGL_fileToLoad += levelsListRaw.levels.Count;
			// try to load all levels
			foreach (DataLevel levelRaw in levelsListRaw.levels)
				MainLoop.instance.StartCoroutine(GetLevelOrScenario_WebRequest(new Uri(Application.streamingAssetsPath + "/" + levelRaw.src).AbsoluteUri));
		}
	}

	private IEnumerator exploreLevelsAndScenarios(string path)
	{
		// try to load all child files
		string[] files = Directory.GetFiles(path,"*.xml");
		webGL_fileToLoad += files.Length;
		foreach (string fileName in files)
		{
			yield return null;
			MainLoop.instance.StartCoroutine(GetLevelOrScenario_WebRequest(fileName));
		}

		// explore subdirectories
		foreach (string directory in Directory.GetDirectories(path))
		{
			yield return null;
			MainLoop.instance.StartCoroutine(exploreLevelsAndScenarios(directory));
		}
	}

	private IEnumerator GetLevelOrScenario_WebRequest(string uri)
	{
		UnityWebRequest www = UnityWebRequest.Get(uri);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			logs.text = "<color=\"red\">(" + gameData.localization[25] + ") " + uri + "</color>\n"+ logs.text;
			yield return new WaitForSeconds(0.5f);
			if (webGL_fileLoaded < webGL_fileToLoad) // recursive call while player does not force launching
			{
				logs.text = "<color=\"orange\">(" + gameData.localization[26] + ") " + uri + "</color>\n" + logs.text;
				MainLoop.instance.StartCoroutine(GetLevelOrScenario_WebRequest(uri));
			}
			GameObjectManager.setGameObjectState(loadingScreen.transform.Find("ForceLaunch").gameObject, true);
		}
		else
		{
			webGL_fileLoaded++;
			progress.text = Mathf.Floor(((float)webGL_fileLoaded / webGL_fileToLoad) * 100) + "%";
			logs.text = "<color=\"green\">(" + gameData.localization[1] + ") " + uri + "</color>\n"+ logs.text;
			string xmlContent = www.downloadHandler.text;
			try
			{
				LoadLevelOrScenario(uri, xmlContent);
			}
			catch (Exception e)
			{
				logs.text = "<color=\"red\">(" + gameData.localization[25] + ") " + uri+" => "+e.Message+ "</color>\n" + logs.text;
			}
		}
	}

	// See ForceLaunch button
	public void forceLaunch()
    {

		Debug.Log(webGL_fileLoaded + " " + webGL_fileToLoad + " " + f_localizationLoaded.Count);
		webGL_fileLoaded = webGL_fileToLoad;
	}

	public void LoadLevelOrScenario(string uri, string xmlContent)
    {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml(xmlContent);
		Utility.removeComments(doc);
		// a valid level must have only one tag "level" and no tag "scenario"
		if (doc.GetElementsByTagName("level").Count == 1 && doc.GetElementsByTagName("scenario").Count == 0)
			gameData.levels[new Uri(uri).AbsoluteUri] = doc.GetElementsByTagName("level")[0];
		// a valid scenario must have only one tag "scenario"
		else if (doc.GetElementsByTagName("scenario").Count == 1)
			updateScenarioContent(uri, doc);
		else
			throw new Exception("\"" + uri + "\"" + gameData.localization[27]);
	}

	public void updateScenarioContent(string uri, XmlDocument doc)
	{
		WebGlScenario scenario = new WebGlScenario();
		scenario.key = Path.GetFileNameWithoutExtension(uri);
		XmlNode xmlScenario = doc.GetElementsByTagName("scenario")[0];
		if (xmlScenario.Attributes.GetNamedItem("name") != null)
			scenario.name = xmlScenario.Attributes.GetNamedItem("name").Value;
		else
			scenario.name = scenario.key;
		if (xmlScenario.Attributes.GetNamedItem("desc") != null)
			scenario.description = xmlScenario.Attributes.GetNamedItem("desc").Value;
		else
			scenario.description = "";
		scenario.levels = new List<DataLevel>();
		foreach (XmlNode child in doc.GetElementsByTagName("scenario")[0])
			if (child.Name.Equals("level"))
			{
				DataLevel dl = new DataLevel();
				// get src
				dl.src = new Uri(Application.streamingAssetsPath + "/" + (child.Attributes.GetNamedItem("src").Value)).AbsoluteUri;

				// get name
				if (child.Attributes.GetNamedItem("name") != null)
					dl.name = child.Attributes.GetNamedItem("name").Value;
				else
					// if not def, use file name
					dl.name = Path.GetFileNameWithoutExtension(dl.src);

				// load overrided dialogs
				foreach (XmlNode subChild in child.ChildNodes)
					if (subChild.Name.Equals("dialogs"))
					{
						dl.overridedDialogs = new List<Dialog>();
						Utility.readXMLDialogs(subChild, dl.overridedDialogs);
						break;
					}

				scenario.levels.Add(dl);
			}
		gameData.scenarios[scenario.key] = scenario;
	}

	private class JavaScriptData
	{
		public string name;
		public string content;
	}

	// Fonction appelée depuis le javascript (voir Assets/WebGLTemplates/Custom/index.html) via le Wrapper du Système
	public void importLevelOrScenario(string content)
	{
		JavaScriptData jsd = JsonUtility.FromJson<JavaScriptData>(content);
		try
		{
			string fakeUri = Application.streamingAssetsPath + "/Levels/LocalFiles/" + jsd.name; 
			LoadLevelOrScenario(fakeUri, jsd.content);
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = gameData.localization[19], OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
		}
		catch (Exception e) {
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(gameData.localization[20], jsd.name, e.Message), OkButton = gameData.localization[0], CancelButton = gameData.localization[1], call = localCallback });
		}
	}

	// see Play Button in TitleScreen scene
	public void displayScenarioList()
	{
		// remove all old scenario
		foreach (Transform child in listOfCampaigns.transform)
		{
			GameObjectManager.unbind(child.gameObject);
			GameObject.Destroy(child.gameObject);
		}

		//create scenarios' button
		List<string> sortedScenarios = new List<string>();
		foreach (string key in gameData.scenarios.Keys)
			if (key != Utility.testFromScenarioEditor && key != Utility.testFromLevelEditor && key !=  Utility.testFromUrl && key != Utility.editingScenario) // we don't create a button for tested level
				sortedScenarios.Add(key);
		sortedScenarios.Sort();
		foreach (string key in sortedScenarios)
		{
			GameObject directoryButton = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/Button") as GameObject, listOfCampaigns.transform);
			directoryButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Utility.extractLocale(gameData.scenarios[key].name);
			GameObjectManager.bind(directoryButton);
			// add on click
			directoryButton.GetComponent<Button>().onClick.AddListener(delegate { showScenarioDetails(key); });
		}
	}

	private void showScenarioDetails(string campaignKey)
	{
		detailsCampaign.SetActive(false); // in order to play animation on next activation
		GameObjectManager.setGameObjectState(detailsCampaign, true);
		Transform content = detailsCampaign.transform.GetChild(0).GetChild(0).GetChild(0);

		TMP_Text title = content.GetChild(0).GetComponent<TMP_Text>();
		title.text = campaignKey;

		TMP_Text campaignDescription = content.GetChild(1).GetComponent<TMP_Text>();
		campaignDescription.text = Utility.extractLocale(gameData.scenarios[campaignKey].description)+"\n\n";

		content.gameObject.AddComponent<AskToRefreshCompetencies>();
		delayRefreshCompetencies(content);

		Button bt_showLevels = detailsCampaign.transform.GetComponentInChildren<Button>();
		bt_showLevels.onClick.RemoveAllListeners();
		bt_showLevels.onClick.AddListener(delegate { showLevels(campaignKey); });
		bt_showLevels.onClick.AddListener(delegate { GameObjectManager.setGameObjectState(detailsCampaign, false); });
	}

	public void delayRefreshCompetencies(Transform content)
    {
		MainLoop.instance.StartCoroutine(refreshCompetencies(content));
    }

	public IEnumerator refreshCompetencies(Transform content)
    {
		yield return new WaitForSeconds(0.1f);
		TMP_Text compDetails = content.GetChild(4).GetComponent<TMP_Text>();
		if (gameData.scenarios.ContainsKey(content.GetChild(0).GetComponent<TMP_Text>().text))
		{
			// Display competencies
			compDetails.text = "<b>"+gameData.localization[28]+"</b>\n";
			string txt = "";
			foreach (GameObject comp in f_competencies)
			{
				foreach (DataLevel levelKey in gameData.scenarios[content.GetChild(0).GetComponent<TMP_Text>().text].levels)
					if (gameData.levels.ContainsKey(levelKey.src) && Utility.isCompetencyMatchWithLevel(comp.GetComponent<Competency>(), gameData.levels[levelKey.src].OwnerDocument))
					{
						txt += "\t" + Utility.extractLocale(comp.GetComponent<Competency>().id) + "\n";
						break;
					}
			}
			if (txt != "")
				compDetails.text += txt;
			else
				compDetails.text += "\t"+ gameData.localization[29] + "\n";
			LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
			// auto move to the top od the panel
			(content as RectTransform).anchoredPosition = new Vector2(0, 0);
		}
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

		// set scenario name
		listOfLevels.transform.parent.parent.parent.GetChild(0).GetComponent<TMP_Text>().text = Utility.extractLocale(gameData.scenarios[campaignKey].name);
		// create level buttons for this campaign
		for (int i = 0; i < gameData.scenarios[campaignKey].levels.Count; i++)
		{
			DataLevel levelData = gameData.scenarios[campaignKey].levels[i];
			GameObject button_go = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/LevelButton") as GameObject, listOfLevels.transform);
			button_go.transform.Find("Button").GetChild(0).GetComponent<TextMeshProUGUI>().text = Utility.extractLocale(levelData.name);
			int id = i;
			button_go.transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate { launchLevel(campaignKey, id); });
			GameObjectManager.bind(button_go);

			Button button = button_go.GetComponentInChildren<Button>();
			// lock/unlock levels
			if ((userData.progression != null && userData.progression.ContainsKey(campaignKey) && userData.progression[campaignKey] >= i) || (userData.progression == null && PlayerPrefs.GetInt(campaignKey, 0) >= i) || i == 0) //by default first level of directory is the only unlocked level of directory
				button.interactable = true;
			else
				button.interactable = false;

			//scores
			string highScoreKey = Utility.extractFileName(levelData.src);
			int scoredStars = (userData.highScore != null ? (userData.highScore.ContainsKey(highScoreKey) ? userData.highScore[highScoreKey] : 0) : PlayerPrefs.GetInt(highScoreKey + gameData.scoreKey, 0)); //0 star by default
			Transform scoreCanvas = button_go.transform.Find("ScoreCanvas");
			for (int nbStar = 0; nbStar < 4; nbStar++)
			{
				if (nbStar == scoredStars && button.interactable) // n'activer les étoiles que pour les niveaux actifs
					GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, true);
				else
					GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, false);
			}
		}
	}

	public void launchLevel(string campaignKey, int levelToLoad) {
		gameData.selectedScenario = campaignKey;
		gameData.levelToLoad = levelToLoad;
		GameObjectManager.loadScene("MainScene");
	}

	public void launchLevelEditor()
    {
		GameObjectManager.loadScene("EditorScene");
	}

	// Fonction appelée depuis le javascript (voir Assets/WebGLTemplates/Custom/index.html) via le Wrapper du Système
	public void askToLoadLevel(string levelToLoad)
    {
		loadLevelWithURL = levelToLoad;
	}

	// Fonction appelée depuis le javascript (voir Assets/WebGLTemplates/Custom/index.html) via le Wrapper du Système
	public void enableSendStatement()
	{
		if (gameData == null)
			webGL_askToEnableSendSystem = true;
		else
			gameData.sendStatementEnabled = true;
	}

	// See Quitter button in editor
	public void quitGame(){
		Application.Quit();
	}
}