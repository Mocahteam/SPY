using UnityEngine;
using FYFY;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using FYFY_plugins.PointerManager;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Manage InGame UI (Play/Pause/Stop, reset, go back to main menu...)
/// Switch to edition/execution view
/// Need to be binded after LevelGenerator
/// </summary>
public class UISystem : FSystem {
	private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Player"));
	private Family f_agents = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef)));
	private Family f_viewportContainer = FamilyManager.getFamily(new AllOfComponents(typeof(ViewportContainer))); // Les containers viewport
	private Family f_scriptContainer = FamilyManager.getFamily(new AllOfComponents(typeof(UIRootContainer)), new AnyOfTags("ScriptConstructor")); // Les containers de scripts editables
	private Family f_removeButton = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("RemoveButton")); // Les petites poubelles de chaque panneau d'édition
	private Family f_pointerOver = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY)); // Tous les objets pointés
	private Family f_tooltipContent = FamilyManager.getFamily(new AllOfComponents(typeof(TooltipContent))); // Tous les tooltips

	private Family f_newEnd = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
	private Family f_updateStartButton = FamilyManager.getFamily(new AllOfComponents(typeof(NeedRefreshPlayButton)));

	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

	private GameData gameData;

	private float touchUp;

	public GameObject LevelGO;
	public GameObject buttonExecute;
	public GameObject buttonPause;
	public GameObject buttonNextStep;
	public GameObject buttonContinue;
	public GameObject buttonSpeed;
	public GameObject buttonStop;
	public GameObject canvas;
	public GameObject libraryPanel;

	public static UISystem instance;

	public UISystem(){
		instance = this;
	}

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();

		f_playingMode.addEntryCallback(delegate {
			copyEditableScriptsToExecutablePanels();
			setExecutionView(true);
		});

		f_editingMode.addEntryCallback(delegate {
			setExecutionView(false);
		});

		f_newEnd.addEntryCallback(delegate { levelFinished(true); });
		f_newEnd.addExitCallback(delegate { levelFinished(false); });

		f_updateStartButton.addEntryCallback(delegate {
			MainLoop.instance.StartCoroutine(updatePlayButton());
			foreach (GameObject go in f_updateStartButton)
				foreach (NeedRefreshPlayButton need in go.GetComponents<NeedRefreshPlayButton>())
					GameObjectManager.removeComponent(need);
		});
	}

	// Lors d'une fin d'exécution de séquence, gére les différents éléments à ré-afficher ou si il faut sauvegarder la progression du joueur
	private void levelFinished(bool state)
	{
		// On réaffiche les différents panels pour la création de séquence
		setExecutionView(false);

		// Hide library panel
		GameObjectManager.setGameObjectState(libraryPanel.transform.parent.parent.parent.gameObject, !state);
		// Hide menu panel
		GameObjectManager.setGameObjectState(buttonExecute.transform.parent.gameObject, !state);
	}

	private bool isTouch()
    {
		foreach (var touch in Touchscreen.current.touches)
			if (touch.isInProgress)
				return true;
		return false;
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
		if (Touchscreen.current != null)
		{
			// With touch device when the finger is up, pointerOver is not removed because OnPointerExit is not called
			// then be sure to clear pointerOver and Tooltips
			if (isTouch())
				touchUp = 0;
			else
				touchUp += Time.deltaTime;
			if (PlayerPrefs.GetInt("interaction") == 1 // 0 means mouse/keyboard; 1 means touch-sensitive
				&& touchUp > 0.25f)
			{
				foreach (GameObject pointed in f_pointerOver)
					pointed.GetComponent<PointerSensitive>().OnPointerExit(null);
				foreach (GameObject tooltip in f_tooltipContent)
					tooltip.GetComponent<TooltipContent>().OnPointerExit(null);
				touchUp = 0;
			}
		}
	}


	// Active ou désactive le bouton play si il y a ou non des actions dans un container script
	private IEnumerator updatePlayButton()
	{
		yield return null;
		buttonExecute.GetComponent<Button>().interactable = false;
		foreach (GameObject container in f_scriptContainer)
		{
			if (container.GetComponentsInChildren<BaseElement>(true).Length > 0)
			{
				buttonExecute.GetComponent<Button>().interactable = true;
			}
		}
	}

	// On affiche ou non la partie librairie/programmation sequence en fonction de la valeur reçue
	public void setExecutionView(bool value){
		// Toggle library and editable panel
		GameObjectManager.setGameObjectState(canvas.transform.Find("LeftPanel").gameObject, !value);
		// Show sentinel panels and toggle player panels
		foreach (GameObject agent in f_agents)
			if (agent.GetComponent<DetectRange>())
				// always enable drone execution panel
				GameObjectManager.setGameObjectState(agent.GetComponent<ScriptRef>().executablePanel, true);
			else
			{
				// toggle player execution panel
				GameObjectManager.setGameObjectState(agent.GetComponent<ScriptRef>().executablePanel, value);
				if (!value)
					freePlayerExecutablePanels();
			}
		// Define Menu button states
		GameObjectManager.setGameObjectState(buttonExecute, !value);
		GameObjectManager.setGameObjectState(buttonPause, value);
		EventSystem.current.SetSelectedGameObject(value ? buttonPause : buttonExecute);
		GameObjectManager.setGameObjectState(buttonNextStep, false);
		GameObjectManager.setGameObjectState(buttonContinue, false);
		GameObjectManager.setGameObjectState(buttonSpeed, value);
		GameObjectManager.setGameObjectState(buttonStop, value);
		if (gameData.actionsHistory != null)
			foreach (GameObject trash in f_removeButton)
				trash.GetComponent<Button>().interactable = false;
	}

	// Permet de relancer le niveau au début
	public void restartScene(){
		initZeroVariableLevel();
		GameObjectManager.addComponent<AskToLoadScene>(MainLoop.instance.gameObject, new { sceneName = "MainScene" });
	}


	// See TitleScreen and MainMenu buttons in editor
	// Permet de revenir à la scéne titre
	public void returnToTitleScreen(){
		initZeroVariableLevel();
		GameObjectManager.addComponent<ActionPerformedForLRS>(LevelGO, new
		{
			verb = "exited",
			objectType = "level",
			activityExtensions = new Dictionary<string, string>() {
					{ "value", Utility.extractFileName(gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad].src) }
				}
		});
		gameData.actionsHistory = null;
		GameObjectManager.addComponent<AskToLoadScene>(MainLoop.instance.gameObject, new { sceneName = "TitleScreen" });
	}


	// Permet de réinitialiser les variables du niveau dans l'objet gameData
	public void initZeroVariableLevel()
    {
		gameData.totalActionBlocUsed = 0;
		gameData.totalStep = 0;
		gameData.totalExecute = 0;
		gameData.totalCoin = 0;
		gameData.levelToLoadScore = null;
}


	// See NextLevel button in editor
	// On charge la scéne suivante
	public void nextLevel()
	{
		GameObjectManager.addComponent<ActionPerformedForLRS>(LevelGO, new
		{
			verb = "exited",
			objectType = "level",
			activityExtensions = new Dictionary<string, string>() {
					{ "value", Utility.extractFileName(gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad].src) }
				}
		});
		// On imcrémente le numéro du niveau
		gameData.levelToLoad++;
		// On efface l'historique
		gameData.actionsHistory = null;
		// On recharge la scéne (mais avec le nouveau numéro de niveau)
		restartScene();
	}


	// See ReloadLevel and RestartLevel buttons in editor
	// Fait recommencer la scéne mais en gardant l'historique des actions
	public void retry()
	{
		GameObjectManager.addComponent<ActionPerformedForLRS>(LevelGO, new
		{
			verb = "exited",
			objectType = "level",
			activityExtensions = new Dictionary<string, string>() {
					{ "value", Utility.extractFileName(gameData.scenarios[gameData.selectedScenario].levels[gameData.levelToLoad].src) }
				}
		});
		if (gameData.actionsHistory != null)
			UnityEngine.Object.DontDestroyOnLoad(gameData.actionsHistory);
		restartScene();
	}

	private void freePlayerExecutablePanels()
	{
		foreach (GameObject robot in f_player)
		{
			GameObject executableContainer = robot.GetComponent<ScriptRef>().executableScript;
			// Clean robot container (but not the first: header)
			for (int i = executableContainer.transform.childCount - 1; i > 0; i--)
			{
				Transform child = executableContainer.transform.GetChild(i);
				GameObjectManager.unbind(child.gameObject);
				child.SetParent(null); // beacause destroying is not immediate, we remove this child from its parent, then Unity can take the time he wants to destroy GameObject
				GameObject.Destroy(child.gameObject);
			}
		}
	}

	// Copie les blocs du panneau d'édition dans le panneau d'exécution
	private void copyEditableScriptsToExecutablePanels()
	{
		// be sure executable panel is free
		freePlayerExecutablePanels();
		// copy the new sequence
		foreach (GameObject robot in f_player)
		{
			GameObject executableContainer = robot.GetComponent<ScriptRef>().executableScript;
			//copy editable script
			GameObject editableContainer = null;
			// On parcourt les scripts containers pour identifer celui associé au robot 
			foreach (GameObject container in f_viewportContainer)
				// Si le container comporte le même nom que le robot
				if (container.GetComponentInChildren<UIRootContainer>().scriptName.ToLower() == robot.GetComponent<AgentEdit>().associatedScriptName.ToLower())
					// On recupére le container qui contient le script à associer au robot
					editableContainer = container.transform.Find("ScriptContainer").gameObject;

			// Si on a bien trouvé un container associé
			if (editableContainer != null)
			{
				// we fill the executable container with actions of the editable container
				UtilityGame.fillExecutablePanel(editableContainer, executableContainer, robot.tag);
				// bind all child (except the first "header")
				for (int i = 1; i < executableContainer.transform.childCount; i++)
					GameObjectManager.bind(executableContainer.transform.GetChild(i).gameObject);
			}
		}
		
		// On notifie les systèmes comme quoi le panneau d'éxecution est rempli
		GameObjectManager.addComponent<ExecutablePanelReady>(MainLoop.instance.gameObject);

		// On harmonise l'affichage de l'UI container des agents
		foreach (GameObject go in f_agents){
			if(go.CompareTag("Player")){				
				GameObjectManager.setGameObjectState(go.GetComponent<ScriptRef>().executablePanel, true);				
			}
		}
	}
}