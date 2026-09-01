using DIG.GBLXAPI;
using FYFY;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Web;
using UnityEngine.Video;

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
	public GameObject RightPanel;

	public Transform CinematicPanel;

    public CurrentSettingsValues currentSettingsValues;

    private int webGL_fileLoaded = 0;
	private int webGL_fileToLoad = 0;
	private GameData gameData;
	private UserData userData;
	private UnityAction localCallback;
	private bool webGL_askToEnableSendSystem = false;
	private bool cinematicPlayed = false;

    private string loadLevelWithURL = "";

	[DllImport("__Internal")]
	private static extern void ShowHtmlImportSettings(); // call javascript

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
		gameData.selectedScenario = "";
		gameData.levelToLoad = -1;
		userData = go.GetComponent<UserData>();
		// Reset user data
		userData.birthYear = "undef";
		userData.isTeacher = false;
		userData.progression = new Dictionary<string, int>();
		userData.highScore = new Dictionary<string, int>();
		userData.currentScenario = "";
		userData.levelToContinue = -1;
		userData.unlockedAvatars = new List<int>();
		userData.avatarSelected = 2; // Le troixième (robot de genre neutre) est celui par défaut
		userData.newAvatarAvailable = -1;

		if (webGL_askToEnableSendSystem)
			gameData.sendStatementEnabled = true;
		GameObjectManager.dontDestroyOnLoadAndRebind(gameData.gameObject);

		// Enable Loading screen
		GameObjectManager.setGameObjectState(loadingScreen, true);

		if (!GameObject.Find("GBLXAPI"))
		{
			if (!GBLXAPI.IsInit)
				GBLXAPI.Init(GBL_Interface.gblConfigs);
			GBLXAPI.debugMode = false;
		}
        else
        {
			// reset account
			GBL_Interface.playerName = "";
			GBL_Interface.userUUID = "";
		}

		MainLoop.instance.StartCoroutine(waitLocalizationLoadedAndContinue());

		if (Application.platform == RuntimePlatform.WebGLPlayer)
			ShowHtmlImportSettings();

		Pause = true;
	}

	private IEnumerator waitLocalizationLoadedAndContinue()
	{
		while (f_localizationLoaded.Count == 0)
			yield return null;
		// check if we have to load streaming assets
		if (gameData.levels == null)
		{
			gameData.levels = new Dictionary<string, XmlNode>();
			gameData.scenarios = new Dictionary<string, WebGlScenario>();

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
				exploreDirectoryAndCount(Application.streamingAssetsPath);
				exploreDirectoryAndCount(Application.persistentDataPath);
                // explore streaming asstets path
                yield return loadLevelsAndScenarios(Application.streamingAssetsPath);
                // explore persistent data path
                yield return loadLevelsAndScenarios(Application.persistentDataPath);
            }
        }
		// check if we have to load competencies (required for level analysis)
		if (gameData.rawReferentials.referentials.Count == 0)
		{
			webGL_fileToLoad++;
			string referentialsPath = new Uri(Application.streamingAssetsPath + "/Competencies/competenciesReferential.json").AbsoluteUri;
			yield return GetCompetenciesWebRequest(referentialsPath);
		}
		// wait level loading
		yield return WaitLoadingData();
		// affectation du loadingscreen au RightPanel
		GameObjectManager.setGameObjectParent(loadingScreen, RightPanel, false);
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
		{
			DataLevel dl = new DataLevel();
			if (loadLevelWithURL.StartsWith("http"))
			{
				dl.filePath = loadLevelWithURL;
				UnityWebRequest www;
				if (loadLevelWithURL.ToLower().StartsWith("https://spy.lip6.fr"))
					www = UnityWebRequest.Get(loadLevelWithURL);
				else
					// On passe par notre proxy pour charger une mission commençant par http qui n'est pas chez nous (spy.lip6.fr)
					www = UnityWebRequest.Get("https://spy.lip6.fr/ServerREST_LIP6/index_new_v2.php?file=" + HttpUtility.UrlEncode(loadLevelWithURL));
				yield return www.SendWebRequest();
				if (www.result == UnityWebRequest.Result.Success)
				{
					try
					{
						UtilityLobby.LoadLevelOrScenario(gameData, loadLevelWithURL, www.downloadHandler.text);
					}
					catch (Exception e)
					{
						logs.text = "<color=\"red\">(" + logs.GetComponent<Localization>().localization[4] + ") " + loadLevelWithURL + " => " + e.Message + "</color>\n" + logs.text;
						Debug.Log("Parsing error:" + www.downloadHandler.text);
					}
				}
				else
				{
					Debug.Log(www.result + " " + www.error);
				}
			}
			else
			{
				dl.filePath = new Uri(Application.persistentDataPath + "/" + loadLevelWithURL).AbsoluteUri;
				if (!gameData.levels.ContainsKey(dl.filePath))
					dl.filePath = new Uri(Application.streamingAssetsPath + "/" + loadLevelWithURL).AbsoluteUri;
			}
			dl.missionName = Path.GetFileNameWithoutExtension(dl.filePath);

			gameData.selectedScenario = UtilityLobby.testFromUrl;
			WebGlScenario test = new WebGlScenario();
			test.levels = new List<DataLevel> { dl };
			gameData.scenarios[UtilityLobby.testFromUrl] = test;
			gameData.levelToLoad = 0;
			GBL_Interface.playerName = UtilityLobby.testFromUrl;
			GBL_Interface.userUUID = UtilityLobby.testFromUrl;
			GameObjectManager.addComponent<AskToLoadScene>(MainLoop.instance.gameObject, new { sceneName = "MainScene" });
		}
		else
		{
			// Disable Loading screen
			GameObjectManager.setGameObjectState(loadingScreen, false);
			// skip cinematic in editor or if already played
			if (!cinematicPlayed)
			{
				cinematicPlayed = true;
				// Enable cinematic panel
				GameObjectManager.setGameObjectState(CinematicPanel.gameObject, true);
				// Wait end of cinematic
				VideoPlayer cinematicVideoPlayer = CinematicPanel.GetComponentInChildren<VideoPlayer>(true);
				cinematicVideoPlayer.clip = Resources.Load<VideoClip>("Video/VideoIntro" + (currentSettingsValues.values.currentLanguage == 1 ? "_en" : "_fr"));
				while (!CinematicPanel.gameObject.activeInHierarchy)
					yield return null;
				cinematicVideoPlayer.Prepare();
				while (!cinematicVideoPlayer.isPrepared)
					yield return null;
				cinematicVideoPlayer.time = 0;
				cinematicVideoPlayer.Play();
				while (cinematicVideoPlayer.isPlaying)
					yield return null;
				// Disable cinematic panel
				GameObjectManager.setGameObjectState(CinematicPanel.gameObject, false);
			}
			else
				cinematicPlayed = true;
        }

        /* (Application.isEditor)
		{
			SPYVersion.transform.parent.parent.GetComponentInChildren<TMP_InputField>().text = "Mathieu";
			SPYVersion.transform.parent.parent.Find("MiddleBegin/ButtonConnexion").GetComponent<Button>().onClick.Invoke();
		}*/
	}

	private IEnumerator GetScenarioWebRequest()
    {
        string uri = new Uri(Application.streamingAssetsPath + "/WebGlData/ScenarioList.json").AbsoluteUri;
        while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get(uri);
            yield return www.SendWebRequest();

			if (www.result != UnityWebRequest.Result.Success)
			{
				logs.text = "<color=\"red\">(" + logs.GetComponent<Localization>().localization[4] + ") " + uri + "</color>\n" + logs.text;
				Debug.Log("(" + logs.GetComponent<Localization>().localization[4] + ") " + uri);
                yield return new WaitForSeconds(1f);
                if (webGL_fileLoaded < webGL_fileToLoad)
					logs.text = "<color=\"orange\">(" + logs.GetComponent<Localization>().localization[5] + ") " + uri + "</color>\n" + logs.text;
				GameObjectManager.setGameObjectState(loadingScreen.transform.Find("ForceLaunch").gameObject, true);
			}
			else
			{
				webGL_fileLoaded++;
				logs.text = "<color=\"green\">(" + gameData.GetComponent<Localization>().localization[1] + ") " + uri + "</color>\n" + logs.text;
				string scenarioJson = www.downloadHandler.text;
				WebGlScenarioList scenarioListRaw = JsonConvert.DeserializeObject<WebGlScenarioList>(scenarioJson);
				foreach (WebGlScenario scenarioRaw in scenarioListRaw.scenarios)
				{
					gameData.scenarios[scenarioRaw.key] = scenarioRaw;
					foreach (DataLevel levelPath in scenarioRaw.levels)
					{
						levelPath.filePath = new Uri(Application.streamingAssetsPath + "/" + levelPath.filePath).AbsoluteUri;
					}
				}
				break; // exit the loop
            }
		}
	}

	private IEnumerator GetLevelsWebRequest()
    {
        string uri = new Uri(Application.streamingAssetsPath + "/WebGlData/LevelsList.json").AbsoluteUri;
        while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get(uri);
            yield return www.SendWebRequest();

			if (www.result != UnityWebRequest.Result.Success)
			{
				logs.text = "<color=\"red\">(" + logs.GetComponent<Localization>().localization[4] + ") " + uri + "</color>\n" + logs.text;
				Debug.Log("(" + logs.GetComponent<Localization>().localization[4] + ") " + uri);
				yield return new WaitForSeconds(1f);
				if (webGL_fileLoaded < webGL_fileToLoad)
					logs.text = "<color=\"orange\">(" + logs.GetComponent<Localization>().localization[5] + ") " + uri + "</color>\n" + logs.text;
				GameObjectManager.setGameObjectState(loadingScreen.transform.Find("ForceLaunch").gameObject, true);
			}
			else
			{
				webGL_fileLoaded++;
				logs.text = "<color=\"green\">(" + gameData.GetComponent<Localization>().localization[1] + ") " + uri + "</color>\n" + logs.text;
				string levelsJson = www.downloadHandler.text;
				WebGlScenario levelsListRaw = JsonUtility.FromJson<WebGlScenario>(levelsJson);
				webGL_fileToLoad += levelsListRaw.levels.Count;
                // try to load all levels in parallel
                foreach (DataLevel levelRaw in levelsListRaw.levels)
					MainLoop.instance.StartCoroutine(GetLevelOrScenario_WebRequest(new Uri(Application.streamingAssetsPath + "/" + levelRaw.filePath).AbsoluteUri));
				break; // exit the loop
            }
		}
	}

	private void exploreDirectoryAndCount(string path)
    {
        // try to load all child files
        string[] files = Directory.GetFiles(path, "*.xml");
        webGL_fileToLoad += files.Length;
        // explore subdirectories
        foreach (string directory in Directory.GetDirectories(path))
            exploreDirectoryAndCount(directory);
    }

    private IEnumerator loadLevelsAndScenarios(string path)
	{
		// try to load all child files
		string[] files = Directory.GetFiles(path, "*.xml");
		foreach (string fileName in files)
			yield return GetLevelOrScenario_WebRequest("file://" + fileName);

        // explore subdirectories
        foreach (string directory in Directory.GetDirectories(path))
			yield return loadLevelsAndScenarios(directory);
	}

	private IEnumerator GetLevelOrScenario_WebRequest(string uri)
	{
		while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get(uri);
            yield return www.SendWebRequest();

			if (www.result != UnityWebRequest.Result.Success)
			{
				logs.text = "<color=\"red\">(" + logs.GetComponent<Localization>().localization[4] + ") " + uri + "</color>\n" + logs.text;
				Debug.Log("(" + logs.GetComponent<Localization>().localization[4] + ") " + uri);
				yield return new WaitForSeconds(1f);
				if (webGL_fileLoaded < webGL_fileToLoad)
					logs.text = "<color=\"orange\">(" + logs.GetComponent<Localization>().localization[5] + ") " + uri + "</color>\n" + logs.text;
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
				break; // exit the loop
            }
		}
	}

	private IEnumerator GetCompetenciesWebRequest(string referentialsPath)
	{
		while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get(referentialsPath);
            yield return www.SendWebRequest();

			if (www.result != UnityWebRequest.Result.Success)
			{
				logs.text = "<color=\"red\">(" + logs.GetComponent<Localization>().localization[4] + ") " + referentialsPath + "</color>\n" + logs.text;
				Debug.Log("(" + logs.GetComponent<Localization>().localization[4] + ") " + referentialsPath);
				yield return new WaitForSeconds(1f);
				if (webGL_fileLoaded < webGL_fileToLoad)
					logs.text = "<color=\"orange\">(" + logs.GetComponent<Localization>().localization[5] + ") " + referentialsPath + "</color>\n" + logs.text;
				GameObjectManager.setGameObjectState(loadingScreen.transform.Find("ForceLaunch").gameObject, true);
			}
			else
			{
				webGL_fileLoaded++;
				Localization loc = gameData.GetComponent<Localization>();
				logs.text = "<color=\"green\">(" + loc.localization[1] + ") " + referentialsPath + "</color>\n" + logs.text;
				try
				{
					gameData.rawReferentials = JsonUtility.FromJson<RawListReferential>(www.downloadHandler.text);
				}
				catch (Exception e)
				{
					logs.text = "<color=\"red\">(" + logs.GetComponent<Localization>().localization[4] + ") " + referentialsPath + " => " + Utility.getFormatedText(loc.localization[7], e.Message) + "</color>\n" + logs.text;
					Debug.Log("(" + logs.GetComponent<Localization>().localization[4] + ") " + referentialsPath);
				}
				break;
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

		logs.text = "";
		progress.text = "0%";
		while (true)
        {
            // Make a request to check if this sessionId is already used
            UnityWebRequest www = UnityWebRequest.Get("https://spy.lip6.fr/ServerREST_LIP6/index_new_v2.php?idSession=" + formatedString);
            yield return www.SendWebRequest();

			if (www.result != UnityWebRequest.Result.Success)
			{
				logs.text = "<color=\"red\">" + Utility.getFormatedText(logs.GetComponent<Localization>().localization[0], formatedString) + "</color>\n" + logs.text;
				Debug.Log(Utility.getFormatedText(logs.GetComponent<Localization>().localization[0], formatedString));
				yield return new WaitForSeconds(2f);
				if (webGL_fileLoaded < webGL_fileToLoad)
					logs.text = "<color=\"orange\">" + Utility.getFormatedText(logs.GetComponent<Localization>().localization[1], formatedString) + "</color>\n" + logs.text;
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

					foreach (GameObject sID in f_sessionId)
						sID.GetComponent<TMP_Text>().text = string.Join(" ", formatedString.ToCharArray());

					// enregistrer cet ID dans la BD pour éviter les collisions
					GameObjectManager.addComponent<SendUserData>(MainLoop.instance.gameObject);
					break; // exit the loop
                }
				// means this sessionId is already used, try to find another in next iteration
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
	public void synchUserData()
	{
		GameObjectManager.addComponent<SendUserData>(MainLoop.instance.gameObject);
		MainLoop.instance.StartCoroutine(AnimCameraAndLoadTitleScreen());
	}

	private IEnumerator AnimCameraAndLoadTitleScreen()
    {
		GameObjectManager.setGameObjectState(RightPanel, false);
		GameObjectManager.addComponent<ForceOpenDoor>(MainLoop.instance.gameObject);
        Animation anim = Camera.main.GetComponent<Animation>();
		anim.Play();
		while(!anim.isPlaying)
            yield return null;
        AnimationState state = anim["CameraMove"];
		while (state.normalizedTime < 0.85f)
            yield return null;
		GameObjectManager.addComponent<AskToLoadScene>(MainLoop.instance.gameObject, new { sceneName = "TitleScreen" });
	}

	private IEnumerator GetProgressionWebRequest(string idSession)
	{
		logs.text = "";
		progress.text = "0%";
		while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get("https://spy.lip6.fr/ServerREST_LIP6/index_new_v2.php?idSession=" + idSession);
            yield return www.SendWebRequest();
			Localization loc = gameData.GetComponent<Localization>();
			if (www.result != UnityWebRequest.Result.Success)
			{
				logs.text = "<color=\"red\">" + Utility.getFormatedText(logs.GetComponent<Localization>().localization[2], idSession) + "</color>\n" + logs.text;
				Debug.Log(Utility.getFormatedText(logs.GetComponent<Localization>().localization[2], idSession));
				yield return new WaitForSeconds(1f);
				if (webGL_fileLoaded < webGL_fileToLoad)
					logs.text = "<color=\"orange\">" + Utility.getFormatedText(logs.GetComponent<Localization>().localization[3], idSession) + "</color>\n" + logs.text;
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
					if (tokens.Length != 9)
					{
						// Session corrupted, ask to enter a new session code.
						localCallback = null;
						GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = loc.localization[17], OkButton = loc.localization[5], CancelButton = loc.localization[0], call = localCallback });
					}
					else
					{
						// Session successfully loaded
						Debug.Log(www.downloadHandler.text);
						userData.progression = JsonConvert.DeserializeObject<Dictionary<string, int>>(tokens[0]);
						if (userData.progression == null)
							userData.progression = new Dictionary<string, int>();
						userData.highScore = JsonConvert.DeserializeObject<Dictionary<string, int>>(tokens[1]);
						if (userData.highScore == null)
							userData.highScore = new Dictionary<string, int>();
						userData.currentScenario = tokens[2];
						int levelToContinue;
						if (!Int32.TryParse(tokens[3], out levelToContinue))
							levelToContinue = -1;
						userData.levelToContinue = levelToContinue;
						userData.birthYear = tokens[4];
						userData.isTeacher = tokens[5] == "1";
						// Si le joueur n'a pas touché aux paramètres (pas d'import de settings et pas de modification via l'UI) on charge le jeu de paramètre de son profil, sinon on saute cette étape pour garder ces choix immédiats
						if (tokens[6] != "{}" && !SettingsManager.instance.settingsUpdated)
						{
							SettingsManager.instance.importSettings(tokens[6]); // => permet de mettre à jour les PlayerPrefs à partir des nouvelles valeurs de manière à ce qu'au chargement de la scène, les bons settings soient pris en compte
						}
						userData.unlockedAvatars = JsonConvert.DeserializeObject<List<int>>(tokens[7]);
						if (userData.unlockedAvatars == null)
							userData.unlockedAvatars = new List<int>();
						int avatarSelected;
						if (!Int32.TryParse(tokens[8], out avatarSelected))
							avatarSelected = 2; // Le troisième est le robot non genré
						userData.avatarSelected = avatarSelected;
						userData.newAvatarAvailable = -1;
						GBL_Interface.playerName = idSession;
						GBL_Interface.userUUID = idSession;
						yield return AnimCameraAndLoadTitleScreen();
                    }
                }
                break; // exit the loop
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