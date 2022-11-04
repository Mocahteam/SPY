using UnityEngine;
using FYFY;
using System.Collections;
using TMPro;
using System.IO;

/// <summary>
/// This system check if the end of the level is reached and display end panel accordingly
/// </summary>
public class EndGameManager : FSystem {

	public static EndGameManager instance;

	private Family f_requireEndPanel = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));

	private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));
    private Family f_newCurrentAction = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction), typeof(BasicAction)));
	private Family f_exit = FamilyManager.getFamily(new AllOfComponents(typeof(Position), typeof(AudioSource)), new AnyOfTags("Exit"));

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

		GameObjectManager.setGameObjectState(endPanel.transform.parent.gameObject, false);

		f_requireEndPanel.addEntryCallback(displayEndPanel);

		// each time a current action is removed, we check if the level is over
		f_newCurrentAction.addExitCallback(delegate {
			MainLoop.instance.StartCoroutine(delayCheckEnd());
		});

		f_playingMode.addExitCallback(delegate {
			MainLoop.instance.StartCoroutine(delayNoMoreAttemptDetection());
		});
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
					{
						nbEnd++;
						// if all players reached end position
						if (nbEnd >= f_exit.Count)
							// trigger end
							GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.Win });
					}
				}
			}
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
		endPanel.transform.parent.gameObject.SetActive(true);
		// Get the first end that occurs
		if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.Detected)
		{
			Transform verticalCanvas = endPanel.transform.Find("VerticalCanvas");
			GameObjectManager.setGameObjectState(verticalCanvas.Find("ScoreCanvas").gameObject, false);
			verticalCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Vous avez été repéré !";
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadLevel").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadState").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("MainMenu").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
			endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/LoseSound") as AudioClip;
			endPanel.GetComponent<AudioSource>().loop = true;
			endPanel.GetComponent<AudioSource>().Play();
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.Win)
		{
			int score = (10000 / (gameData.totalActionBlocUsed + 1) + 5000 / (gameData.totalStep + 1) + 6000 / (gameData.totalExecute + 1) + 5000 * gameData.totalCoin);
			Transform verticalCanvas = endPanel.transform.Find("VerticalCanvas");
			GameObjectManager.setGameObjectState(verticalCanvas.Find("ScoreCanvas").gameObject, true);
			verticalCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Bravo vous avez gagné !\nScore: " + score;
			setScoreStars(score, verticalCanvas.Find("ScoreCanvas"));

			endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/VictorySound") as AudioClip;
			endPanel.GetComponent<AudioSource>().loop = false;
			endPanel.GetComponent<AudioSource>().Play();
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadLevel").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadState").gameObject, false);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("MainMenu").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, true);
			//Check if next level exists in campaign
			if (gameData.scenario.FindIndex(x => x == gameData.levelToLoad) >= gameData.scenario.Count - 1)
				GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.BadCondition)
		{
			Transform verticalCanvas = endPanel.transform.Find("VerticalCanvas");
			GameObjectManager.setGameObjectState(verticalCanvas.Find("ScoreCanvas").gameObject, false);
			verticalCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Une condition est mal remplie !";
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadLevel").gameObject, false);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadState").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("MainMenu").gameObject, false);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
			endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/LoseSound") as AudioClip;
			endPanel.GetComponent<AudioSource>().loop = true;
			endPanel.GetComponent<AudioSource>().Play();
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.NoMoreAttempt)
		{
			Transform verticalCanvas = endPanel.transform.Find("VerticalCanvas");
			GameObjectManager.setGameObjectState(verticalCanvas.Find("ScoreCanvas").gameObject, false);
			verticalCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Vous n'avez pas réussi à atteindre le téléporteur et vous n'avez plus d'exécution disponible.\nEssayez de résoudre ce niveau en moins de coup !";
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadLevel").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadState").gameObject, false);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("MainMenu").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
			endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/LoseSound") as AudioClip;
			endPanel.GetComponent<AudioSource>().loop = true;
			endPanel.GetComponent<AudioSource>().Play();
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.NoAction)
		{
			Transform verticalCanvas = endPanel.transform.Find("VerticalCanvas");
			GameObjectManager.setGameObjectState(verticalCanvas.Find("ScoreCanvas").gameObject, false);
			verticalCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Aucune action ne peut être exécutée !";
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadLevel").gameObject, false);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadState").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("MainMenu").gameObject, false);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
			endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/LoseSound") as AudioClip;
			endPanel.GetComponent<AudioSource>().loop = true;
			endPanel.GetComponent<AudioSource>().Play();
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.InfiniteLoop)
		{
			Transform verticalCanvas = endPanel.transform.Find("VerticalCanvas");
			GameObjectManager.setGameObjectState(verticalCanvas.Find("ScoreCanvas").gameObject, false);
			verticalCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "ATTENTION, boucle infinie détectée...\nRisque de surchauffe du processeur du robot, interuption du programme d'urgence !";
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadLevel").gameObject, false);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadState").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("MainMenu").gameObject, false);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
			endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/LoseSound") as AudioClip;
			endPanel.GetComponent<AudioSource>().loop = true;
			endPanel.GetComponent<AudioSource>().Play();
		}
		else if (f_requireEndPanel.First().GetComponent<NewEnd>().endType == NewEnd.Error)
		{
			Transform verticalCanvas = endPanel.transform.Find("VerticalCanvas");
			GameObjectManager.setGameObjectState(verticalCanvas.Find("ScoreCanvas").gameObject, false);
			verticalCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "ERREUR de chargement du niveau, veuillez retourner au menu principal !";
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadLevel").gameObject, false);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadState").gameObject, false);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("MainMenu").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
			endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/LoseSound") as AudioClip;
			endPanel.GetComponent<AudioSource>().loop = true;
			endPanel.GetComponent<AudioSource>().Play();
		}
	}

	// Gére le nombre d'étoile à afficher selon le score obtenue
	private void setScoreStars(int score, Transform scoreCanvas)
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
		for (int nbStar = 0; nbStar < 4; nbStar++)
		{
			if (nbStar == scoredStars)
				GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, true);
			else
				GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, false);
		}

		//save score only if better score
		int savedScore = PlayerPrefs.GetInt(gameData.levelToLoad + gameData.scoreKey, 0);
		if (savedScore < scoredStars)
		{
			PlayerPrefs.SetInt(gameData.levelToLoad + gameData.scoreKey, scoredStars);
			PlayerPrefs.Save();
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
			GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.NoMoreAttempt });
	}
}
