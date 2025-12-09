using UnityEngine;
using FYFY;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;
using System.Collections;

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
	public GameObject TileScenarioPrefab;
	public GameObject TileMissionPrefab;
	public GameObject quitButton;
	public TMP_Text SPYVersion;

	private Transform gameList;
	private Transform gameDetails;
	private Transform competencyPanel;
	private string lastScenarioSelected = "";

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
		GameObject gameDataGO = GameObject.Find("GameData");
		if (gameDataGO == null)
			GameObjectManager.loadScene("ConnexionScene");
		else
		{
			SPYVersion.text = "V" + Application.version;

			gameData = gameDataGO.GetComponent<GameData>();
			userData = gameDataGO.GetComponent<UserData>();

			gameList = gameSelector.transform.Find("GamePanel/Viewport/GameList");
			gameDetails = gameSelector.transform.Find("GameDetails");
			competencyPanel = gameSelector.transform.Find("CompetencyPanel");

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
				showLevels(gameData.selectedScenario);
			}

			if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				ShowHtmlButtons();
				GameObjectManager.setGameObjectState(quitButton, false);
			}
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

	private void removeAllOldTiles()
    {
		for (int i = gameList.childCount-1; i >= 0; i--)
		{
			GameObject child = gameList.GetChild(i).gameObject;
			GameObjectManager.unbind(child);
			child.transform.SetParent(null); // remove from parent because Destroy is not immediate
			GameObject.Destroy(child);
		}
	}

	// see Play Button in TitleScreen scene
	public void displayScenarioList()
	{
		// remove all old tiles
		removeAllOldTiles();

		GameObjectManager.setGameObjectState(gameSelector.transform.Find("Header/BackMainMenu").gameObject, true);
		GameObjectManager.setGameObjectState(gameSelector.transform.Find("Header/BackScenarios").gameObject, false);
		gameSelector.transform.Find("Header/Title").GetComponent<TMP_Text>().text = "";

		//create scenarios' button
		List<string> sortedScenarios = new List<string>();
		foreach (string key in gameData.scenarios.Keys)
			if (key != UtilityLobby.testFromScenarioEditor && key != UtilityLobby.testFromLevelEditor && key !=  UtilityLobby.testFromUrl && key != UtilityLobby.editingScenario) // we don't create a button for tested level
				sortedScenarios.Add(key);
		sortedScenarios.Sort();
		foreach (string key in sortedScenarios)
		{
			GameObject scenarioTile = GameObject.Instantiate<GameObject>(TileScenarioPrefab, gameList.transform);

			scenarioTile.transform.Find("Finished").gameObject.SetActive(userData.progression != null && userData.progression.ContainsKey(key) && userData.progression[key] == gameData.scenarios[key].levels.Count);
			
			int totalStars = 0;
			foreach (DataLevel dl in gameData.scenarios[key].levels)
			{
				string highScoreKey = Utility.extractFileName(dl.src);
				totalStars += (userData.highScore != null ? (userData.highScore.ContainsKey(highScoreKey) ? userData.highScore[highScoreKey] : 0) : PlayerPrefs.GetInt(highScoreKey + gameData.scoreKey, 0)); //0 star by default
			}
			scenarioTile.transform.Find("TotalStars").GetComponent<TextMeshProUGUI>().text = totalStars + "/" + (gameData.scenarios[key].levels.Count * 3);

			if (userData.progression != null && userData.progression.ContainsKey(key))
				scenarioTile.transform.Find("Percentage").GetComponent<TextMeshProUGUI>().text = (100*userData.progression[key]/ gameData.scenarios[key].levels.Count) +"%";
			else
				scenarioTile.transform.Find("Percentage").GetComponent<TextMeshProUGUI>().text = "0%";
			scenarioTile.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = Utility.extractLocale(gameData.scenarios[key].name);

			GameKeys gk = scenarioTile.GetComponent<GameKeys>();
			gk.scenarioKey = key;
			gk.missionNumber = -1;

			scenarioTile.GetComponent<Button>().StartCoroutine(delayScenarioTooltipContent(scenarioTile));

			GameObjectManager.bind(scenarioTile);
		}
		// focus on the first scenario
		if (gameList.transform.childCount > 0)
		{
			if (lastScenarioSelected != "")
			{
				foreach (Transform child in gameList.transform)
					if (child.GetComponent<GameKeys>().scenarioKey == lastScenarioSelected)
					{
						MainLoop.instance.StartCoroutine(Utility.delayGOSelection(child.gameObject));
						break;
					}
			}
			else
				MainLoop.instance.StartCoroutine(Utility.delayGOSelection(gameList.transform.GetChild(0).gameObject));
		}
	}

	private IEnumerator delayScenarioTooltipContent(GameObject scenarioTile)
	{
		yield return null;
		StringList sl = scenarioTile.GetComponent<StringList>();
		scenarioTile.GetComponent<TooltipContent>().text = scenarioTile.transform.Find("Name").GetComponent<TextMeshProUGUI>().text + "<br>" + sl.texts[0] + scenarioTile.transform.Find("Percentage").GetComponent<TextMeshProUGUI>().text + "<br>" + sl.texts[0]+ scenarioTile.transform.Find("TotalStars").GetComponent<TextMeshProUGUI>().text;
	}

	public void showLevels(GameKeys keys)
    {
		showLevels(keys.scenarioKey);
    }

	private void showLevels(string scenarioKey)
	{
		lastScenarioSelected = scenarioKey;

		// delete all old tiles
		removeAllOldTiles();

		GameObjectManager.setGameObjectState(gameSelector.transform.Find("Header/BackMainMenu").gameObject, false);
		GameObjectManager.setGameObjectState(gameSelector.transform.Find("Header/BackScenarios").gameObject, true);

		// set scenario name as Title
		gameSelector.transform.Find("Header/Title").GetComponent<TMP_Text>().text = Utility.extractLocale(gameData.scenarios[scenarioKey].name);
		// create level buttons for this campaign
		for (int i = 0; i < gameData.scenarios[scenarioKey].levels.Count; i++)
		{
			GameObject missionTile = GameObject.Instantiate<GameObject>(TileMissionPrefab, gameList.transform);
			Button missionButton = missionTile.GetComponent<Button>();

			DataLevel levelData = gameData.scenarios[scenarioKey].levels[i];
			//scores
			string highScoreKey = Utility.extractFileName(levelData.src);
			int scoredStars = (userData.highScore != null ? (userData.highScore.ContainsKey(highScoreKey) ? userData.highScore[highScoreKey] : 0) : PlayerPrefs.GetInt(highScoreKey + gameData.scoreKey, 0)); //0 star by default

			Image star1 = missionTile.transform.Find("Star1").GetComponent<Image>();
			Image star2 = missionTile.transform.Find("Star2").GetComponent<Image>();
			Image star3 = missionTile.transform.Find("Star3").GetComponent<Image>();
			star1.color = scoredStars >= 1 ? missionButton.colors.highlightedColor : missionButton.colors.disabledColor;
			star2.color = scoredStars >= 2 ? missionButton.colors.highlightedColor : missionButton.colors.disabledColor;
			star3.color = scoredStars == 3 ? missionButton.colors.highlightedColor : missionButton.colors.disabledColor;

			missionTile.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = Utility.extractLocale(levelData.name);

			int tooltipText = scoredStars;
			// lock/unlock levels
			if ((userData.progression != null && userData.progression.ContainsKey(scenarioKey) && userData.progression[scenarioKey] >= i) || (userData.progression == null && PlayerPrefs.GetInt(scenarioKey, 0) >= i) || i == 0) //by default first level of directory is the only unlocked level of directory
			{
				missionButton.interactable = true;

				GameKeys gk = missionTile.GetComponent<GameKeys>();
				gk.scenarioKey = scenarioKey;
				gk.missionNumber = i;
			}
            else
			{
				missionButton.interactable = false;
				tooltipText = 4;
			}

			missionButton.StartCoroutine(delayMissionTooltipContent(missionTile, tooltipText));

			missionTile.transform.Find("Finished").gameObject.SetActive(scoredStars > 0);
			missionTile.transform.Find("Locked").gameObject.SetActive(!missionButton.interactable);

			GameObjectManager.bind(missionTile);
		}

		if (gameList.transform.childCount > 0)
			MainLoop.instance.StartCoroutine(Utility.delayGOSelection(gameList.transform.GetChild(0).gameObject));
	}

	private IEnumerator delayMissionTooltipContent(GameObject missionTile, int tooltipText)
    {
		yield return null;
		missionTile.GetComponent<TooltipContent>().text = missionTile.GetComponent<StringList>().texts[tooltipText];
	}

	public void showDetails(GameKeys keys)
	{
		TMP_Text title = gameDetails.Find("Title").GetComponentInChildren<TMP_Text>(true);
		TMP_Text gameDescription = gameDetails.Find("Scroll View").GetComponentInChildren<TMP_Text>(true);
		
		GameKeys comp_gk = competencyPanel.GetComponent<GameKeys>();
		comp_gk.scenarioKey = keys.scenarioKey;
		comp_gk.missionNumber = keys.missionNumber;

		// If the keys refer a scenario without a mission defined
		if (keys.scenarioKey != "" && keys.missionNumber == -1 && gameData.scenarios.ContainsKey(keys.scenarioKey))
		{
			GameObjectManager.setGameObjectState(gameDetails.gameObject, true);
			title.text = Utility.extractLocale(gameData.scenarios[keys.scenarioKey].name);
			gameDescription.text = Utility.extractLocale(gameData.scenarios[keys.scenarioKey].description);
		}
		// If the keys refer a scenario and a mission
		else if (keys.scenarioKey != "" && keys.missionNumber != -1 && gameData.scenarios.ContainsKey(keys.scenarioKey) && gameData.scenarios[keys.scenarioKey].levels.Count > keys.missionNumber)
        {
			GameObjectManager.setGameObjectState(gameDetails.gameObject, true);
			title.text = Utility.extractLocale(gameData.scenarios[keys.scenarioKey].levels[keys.missionNumber].name);
			gameDescription.text = "";
		}
		else
			GameObjectManager.setGameObjectState(gameDetails.gameObject, false);
	}

	// See competency selector in CompetencyPanel
	public void refreshCompetencies()
	{
		GameKeys comp_gk = competencyPanel.GetComponent<GameKeys>();
		TextMeshProUGUI details = competencyPanel.Find("CompetencyPopup/Scroll View").GetComponentInChildren<TextMeshProUGUI>(true);
		details.text = "";
		if (gameData.scenarios.ContainsKey(comp_gk.scenarioKey))
		{
			// Get current referentiel selected
			TMP_Dropdown compSelector = f_compSelector.First().GetComponent<TMP_Dropdown>();
			string referentialName = compSelector.options[compSelector.value].text;
			// Display competencies
			string txt = "";

			List<DataLevel> levelKeys = new List<DataLevel>();
			if (comp_gk.missionNumber == -1 || comp_gk.missionNumber >= gameData.scenarios[comp_gk.scenarioKey].levels.Count)
				levelKeys = gameData.scenarios[comp_gk.scenarioKey].levels;
			else
				levelKeys.Add(gameData.scenarios[comp_gk.scenarioKey].levels[comp_gk.missionNumber]);

			foreach (GameObject comp in f_competencies)
				if (comp.GetComponent<Competency>().referential == referentialName)
					foreach (DataLevel levelKey in levelKeys)
						if (gameData.levels.ContainsKey(levelKey.src) && UtilityLobby.isCompetencyMatchWithLevel(comp.GetComponent<Competency>(), gameData.levels[levelKey.src].OwnerDocument))
						{
							txt += Utility.extractLocale(comp.GetComponent<Competency>().id) + "\n";
							break;
						}
            
			if (txt != "")
				details.text += txt;
			else
				details.text += competencyPanel.GetComponent<Localization>().localization[0];
			/*LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
			// auto move to the top od the panel
			(content as RectTransform).anchoredPosition = new Vector2(0, 0);*/
		}
	}

	public void continueScenario()
    {
		// garde pour être sûr que le niveau existe dans le scénario visé, mais normalement elle est déjà gérée par l'activation ou pas du bouton "Continue"
		if (gameData.scenarios.ContainsKey(userData.currentScenario) && userData.levelToContinue != -1 && userData.levelToContinue < gameData.scenarios[userData.currentScenario].levels.Count)
			launchLevel(userData.currentScenario, userData.levelToContinue);
	}

	public void launchLevel(GameKeys gk)
	{
		launchLevel(gk.scenarioKey, gk.missionNumber);
	}

	public void launchLevel(string scenarioKey, int missionNumber) {
		gameData.selectedScenario = scenarioKey;
		gameData.levelToLoad = missionNumber;
		userData.currentScenario = scenarioKey;
		userData.levelToContinue = missionNumber;
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