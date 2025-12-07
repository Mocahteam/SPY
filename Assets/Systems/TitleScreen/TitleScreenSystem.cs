using UnityEngine;
using FYFY;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;

/// <summary>
/// Manage main menu to launch a specific mission
/// </summary>
public class TitleScreenSystem : FSystem {
	private Family f_sessionId = FamilyManager.getFamily(new AllOfComponents(typeof(TextMeshProUGUI)), new AnyOfTags("SessionId"));
	private Family f_competencies = FamilyManager.getFamily(new AllOfComponents(typeof(Competency))); // Les compétences
	private Family f_compSelector = FamilyManager.getFamily(new AnyOfTags("CompetencySelector"), new AllOfComponents(typeof(TMP_Dropdown)));

	private GameData gameData;
	private UserData userData;
	public Button continueButton;
	public GameObject playButton;
	public GameObject gameSelector;
	public GameObject gameTilePrefab;
	public GameObject quitButton;
	public TMP_Text SPYVersion;

	private Transform gameList;
	private Transform gameDetails;

	private UnityAction localCallback;

	[DllImport("__Internal")]
	private static extern void ShowHtmlButtons(); // call javascript

	// L'instance
	public static TitleScreenSystem instance;

	public TitleScreenSystem()
	{
		instance = this;
	}

	protected override void onStart()
	{
		SPYVersion.text = "V" + Application.version;

		GameObject gameDataGO = GameObject.Find("GameData");
		if (gameDataGO == null)
			GameObjectManager.loadScene("ConnexionScene");
		else
		{
			gameData = gameDataGO.GetComponent<GameData>();
			userData = gameDataGO.GetComponent<UserData>();

			foreach (GameObject sID in f_sessionId)
				sID.GetComponent<TMP_Text>().text = string.Join(" ", GBL_Interface.playerName.ToCharArray());

			// gestion du bouton continue
			continueButton.interactable = (gameData.scenarios.ContainsKey(userData.currentScenario) && userData.levelToContinue != -1 && userData.levelToContinue < gameData.scenarios[userData.currentScenario].levels.Count);

			if (gameData.selectedScenario == UtilityLobby.testFromScenarioEditor) // reload scenario editor
			{
				launchScenarioEditor();
			}
			else if (gameData.selectedScenario == UtilityLobby.testFromLevelEditor) // reload level editor
			{
				launchLevelEditor();
			}
			else if (gameData.selectedScenario != "" && gameData.selectedScenario != UtilityLobby.testFromUrl)
			{
				// reload last opened scenario
				playButton.GetComponent<Button>().onClick.Invoke();
				GameKeys gk = new GameKeys();
				gk.scenarioKey = gameData.selectedScenario;
				gk.missionNumber = -1;
				showLevels(gk);
				GameObjectManager.setGameObjectState(gameSelector, true); // enable gameSelector menu
			}

			if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				ShowHtmlButtons();
				GameObjectManager.setGameObjectState(quitButton, false);
			}

			gameList = gameSelector.transform.Find("GamePanel/Viewport/GameList");
			gameDetails = gameSelector.transform.Find("GameDetails");
		}
	}

	private class JavaScriptData
	{
		public string name;
		public string content;
	}

	// Fonction appelée depuis le javascript (voir Assets/WebGLTemplates/Custom/game.html) via le Wrapper du Système
	public void importLevelOrScenario(string content)
	{
		Localization loc = gameData.GetComponent<Localization>();
		JavaScriptData jsd = JsonUtility.FromJson<JavaScriptData>(content);
		try
		{
			string fakeUri = Application.streamingAssetsPath + "/Levels/LocalFiles/" + jsd.name; 
			UtilityLobby.LoadLevelOrScenario(gameData, fakeUri, jsd.content);
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = loc.localization[19], OkButton = loc.localization[0], CancelButton = loc.localization[1], call = localCallback });
		}
		catch (Exception e) {
			localCallback = null;
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(loc.localization[20], jsd.name, e.Message), OkButton = loc.localization[0], CancelButton = loc.localization[1], call = localCallback });
		}
	}

	// see Play Button in TitleScreen scene
	public void displayScenarioList()
	{
		// remove all old tiles
		foreach (Transform child in gameList)
		{
			GameObjectManager.unbind(child.gameObject);
			GameObject.Destroy(child.gameObject);
		}

		GameObjectManager.setGameObjectState(gameSelector.transform.Find("Header/BackMainMenu").gameObject, true);
		GameObjectManager.setGameObjectState(gameSelector.transform.Find("Header/BackScenarios").gameObject, false);

		//create scenarios' button
		List<string> sortedScenarios = new List<string>();
		foreach (string key in gameData.scenarios.Keys)
			if (key != UtilityLobby.testFromScenarioEditor && key != UtilityLobby.testFromLevelEditor && key !=  UtilityLobby.testFromUrl && key != UtilityLobby.editingScenario) // we don't create a button for tested level
				sortedScenarios.Add(key);
		sortedScenarios.Sort();
		foreach (string key in sortedScenarios)
		{
			GameObject scenarioButton = GameObject.Instantiate<GameObject>(gameTilePrefab, gameList.transform);

			scenarioButton.transform.Find("Finished").gameObject.SetActive(userData.progression != null && userData.progression.ContainsKey(key) && userData.progression[key] == gameData.scenarios[key].levels.Count);
			
			int totalStars = 0;
			foreach (DataLevel dl in gameData.scenarios[key].levels)
			{
				string highScoreKey = Utility.extractFileName(dl.src);
				totalStars += (userData.highScore != null ? (userData.highScore.ContainsKey(highScoreKey) ? userData.highScore[highScoreKey] : 0) : PlayerPrefs.GetInt(highScoreKey + gameData.scoreKey, 0)); //0 star by default
			}
			scenarioButton.transform.Find("TotalStars").GetComponent<TextMeshProUGUI>().text = totalStars + "/" + (gameData.scenarios[key].levels.Count * 3);

			if (userData.progression != null && userData.progression.ContainsKey(key))
				scenarioButton.transform.Find("Percentage").GetComponent<TextMeshProUGUI>().text = (100*userData.progression[key]/ gameData.scenarios[key].levels.Count) +"%";
			else
				scenarioButton.transform.Find("Percentage").GetComponent<TextMeshProUGUI>().text = "0%";
			scenarioButton.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = Utility.extractLocale(gameData.scenarios[key].name);

			GameKeys gk = scenarioButton.GetComponent<GameKeys>();
			gk.scenarioKey = key;
			gk.missionNumber = -1;

			GameObjectManager.bind(scenarioButton);
		}
		// focus on the first scenario
		EventSystem.current.SetSelectedGameObject(gameList.transform.GetChild(0).gameObject);
	}

	public void showDetails(GameKeys keys)
	{
		TMP_Text title = gameDetails.Find("Title").GetComponentInChildren<TMP_Text>(true);
		TMP_Text gameDescription = gameDetails.Find("Scroll View").GetComponentInChildren<TMP_Text>(true);
		
		// If the keys refer a scenario without a mission defined
		if (keys.scenarioKey != "" && keys.missionNumber == -1 && gameData.scenarios.ContainsKey(keys.scenarioKey))
		{
			GameObjectManager.setGameObjectState(gameDetails.gameObject, true);
			title.text = Utility.extractLocale(gameData.scenarios[keys.scenarioKey].name);
			gameDescription.text = Utility.extractLocale(gameData.scenarios[keys.scenarioKey].description);
		}
		// If the keys refer a scenario and a mission
		else if (keys.scenarioKey != "" && keys.missionNumber == -1 && gameData.scenarios.ContainsKey(keys.scenarioKey) && gameData.scenarios[keys.scenarioKey].levels.Count > keys.missionNumber)
        {
			GameObjectManager.setGameObjectState(gameDetails.gameObject, true);
			title.text = gameData.scenarios[keys.scenarioKey].levels[keys.missionNumber].name;
			gameDescription.text = "";
		}
		else
			GameObjectManager.setGameObjectState(gameDetails.gameObject, false);
	}

	public void showLevels(GameKeys keys)
	{

		GameObjectManager.setGameObjectState(gameSelector.transform.Find("Header/BackMainMenu").gameObject, false);
		GameObjectManager.setGameObjectState(gameSelector.transform.Find("Header/BackScenarios").gameObject, true);

		/*GameObjectManager.setGameObjectState(menuCampaigns, false);
		GameObjectManager.setGameObjectState(menuMissions, true);
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
			Transform stars = button_go.transform.Find("ScoreCanvas").Find("Stars");
			GameObjectManager.setGameObjectState(stars.gameObject, button.interactable);
			stars.GetComponent<Image>().sprite = stars.GetComponent<SpriteList>().source[scoredStars];
			stars.GetComponent<TooltipContent>().text = stars.GetComponent<StringList>().texts[scoredStars];
		}

		if (listOfLevels.transform.childCount > 0)
			MainLoop.instance.StartCoroutine(Utility.delayGOSelection(listOfLevels.transform.GetChild(0).Find("Button").gameObject));*/
	}

	// See competency selector (DropdownReferential GameObject)
	public void refreshCompetencies(Transform content)
    {
		TMP_Text compDetails = content.GetChild(4).GetComponent<TMP_Text>();
		if (gameData.scenarios.ContainsKey(content.GetChild(0).GetComponent<TMP_Text>().text))
		{
			// Get current referentiel selected
			TMP_Dropdown compSelector = f_compSelector.First().GetComponent<TMP_Dropdown>();
			string referentialName = compSelector.options[compSelector.value].text;
			// Display competencies
			compDetails.text = "<b>"+ compDetails.GetComponent<Localization>().localization[0]+"</b>\n";
			string txt = "";
			foreach (GameObject comp in f_competencies)
			{
				if (comp.GetComponent<Competency>().referential == referentialName)
				{
					foreach (DataLevel levelKey in gameData.scenarios[content.GetChild(0).GetComponent<TMP_Text>().text].levels)
						if (gameData.levels.ContainsKey(levelKey.src) && UtilityLobby.isCompetencyMatchWithLevel(comp.GetComponent<Competency>(), gameData.levels[levelKey.src].OwnerDocument))
						{
							txt += "\t" + Utility.extractLocale(comp.GetComponent<Competency>().id) + "\n";
							break;
						}
				}
			}
			if (txt != "")
				compDetails.text += txt;
			else
				compDetails.text += "\t"+ compDetails.GetComponent<Localization>().localization[1] + "\n";
			LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
			// auto move to the top od the panel
			(content as RectTransform).anchoredPosition = new Vector2(0, 0);
		}
	}

	public void continueScenario()
    {
		// garde pour être sûr que le niveau existe dans le scénario visé, mais normalement elle est déjà gérée par l'activation ou pas du bouton "Continue"
		if (gameData.scenarios.ContainsKey(userData.currentScenario) && userData.levelToContinue != -1 && userData.levelToContinue < gameData.scenarios[userData.currentScenario].levels.Count)
			launchLevel(userData.currentScenario, userData.levelToContinue);

	}

	public void launchLevel(string campaignKey, int levelToLoad) {
		gameData.selectedScenario = campaignKey;
		gameData.levelToLoad = levelToLoad;
		userData.currentScenario = campaignKey;
		userData.levelToContinue = levelToLoad;
		GameObjectManager.loadScene("MainScene");
	}

	public void launchLevelEditor()
    {
		GameObjectManager.loadScene("MissionEditor");
	}

	public void launchScenarioEditor()
	{
		GameObjectManager.loadScene("ScenarioEditor");
	}

	public void launchConnexionScene()
    {
		GameObjectManager.loadScene("ConnexionScene");
	}

	// See Quitter button in editor
	public void quitGame(){
		Application.Quit();
	}
}