using UnityEngine;
using FYFY;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using System;

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
	private Family f_resetButton = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("ResetButton")); // Les petites balayettes de chaque panneau d'édition
	private Family f_removeButton = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("RemoveButton")); // Les petites poubelles de chaque panneau d'édition

	private Family f_newEnd = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
	private Family f_updateStartButton = FamilyManager.getFamily(new AllOfComponents(typeof(NeedRefreshPlayButton)));

	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

	private Family f_enabledinventoryBlocks = FamilyManager.getFamily(new AllOfComponents(typeof(ElementToDrag)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	private GameData gameData;

	public GameObject buttonExecute;
	public GameObject buttonPause;
	public GameObject buttonNextStep;
	public GameObject buttonContinue;
	public GameObject buttonSpeed;
	public GameObject buttonStop;
	public GameObject menuEchap;
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
		
		f_currentActions.addEntryCallback(keepCurrentActionViewable);

		f_playingMode.addEntryCallback(delegate {
			copyEditableScriptsToExecutablePanels();
			setExecutionView(true);
		});

		f_editingMode.addEntryCallback(delegate {
			setExecutionView(false);
		});

		f_enabledinventoryBlocks.addEntryCallback(delegate { MainLoop.instance.StartCoroutine(forceLibraryRefresh()); });
		f_enabledinventoryBlocks.addExitCallback(delegate { MainLoop.instance.StartCoroutine(forceLibraryRefresh()); });

		f_newEnd.addEntryCallback(levelFinished);

		f_updateStartButton.addEntryCallback(delegate {
			MainLoop.instance.StartCoroutine(updatePlayButton());
			foreach (GameObject go in f_updateStartButton)
				foreach (NeedRefreshPlayButton need in go.GetComponents<NeedRefreshPlayButton>())
					GameObjectManager.removeComponent(need);
		});

		MainLoop.instance.StartCoroutine(forceLibraryRefresh());
	}

	// Lors d'une fin d'exécution de séquence, gére les différents éléments à ré-afficher ou si il faut sauvegarder la progression du joueur
	private void levelFinished(GameObject go)
	{
		// On réaffiche les différents panels pour la création de séquence
		setExecutionView(false);

		// En cas de fin de niveau
		if (go.GetComponent<NewEnd>().endType == NewEnd.Win)
		{
			// Hide library panel
			GameObjectManager.setGameObjectState(libraryPanel.transform.parent.parent.gameObject, false);
			// Hide menu panel
			GameObjectManager.setGameObjectState(buttonExecute.transform.parent.gameObject, false);
			// Inactive of each editable panel
			foreach (GameObject brush in f_resetButton)
				brush.GetComponent<Button>().interactable = false;
			foreach (GameObject trash in f_removeButton)
				trash.GetComponent<Button>().interactable = false;
			// Sauvegarde de l'état d'avancement des niveaux (niveau et étoile)
			int currentLevelNum = gameData.scenario.FindIndex(x => x == gameData.levelToLoad);
			if (PlayerPrefs.GetInt(gameData.scenarioName,0) < currentLevelNum + 1)
				PlayerPrefs.SetInt(gameData.scenarioName, currentLevelNum + 1);
			PlayerPrefs.Save();
		}
		// for other end type, nothing to do more
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
        //Active/désactive le menu echap si on appuit sur echap
        if (Input.GetKeyDown(KeyCode.Escape))
        {
			setActiveEscapeMenu();
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
	private void keepCurrentActionViewable(GameObject go){
		if (go.activeInHierarchy)
		{
			Vector3 v = GetGUIElementOffset(go.GetComponent<RectTransform>());
			if (v != Vector3.zero)
			{ // if not visible in UI
				ScrollRect containerScrollRect = go.GetComponentInParent<ScrollRect>();
				containerScrollRect.content.localPosition += GetSnapToPositionToBringChildIntoView(containerScrollRect, go.GetComponent<RectTransform>());
			}
		}
	}

	public Vector3 GetSnapToPositionToBringChildIntoView(ScrollRect scrollRect, RectTransform child){
		Canvas.ForceUpdateCanvases();
		Vector3 viewportLocalPosition = scrollRect.viewport.localPosition;
		Vector3 childLocalPosition   = child.localPosition;
		Vector3 result = new Vector3(
			0,
			0 - (viewportLocalPosition.y + childLocalPosition.y),
			0
		);
		return result;
	}

	public Vector3 GetGUIElementOffset(RectTransform rect){
        Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height);
        Vector3[] objectCorners = new Vector3[4];
        rect.GetWorldCorners(objectCorners);


		var xnew = 0f;
        var ynew = 0f;
        var znew = 0f;
 
        for (int i = 0; i < objectCorners.Length; i++){
			if (objectCorners[i].x < screenBounds.xMin)
                xnew = screenBounds.xMin - objectCorners[i].x;

            if (objectCorners[i].x > screenBounds.xMax)
                xnew = screenBounds.xMax - objectCorners[i].x;

            if (objectCorners[i].y < screenBounds.yMin)
                ynew = screenBounds.yMin - objectCorners[i].y;

            if (objectCorners[i].y > screenBounds.yMax)
                ynew = screenBounds.yMax - objectCorners[i].y;
				
        }
 
        return new Vector3(xnew, ynew, znew);
 
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
		gameData.dialogMessage = new List<(string, string, float, int, int)>();
}


	// See NextLevel button in editor
	// On charge la scéne suivante
	public void nextLevel(){
		// On imcrémente le numéro du niveau
		gameData.levelToLoad = gameData.scenario[gameData.scenario.FindIndex(x => x == gameData.levelToLoad)+1];
		// On efface l'historique
		gameData.actionsHistory = null;
		// On recharge la scéne (mais avec le nouveau numéro de niveau)
		restartScene();
	}


	// See ReloadLevel and RestartLevel buttons in editor
	// Fait recommencer la scéne mais en gardant l'historique des actions
	public void retry(){
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
				if (container.GetComponentInChildren<UIRootContainer>().scriptName == robot.GetComponent<AgentEdit>().associatedScriptName)
					// On recupére le container qui contient le script à associer au robot
					editableContainer = container.transform.Find("ScriptContainer").gameObject;

			// Si on a bien trouvé un container associé
			if (editableContainer != null)
			{
				// we fill the executable container with actions of the editable container
				EditingUtility.fillExecutablePanel(editableContainer, executableContainer, robot.tag);
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
		}// Et inversement
        else
        {
			menuEchap.SetActive(true);
		}
    }

	// see inputFiels in ForBloc prefab in inspector
	public void onlyPositiveInteger(GameObject input, string newValue)
	{
		int res;
		bool success = Int32.TryParse(newValue, out res);
		if (!success || (success && Int32.Parse(newValue) < 0))
		{
			input.GetComponent<TMP_InputField>().text = "0";
			res = 0;
		}
		input.GetComponentInParent<ForControl>(true).nbFor = res;
	}
}