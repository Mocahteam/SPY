using UnityEngine;
using FYFY;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using FYFY_plugins.PointerManager;
using System;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Manage InGame UI (Play/Pause/Stop, reset, go back to main menu...)
/// Switch to edition/execution view
/// Need to be binded after LevelGenerator
/// </summary>
public class UISystem : FSystem {
	private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Player"));
	private Family f_currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(BasicAction), typeof(LibraryItemRef), typeof(CurrentAction)));
	private Family f_agents = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef)));
	private Family f_viewportContainer = FamilyManager.getFamily(new AllOfComponents(typeof(ViewportContainer))); // Les containers viewport
	private Family f_scriptContainer = FamilyManager.getFamily(new AllOfComponents(typeof(UIRootContainer)), new AnyOfTags("ScriptConstructor")); // Les containers de scripts
	private Family f_removeButton = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("RemoveButton")); // Les petites poubelles de chaque panneau d'édition
	private Family f_pointerOver = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY)); // Tous les objets pointés
	private Family f_tooltipContent = FamilyManager.getFamily(new AllOfComponents(typeof(TooltipContent))); // Tous les tooltips

	private Family f_newEnd = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
	private Family f_updateStartButton = FamilyManager.getFamily(new AllOfComponents(typeof(NeedRefreshPlayButton)));

	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

	private Family f_enabledinventoryBlocks = FamilyManager.getFamily(new AllOfComponents(typeof(ElementToDrag)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	private Family f_dragging = FamilyManager.getFamily(new AllOfComponents(typeof(Dragging)));

	private Family f_buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	private GameData gameData;

	private float touchUp;
	private Coroutine viewCurrentAction = null;

	public GameObject LevelGO;
	public GameObject buttonMenu;
	public GameObject buttonExecute;
	public GameObject buttonPause;
	public GameObject buttonNextStep;
	public GameObject buttonContinue;
	public GameObject buttonSpeed;
	public GameObject buttonStop;
	public GameObject menuEchap;
	public GameObject canvas;
	public GameObject libraryPanel;
	public GameObject virtualKeyboard;

	public static UISystem instance;

	public UISystem(){
		instance = this;
	}

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();
		
		f_currentActions.addEntryCallback(delegate (GameObject go)
		{
			if (viewCurrentAction != null)
				MainLoop.instance.StopCoroutine(viewCurrentAction);
			viewCurrentAction = MainLoop.instance.StartCoroutine(keepCurrentActionViewable(go));
		});

		f_playingMode.addEntryCallback(delegate {
			copyEditableScriptsToExecutablePanels();
			setExecutionView(true);
		});

		f_editingMode.addEntryCallback(delegate {
			setExecutionView(false);
		});

		f_enabledinventoryBlocks.addEntryCallback(delegate { MainLoop.instance.StartCoroutine(forceLibraryRefresh()); });
		f_enabledinventoryBlocks.addExitCallback(delegate { MainLoop.instance.StartCoroutine(forceLibraryRefresh()); });

		f_newEnd.addEntryCallback(delegate { levelFinished(true); });
		f_newEnd.addExitCallback(delegate { levelFinished(false); });

		f_updateStartButton.addEntryCallback(delegate {
			MainLoop.instance.StartCoroutine(updatePlayButton());
			foreach (GameObject go in f_updateStartButton)
				foreach (NeedRefreshPlayButton need in go.GetComponents<NeedRefreshPlayButton>())
					GameObjectManager.removeComponent(need);
		});

		MainLoop.instance.StartCoroutine(forceLibraryRefresh());
	}

	// Lors d'une fin d'exécution de séquence, gére les différents éléments à ré-afficher ou si il faut sauvegarder la progression du joueur
	private void levelFinished(bool state)
	{
		// On réaffiche les différents panels pour la création de séquence
		setExecutionView(false);

		// Hide library panel
		GameObjectManager.setGameObjectState(libraryPanel.transform.parent.parent.gameObject, !state);
		// Hide menu panel
		GameObjectManager.setGameObjectState(buttonExecute.transform.parent.gameObject, !state);
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
        //Active/désactive le menu echap si on appuit sur echap et que le focus n'est pas sur un input field et qu'on n'est pas en train de drag un element et que le clavier virtuel n'est pas ouvert
        if (Input.GetKeyDown(KeyCode.Escape) && (EventSystem.current.currentSelectedGameObject == null || (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() == null)) && f_dragging.Count == 0 && !virtualKeyboard.activeInHierarchy)
			setActiveEscapeMenu();

		if (Input.GetKeyDown(KeyCode.Tab))
		{
			if (virtualKeyboard.activeInHierarchy)
				EventSystem.current.SetSelectedGameObject(virtualKeyboard.transform.Find("Panel").Find("Close").gameObject);
			else if (buttonMenu.activeInHierarchy)
				EventSystem.current.SetSelectedGameObject(buttonMenu);
			else
				EventSystem.current.SetSelectedGameObject(f_buttons.getAt(f_buttons.Count - 1));
		}

		// With touch device when the finger is up, pointerOver is not removed because OnPointerExit is not called
		// then be sure to clear pointerOver and Tooltips
		if (Input.touchCount > 0)
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

	private IEnumerator forceLibraryRefresh()
    {
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(libraryPanel.GetComponent<RectTransform>());
	}

	// keep current executed action viewable in the executable panel
	private IEnumerator keepCurrentActionViewable(GameObject go){
		if (go.activeInHierarchy)
		{
			RectTransform goRect = go.transform as RectTransform;

			// We look for the script container
			RectTransform scriptContainer = goRect.parent as RectTransform;
			while (scriptContainer.tag != "ScriptConstructor")
				scriptContainer = scriptContainer.parent as RectTransform;
			RectTransform viewport = scriptContainer.parent as RectTransform;

			float goRectY = Mathf.Abs(scriptContainer.InverseTransformPoint(goRect.transform.position).y);

			Vector2 targetAnchoredPosition = new Vector2(scriptContainer.anchoredPosition.x, scriptContainer.anchoredPosition.y);
			// we auto focus on current action if it is not visible
			// check if current action is too high
			if ((goRectY-goRect.rect.height) - scriptContainer.anchoredPosition.y < 0)
			{
				targetAnchoredPosition = new Vector2(
					targetAnchoredPosition.x,
					goRectY - (goRect.rect.height * 2f) // move view a little bit higher than last current action position
				);
			}
			// check if current action is too low
			else if ((goRectY + goRect.rect.height) - scriptContainer.anchoredPosition.y > viewport.rect.height)
			{
				targetAnchoredPosition = new Vector2(
					targetAnchoredPosition.x,
					-viewport.rect.height + goRectY + goRect.rect.height * 2
				);
			}

			float distance = Vector2.Distance(scriptContainer.anchoredPosition, targetAnchoredPosition);
			while (Vector2.Distance(scriptContainer.anchoredPosition, targetAnchoredPosition) > 0.1f)
			{
				scriptContainer.anchoredPosition = Vector2.MoveTowards(scriptContainer.anchoredPosition, targetAnchoredPosition, distance / 10);
				yield return null;
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
		GameObjectManager.loadScene("MainScene");
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
		GameObjectManager.loadScene("TitleScreen");
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
			// Clean robot container
			for (int i = executableContainer.transform.childCount - 1; i >= 0; i--)
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
				Utility.fillExecutablePanel(editableContainer, executableContainer, robot.tag);
				// bind all child
				foreach (Transform child in executableContainer.transform)
					GameObjectManager.bind(child.gameObject);
				// On développe le panneau au cas où il aurait été réduit
				robot.GetComponent<ScriptRef>().executablePanel.transform.Find("Header").Find("Toggle").GetComponent<Toggle>().isOn = true;
			}
		}
		
		// On notifie les systèmes comme quoi le panneau d'éxecution est rempli
		GameObjectManager.addComponent<ExecutablePanelReady>(MainLoop.instance.gameObject);

		// On harmonise l'affichage de l'UI container des agents
		foreach (GameObject go in f_agents){
			LayoutRebuilder.ForceRebuildLayoutImmediate(go.GetComponent<ScriptRef>().executablePanel.GetComponent<RectTransform>());
			if(go.CompareTag("Player")){				
				GameObjectManager.setGameObjectState(go.GetComponent<ScriptRef>().executablePanel, true);				
			}
		}
	}

	// Permet d'activer ou désactiver le menu echap
	public void setActiveEscapeMenu()
    {
		// Si le menu est activé, le désactiver
        if (menuEchap.activeInHierarchy)
        {
			menuEchap.SetActive(false);
			EventSystem.current.SetSelectedGameObject(buttonMenu);
		}// Et inversement
        else
        {
			menuEchap.SetActive(true);
			EventSystem.current.SetSelectedGameObject(menuEchap.GetComponentInChildren<Button>().gameObject);
		}
    }
}