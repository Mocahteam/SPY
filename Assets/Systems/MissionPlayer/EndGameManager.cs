using UnityEngine;
using FYFY;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// This system check if the end of the level is reached and display end panel accordingly
/// </summary>
public class EndGameManager : FSystem {

	public static EndGameManager instance;

	private Family f_requireEndPanel = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));

	private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));
    private Family f_newCurrentAction = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction), typeof(BasicAction)));
	private Family f_exit = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Exit"));

	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	
	private GameData gameData;

	public GameObject playButtonAmount;
	public GameObject endPanel;

	public EndGameManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();

		// Pour être sûr que le composant de Localization se synchronise bien avec la langue choisie, on active le end panel...
		if (!endPanel.transform.parent.gameObject.activeInHierarchy)
			endPanel.transform.parent.gameObject.SetActive(true);
		// ... et on le désactive pour laisser le temps pour la synchronisation de la Localization et surtout pour être sûr que le panneau n'est pas visible au joueur
		MainLoop.instance.StartCoroutine(delayDisableEndPanel());

		f_requireEndPanel.addEntryCallback(displayEndPanel);

		// each time a current action is removed, we check if the level is over
		f_newCurrentAction.addExitCallback(delegate {
			MainLoop.instance.StartCoroutine(delayCheckEnd());
		});

		f_playingMode.addExitCallback(delegate {
			MainLoop.instance.StartCoroutine(delayNoMoreAttemptDetection());
		});
	}

	private IEnumerator delayDisableEndPanel()
    {
		yield return null;
		GameObjectManager.setGameObjectState(endPanel.transform.parent.gameObject, false);
	}

	private IEnumerator delayCheckEnd()
	{
		// wait 2 frames before checking if a new currentAction was produced
		yield return null; // this frame the currentAction is removed
		yield return null; // this frame a probably new current action is created
		
		// Now, families are informed if new current action was produced, we can check if no currentAction exists on players and if all players are on the end of the level
		if (!playerHasCurrentAction())
		{
			int nbEnd = 0;
			bool endDetected = false;
			// parse all exits
			for (int e = 0; e < f_exit.Count && !endDetected; e++)
			{
				GameObject exit = f_exit.getAt(e);
				// parse all players
				for (int p = 0; p < f_player.Count && !endDetected; p++)
				{
					GameObject player = f_player.getAt(p);
					// check if positions are equals
					if (player.GetComponent<Position>().x == exit.GetComponent<Position>().x && player.GetComponent<Position>().y == exit.GetComponent<Position>().y)
						nbEnd++;
				}
			}
			// if all players reached end position or all exits are filled
			if (nbEnd >= f_exit.Count || nbEnd >= f_player.Count)
				// trigger end
				GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.Win });
		}
	}

	private bool playerHasCurrentAction()
	{
		foreach (GameObject go in f_newCurrentAction)
		{
			if (go.GetComponent<CurrentAction>().agent.CompareTag("Player"))
				return true;
		}
		return false;
	}

	// Display panel with appropriate content depending on end
	private void displayEndPanel(GameObject unused)
	{
		// display end panel (we need immediate enabling)
		endPanel.GetComponentInParent<Canvas>().GetComponent<CanvasGroup>().interactable = false;
		endPanel.transform.parent.gameObject.SetActive(true);
		GameObjectManager.setGameObjectState(endPanel.transform.Find("Score").gameObject, false);
		GameObjectManager.setGameObjectState(endPanel.transform.Find("Feedback").gameObject, false);
		// Get the first end that occurs
		if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.Detected)
		{
			GameObjectManager.setGameObjectState(endPanel.transform.Find("StarsCanvas").gameObject, false);
			endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = endPanel.GetComponent<Localization>().localization[0];
			Transform buttons = endPanel.transform.Find("Buttons");
			GameObjectManager.setGameObjectState(buttons.Find("ReloadLevel").gameObject, true);
			GameObjectManager.setGameObjectState(buttons.Find("ReloadState").gameObject, true);
			GameObjectManager.setGameObjectState(buttons.Find("MainMenu").gameObject, true);
			GameObjectManager.setGameObjectState(buttons.Find("NextLevel").gameObject, false);

			AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
			audio.clip = Resources.Load("Sound/LoseSound") as AudioClip;
			audio.loop = true;
			audio.Play();

			MainLoop.instance.StartCoroutine(delaySendStatement(endPanel, new
			{
				verb = "completed",
				objectType = "level",
				result = true,
				success = -1,
				resultExtensions = new Dictionary<string, string>() {
					{ "error", "Detected" }
				}
			}));
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.Win)
		{
			int _score = (10000 / (gameData.totalActionBlocUsed + 1) + 5000 / (gameData.totalStep + 1) + 6000 / (gameData.totalExecute + 1) + 5000 * gameData.totalCoin);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("StarsCanvas").gameObject, true);
			Debug.Log("Score: " + _score);
			setScoreStars(_score);

			endPanel.GetComponentInParent<AudioSource>().PlayOneShot(Resources.Load("Sound/VictorySound") as AudioClip);
			
			Transform buttons = endPanel.transform.Find("Buttons");
			GameObjectManager.setGameObjectState(buttons.Find("ReloadLevel").gameObject, true);
			GameObjectManager.setGameObjectState(buttons.Find("ReloadState").gameObject, false);
			GameObjectManager.setGameObjectState(buttons.Find("MainMenu").gameObject, true);
			GameObjectManager.setGameObjectState(buttons.Find("NextLevel").gameObject, true);

			// Sauvegarde de l'état d'avancement des niveaux dans le scénario
			UserData ud = gameData.GetComponent<UserData>();
			if (ud.progression != null && (!ud.progression.ContainsKey(gameData.selectedScenario) || ud.progression[gameData.selectedScenario] < gameData.levelToLoad + 1))
				ud.progression[gameData.selectedScenario] = gameData.levelToLoad + 1;

			if (PlayerPrefs.GetInt(gameData.selectedScenario, 0) < gameData.levelToLoad + 1)
				PlayerPrefs.SetInt(gameData.selectedScenario, gameData.levelToLoad + 1);
			PlayerPrefs.Save();

			//Check if next level exists in campaign
			if (gameData.levelToLoad >= gameData.scenarios[gameData.selectedScenario].levels.Count - 1)
			{
				GameObjectManager.setGameObjectState(buttons.Find("NextLevel").gameObject, false);
				endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = endPanel.GetComponent<Localization>().localization[1];
				if (gameData.selectedScenario != UtilityLobby.testFromScenarioEditor && gameData.selectedScenario != UtilityLobby.testFromLevelEditor && gameData.selectedScenario != UtilityLobby.testFromUrl)
				{
					ud.currentScenario = "";
					ud.levelToContinue = -1;
				}
			}
			else
			{
				endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = endPanel.GetComponent<Localization>().localization[2];
				if (gameData.selectedScenario != UtilityLobby.testFromScenarioEditor && gameData.selectedScenario != UtilityLobby.testFromLevelEditor && gameData.selectedScenario != UtilityLobby.testFromUrl)
					ud.levelToContinue++;
			}
			MainLoop.instance.StartCoroutine(delaySendStatement(endPanel, new
			{
				verb = "completed",
				objectType = "level",
				result = true,
				success = 1,
				resultExtensions = new Dictionary<string, string>() {
					{ "score", _score.ToString() }
				}
			}));
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.BadCondition)
		{
			GameObjectManager.setGameObjectState(endPanel.transform.Find("StarsCanvas").gameObject, false);
			endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = endPanel.GetComponent<Localization>().localization[3];
			Transform buttons = endPanel.transform.Find("Buttons");
			GameObjectManager.setGameObjectState(buttons.Find("ReloadLevel").gameObject, false);
			GameObjectManager.setGameObjectState(buttons.Find("ReloadState").gameObject, true);
			GameObjectManager.setGameObjectState(buttons.Find("MainMenu").gameObject, false);
			GameObjectManager.setGameObjectState(buttons.Find("NextLevel").gameObject, false);

			AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
			audio.clip = Resources.Load("Sound/LoseSound") as AudioClip;
			audio.loop = true;
			audio.Play();

			MainLoop.instance.StartCoroutine(delaySendStatement(endPanel, new
			{
				verb = "bugged",
				objectType = "program",
				activityExtensions = new Dictionary<string, string>() {
					{ "error", "BadCondition" }
				}
			}));
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.NoMoreAttempt)
		{
			GameObjectManager.setGameObjectState(endPanel.transform.Find("StarsCanvas").gameObject, false);
			endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = endPanel.GetComponent<Localization>().localization[4];
			Transform buttons = endPanel.transform.Find("Buttons");
			GameObjectManager.setGameObjectState(buttons.Find("ReloadLevel").gameObject, true);
			GameObjectManager.setGameObjectState(buttons.Find("ReloadState").gameObject, false);
			GameObjectManager.setGameObjectState(buttons.Find("MainMenu").gameObject, true);
			GameObjectManager.setGameObjectState(buttons.Find("NextLevel").gameObject, false);

			AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
			audio.clip = Resources.Load("Sound/LoseSound") as AudioClip;
			audio.loop = true;
			audio.Play();

			MainLoop.instance.StartCoroutine(delaySendStatement(endPanel, new
			{
				verb = "completed",
				objectType = "level",
				result = true,
				success = -1,
				resultExtensions = new Dictionary<string, string>() {
					{ "error", "NoMoreAttempt" }
				}
			}));
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.NoAction)
		{
			GameObjectManager.setGameObjectState(endPanel.transform.Find("StarsCanvas").gameObject, false);
			endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = endPanel.GetComponent<Localization>().localization[5];
			Transform buttons = endPanel.transform.Find("Buttons");
			GameObjectManager.setGameObjectState(buttons.Find("ReloadLevel").gameObject, false);
			GameObjectManager.setGameObjectState(buttons.Find("ReloadState").gameObject, true);
			GameObjectManager.setGameObjectState(buttons.Find("MainMenu").gameObject, false);
			GameObjectManager.setGameObjectState(buttons.Find("NextLevel").gameObject, false);

			AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
			audio.clip = Resources.Load("Sound/LoseSound") as AudioClip;
			audio.loop = true;
			audio.Play();

			MainLoop.instance.StartCoroutine(delaySendStatement(endPanel, new
			{
				verb = "bugged",
				objectType = "program",
				activityExtensions = new Dictionary<string, string>() {
					{ "error", "NoActionToExecute" }
				}
			}));
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.NamingError)
		{
			GameObjectManager.setGameObjectState(endPanel.transform.Find("StarsCanvas").gameObject, false);
			endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = endPanel.GetComponent<Localization>().localization[8];
			Transform buttons = endPanel.transform.Find("Buttons");
			GameObjectManager.setGameObjectState(buttons.Find("ReloadLevel").gameObject, false);
			GameObjectManager.setGameObjectState(buttons.Find("ReloadState").gameObject, true);
			GameObjectManager.setGameObjectState(buttons.Find("MainMenu").gameObject, false);
			GameObjectManager.setGameObjectState(buttons.Find("NextLevel").gameObject, false);

			AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
			audio.clip = Resources.Load("Sound/LoseSound") as AudioClip;
			audio.loop = true;
			audio.Play();

			MainLoop.instance.StartCoroutine(delaySendStatement(endPanel, new
			{
				verb = "bugged",
				objectType = "program",
				activityExtensions = new Dictionary<string, string>() {
					{ "error", "NamingError" }
				}
			}));
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.InfiniteLoop)
		{
			GameObjectManager.setGameObjectState(endPanel.transform.Find("StarsCanvas").gameObject, false);
			endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = endPanel.GetComponent<Localization>().localization[6];
			Transform buttons = endPanel.transform.Find("Buttons");
			GameObjectManager.setGameObjectState(buttons.Find("ReloadLevel").gameObject, false);
			GameObjectManager.setGameObjectState(buttons.Find("ReloadState").gameObject, true);
			GameObjectManager.setGameObjectState(buttons.Find("MainMenu").gameObject, false);
			GameObjectManager.setGameObjectState(buttons.Find("NextLevel").gameObject, false);

			AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
			audio.clip = Resources.Load("Sound/LoseSound") as AudioClip;
			audio.loop = true;
			audio.Play();

			MainLoop.instance.StartCoroutine(delaySendStatement(endPanel, new
			{
				verb = "bugged",
				objectType = "program",
				activityExtensions = new Dictionary<string, string>() {
					{ "error", "InfiniteLoop" }
				}
			}));
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.Error)
		{
			GameObjectManager.setGameObjectState(endPanel.transform.Find("StarsCanvas").gameObject, false);
			endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = endPanel.GetComponent<Localization>().localization[7];
			Transform buttons = endPanel.transform.Find("Buttons");
			GameObjectManager.setGameObjectState(buttons.Find("ReloadLevel").gameObject, false);
			GameObjectManager.setGameObjectState(buttons.Find("ReloadState").gameObject, false);
			GameObjectManager.setGameObjectState(buttons.Find("MainMenu").gameObject, true);
			GameObjectManager.setGameObjectState(buttons.Find("NextLevel").gameObject, false);

			AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
			audio.clip = Resources.Load("Sound/LoseSound") as AudioClip;
			audio.loop = true;
			audio.Play();

			MainLoop.instance.StartCoroutine(delaySendStatement(endPanel, new
			{
				verb = "bugged",
				objectType = "level",
				activityExtensions = new Dictionary<string, string>() {
					{ "error", "XMLError" }
				}
			}));
		}
		// Force to focus on "Content" child
		MainLoop.instance.StartCoroutine(delayNewButtonFocused(endPanel.transform.Find("Content").gameObject));
	}

	private IEnumerator delayNewButtonFocused(GameObject target)
    {
		yield return new WaitForSeconds(0.5f); // Wait other scripts define wanted selected button to override it
		EventSystem.current.SetSelectedGameObject(target);
	}

	private IEnumerator delaySendStatement(GameObject src, object componentValues)
    {
		yield return null;
		GameObjectManager.addComponent<ActionPerformedForLRS>(src, componentValues);
		yield return null;
		yield return null;
		GameObjectManager.addComponent<SendUserData>(MainLoop.instance.gameObject);
	}

	// Gére le nombre d'étoile à afficher selon le score obtenue
	private void setScoreStars(int score)
	{
		// Détermine le nombre d'étoile à afficher
		int scoredStars = 0;
		if (gameData.levelToLoadScore != null)
		{
			//check 0, 1, 2 or 3 stars
			if (score >= gameData.levelToLoadScore[0])
			{
				scoredStars = 3;
			}
			else if (score >= gameData.levelToLoadScore[1])
			{
				scoredStars = 2;
			}
			else
			{
				scoredStars = 1;
			}
		}

		// Affiche le nombre d'étoile désiré
		Transform stars = endPanel.transform.Find("StarsCanvas");
		Button colorModel = endPanel.GetComponentInChildren<Button>(true);

		Image star1 = stars.transform.Find("Star1").GetComponent<Image>();
		Image star2 = stars.transform.Find("Star2").GetComponent<Image>();
		Image star3 = stars.transform.Find("Star3").GetComponent<Image>();
		star1.color = scoredStars >= 1 ? colorModel.colors.highlightedColor : colorModel.colors.disabledColor;
		star2.color = scoredStars >= 2 ? colorModel.colors.highlightedColor : colorModel.colors.disabledColor;
		star3.color = scoredStars == 3 ? colorModel.colors.highlightedColor : colorModel.colors.disabledColor;
		stars.GetComponent<TooltipContent>().text = stars.GetComponent<StringList>().texts[scoredStars];

		// Affichage du score
		GameObject score_go = endPanel.transform.Find("Score").gameObject;
		GameObjectManager.setGameObjectState(score_go, true);
		score_go.GetComponent<TMP_Text>().text = score + " / " + gameData.levelToLoadScore[0];

		// Si moins de 3 étoiles affichage du feedback
		if (scoredStars < 3)
			GameObjectManager.setGameObjectState(endPanel.transform.Find("Feedback").gameObject, true);

		//save score only if better score
		UserData ud = gameData.GetComponent<UserData>();
		DataLevel levelToLoad = gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad];
		string highScoreKey = Utility.extractFileName(levelToLoad.src);
		int savedScore = ud.highScore != null ? (ud.highScore.ContainsKey(highScoreKey) ? ud.highScore[highScoreKey] : 0) : PlayerPrefs.GetInt(highScoreKey + gameData.scoreKey, 0);
		
		if (savedScore < scoredStars)
		{
			PlayerPrefs.SetInt(highScoreKey + gameData.scoreKey, scoredStars);
			if (ud.highScore != null)
				ud.highScore[highScoreKey] = scoredStars;
		}
	}

	// Cancel End (see ReloadState button in editor)
	public void cancelEnd()
	{
		foreach (GameObject endGO in f_requireEndPanel)
			// in case of several ends pop in the same time (for instance exit reached and detected)
			foreach (NewEnd end in endGO.GetComponents<NewEnd>())
				GameObjectManager.removeComponent(end);
	}

	private IEnumerator delayNoMoreAttemptDetection()
	{
		// wait three frames in case win will be detected (win is priority with noMoreAttempt)
		yield return null;
		yield return null;
		yield return null;
		if (f_requireEndPanel.Count <= 0 && playButtonAmount.activeSelf && playButtonAmount.GetComponentInChildren<TMP_Text>().text == "0")
		{
			GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.NoMoreAttempt });
		}
	}
}
