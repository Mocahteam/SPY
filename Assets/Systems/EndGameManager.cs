using UnityEngine;
using FYFY;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.IO;

/// <summary>
/// This system check if the end of the level is reached
/// </summary>
public class EndGameManager : FSystem {

	public static EndGameManager instance;

	private Family requireEndPanel = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family displayedEndPanel = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd), typeof(AudioSource)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));
    private Family newCurrentAction_f = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction), typeof(BasicAction)));
	private Family exitGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position), typeof(AudioSource)), new AnyOfTags("Exit"));

	private Family playingMode_f = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	
	private GameData gameData;
	private FunctionalityParam funcPram;

	public GameObject endPanel;

	public EndGameManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
		{
			gameData = go.GetComponent<GameData>();
			funcPram = go.GetComponent<FunctionalityParam>();
		}

		requireEndPanel.addEntryCallback(displayEndPanel);
		displayedEndPanel.addEntryCallback(onDisplayedEndPanel);

		// each time a current action is removed, we check if the level is over
		newCurrentAction_f.addExitCallback(delegate {
			MainLoop.instance.StartCoroutine(delayCheckEnd());
		});

		playingMode_f.addExitCallback(delegate {
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
			// parse all players
			foreach (GameObject player in playerGO)
			{
				// parse all exits
				foreach (GameObject exit in exitGO)
				{
					// check if positions are equals
					if (player.GetComponent<Position>().x == exit.GetComponent<Position>().x && player.GetComponent<Position>().z == exit.GetComponent<Position>().z)
					{
						nbEnd++;
						// if all players reached end position
						if (nbEnd >= playerGO.Count)
							// trigger end
							GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.Win });
					}
				}
			}
		}
	}

	private bool playerHasCurrentAction()
	{
		foreach (GameObject go in newCurrentAction_f)
		{
			if (go.GetComponent<CurrentAction>().agent.CompareTag("Player"))
				return true;
		}
		return false;
	}

	// Permet la gestion de l'affiche du panel de fin de niveau
	private void displayEndPanel(GameObject endPanel)
	{
		GameObjectManager.setGameObjectState(endPanel.transform.parent.gameObject, true);
	}


	// Permet de switcher entre les différents affichages de fin de niveau
	// Cas 1 : Un ennemie à repéré le robot
	// Cas 2 : Le robot est sortie du labyrinth
	// Cas 3 : Le joueur à mal remplit une condition
	private void onDisplayedEndPanel(GameObject endPanel)
	{
		// Get the first end that occurs
		if (endPanel.GetComponent<NewEnd>().endType == NewEnd.Detected)
		{
			endPanel.transform.Find("VerticalCanvas").GetComponentInChildren<TextMeshProUGUI>().text = "Vous avez été repéré !";
			GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadLevel").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadState").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("MainMenu").gameObject, true);
			endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/LoseSound") as AudioClip;
			endPanel.GetComponent<AudioSource>().loop = true;
			endPanel.GetComponent<AudioSource>().Play();
		}
		else if (endPanel.GetComponent<NewEnd>().endType == NewEnd.Win)
		{
			int score = (10000 / (gameData.totalActionBloc + 1) + 5000 / (gameData.totalStep + 1) + 6000 / (gameData.totalExecute + 1) + 5000 * gameData.totalCoin);
			Transform verticalCanvas = endPanel.transform.Find("VerticalCanvas");
			verticalCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Bravo vous avez gagné !\nScore: " + score;
			setScoreStars(score, verticalCanvas.Find("ScoreCanvas"));

			endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/VictorySound") as AudioClip;
			endPanel.GetComponent<AudioSource>().loop = false;
			endPanel.GetComponent<AudioSource>().Play();
			GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("MainMenu").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadLevel").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadState").gameObject, false);
			//Check if next level exists in campaign
			if (gameData.levelToLoad.Item2 >= gameData.levelList[gameData.levelToLoad.Item1].Count - 1)
			{
				GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
			}
		}
		else if (endPanel.GetComponent<NewEnd>().endType == NewEnd.BadCondition)
		{
			endPanel.transform.Find("VerticalCanvas").GetComponentInChildren<TextMeshProUGUI>().text = "Une condition est mal remplie !";
			GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadLevel").gameObject, false);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadState").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("MainMenu").gameObject, false);
			endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/LoseSound") as AudioClip;
			endPanel.GetComponent<AudioSource>().loop = true;
			endPanel.GetComponent<AudioSource>().Play();
		} else if (endPanel.GetComponent<NewEnd>().endType == NewEnd.NoMoreAttempt)
		{
			endPanel.transform.Find("VerticalCanvas").GetComponentInChildren<TextMeshProUGUI>().text = "Vous n'avez plus d'exécution disponible. Essayez de résoudre ce niveau en moins de coup";
			GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadLevel").gameObject, true);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadState").gameObject, false);
			GameObjectManager.setGameObjectState(endPanel.transform.Find("MainMenu").gameObject, true);
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
		int savedScore = PlayerPrefs.GetInt(gameData.levelToLoad.Item1 + Path.DirectorySeparatorChar + gameData.levelToLoad.Item2 + gameData.scoreKey, 0);
		if (savedScore < scoredStars)
		{
			PlayerPrefs.SetInt(gameData.levelToLoad.Item1 + Path.DirectorySeparatorChar + gameData.levelToLoad.Item2 + gameData.scoreKey, scoredStars);
			PlayerPrefs.Save();
		}
	}

	// Cancel End (see ReloadState button in editor)
	public void cancelEnd()
	{
		// in case of several ends pop in the same time (for instance exit reached and detected)
		foreach (NewEnd end in endPanel.GetComponents<NewEnd>())
			GameObjectManager.removeComponent(end);
	}

	private IEnumerator delayNoMoreAttemptDetection()
	{
		// wait three frames in case win will be detected (win is priority with noMoreAttempt)
		yield return null;
		yield return null;
		yield return null;
		if (requireEndPanel.Count <= 0 && displayedEndPanel.Count <= 0 && !funcPram.funcActiveInLevel.Contains("F5"))
			GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.NoMoreAttempt });
	}
}
