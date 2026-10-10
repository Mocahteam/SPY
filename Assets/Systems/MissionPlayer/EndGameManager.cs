using UnityEngine;
using FYFY;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// This system check if the end of the level is reached and display end panel accordingly
/// </summary>
public class EndGameManager : FSystem {

	public static EndGameManager instance;

	private Family f_requireEndPanel = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));

	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	
	private GameData gameData;

	public GameObject playButtonAmount;
	public GameObject endPanel;

	public AudioClip LoseSound;
	public AudioClip VictorySound;

	public EndGameManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();

        // on s'assure que le end panel est bien désactivé au démarrage du jeu
        endPanel.transform.parent.gameObject.SetActive(false);

		f_requireEndPanel.addEntryCallback(displayEndPanel);

		f_playingMode.addExitCallback(delegate {
			MainLoop.instance.StartCoroutine(delayNoMoreAttemptDetection());
		});

		Pause = true;
	}

	// Display panel with appropriate content depending on end
	private void displayEndPanel(GameObject unused)
	{
		// display end panel
		endPanel.GetComponentInParent<Canvas>().GetComponent<CanvasGroup>().interactable = false;
		endPanel.transform.parent.gameObject.SetActive(true);
		endPanel.transform.Find("Score").gameObject.SetActive(false);
		endPanel.transform.Find("Feedback").gameObject.SetActive(false);
		// Switch to edit mode
		GameObjectManager.addComponent<EditMode>(MainLoop.instance.gameObject);
		// Get the first end that occurs
		if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.Detected)
		{
			endPanel.transform.Find("StarsCanvas").gameObject.SetActive(false);
			endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = Utility.GetLocalizedString("EndYouHaveBeenSpotted");
			Transform buttons = endPanel.transform.Find("Buttons");
			buttons.Find("ReloadLevel").gameObject.SetActive(true);
			buttons.Find("ReloadState").gameObject.SetActive(true);
			buttons.Find("MainMenu").gameObject.SetActive(true);
			buttons.Find("NextLevel").gameObject.SetActive(false);

			AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
			audio.clip = LoseSound;
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
		if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.Collision)
		{
			endPanel.transform.Find("StarsCanvas").gameObject.SetActive(false);
			endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = Utility.GetLocalizedString("EndWatchOutForCollision");
			Transform buttons = endPanel.transform.Find("Buttons");
			buttons.Find("ReloadLevel").gameObject.SetActive(true);
			buttons.Find("ReloadState").gameObject.SetActive(true);
			buttons.Find("MainMenu").gameObject.SetActive(true);
			buttons.Find("NextLevel").gameObject.SetActive(false);

			AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
			audio.clip = LoseSound;
			audio.loop = true;
			audio.Play();

			MainLoop.instance.StartCoroutine(delaySendStatement(endPanel, new
			{
				verb = "completed",
				objectType = "level",
				result = true,
				success = -1,
				resultExtensions = new Dictionary<string, string>() {
					{ "error", "Collision" }
				}
			}));
        }
        else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.WrongActionChosen)
        {
            endPanel.transform.Find("StarsCanvas").gameObject.SetActive(false);
            endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = Utility.GetLocalizedString("EndTracingError");
            Transform buttons = endPanel.transform.Find("Buttons");
            buttons.Find("ReloadLevel").gameObject.SetActive(false);
            buttons.Find("ReloadState").gameObject.SetActive(true);
            buttons.Find("MainMenu").gameObject.SetActive(false);
            buttons.Find("NextLevel").gameObject.SetActive(false);

            AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
            audio.clip = LoseSound;
            audio.loop = true;
            audio.Play();

            MainLoop.instance.StartCoroutine(delaySendStatement(endPanel, new
            {
                verb = "completed",
                objectType = "level",
                activityExtensions = new Dictionary<string, string>() {
                    { "error", "WrongActionChosen" }
                }
            }));
        }
        else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.Win)
        {
            int _score = (10000 / (gameData.totalActionBlocUsed + 1) + 5000 / (gameData.totalStep + 1) + 6000 / (gameData.totalExecute + 1) + 5000 * gameData.totalCoin);
			endPanel.transform.Find("StarsCanvas").gameObject.SetActive(true);
			Debug.Log("Score: " + _score);
			setScoreStars(_score);

			endPanel.GetComponentInParent<AudioSource>().PlayOneShot(VictorySound);
			
			Transform buttons = endPanel.transform.Find("Buttons");
			buttons.Find("ReloadLevel").gameObject.SetActive(true);
			buttons.Find("ReloadState").gameObject.SetActive(false);
			buttons.Find("MainMenu").gameObject.SetActive(true);
			buttons.Find("NextLevel").gameObject.SetActive(true);

			// Sauvegarde de l'état d'avancement des niveaux dans le scénario
			UserData ud = gameData.GetComponent<UserData>();
			if (!ud.progression.ContainsKey(gameData.selectedScenario) || ud.progression[gameData.selectedScenario] < gameData.levelToLoad + 1)
				ud.progression[gameData.selectedScenario] = gameData.levelToLoad + 1;

			//Check if next level exists in campaign
			if (gameData.levelToLoad >= gameData.scenarios[gameData.selectedScenario].levels.Count - 1)
			{
				buttons.Find("NextLevel").gameObject.SetActive(false);
				endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = Utility.GetLocalizedString("EndCongratulationsScenario");
				if (gameData.selectedScenario != UtilityLobby.testFromScenarioEditor && gameData.selectedScenario != UtilityLobby.testFromLevelEditor && gameData.selectedScenario != UtilityLobby.testFromUrl)
				{
					ud.currentScenario = "";
					ud.levelToContinue = -1;
					if (gameData.selectedScenario == "0 - Tutoriel" && !ud.unlockedAvatars.Contains(3))
						ud.newAvatarAvailable = 3;
					else if (gameData.selectedScenario == "1 - Explorateur" && !ud.unlockedAvatars.Contains(4))
						ud.newAvatarAvailable = 4;
					else if (gameData.selectedScenario == "2 - Collaborateur" && !ud.unlockedAvatars.Contains(5))
						ud.newAvatarAvailable = 5;
					else if (gameData.selectedScenario == "3 - Repetiteur" && !ud.unlockedAvatars.Contains(6))
						ud.newAvatarAvailable = 6;
					else if (gameData.selectedScenario == "4 - Selectionneur" && !ud.unlockedAvatars.Contains(7))
						ud.newAvatarAvailable = 7;
					else if (gameData.selectedScenario == "Infiltration" && !ud.unlockedAvatars.Contains(8))
						ud.newAvatarAvailable = 8;
					else if (gameData.selectedScenario == "BlocklyMaze" && !ud.unlockedAvatars.Contains(9))
						ud.newAvatarAvailable = 9;

					if (ud.newAvatarAvailable > 2)
						ud.unlockedAvatars.Add(ud.newAvatarAvailable);
				}
			}
			else
			{
				endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = Utility.GetLocalizedString("EndCongratulationMission");
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
			endPanel.transform.Find("StarsCanvas").gameObject.SetActive(false);
			endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = Utility.GetLocalizedString("EndConditionIncorrect");
			Transform buttons = endPanel.transform.Find("Buttons");
			buttons.Find("ReloadLevel").gameObject.SetActive(false);
			buttons.Find("ReloadState").gameObject.SetActive(true);
			buttons.Find("MainMenu").gameObject.SetActive(false);
			buttons.Find("NextLevel").gameObject.SetActive(false);

			AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
			audio.clip = LoseSound;
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
			endPanel.transform.Find("StarsCanvas").gameObject.SetActive(false);
			endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = Utility.GetLocalizedString("EndExecutionLimit");
			Transform buttons = endPanel.transform.Find("Buttons");
			buttons.Find("ReloadLevel").gameObject.SetActive(true);
			buttons.Find("ReloadState").gameObject.SetActive(false);
			buttons.Find("MainMenu").gameObject.SetActive(true);
			buttons.Find("NextLevel").gameObject.SetActive(false);

			AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
			audio.clip = LoseSound;
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
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.NoActionAvailableForExecution)
		{
			endPanel.transform.Find("StarsCanvas").gameObject.SetActive(false);
			endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = Utility.GetLocalizedString("EndNoActionExecuted");
			Transform buttons = endPanel.transform.Find("Buttons");
			buttons.Find("ReloadLevel").gameObject.SetActive(false);
			buttons.Find("ReloadState").gameObject.SetActive(true);
			buttons.Find("MainMenu").gameObject.SetActive(false);
			buttons.Find("NextLevel").gameObject.SetActive(false);

			AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
			audio.clip = LoseSound;
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
        else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.NoMoreActionAvailableInInventory)
        {
            endPanel.transform.Find("StarsCanvas").gameObject.SetActive(false);
            endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = Utility.GetLocalizedString("EndNoMoreActionsAvailable");
            Transform buttons = endPanel.transform.Find("Buttons");
            buttons.Find("ReloadLevel").gameObject.SetActive(true);
            buttons.Find("ReloadState").gameObject.SetActive(false);
            buttons.Find("MainMenu").gameObject.SetActive(true);
            buttons.Find("NextLevel").gameObject.SetActive(false);

            AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
            audio.clip = LoseSound;
            audio.loop = true;
            audio.Play();

            MainLoop.instance.StartCoroutine(delaySendStatement(endPanel, new
            {
                verb = "completed",
                objectType = "level",
                result = true,
                success = -1,
                resultExtensions = new Dictionary<string, string>() {
                    { "error", "NoMoreActionAvailableInInventory" }
                }
            }));
        }
        else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.NamingError)
		{
			endPanel.transform.Find("StarsCanvas").gameObject.SetActive(false);
			endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = Utility.GetLocalizedString("EndNamingError");
			Transform buttons = endPanel.transform.Find("Buttons");
			buttons.Find("ReloadLevel").gameObject.SetActive(false);
			buttons.Find("ReloadState").gameObject.SetActive(true);
			buttons.Find("MainMenu").gameObject.SetActive(false);
			buttons.Find("NextLevel").gameObject.SetActive(false);

			AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
			audio.clip = LoseSound;
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
			endPanel.transform.Find("StarsCanvas").gameObject.SetActive(false);
			endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = Utility.GetLocalizedString("EndWarningInfiniteLoop");
			Transform buttons = endPanel.transform.Find("Buttons");
			buttons.Find("ReloadLevel").gameObject.SetActive(false);
			buttons.Find("ReloadState").gameObject.SetActive(true);
			buttons.Find("MainMenu").gameObject.SetActive(false);
			buttons.Find("NextLevel").gameObject.SetActive(false);

			AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
			audio.clip = LoseSound;
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
			endPanel.transform.Find("StarsCanvas").gameObject.SetActive(false);
			endPanel.transform.Find("Content").GetComponent<TextMeshProUGUI>().text = Utility.GetLocalizedString("EndErrorLoadingMission");
			Transform buttons = endPanel.transform.Find("Buttons");
			buttons.Find("ReloadLevel").gameObject.SetActive(false);
			buttons.Find("ReloadState").gameObject.SetActive(false);
			buttons.Find("MainMenu").gameObject.SetActive(true);
			buttons.Find("NextLevel").gameObject.SetActive(false);

			AudioSource audio = endPanel.GetComponentInParent<AudioSource>(true);
			audio.clip = LoseSound;
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
		stars.GetComponent<TooltipContent>().text = Utility.GetLocalizedString("StarWon" + scoredStars);

		// Affichage du score
		GameObject score_go = endPanel.transform.Find("Score").gameObject;
		score_go.SetActive(true);
		score_go.GetComponent<TMP_Text>().text = score + " / " + gameData.levelToLoadScore[0];

		// Si moins de 3 étoiles affichage du feedback
		if (scoredStars < 3)
			endPanel.transform.Find("Feedback").gameObject.SetActive(true);

		//save score only if better score
		UserData ud = gameData.GetComponent<UserData>();
		DataLevel levelToLoad = gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad];
		string highScoreKey = Utility.extractFileName(levelToLoad.filePath);
		int savedScore = !ud.highScore.ContainsKey(highScoreKey) ? 0 : ud.highScore[highScoreKey];
		
		if (savedScore < scoredStars)
			ud.highScore[highScoreKey] = scoredStars;
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
