using UnityEngine;
using FYFY;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Collections.Generic;
using DIG.GBLXAPI;
using System.Xml;
using Newtonsoft.Json;
using System.IO;

/// <summary>
/// This manager manages connexion data requests
/// </summary>
public class ConnexionManager : FSystem
{
	private Family f_localizationLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(LocalizationLoaded)));
	private Family f_sessionId = FamilyManager.getFamily(new AllOfComponents(typeof(TextMeshProUGUI)), new AnyOfTags("SessionId"));

	public GameObject prefabGameData;
	public GameObject loadingScreen;
	public TMP_Text logs;
	public TMP_Text progress;
	public TMP_Text SPYVersion;

	private int webGL_fileLoaded = 0;
	private int webGL_fileToLoad = 0;
	private GameData gameData;
	private UserData userData;
	private UnityAction localCallback;
	private bool webGL_askToEnableSendSystem = false;

	private string loadLevelWithURL = "";

	[Serializable]
	public class WebGlScenarioList
	{
		public List<WebGlScenario> scenarios;
	}

	public static ConnexionManager instance;

    public ConnexionManager()
    {
        instance = this;
    }

    protected override void onStart()
	{
		SPYVersion.text = "V" + Application.version;

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

		if (!GameObject.Find("GBLXAPI"))
		{
			if (!GBLXAPI.IsInit)
				GBLXAPI.Init(GBL_Interface.lrsAddresses);
			GBLXAPI.debugMode = false;
		}

		MainLoop.instance.StartCoroutine(waitLocalizationLoaded());
	}

	private IEnumerator waitLocalizationLoaded()
	{
		while (f_localizationLoaded.Count == 0)
			yield return null;
		// check if we have to load streaming assets
		if (gameData.levels == null)
		{
			gameData.levels = new Dictionary<string, XmlNode>();
			gameData.scenarios = new Dictionary<string, WebGlScenario>();
			GetScenariosAndLevels();
		}
		// check if we have to load competencies (required for level analysis)
		if (gameData.rawReferentials.referentials.Count == 0)
		{
			webGL_fileToLoad++;
			string referentialsPath = new Uri(Application.streamingAssetsPath + "/Competencies/competenciesReferential.json").AbsoluteUri;
			MainLoop.instance.StartCoroutine(GetCompetenciesWebRequest(referentialsPath));
		}
		// wait level loading
		MainLoop.instance.StartCoroutine(WaitLoadingData());
	}

	private IEnumerator WaitLoadingData()
	{
		// Enable Loading screen
		GameObjectManager.setGameObjectState(loadingScreen, true);

		// Attendre une seconde pour laisser le temps à webGL_fileToLoad d'être initialisé par les différents scénarios de chargement
		yield return new WaitForSeconds(1f);

		while (webGL_fileLoaded < webGL_fileToLoad)
			yield return null;

		// and, if require, we can load requested level by URL
		if (loadLevelWithURL != "")
			GameObjectManager.addComponent<AskToTestLevel>(MainLoop.instance.gameObject, new { url = loadLevelWithURL });
		// Disable Loading screen
		GameObjectManager.setGameObjectState(loadingScreen, false);

		if (Application.isEditor && false)
		{
			SPYVersion.transform.parent.parent.GetComponentInChildren<TMP_InputField>().text = "Mathieu";
			SPYVersion.transform.parent.parent.Find("MiddleBegin/ButtonConnexion").GetComponent<Button>().onClick.Invoke();
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

	private IEnumerator GetScenarioWebRequest()
	{
		string uri = new Uri(Application.streamingAssetsPath + "/WebGlData/ScenarioList.json").AbsoluteUri;
		UnityWebRequest www = UnityWebRequest.Get(uri);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			logs.text = "<color=\"red\">(" + logs.GetComponent<Localization>().localization[4] + ") " + uri + "</color>\n" + logs.text;
			yield return new WaitForSeconds(1f);
			if (webGL_fileLoaded < webGL_fileToLoad) // recursive call while player does not force launching
			{
				logs.text = "<color=\"orange\">(" + logs.GetComponent<Localization>().localization[5] + ") " + uri + "</color>\n" + logs.text;
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
			logs.text = "<color=\"red\">(" + logs.GetComponent<Localization>().localization[4] + ") " + uri + "</color>\n" + logs.text;
			yield return new WaitForSeconds(1f);
			if (webGL_fileLoaded < webGL_fileToLoad) // recursive call while player does not force launching
			{
				logs.text = "<color=\"orange\">(" + logs.GetComponent<Localization>().localization[5] + ") " + uri + "</color>\n" + logs.text;
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
		string[] files = Directory.GetFiles(path, "*.xml");
		webGL_fileToLoad += files.Length;
		foreach (string fileName in files)
		{
			yield return null;
			MainLoop.instance.StartCoroutine(GetLevelOrScenario_WebRequest("file://" + fileName));
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
			logs.text = "<color=\"red\">(" + logs.GetComponent<Localization>().localization[4] + ") " + uri + "</color>\n" + logs.text;
			yield return new WaitForSeconds(1f);
			if (webGL_fileLoaded < webGL_fileToLoad) // recursive call while player does not force launching
			{
				logs.text = "<color=\"orange\">(" + logs.GetComponent<Localization>().localization[5] + ") " + uri + "</color>\n" + logs.text;
				MainLoop.instance.StartCoroutine(GetLevelOrScenario_WebRequest(uri));
			}
			GameObjectManager.setGameObjectState(loadingScreen.transform.Find("ForceLaunch").gameObject, true);
		}
		else
		{
			webGL_fileLoaded++;
			progress.text = Mathf.Floor(((float)webGL_fileLoaded / webGL_fileToLoad) * 100) + "%";
			logs.text = "<color=\"green\">(" + gameData.GetComponent<Localization>().localization[1] + ") " + uri + "</color>\n" + logs.text;
			string xmlContent = www.downloadHandler.text;
			try
			{
				UtilityLobby.LoadLevelOrScenario(gameData, uri, xmlContent);
			}
			catch (Exception e)
			{
				logs.text = "<color=\"red\">(" + logs.GetComponent<Localization>().localization[4] + ") " + uri + " => " + e.Message + "</color>\n" + logs.text;
			}
		}
	}

	private IEnumerator GetCompetenciesWebRequest(string referentialsPath)
	{
		UnityWebRequest www = UnityWebRequest.Get(referentialsPath);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			logs.text = "<color=\"red\">(" + logs.GetComponent<Localization>().localization[4] + ") " + referentialsPath + "</color>\n" + logs.text;
			yield return new WaitForSeconds(1f);
			if (webGL_fileLoaded < webGL_fileToLoad) // recursive call while player does not force launching
			{
				logs.text = "<color=\"orange\">(" + logs.GetComponent<Localization>().localization[5] + ") " + referentialsPath + "</color>\n" + logs.text;
				MainLoop.instance.StartCoroutine(GetCompetenciesWebRequest(referentialsPath));
			}
			GameObjectManager.setGameObjectState(loadingScreen.transform.Find("ForceLaunch").gameObject, true);
		}
		else
		{
			webGL_fileLoaded++;
			Localization loc = gameData.GetComponent<Localization>();
			logs.text = "<color=\"green\">(" + loc.localization[1] + ") " + referentialsPath + "</color>\n" + logs.text;
			try
			{
				gameData.lastReferentialSelected = 0;
				gameData.rawReferentials = JsonUtility.FromJson<RawListReferential>(www.downloadHandler.text);
			}
			catch (Exception e)
			{
				logs.text = "<color=\"red\">(" + logs.GetComponent<Localization>().localization[4] + ") " + referentialsPath + " => " + Utility.getFormatedText(loc.localization[7], e.Message) + "</color>\n" + logs.text;
			}
		}
	}

	// See ForceLaunch button
	public void forceLaunch()
	{
		webGL_fileLoaded = webGL_fileToLoad;
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
			logs.text = "<color=\"red\">" + Utility.getFormatedText(logs.GetComponent<Localization>().localization[0], formatedString) + "</color>\n" + logs.text;
			yield return new WaitForSeconds(1f);
			if (webGL_fileLoaded < webGL_fileToLoad) // recursive call while player does not cancel loading
			{
				logs.text = "<color=\"orange\">" + Utility.getFormatedText(logs.GetComponent<Localization>().localization[1], formatedString) + "</color>\n" + logs.text;
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

				userData.progression = null;
				userData.highScore = null;
				userData.currentScenario = "";
				userData.levelToContinue = -1;

				foreach (GameObject sID in f_sessionId)
					sID.GetComponent<TMP_Text>().text = string.Join(" ", formatedString.ToCharArray());

				// enregistrer cet ID dans la BD pour éviter les collisions
				askToSendUserData("undef", false);
			}
			else
			{
				// means this sessionId is already used, try to find another
				MainLoop.instance.StartCoroutine(FindAvailableSessionId());
			}
		}
	}

	// See ButtonConnexion button in ConnexionPanel in ConnexionScene scene
	public void GetProgression(TMP_InputField idSession)
	{
		webGL_fileToLoad = 1;
		webGL_fileLoaded = 0;
		string formatedString = idSession.text.ToUpper().Replace(" ", "");
		formatedString = String.Concat(formatedString.Where(c => !Char.IsWhiteSpace(c)));

		MainLoop.instance.StartCoroutine(GetProgressionWebRequest(formatedString));
		MainLoop.instance.StartCoroutine(WaitLoadingData());
	}

	// See ButtonNewGame button in ConnexionPanel panel in ConnexionScene scene
	public void newGame()
	{
		webGL_fileToLoad = 1;
		webGL_fileLoaded = 0;
		
		MainLoop.instance.StartCoroutine(FindAvailableSessionId());
		MainLoop.instance.StartCoroutine(WaitLoadingData());
	}

	// See ButtonOkNoted button in ConnexionPanel panel in ConnexionScene scene
	public void synchUserData(GameObject go)
	{
		askToSendUserData(go.GetComponentInChildren<TMP_InputField>().text, go.GetComponentInChildren<Toggle>().isOn);
		MainLoop.instance.StartCoroutine(delayLoadingTitleScreen());
	}

	private IEnumerator delayLoadingTitleScreen()
    {
		yield return null;
		yield return null;
		GameObjectManager.loadScene("TitleScreen");
	}

	private void askToSendUserData(string schoolClass, bool isTeacher)
	{
		if (userData.progression == null)
			userData.progression = new Dictionary<string, int>();
		if (userData.highScore == null)
			userData.highScore = new Dictionary<string, int>();
		userData.schoolClass = schoolClass;
		userData.isTeacher = isTeacher;
		GameObjectManager.addComponent<SendUserData>(MainLoop.instance.gameObject);

	}

	private IEnumerator GetProgressionWebRequest(string idSession)
	{
		UnityWebRequest www = UnityWebRequest.Get("https://spy.lip6.fr/ServerREST_LIP6/index_new.php?idSession=" + idSession);
		logs.text = "";
		progress.text = "0%";
		yield return www.SendWebRequest();
		Localization loc = gameData.GetComponent<Localization>();
		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.result+" "+www.error);
			logs.text = "<color=\"red\">" + Utility.getFormatedText(logs.GetComponent<Localization>().localization[2], idSession) + "</color>\n" + logs.text;
			yield return new WaitForSeconds(1f);
			if (webGL_fileLoaded < webGL_fileToLoad) // recursive call while player does not cancel loading
			{
				logs.text = "<color=\"orange\">" + Utility.getFormatedText(logs.GetComponent<Localization>().localization[3], idSession) + "</color>\n" + logs.text;
				MainLoop.instance.StartCoroutine(GetProgressionWebRequest(idSession));
			}
			GameObjectManager.setGameObjectState(loadingScreen.transform.Find("ForceLaunch").gameObject, true);
		}
		else
		{
			webGL_fileLoaded++;
			if (www.downloadHandler.text == "")
			{
				// Unable to retrieve progress data
				localCallback = null;
				GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(loc.localization[16], idSession), OkButton = loc.localization[5], CancelButton = loc.localization[0], call = localCallback });
			}
			else
			{
				string[] stringSeparators = new string[] { "#SEP#" };
				string[] tokens = www.downloadHandler.text.Split(stringSeparators, StringSplitOptions.None);
				Debug.Log(www.downloadHandler.text);
				if (tokens.Length != 6)
				{
					// Session corrupted, ask to enter a new session code.
					Debug.LogWarning(www.error);
					localCallback = null;
					GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = loc.localization[17], OkButton = loc.localization[5], CancelButton = loc.localization[0], call = localCallback });
				}
				else
				{
					// Session successfully loaded
					userData.progression = JsonConvert.DeserializeObject<Dictionary<string, int>>(tokens[0]);
					if (userData.progression == null)
						userData.progression = new Dictionary<string, int>();
					userData.highScore = JsonConvert.DeserializeObject<Dictionary<string, int>>(tokens[1]);
					if (userData.highScore == null)
						userData.highScore = new Dictionary<string, int>();
					userData.currentScenario = tokens[2];
					int levelToContinue = -1;
					Int32.TryParse(tokens[3], out levelToContinue);
					userData.levelToContinue = levelToContinue;
					userData.schoolClass = tokens[4];
					userData.isTeacher = tokens[5] == "1";
					GBL_Interface.playerName = idSession;
					GBL_Interface.userUUID = idSession;
					GameObjectManager.loadScene("TitleScreen");
				}
			}
		}
	}

	// Fonction appelée depuis le javascript (voir Assets/WebGLTemplates/Custom/game.html) via le Wrapper du Système
	public void askToLoadLevel(string levelToLoad)
	{
		loadLevelWithURL = levelToLoad;
	}

	// Fonction appelée depuis le javascript (voir Assets/WebGLTemplates/Custom/game.html) via le Wrapper du Système
	public void enableSendStatement()
	{
		if (gameData == null)
			webGL_askToEnableSendSystem = true;
		else
			gameData.sendStatementEnabled = true;
	}

}