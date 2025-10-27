﻿using UnityEngine;
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
	public GameObject mainCanvas;
	public GameObject mainMenu;
	public GameObject listOfCampaigns;
	public GameObject listOfLevels;
	public GameObject playButton;
	public GameObject quitButton;
	public GameObject detailsCampaign;

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
			gameData = gameDataGO.GetComponent<GameData>();
			userData = gameDataGO.GetComponent<UserData>();

			foreach (GameObject sID in f_sessionId)
				sID.GetComponent<TMP_Text>().text = string.Join(" ", GBL_Interface.playerName.ToCharArray());

			
			Transform spyMenu = mainCanvas.transform.Find("SPYMenu");
			mainMenu.GetComponentInParent<CanvasGroup>().interactable = true;
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
				GameObjectManager.setGameObjectState(spyMenu.Find("MenuCampaigns").gameObject, false); // be sure campaign menu is disabled
				GameObjectManager.setGameObjectState(spyMenu.Find("MenuLevels").gameObject, true); // enable levels menu
				MainLoop.instance.StartCoroutine(Utility.delayGOSelection(spyMenu.Find("MenuLevels").Find("Retour").gameObject));
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
			if (key != UtilityLobby.testFromScenarioEditor && key != UtilityLobby.testFromLevelEditor && key !=  UtilityLobby.testFromUrl && key != UtilityLobby.editingScenario) // we don't create a button for tested level
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
		if (listOfCampaigns.transform.childCount > 0)
			MainLoop.instance.StartCoroutine(Utility.delayGOSelection(listOfCampaigns.transform.GetChild(0).gameObject));
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

		refreshCompetencies(content);

		Button bt_showLevels = detailsCampaign.transform.GetComponentInChildren<Button>();
		bt_showLevels.onClick.RemoveAllListeners();
		bt_showLevels.onClick.AddListener(delegate { showLevels(campaignKey); });
		bt_showLevels.onClick.AddListener(delegate { GameObjectManager.setGameObjectState(detailsCampaign, false); });
		EventSystem.current.SetSelectedGameObject(bt_showLevels.gameObject);
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
			Transform stars = button_go.transform.Find("ScoreCanvas").Find("Stars");
			GameObjectManager.setGameObjectState(stars.gameObject, button.interactable);
			stars.GetComponent<Image>().sprite = stars.GetComponent<SpriteList>().source[scoredStars];
			stars.GetComponent<TooltipContent>().text = stars.GetComponent<StringList>().texts[scoredStars];
		}

		if (listOfLevels.transform.childCount > 0)
			MainLoop.instance.StartCoroutine(Utility.delayGOSelection(listOfLevels.transform.GetChild(0).Find("Button").gameObject));
	}

	public void launchLevel(string campaignKey, int levelToLoad) {
		gameData.selectedScenario = campaignKey;
		gameData.levelToLoad = levelToLoad;
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

	// See Quitter button in editor
	public void quitGame(){
		Application.Quit();
	}
}