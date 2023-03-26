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

/// <summary>
/// Manage main menu to launch a specific mission
/// </summary>
public class TitleScreenSystem : FSystem {
	private Family f_sessionId = FamilyManager.getFamily(new AllOfComponents(typeof(TextMeshProUGUI)), new AnyOfTags("SessionId"));
	private Family f_competencies = FamilyManager.getFamily(new AllOfComponents(typeof(Competency))); // Les compétences

	private Family f_buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	private GameData gameData;
	private UserData userData;
	public GameObject prefabGameData;
	public GameObject mainCanvas;
	public GameObject compLevelButton;
	public GameObject listOfCampaigns;
	public GameObject listOfLevels;
	public GameObject loadingScenarioContent;
	public GameObject scenarioContent;
	public GameObject playButton;
	public GameObject quitButton;
	public GameObject loadingScreen;
	public GameObject sessionIdPanel;
	public GameObject deletableElement;
	public TMP_InputField scenarioAbstract;
	public GameObject detailsCampaign;
	public GameObject virtualKeyboard;
	public TMP_Text progress;
	public TMP_Text logs;

	private GameObject selectedScenarioGO;
	private UnityAction localCallback;

	private GameObject lastSelected;

	private string loadLevelWithURL = "";
	private int webGL_fileLoaded = 0;
	private int webGL_fileToLoad = 0;
	private bool webGL_askToEnableSendSystem = false;

	private const string testLevelToken = "testLevel";

	[DllImport("__Internal")]
	private static extern void ShowHtmlButtons(); // call javascript

	[DllImport("__Internal")]
	private static extern void DownloadLevel(string uri); // call javascript

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
			MainLoop.instance.StartCoroutine(GetScenariosAndLevels());
		}
		else // means we come back from a playing session, streaming assets are already loaded
		{
			Transform spyMenu = playButton.transform.parent.parent;
			if (gameData.selectedScenario == testLevelToken) // reload scenario editor
            {
                MainLoop.instance.StartCoroutine(delayOpeningScenarioEditor());
				EventSystem.current.SetSelectedGameObject(spyMenu.Find("MenuLevels").Find("Retour").gameObject);
			}
            else
            {
				// reload last openned scenario
				playButton.GetComponent<Button>().onClick.Invoke();
				showLevels(gameData.selectedScenario);
				GameObjectManager.setGameObjectState(spyMenu.Find("MenuCampaigns").gameObject, false); // be sure campaign menu is disabled
				GameObjectManager.setGameObjectState(spyMenu.Find("MenuLevels").gameObject, true); // enable levels menu
				EventSystem.current.SetSelectedGameObject(spyMenu.Find("MenuLevels").Find("Retour").gameObject);
			}
			
		}

		selectedScenarioGO = null;
	}

	private IEnumerator delayOpeningScenarioEditor()
    {
		yield return new WaitForEndOfFrame();
		compLevelButton.GetComponent<Button>().onClick.Invoke();
	}

	public void initGBLXAPI()
	{
		if (!GBLXAPI.IsInit)
			GBLXAPI.Init(GBL_Interface.lrsAddresses);

		GBLXAPI.debugMode = false;

		string sessionID = Environment.MachineName + "-" + DateTime.Now.ToString("yyyy.MM.dd.hh.mm.ss");
		//Generate player name unique to each playing session (computer name + date + hour)
		GBL_Interface.playerName = String.Format("{0:X}", sessionID.GetHashCode());
		GBL_Interface.userUUID = GBL_Interface.playerName;
		foreach (GameObject go in f_sessionId)
			go.GetComponent<TMP_Text>().text = GBL_Interface.playerName;

		userData.progression = null;
		userData.highScore = null;

		if (gameData.sendStatementEnabled || !Application.isEditor)
		{
			GameObjectManager.setGameObjectState(sessionIdPanel, true);
			// select ok button
			EventSystem.current.SetSelectedGameObject(sessionIdPanel.transform.Find("ShowSessionId").Find("Buttons").Find("OkButton").gameObject);
		}
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
		MainLoop.instance.StartCoroutine(GetProgressionWebRequest(formatedString));
		MainLoop.instance.StartCoroutine(WaitLoadingData());
	}

	private IEnumerator GetProgressionWebRequest(string idSession)
    {
		UnityWebRequest www = UnityWebRequest.Get("https://spy.lip6.fr/ServerREST_LIP6/?idSession="+idSession);
		GameObjectManager.setGameObjectState(loadingScreen, true);
		logs.text = "";
		progress.text = "0%";
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			logs.text = "<color=\"red\">Echec de récupération des données de la session \"" + idSession + "\"</color>\n" + logs.text;
			yield return new WaitForSeconds(0.5f);
			if (webGL_fileLoaded < webGL_fileToLoad) // recursive call while player does not cancel loading
			{
				logs.text = "<color=\"orange\">Nouvelle tentative de récupération des données de la session \"" + idSession + "\"</color>\n" + logs.text;
				MainLoop.instance.StartCoroutine(GetProgressionWebRequest(idSession));
			}
			GameObjectManager.setGameObjectState(loadingScreen.transform.Find("ForceLaunch").gameObject, true);
		}
		else if (www.downloadHandler.text == "")
		{
			webGL_fileLoaded++;
			localCallback = null;
			localCallback += delegate {
				GameObjectManager.setGameObjectState(sessionIdPanel, true);
				GameObjectManager.setGameObjectState(sessionIdPanel.transform.Find("ShowSessionId").gameObject, false);
				GameObjectManager.setGameObjectState(sessionIdPanel.transform.Find("SetSessionId").gameObject, true);
			};
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Impossible de récupérer les données de progression pour \"" + idSession + "\".", OkButton = "Réessayer", CancelButton = "", call = localCallback });
		}
		else
		{
			webGL_fileLoaded++;
			string[] stringSeparators = new string[] { "#SEP#" };
			string[] tokens = www.downloadHandler.text.Split(stringSeparators, StringSplitOptions.None);
			Debug.Log(www.downloadHandler.text);
			if (tokens.Length != 4)
			{
				Debug.LogWarning(www.error);
				localCallback = null;
				localCallback += delegate {
					GameObjectManager.setGameObjectState(sessionIdPanel, true);
					GameObjectManager.setGameObjectState(sessionIdPanel.transform.Find("ShowSessionId").gameObject, false);
					GameObjectManager.setGameObjectState(sessionIdPanel.transform.Find("SetSessionId").gameObject, true);
				};
				GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Session corrompue, veuillez entrer un nouveau code de session.", OkButton = "Réessayer", CancelButton = "", call = localCallback });
			}
			else
			{
				localCallback = null;
				GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Session \"" + idSession + "\" chargée avec succès.", OkButton = "", CancelButton = "Ok", call = localCallback });
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

	private IEnumerator GetScenariosAndLevels()
	{
		// Enable Loading screen
		GameObjectManager.setGameObjectState(loadingScreen, true);
		logs.text = "";
		progress.text = "0%";
		webGL_fileLoaded = 0;
		webGL_fileToLoad = 0;
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			ShowHtmlButtons();
			GameObjectManager.setGameObjectState(quitButton, false);
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

		yield return new WaitForSeconds(0.5f);

		// wait level loading
		MainLoop.instance.StartCoroutine(WaitLoadingData());
	}

	private IEnumerator WaitLoadingData()
    {
		while (webGL_fileLoaded < webGL_fileToLoad)
			yield return null;
		// Now, if require, we can load requested level by URL
		if (loadLevelWithURL != "")
			testLevelPath(loadLevelWithURL);
		// Disable Loading screen
		GameObjectManager.setGameObjectState(loadingScreen, false);
	}

	private IEnumerator GetScenarioWebRequest()
	{
		string uri = new Uri(Application.streamingAssetsPath + "/WebGlData/ScenarioList.json").AbsoluteUri;
		UnityWebRequest www = UnityWebRequest.Get(uri);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			logs.text = "<color=\"red\">(Echec) " + uri + "</color>\n" + logs.text;
			yield return new WaitForSeconds(0.5f);
			if (webGL_fileLoaded < webGL_fileToLoad) // recursive call while player does not force launching
			{
				logs.text = "<color=\"orange\">(Nouvel essai) " + uri + "</color>\n" + logs.text;
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
				gameData.scenarios[scenarioRaw.scenarioName] = scenarioRaw;
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
			logs.text = "<color=\"red\">(Echec) " + uri + "</color>\n" + logs.text;
			yield return new WaitForSeconds(0.5f);
			if (webGL_fileLoaded < webGL_fileToLoad) // recursive call while player does not force launching
			{
				logs.text = "<color=\"orange\">(Nouvel essai) " + uri + "</color>\n" + logs.text;
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
			logs.text = "<color=\"red\">(Echec) " + uri + "</color>\n"+ logs.text;
			yield return new WaitForSeconds(0.5f);
			if (webGL_fileLoaded < webGL_fileToLoad) // recursive call while player does not force launching
			{
				logs.text = "<color=\"orange\">(Nouvel essai) " + uri + "</color>\n" + logs.text;
				MainLoop.instance.StartCoroutine(GetLevelOrScenario_WebRequest(uri));
			}
			GameObjectManager.setGameObjectState(loadingScreen.transform.Find("ForceLaunch").gameObject, true);
		}
		else
		{
			webGL_fileLoaded++;
			progress.text = Mathf.Floor(((float)webGL_fileLoaded / webGL_fileToLoad) * 100) + "%";
			logs.text = "<color=\"green\">(Ok) " + uri + "</color>\n"+ logs.text;
			string xmlContent = www.downloadHandler.text;
			try
			{
				LoadLevelOrScenario(uri, xmlContent);
			}
			catch (Exception e)
			{
				logs.text = "<color=\"red\">(Echec) "+uri+" => "+e.Message+ "</color>\n" + logs.text;
			}
		}
	}

	public void forceLaunch()
    {
		webGL_fileLoaded = webGL_fileToLoad;
	}

	private void LoadLevelOrScenario(string uri, string xmlContent)
    {
		XmlDocument doc = new XmlDocument();
		doc.LoadXml(xmlContent);
		EditingUtility.removeComments(doc);
		// a valid level must have only one tag "level" and no tag "scenario"
		if (doc.GetElementsByTagName("level").Count == 1 && doc.GetElementsByTagName("scenario").Count == 0)
			gameData.levels[new Uri(uri).AbsoluteUri] = doc.GetElementsByTagName("level")[0];
		// a valid scenario must have only one tag "scenario"
		else if (doc.GetElementsByTagName("scenario").Count == 1)
			updateScenarioContent(uri, doc);
		else
			throw new Exception("\"" + uri + "\" n'est pas un scénario ou un niveau valide!");
	}

	public void updateScenarioContent(string uri, XmlDocument doc)
	{
		WebGlScenario scenario = new WebGlScenario();
		scenario.scenarioName = Path.GetFileNameWithoutExtension(uri);
		XmlNode xmlScenario = doc.GetElementsByTagName("scenario")[0];
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
						EditingUtility.readXMLDialogs(subChild, dl.overridedDialogs);
						break;
					}

				scenario.levels.Add(dl);
			}
		gameData.scenarios[Path.GetFileNameWithoutExtension(uri)] = scenario;
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
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Fichier(s) chargé(s)", OkButton = "", CancelButton = "OK", call = localCallback });
		}
		catch (Exception e) {
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = "Erreur lors du chargement du fichier \""+jsd.name+"\" : "+e.Message, OkButton = "", CancelButton = "OK", call = localCallback });
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
			if (key != testLevelToken) // we don't create a button for testLevel scenario
				sortedScenarios.Add(key);
		sortedScenarios.Sort();
		foreach (string key in sortedScenarios)
		{
			GameObject directoryButton = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/Button") as GameObject, listOfCampaigns.transform);
			directoryButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Path.GetFileNameWithoutExtension(key);
			GameObjectManager.bind(directoryButton);
			// add on click
			directoryButton.GetComponent<Button>().onClick.AddListener(delegate { showDetails(key); });
		}
	}

	private void showDetails(string campaignKey)
	{
		detailsCampaign.SetActive(false); // in order to play animation on next activation
		GameObjectManager.setGameObjectState(detailsCampaign, true);
		Transform content = detailsCampaign.transform.GetChild(0).GetChild(0).GetChild(0);

		TMP_Text title = content.GetChild(0).GetComponent<TMP_Text>();
		title.text = campaignKey;

		TMP_Text campaignDescription = content.GetChild(1).GetComponent<TMP_Text>();
		campaignDescription.text = gameData.scenarios[campaignKey].description+"\n\n";

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
			compDetails.text = "<b>Compétences en jeu dans ce scénario :</b>\n";
			string txt = "";
			foreach (GameObject comp in f_competencies)
			{
				foreach (DataLevel levelKey in gameData.scenarios[content.GetChild(0).GetComponent<TMP_Text>().text].levels)
					if (gameData.levels.ContainsKey(levelKey.src) && EditingUtility.isCompetencyMatchWithLevel(comp.GetComponent<Competency>(), gameData.levels[levelKey.src].OwnerDocument))
					{
						txt += "\t" + comp.GetComponent<Competency>().GetComponentInChildren<TMP_Text>().text + "\n";
						break;
					}
			}
			if (txt != "")
				compDetails.text += txt;
			else
				compDetails.text += "\tAucune compétence identifiée dans ce scénario\n";
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
		listOfLevels.transform.parent.parent.parent.GetChild(0).GetComponent<TMP_Text>().text = Path.GetFileNameWithoutExtension(campaignKey);
		// create level buttons for this campaign
		for (int i = 0; i < gameData.scenarios[campaignKey].levels.Count; i++)
		{
			DataLevel levelData = gameData.scenarios[campaignKey].levels[i];
			GameObject button = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/LevelButton") as GameObject, listOfLevels.transform);
			button.transform.Find("Button").GetChild(0).GetComponent<TextMeshProUGUI>().text = levelData.name;
			int id = i;
			button.transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate { launchLevel(campaignKey, id); });
			GameObjectManager.bind(button);
			//locked levels
			if ((userData.progression != null && userData.progression.ContainsKey(campaignKey) && userData.progression[campaignKey] >= i) || (userData.progression == null && PlayerPrefs.GetInt(campaignKey, 0) >= i) || i == 0) //by default first level of directory is the only unlocked level of directory
				button.GetComponentInChildren<Button>().interactable = true;
			//unlocked levels
			else
				button.GetComponentInChildren<Button>().interactable = false;
			//scores
			string highScoreKey = levelData.src.Replace(new Uri(Application.streamingAssetsPath + "/").AbsoluteUri, "");
			int scoredStars = (userData.highScore != null ? (userData.highScore.ContainsKey(highScoreKey) ? userData.highScore[highScoreKey] : 0) : PlayerPrefs.GetInt(highScoreKey + gameData.scoreKey, 0)); //0 star by default
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

	public void launchLevel(string campaignKey, int levelToLoad) {
		gameData.selectedScenario = campaignKey;
		gameData.levelToLoad = levelToLoad;
		GameObjectManager.loadScene("MainScene");
	}

	//Used on scenario editing window (see button ButtonTestLevel)
	public void testLevel(DataLevelBehaviour dlb)
	{
		gameData.selectedScenario = testLevelToken;
		WebGlScenario test = new WebGlScenario();
		test.levels = new List<DataLevel>();
		test.levels.Add(dlb.data);
		gameData.scenarios[testLevelToken] = test;
		gameData.levelToLoad = 0;
		GameObjectManager.loadScene("MainScene");
	}

	private void testLevelPath(string levelToLoad)
	{
		DataLevelBehaviour dlb = new DataLevelBehaviour();
		dlb.data.src = new Uri(Application.streamingAssetsPath + "/" + levelToLoad).AbsoluteUri;
		testLevel(dlb);
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

	// See ButtonLoadScenario
	public void displayLoadingPanel()
	{
		selectedScenarioGO = null;
		GameObjectManager.setGameObjectState(mainCanvas.transform.Find("LoadingPanel").gameObject, true);
		// remove all old scenario
		foreach (Transform child in loadingScenarioContent.transform)
		{
			GameObjectManager.unbind(child.gameObject);
			GameObject.Destroy(child.gameObject);
		}

		//create level directory buttons
		foreach (string key in gameData.scenarios.Keys)
		{
			if (key != testLevelToken) // we don't add new line for testLevel
			{
				GameObject scenarioItem = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/ScenarioAvailable") as GameObject, loadingScenarioContent.transform);
				scenarioItem.GetComponent<TextMeshProUGUI>().text = key;
				GameObjectManager.bind(scenarioItem);
			}
		}
	}

	public void onScenarioSelected(GameObject go)
    {
		selectedScenarioGO = go;
    }

	// see LoadButton in LoadingPanel in TitleScreen scene
	public void loadScenario()
	{
		if (selectedScenarioGO != null && gameData.scenarios.ContainsKey(selectedScenarioGO.GetComponentInChildren<TMP_Text>().text))
		{
			//remove all old scenario
			foreach (Transform child in scenarioContent.transform)
            {
				GameObjectManager.unbind(child.gameObject);
				GameObject.Destroy(child.gameObject);
            }

			scenarioAbstract.text = gameData.scenarios[selectedScenarioGO.GetComponentInChildren<TMP_Text>().text].description;

			foreach (DataLevel levelPath in gameData.scenarios[selectedScenarioGO.GetComponentInChildren<TMP_Text>().text].levels)
			{
				GameObject newLevel = GameObject.Instantiate(deletableElement, scenarioContent.transform);
				newLevel.GetComponentInChildren<TMP_Text>().text = levelPath.src.Replace(new Uri(Application.streamingAssetsPath + "/").AbsoluteUri, "");
				LayoutRebuilder.ForceRebuildLayoutImmediate(newLevel.transform as RectTransform);
				newLevel.GetComponent<DataLevelBehaviour>().data = levelPath.clone();
				GameObjectManager.bind(newLevel);
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(scenarioContent.transform as RectTransform);
		}
    }

	public void downloadLevel(DataLevelBehaviour dlb)
    {
		Debug.Log(dlb.data.src);
		DownloadLevel(dlb.data.src);
	}
}