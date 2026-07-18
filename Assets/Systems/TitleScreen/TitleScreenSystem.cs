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
using UnityEngine.Localization.Components;

/// <summary>
/// Manage main menu to launch a specific mission
/// </summary>
public class TitleScreenSystem : FSystem {
	private Family f_sessionId = FamilyManager.getFamily(new AllOfComponents(typeof(TextMeshProUGUI)), new AnyOfTags("SessionId"));
	private Family f_competencies = FamilyManager.getFamily(new AllOfComponents(typeof(Competency))); // Les compétences
	private Family f_avatarTarget = FamilyManager.getFamily(new AllOfComponents(typeof(Image)), new AnyOfTags("UI_AvatarTarget"));
	private Family f_fadeOutEnd = FamilyManager.getFamily(new AllOfComponents(typeof(FadeOutEnd)));
	private Family f_canvasGroup = FamilyManager.getFamily(new AllOfComponents(typeof(Canvas), typeof(CanvasGroup)));

	private GameData gameData;
	private UserData userData;
	public Button continueButton;
	public GameObject playButton;
	public GameObject gameSelector;
	public GameObject TileScenarioPrefab;
	public GameObject TileMissionPrefab;
	public GameObject quitButton;
	public TMP_Text SPYVersion;
	public CurrentSettingsValues currentSettingsValues;
	public Transform profilPanel;
	public GameObject newAvatarPanel;

	private Transform gameList;
	private Transform gameDetails;
	private string lastScenarioSelected = "";
	private int lastMissionSelected = -1;

	private bool isTileListDragged = false; // pour savoir si on est en train de dragger le panneau de présentation des scénarios/missions pour ne pas prendre en compte le clic

	private UnityAction localCallback;

	[DllImport("__Internal")]
	private static extern void ShowHtmlLoadMissions(); // call javascript

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
			GameObjectManager.addComponent<AskToLoadScene>(MainLoop.instance.gameObject, new { sceneName = "ConnexionScene" });
		else
		{
			SPYVersion.text = "V" + Application.version;

			gameData = gameDataGO.GetComponent<GameData>();
			userData = gameDataGO.GetComponent<UserData>();

			// après la fin de transition de l'animation du chargement de scène, afficher le panneau d'un nouvel avatar si un nouveau est disponible
			f_fadeOutEnd.addEntryCallback(delegate
			{
				if (userData.newAvatarAvailable > 2) // > 2 car les trois premiers sont les avatars accessibles par défaut
				{
					foreach (GameObject canvas in f_canvasGroup)
						canvas.GetComponent<CanvasGroup>().interactable = false;
					newAvatarPanel.SetActive(true);
					Transform avatarSrc = profilPanel.Find("Scroll View/Viewport/Content").GetChild(userData.newAvatarAvailable).Find("Photo");
					Transform avatarTarget = newAvatarPanel.transform.Find("Panel/AvatarIcon");
					EventSystem.current.SetSelectedGameObject(newAvatarPanel.transform.Find("Panel/Message").gameObject); // parce qu'on attend le fadeOut avant d'activer cette popup, on met à la main le focus sur son texte
					avatarTarget.GetComponent<Image>().sprite = avatarSrc.GetComponent<Image>().sprite;
					avatarTarget.GetComponent<ImgReplacementText>().replacementText = avatarSrc.GetComponent<LocalizeStringEvent>().StringReference.GetLocalizedString();
					userData.newAvatarAvailable = -1;
				}
			});
			

			gameList = gameSelector.transform.Find("GamePanel/Viewport/GameList");
			gameDetails = gameSelector.transform.Find("GameDetails");

			foreach (GameObject sID in f_sessionId)
				sID.GetComponent<TMP_Text>().text = string.Join(" ", GBL_Interface.playerName.ToCharArray());

			// Mise à jour du profil du joueur
			updatePlayerProfile();

			// gestion du bouton continue
			GameObjectManager.setGameObjectState(continueButton.gameObject, gameData.scenarios.ContainsKey(userData.currentScenario) && userData.levelToContinue != -1 && userData.levelToContinue < gameData.scenarios[userData.currentScenario].levels.Count);

			if (gameData.selectedScenario == UtilityLobby.testFromScenarioEditor) // reload scenario editor
			{
				launchScenarioEditor();
			}
			else if (gameData.selectedScenario == UtilityLobby.testFromLevelEditor) // reload level editor
			{
				launchLevelEditor();
			}
			else if (gameData.selectedScenario == UtilityLobby.testFromUrl) // go to connexion scene
            {
                launchConnexionScene();
            }
            else if (gameData.selectedScenario != "")
			{
				// Show scenario list
				lastScenarioSelected = gameData.selectedScenario;
				playButton.GetComponent<Button>().onClick.Invoke();
				// Si on n'est pas sur la dernière mission, afficher la liste des missions
				if (gameData.levelToLoad < gameData.scenarios[gameData.selectedScenario].levels.Count - 1)
				{
					// reload last opened scenario
					lastMissionSelected = gameData.levelToLoad;
					showLevels(gameData.selectedScenario);
				}
			}

			if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				ShowHtmlLoadMissions();
				GameObjectManager.setGameObjectState(quitButton, false);
			}
		}

		Pause = true;
	}

	public void updatePlayerProfile()
	{
		// Affichage du bon avatar come icône de profil
		foreach (GameObject go in f_avatarTarget)
			go.GetComponent<Image>().sprite = profilPanel.Find("Scroll View/Viewport/Content").GetChild(userData.avatarSelected).Find("Photo").GetComponent<Image>().sprite;

		// Calcul du pourcentage de progression et du nombre d'étoiles obtenues
		int nbMissions = 0;
		int acquiredStars = 0;
		int progression = 0;
		foreach (string key in gameData.scenarios.Keys)
		{
			foreach (DataLevel dl in gameData.scenarios[key].levels)
			{
				string highScoreKey = Utility.extractFileName(dl.filePath);
				acquiredStars += !userData.highScore.ContainsKey(highScoreKey) ? 0 : userData.highScore[highScoreKey]; //0 star by default
			}
			nbMissions += gameData.scenarios[key].levels.Count;
			progression += userData.progression.ContainsKey(key) ? userData.progression[key] : 0;
		}
		int progress = 100 * progression / nbMissions;
		// affichage de la progression numérique
		profilPanel.Find("ProgressValue").GetComponent<TMP_Text>().text = progress + "%";
		// ajustement de la barre de progression
		RectTransform progressBar = profilPanel.Find("ProgressBar") as RectTransform;
		RectTransform bar = profilPanel.Find("ProgressBar/Bar") as RectTransform;
		bar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, progressBar.rect.width * ((float)progress / 100));
		// affichage du nombre d'étoiles
		profilPanel.Find("StarsWon/EarnedStars").GetComponent<TMP_Text>().text = "" + acquiredStars;
		profilPanel.Find("StarsWon/StarsCount").GetComponent<TMP_Text>().text = "" + (nbMissions*3);

		int newAvatarAvailable = 0;
		if (acquiredStars >= 60 && !userData.unlockedAvatars.Contains(10))
			newAvatarAvailable = 10;
		else if (acquiredStars >= 120 && !userData.unlockedAvatars.Contains(11))
			newAvatarAvailable = 11;
		else if (acquiredStars >= 186 && !userData.unlockedAvatars.Contains(12))
			newAvatarAvailable = 12;

		if (newAvatarAvailable != 0)
        {
			userData.newAvatarAvailable = newAvatarAvailable;
			userData.unlockedAvatars.Add(newAvatarAvailable);
			sendUserData();
		}


		// Dévérouiller les bons avatars dans la bibliothèque des avatars
		foreach (int avatarId in userData.unlockedAvatars)
		{
			Transform avatar = profilPanel.Find("Scroll View/Viewport/Content").GetChild(avatarId);
			avatar.GetComponent<Toggle>().interactable = true;
			GameObjectManager.setGameObjectState(avatar.Find("Locked").gameObject, false);
		}
	}

	// Fonction appelée depuis le javascript (voir Assets/WebGLTemplates/Custom/game.html) via le Wrapper du Système
	public void importLevelOrScenario(string content)
	{
		Localization loc = gameData.GetComponent<Localization>();
		Utility.JavaScriptData jsd = JsonUtility.FromJson<Utility.JavaScriptData>(content);
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

		Transform backButton = gameSelector.transform.Find("Header/BackButtonsProxy/BackMainMenu");
		GameObjectManager.setGameObjectState(backButton.gameObject, true);
		GameObjectManager.setGameObjectState(gameSelector.transform.Find("Header/BackButtonsProxy/BackScenarios").gameObject, false);
		GameObjectManager.setGameObjectState(gameSelector.transform.Find("Header/Title").gameObject, false);
		Selectable detailstitle = gameDetails.Find("Title").GetComponentInChildren<Selectable>(true);

		//create scenarios' button
		List<string> sortedScenarios = new List<string>();
		foreach (string key in gameData.scenarios.Keys)
			if (key != UtilityLobby.testFromScenarioEditor && key != UtilityLobby.testFromLevelEditor && key !=  UtilityLobby.testFromUrl && key != UtilityLobby.editingScenario) // we don't create a button for tested level
				sortedScenarios.Add(key);
		sortedScenarios.Sort();
		foreach (string key in sortedScenarios)
		{
			GameObject scenarioTile = GameObject.Instantiate<GameObject>(TileScenarioPrefab, gameList);

			scenarioTile.transform.Find("Finished").gameObject.SetActive(userData.progression.ContainsKey(key) && userData.progression[key] >= gameData.scenarios[key].levels.Count);
			
			int totalStars = 0;
			foreach (DataLevel dl in gameData.scenarios[key].levels)
			{
				string highScoreKey = Utility.extractFileName(dl.filePath);
				totalStars += !userData.highScore.ContainsKey(highScoreKey) ? 0 : userData.highScore[highScoreKey]; //0 star by default
			}
			scenarioTile.transform.Find("TotalStars").GetComponent<TextMeshProUGUI>().text = totalStars + "/" + (gameData.scenarios[key].levels.Count * 3);

			if (userData.progression.ContainsKey(key))
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

		// reset navigation links
		DynamicNavigation backDN = backButton.GetComponent<DynamicNavigation>();
		backDN.UpLeft[backDN.UpLeft.Length-1] = null;
		backDN.DownRight[backDN.DownRight.Length - 1] = null;

		if (gameList.childCount > 0)
		{
			// build navigation links
			backDN.UpLeft[backDN.UpLeft.Length - 1] = gameList.GetChild(0).gameObject.GetComponent<Selectable>();
			backDN.DownRight[backDN.DownRight.Length - 1] = gameList.GetChild(0).gameObject.GetComponent<Selectable>();
			for (int i = 0; i < gameList.childCount  ; i++)
            {
				Selectable tile = gameList.GetChild(i).GetComponent<Selectable>();
				Navigation nav = tile.navigation;
				nav.selectOnUp = backButton.GetComponent<Selectable>();
				nav.selectOnLeft = i>0 ? gameList.GetChild(i-1).GetComponent<Selectable>() : null;
				nav.selectOnRight = i<gameList.childCount-1 ? gameList.GetChild(i + 1).GetComponent<Selectable>() : null;
				nav.selectOnDown = detailstitle;
				tile.navigation = nav;
			}

			if (lastScenarioSelected != "")
			{
				// focus on previous selected scenario
				foreach (Transform child in gameList)
					if (child.GetComponent<GameKeys>().scenarioKey == lastScenarioSelected)
					{
						MainLoop.instance.StartCoroutine(Utility.delayGOSelection(child.gameObject));
						break;
					}
			}
			else
				// focus on the first scenario
				MainLoop.instance.StartCoroutine(Utility.delayGOSelection(gameList.GetChild(0).gameObject));
		}
		else
			EventSystem.current.SetSelectedGameObject(backButton.gameObject);
	}

	private IEnumerator delayScenarioTooltipContent(GameObject scenarioTile)
	{
		yield return null;
		StringList sl = scenarioTile.GetComponent<StringList>();
		scenarioTile.GetComponent<TooltipContent>().text = scenarioTile.transform.Find("Name").GetComponent<TextMeshProUGUI>().text + "<br>" + sl.texts[0] + scenarioTile.transform.Find("Percentage").GetComponent<TextMeshProUGUI>().text + "<br>" + sl.texts[0]+ scenarioTile.transform.Find("TotalStars").GetComponent<TextMeshProUGUI>().text;
	}

	public void setDraggingState(bool newState){
		isTileListDragged = newState;
	}

	public void showLevels(GameKeys keys)
    {
		showLevels(keys.scenarioKey);
    }

	private void showLevels(string scenarioKey)
	{
		if (isTileListDragged)
			return;

		lastScenarioSelected = scenarioKey;

		// delete all old tiles
		removeAllOldTiles();

		GameObjectManager.setGameObjectState(gameSelector.transform.Find("Header/BackButtonsProxy/BackMainMenu").gameObject, false);
		Transform backButton = gameSelector.transform.Find("Header/BackButtonsProxy/BackScenarios");
		GameObjectManager.setGameObjectState(backButton.gameObject, true);
		Transform headerTitle = gameSelector.transform.Find("Header/Title");
		Selectable detailstitle = gameDetails.Find("Title").GetComponentInChildren<Selectable>(true);
		GameObjectManager.setGameObjectState(headerTitle.gameObject, true);

		// set scenario name as Title
		headerTitle.GetComponent<TMP_Text>().text = Utility.extractLocale(gameData.scenarios[scenarioKey].name);
		// create level buttons for this campaign
		for (int i = 0; i < gameData.scenarios[scenarioKey].levels.Count; i++)
		{
			GameObject missionTile = GameObject.Instantiate<GameObject>(TileMissionPrefab, gameList);
			Button missionButton = missionTile.GetComponent<Button>();

			DataLevel levelData = gameData.scenarios[scenarioKey].levels[i];
			//scores
			string highScoreKey = Utility.extractFileName(levelData.filePath);
			int scoredStars = !userData.highScore.ContainsKey(highScoreKey) ? 0 : userData.highScore[highScoreKey]; //0 star by default

			Image star1 = missionTile.transform.Find("Star1").GetComponent<Image>();
			Image star2 = missionTile.transform.Find("Star2").GetComponent<Image>();
			Image star3 = missionTile.transform.Find("Star3").GetComponent<Image>();
			star1.color = scoredStars >= 1 ? missionButton.colors.highlightedColor : missionButton.colors.disabledColor;
			star2.color = scoredStars >= 2 ? missionButton.colors.highlightedColor : missionButton.colors.disabledColor;
			star3.color = scoredStars == 3 ? missionButton.colors.highlightedColor : missionButton.colors.disabledColor;

			missionTile.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = Utility.extractLocale(levelData.missionName);

			int tooltipText = scoredStars;
			// lock/unlock levels
			if ((userData.progression.ContainsKey(scenarioKey) && userData.progression[scenarioKey] >= i) || i == 0) //by default first level of directory is the only unlocked level of directory
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

		// reset navigation links
		DynamicNavigation backDN = backButton.GetComponent<DynamicNavigation>();
		backDN.UpLeft[backDN.UpLeft.Length - 1] = null;
		DynamicNavigation titleDN = headerTitle.GetComponent<DynamicNavigation>();
		titleDN.DownRight[titleDN.DownRight.Length - 1] = null;

		if (gameList.childCount > 0)
		{
			// build navigation links
			backDN.UpLeft[backDN.UpLeft.Length - 1] = gameList.GetChild(0).gameObject.GetComponent<Selectable>();
			titleDN.DownRight[titleDN.DownRight.Length - 1] = gameList.GetChild(0).gameObject.GetComponent<Selectable>();
			for (int i = 0; i < gameList.childCount; i++)
			{
				Selectable tile = gameList.GetChild(i).GetComponent<Selectable>();
				Navigation nav = tile.navigation;
				nav.selectOnUp = headerTitle.GetComponent<Selectable>();
				nav.selectOnLeft = i > 0 ? gameList.GetChild(i - 1).GetComponent<Selectable>() : null;
				nav.selectOnRight = i < gameList.childCount - 1 ? gameList.GetChild(i + 1).GetComponent<Selectable>() : null;
				nav.selectOnDown = detailstitle;
				tile.navigation = nav;
			}

			if (lastMissionSelected != -1)
			{
				// focus on previous selected scenario
				foreach (Transform child in gameList)
					if (child.GetComponent<GameKeys>().missionNumber == lastMissionSelected)
					{
						MainLoop.instance.StartCoroutine(Utility.delayGOSelection(child.gameObject));
						break;
					}
			}
			else
				// focus on the first mission
				MainLoop.instance.StartCoroutine(Utility.delayGOSelection(gameList.GetChild(0).gameObject));
		}
		else
			EventSystem.current.SetSelectedGameObject(backButton.gameObject);
	}

	private IEnumerator delayMissionTooltipContent(GameObject missionTile, int tooltipText)
    {
		yield return null;
		missionTile.GetComponent<TooltipContent>().text = missionTile.GetComponent<StringList>().texts[tooltipText];
	}

	public void showDetails(GameKeys keys)
	{
		Selectable curTile = keys.GetComponent<Selectable>();

		// Set down navigation for header
		Transform headerTitle = gameSelector.transform.Find("Header/Title");
		DynamicNavigation titleDN = headerTitle.GetComponent<DynamicNavigation>();
		titleDN.DownRight[titleDN.DownRight.Length - 1] = curTile;
		Transform backButton = gameSelector.transform.Find("Header/BackButtonsProxy/BackMainMenu");
		DynamicNavigation backDN = backButton.GetComponent<DynamicNavigation>();
		backDN.DownRight[backDN.DownRight.Length - 1] = curTile;

		// Set up/left navigation for details title
		TMP_Text detailsTitle = gameDetails.Find("Title").GetComponentInChildren<TMP_Text>(true);
		Selectable titleSel = detailsTitle.GetComponent<Selectable>();
		Navigation titleNav = titleSel.navigation;
		titleNav.selectOnLeft = curTile;
		titleNav.selectOnUp = curTile;
		titleSel.navigation = titleNav;

		Localization loc = gameDetails.GetComponentInParent<Localization>(true);
        detailsTitle.text = loc.localization[1]; // default show "Mission locked"
		GameObject gameDescription = gameDetails.Find("Scroll View").gameObject;
		Image miniView = gameDetails.Find("MiniView").GetComponent<Image>();

		TMP_Text descDetails = gameDescription.GetComponentInChildren<TMP_Text>(true);
        // If the keys refer a scenario without a mission defined => show scenario data
        if (keys.scenarioKey != "" && keys.missionNumber == -1 && gameData.scenarios.ContainsKey(keys.scenarioKey))
		{
			detailsTitle.text = Utility.extractLocale(gameData.scenarios[keys.scenarioKey].name);
            descDetails.text = Utility.extractLocale(gameData.scenarios[keys.scenarioKey].description);
			GameObjectManager.setGameObjectState(miniView.gameObject, false);
            GameObjectManager.setGameObjectState(gameDescription, true);
        }
		// If the keys refer a scenario and a mission => show mission data
		else if (keys.scenarioKey != "" && keys.missionNumber != -1 && gameData.scenarios.ContainsKey(keys.scenarioKey) && gameData.scenarios[keys.scenarioKey].levels.Count > keys.missionNumber)
        {
			detailsTitle.text = Utility.extractLocale(gameData.scenarios[keys.scenarioKey].levels[keys.missionNumber].missionName);
            descDetails.text = "";
			GameObjectManager.setGameObjectState(miniView.gameObject, true);
            GameObjectManager.setGameObjectState(gameDescription, true);
            // try to load mini view
            MainLoop.instance.StartCoroutine(Utility.GetTextureWebRequest(gameData.scenarios[keys.scenarioKey].levels[keys.missionNumber].filePath.Replace(".xml", currentSettingsValues.values.currentLanguage == 1 ? "_en.png" : ".png"), miniView));
		}
		// Else locked mission
        else
		{
			GameObjectManager.setGameObjectState(gameDescription, false);
			GameObjectManager.setGameObjectState(miniView.gameObject, false);
		}

		// Show skills
		if (descDetails.text != "")
            descDetails.text += "\n\n";
        descDetails.text += "<b>"+loc.localization[3]+"</b>\n";
        if (gameData.scenarios.ContainsKey(keys.scenarioKey))
        {
            // Display competencies
            string txt = "";

            List<DataLevel> levelKeys = new List<DataLevel>();
            if (keys.missionNumber == -1 || keys.missionNumber >= gameData.scenarios[keys.scenarioKey].levels.Count)
                levelKeys = gameData.scenarios[keys.scenarioKey].levels;
            else
                levelKeys.Add(gameData.scenarios[keys.scenarioKey].levels[keys.missionNumber]);

            foreach (GameObject comp in f_competencies)
                if (comp.GetComponent<Competency>().referentialId == currentSettingsValues.values.currentSkillsRepository)
                    foreach (DataLevel levelKey in levelKeys)
                        if (gameData.levels.ContainsKey(levelKey.filePath) && UtilityLobby.isCompetencyMatchWithLevel(comp.GetComponent<Competency>(), gameData.levels[levelKey.filePath].OwnerDocument))
                        {
                            txt += "\t - "+Utility.extractLocale(comp.GetComponent<Competency>().id) + "\n";
                            break;
                        }

            if (txt != "")
                descDetails.text += txt;
            else
                descDetails.text += "\t - "+loc.localization[0];
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
		if (!isTileListDragged)
			launchLevel(gk.scenarioKey, gk.missionNumber);
	}

	public void launchLevel(string scenarioKey, int missionNumber) {
		gameData.selectedScenario = scenarioKey;
		gameData.levelToLoad = missionNumber;
		userData.currentScenario = scenarioKey;
		userData.levelToContinue = missionNumber;
		GameObjectManager.addComponent<AskToLoadScene>(MainLoop.instance.gameObject, new { sceneName = "MainScene" });
	}

	public void launchLevelEditor()
	{
		GameObjectManager.addComponent<AskToLoadScene>(MainLoop.instance.gameObject, new { sceneName = "MissionEditor" });
	}

	public void launchScenarioEditor()
	{
		GameObjectManager.addComponent<AskToLoadScene>(MainLoop.instance.gameObject, new { sceneName = "ScenarioEditor" });
	}

	public void launchConnexionScene()
	{
		GameObjectManager.addComponent<AskToLoadScene>(MainLoop.instance.gameObject, new { sceneName = "ConnexionScene" });
	}

	// See Quitter button in editor
	public void quitGame(){
		Application.Quit();
	}

	// See OkButton in ProfilPanel in ConnexionScene scene
	public void sendUserData()
	{
		GameObjectManager.addComponent<SendUserData>(MainLoop.instance.gameObject);
	}
}