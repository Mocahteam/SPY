using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

/// <summary>
/// Manage dialogs at the begining of the level
/// Manage InGame UI (Play/Pause/Stop, reset, go back to main menu...)
/// Manage history
/// Manage end panel (compute Score and stars)
/// Need to be binded after LevelGenerator
/// </summary>
public class UISystem : FSystem {
	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Player"));
	private Family actions = FamilyManager.getFamily(new AllOfComponents(typeof(PointerSensitive), typeof(LibraryItemRef)));
	private Family currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(BasicAction), typeof(LibraryItemRef), typeof(CurrentAction)));
	private Family resetBlocLimit_f = FamilyManager.getFamily(new AllOfComponents(typeof(ResetBlocLimit)));
	private Family agents = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef)));
	private Family viewportContainer_f = FamilyManager.getFamily(new AllOfComponents(typeof(ViewportContainer))); // Les containers viewport
	private Family scriptContainer_f = FamilyManager.getFamily(new AllOfComponents(typeof(UIRootContainer)), new AnyOfTags("ScriptConstructor")); // Les containers de scripts
	private Family agent_f = FamilyManager.getFamily(new AllOfComponents(typeof(AgentEdit), typeof(ScriptRef))); // On récupére les agents pouvant être édité
	private Family resetButton_f = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("ResetButton")); // Les petites balayettes de chaque panneau d'édition
	private Family removeButton_f = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AnyOfTags("RemoveButton")); // Les petites poubelles de chaque panneau d'édition

	private Family newEnd_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
	private Family updateStartButton_f = FamilyManager.getFamily(new AllOfComponents(typeof(NeedRefreshPlayButton)));

	private Family playingMode_f = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family editingMode_f = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

	private Family enabledinventoryBlocks = FamilyManager.getFamily(new AllOfComponents(typeof(ElementToDrag)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family inventoryBlocks = FamilyManager.getFamily(new AllOfComponents(typeof(ElementToDrag)));

	private Family askToSaveHistory_f = FamilyManager.getFamily(new AllOfComponents(typeof(AskToSaveHistory)));

	private GameData gameData;
	private int nDialog = 0;

	public GameObject buttonExecute;
	public GameObject buttonPause;
	public GameObject buttonNextStep;
	public GameObject buttonContinue;
	public GameObject buttonSpeed;
	public GameObject buttonStop;
	public GameObject menuEchap;
	public GameObject buttonAddEditableContainer;
	public GameObject endPanel;
	public GameObject dialogPanel;
	public GameObject canvas;
	public GameObject libraryPanel;
	public GameObject EditableCanvas;
	public GameObject libraryFor;
	public GameObject libraryWait;

	public static UISystem instance;

	public UISystem(){
		instance = this;
	}

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();

		GameObjectManager.setGameObjectState(endPanel.transform.parent.gameObject, false);
		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, false);

		actions.addEntryCallback(linkTo);
		resetBlocLimit_f.addEntryCallback(delegate (GameObject go) {
			destroyScript(go, true);
			GameObjectManager.unbind(go);
			UnityEngine.Object.Destroy(go);
		});
		
		currentActions.addEntryCallback(onNewCurrentAction);

		playingMode_f.addEntryCallback(delegate {
			copyEditableScriptsToExecutablePanels();
			setExecutionView(true);
		});

		editingMode_f.addEntryCallback(delegate {
			setExecutionView(false);
		});

		enabledinventoryBlocks.addEntryCallback(delegate { MainLoop.instance.StartCoroutine(forceLibraryRefresh()); });
		enabledinventoryBlocks.addExitCallback(delegate { MainLoop.instance.StartCoroutine(forceLibraryRefresh()); });

		newEnd_f.addEntryCallback(levelFinished);

		updateStartButton_f.addEntryCallback(delegate {
			MainLoop.instance.StartCoroutine(updatePlayButton());
			foreach (GameObject go in updateStartButton_f)
				foreach (NeedRefreshPlayButton need in go.GetComponents<NeedRefreshPlayButton>())
					GameObjectManager.removeComponent(need);
		});

		askToSaveHistory_f.addEntryCallback(delegate (GameObject go)
		{
			saveHistory();
			GameObjectManager.removeComponent<AskToSaveHistory>(go);
		});
		MainLoop.instance.StartCoroutine(delayLoadHistory());

		MainLoop.instance.StartCoroutine(forceLibraryRefresh());
	}


	private IEnumerator delayLoadHistory()
    {
		yield return null;// delay one frame time the editable container will be created
		loadHistory();
    }

	// Lors d'une fin d'exécution de séquence, gére les différents éléments à ré-afficher ou si il faut sauvegarder la progression du joueur
	private void levelFinished(GameObject go)
	{
		// On réaffiche les différent panel pour la création de séquence
		setExecutionView(false);

		// En cas de fin de niveau
		if (go.GetComponent<NewEnd>().endType == NewEnd.Win)
		{
			// Affichage de l'historique de l'ensemble des actions exécutés
			saveHistory();
			loadHistory();
			// Hide library panel
			GameObjectManager.setGameObjectState(libraryPanel.transform.parent.parent.gameObject, false);
			// Hide menu panel
			GameObjectManager.setGameObjectState(buttonExecute.transform.parent.gameObject, false);
			// Inactive of each editable panel
			foreach (GameObject brush in resetButton_f)
				brush.GetComponent<Button>().interactable = false;
			foreach (GameObject trash in removeButton_f)
				trash.GetComponent<Button>().interactable = false;
			// Sauvegarde de l'état d'avancement des niveaux (niveau et étoile)
			PlayerPrefs.SetInt(gameData.levelToLoad.Item1, gameData.levelToLoad.Item2 + 1);
			PlayerPrefs.Save();
		}
		// for other end type, nothing to do more
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
		//Activate DialogPanel if there is a message
		if (gameData != null && gameData.dialogMessage.Count > 0 && !dialogPanel.transform.parent.gameObject.activeSelf)
		{
			showDialogPanel();
		}

        //Active/désactive le menu echap si on appuie su echap
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
		foreach (GameObject container in scriptContainer_f)
		{
			if (container.GetComponentsInChildren<BaseElement>().Length > 0)
			{
				buttonExecute.GetComponent<Button>().interactable = true;
			}
		}
	}

	///  ????
	IEnumerator GetTextureWebRequest(Image img, string path)
	{
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(path);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.error);
		}
		else
		{
			Texture2D tex2D = ((DownloadHandlerTexture)www.downloadHandler).texture;
			img.sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0, 0), 100.0f);
		}
	}

	private IEnumerator forceLibraryRefresh()
    {
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(libraryPanel.GetComponent<RectTransform>());
	}

	// ?????
	private void onNewCurrentAction(GameObject go){
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


	// ?????
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

	
	// ?????
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
		// Toggle all execution panels
		foreach (Transform executablePanel in canvas.transform.Find("ExecutableCanvas"))
			GameObjectManager.setGameObjectState(executablePanel.gameObject, value);
		// Define Menu button states
		GameObjectManager.setGameObjectState(buttonExecute, !value);
		GameObjectManager.setGameObjectState(buttonPause, value);
		GameObjectManager.setGameObjectState(buttonNextStep, false);
		GameObjectManager.setGameObjectState(buttonContinue, false);
		GameObjectManager.setGameObjectState(buttonSpeed, value);
		GameObjectManager.setGameObjectState(buttonStop, value);
		if (gameData.actionsHistory != null)
			foreach (GameObject trash in removeButton_f)
				trash.GetComponent<Button>().interactable = false;
	}


	// Add the executed scripts to the containers history
	public void saveHistory() {
		if (gameData.actionsHistory == null) {
			// set history as a copy of editable canvas
			gameData.actionsHistory = GameObject.Instantiate(EditableCanvas.transform.GetChild(0).transform).gameObject;
			gameData.actionsHistory.SetActive(false); // keep this gameObject as a ghost
			// We don't bind the history to FYFY
		}
		else {
			// parse all containers inside editable canvas
			for (int containerCpt = 0; containerCpt < EditableCanvas.transform.GetChild(0).childCount; containerCpt++) {
				Transform viewportForEditableContainer = EditableCanvas.transform.GetChild(0).GetChild(containerCpt);
				// the first child is the script container that contains script elements
				foreach (Transform child in viewportForEditableContainer.GetChild(0))
				{
					if (child.GetComponent<BaseElement>())
					{
						// copy this child inside the appropriate history
						GameObject.Instantiate(child, gameData.actionsHistory.transform.GetChild(containerCpt).GetChild(0));
						// We don't bind the history to FYFY
					}
				}
			}
		}
		// Erase all editable containers
		foreach (Transform viewportForEditableContainer in EditableCanvas.transform.GetChild(0))
			for (int i = viewportForEditableContainer.GetChild(0).childCount - 1; i >= 0; i--) {
				Transform child = viewportForEditableContainer.GetChild(0).GetChild(i);
				if (child.GetComponent<BaseElement>())
				{
					EditingUtility.manageEmptyZone(child.gameObject);
					GameObjectManager.unbind(child.gameObject);
					child.SetParent(null); // because destroying is not immediate
					GameObject.Destroy(child.gameObject);
				}
			}
		// Add Wait action for each inaction
		for (int containerCpt = 0; containerCpt < EditableCanvas.transform.GetChild(0).childCount; containerCpt++)
		{
			// look for associated agent
			string associatedAgent = EditableCanvas.transform.GetChild(0).GetChild(containerCpt).GetComponentInChildren<UIRootContainer>().associedAgentName;
			foreach (GameObject agent in agent_f)
				if (associatedAgent == agent.GetComponent<AgentEdit>().agentName)
				{
					ScriptRef sr = agent.GetComponent<ScriptRef>();
					if (sr.nbOfInactions == 1)
					{
						GameObject newWait = EditingUtility.createEditableBlockFromLibrary(libraryWait, canvas);
						newWait.transform.SetParent(gameData.actionsHistory.transform.GetChild(containerCpt).GetChild(0), false);
						newWait.transform.SetAsLastSibling();
					}
					else if (sr.nbOfInactions > 1)
					{
						// Create for control
						ForControl forCont = EditingUtility.createEditableBlockFromLibrary(libraryFor, canvas).GetComponent<ForControl>();
						forCont.currentFor = 0;
						forCont.nbFor = sr.nbOfInactions;
						forCont.transform.GetComponentInChildren<TMP_InputField>().text = forCont.nbFor.ToString();
						forCont.transform.SetParent(gameData.actionsHistory.transform.GetChild(containerCpt).GetChild(0), false);
						// Create Wait action
						Transform forContainer = forCont.transform.Find("Container");
						GameObject newWait = EditingUtility.createEditableBlockFromLibrary(libraryWait, canvas);
						newWait.transform.SetParent(forContainer, false);
						newWait.transform.SetAsFirstSibling();
						// Set drop/empty zone
						forContainer.GetChild(forContainer.childCount - 2).gameObject.SetActive(true); // enable drop zone
						forContainer.GetChild(forContainer.childCount - 1).gameObject.SetActive(false); // disable empty zone
					}
					sr.nbOfInactions = 0;
				}
		}

		// Disable add container button
		buttonAddEditableContainer.GetComponent<Button>().interactable = false;
		buttonAddEditableContainer.GetComponent<TooltipContent>().text = "Ajout impossible après avoir\ncommencé à résoudre le niveau";
	}


	// Restore saved scripts in history inside editable script containers
	private void loadHistory(){
		if(gameData != null && gameData.actionsHistory != null)
		{
			// For security, erase all editable containers
			foreach (Transform viewportForEditableContainer in EditableCanvas.transform.GetChild(0))
				for (int i = viewportForEditableContainer.GetChild(0).childCount -1; i >= 0; i--) {
					Transform child = viewportForEditableContainer.GetChild(0).GetChild(i);
					if (child.GetComponent<BaseElement>())
					{
						GameObjectManager.unbind(child.gameObject);
						child.transform.SetParent(null); // beacause destroying is not immediate, we remove this child from its parent, then Unity can take the time he wants to destroy GameObject
						GameObject.Destroy(child.gameObject);
					}
				}
			// Restore history
			for (int i = 0 ; i < gameData.actionsHistory.transform.childCount ; i++){
				Transform history_viewportForEditableContainer = gameData.actionsHistory.transform.GetChild(i);
				// the first child is the script container that contains script elements
				foreach (Transform history_child in history_viewportForEditableContainer.GetChild(0))
				{
					if (history_child.GetComponent<BaseElement>())
					{
						// copy this child inside the appropriate editable container
						Transform history_childCopy = GameObject.Instantiate(history_child, EditableCanvas.transform.GetChild(0).GetChild(i).GetChild(0));
						// Place this child copy at the end of the container
						history_childCopy.SetAsFirstSibling();
						history_childCopy.SetSiblingIndex(history_childCopy.parent.childCount - 3);
						GameObjectManager.bind(history_childCopy.gameObject);
						// Disable emptyzone
						GameObjectManager.setGameObjectState(history_childCopy.parent.GetChild(history_childCopy.parent.childCount - 1).gameObject, false);
						// Enable dropzone
						GameObjectManager.setGameObjectState(history_childCopy.parent.GetChild(history_childCopy.parent.childCount - 2).gameObject, true);
					}
				}
			}
			// Count used elements
			foreach (Transform viewportForEditableContainer in EditableCanvas.transform.GetChild(0))
				foreach (BaseElement act in viewportForEditableContainer.GetComponentsInChildren<BaseElement>())
					GameObjectManager.addComponent<Dropped>(act.gameObject);
			//destroy history
			GameObject.Destroy(gameData.actionsHistory);
			LayoutRebuilder.ForceRebuildLayoutImmediate(EditableCanvas.GetComponent<RectTransform>());
			//enable Play button
			buttonExecute.GetComponent<Button>().interactable = true;
		}
	}

	private GameObject getLibraryItemByName(string name)
    {
		foreach(GameObject item in inventoryBlocks)
			if (item.name == name)
				return item;
		return null;
    }

	// Find item in library to hook to this GameObject
	private void linkTo(GameObject go){
		if(go.GetComponent<LibraryItemRef>().linkedTo == null){
			if (go.GetComponent<BasicAction>())
				go.GetComponent<LibraryItemRef>().linkedTo = getLibraryItemByName(go.GetComponent<BasicAction>().actionType.ToString());
			else if (go.GetComponent<BaseCaptor>())
				go.GetComponent<LibraryItemRef>().linkedTo = getLibraryItemByName(go.GetComponent<BaseCaptor>().captorType.ToString());
			else if (go.GetComponent<BaseOperator>())
				go.GetComponent<LibraryItemRef>().linkedTo = getLibraryItemByName(go.GetComponent<BaseOperator>().operatorType.ToString());
			else if (go.GetComponent<WhileControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = getLibraryItemByName("While");
			else if (go.GetComponent<ForeverControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = getLibraryItemByName("Forever");
			else if (go.GetComponent<ForControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = getLibraryItemByName("For");
			else if (go.GetComponent<IfElseControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = getLibraryItemByName("IfElse");
			else if(go.GetComponent<IfControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = getLibraryItemByName("If");
		}
	}

	//Recursive script destroyer
	private void destroyScript(GameObject go,  bool refund = false){
		if (go.GetComponent<LibraryItemRef>())
		{
			if (!refund)
				gameData.totalActionBloc++;
			else
				GameObjectManager.addComponent<AddOne>(go.GetComponent<LibraryItemRef>().linkedTo);
		}

		foreach (Transform child in go.transform)
			destroyScript(child.gameObject, refund);
	}

	// Affiche l'image associée au dialogue
	public void setImageSprite(Image img, string path){
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			MainLoop.instance.StartCoroutine(GetTextureWebRequest(img, path));
		}
		else
		{
			Texture2D tex2D = new Texture2D(2, 2); //create new "empty" texture
			byte[] fileData = File.ReadAllBytes(path); //load image from SPY/path
			if (tex2D.LoadImage(fileData))
				img.sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0, 0), 100.0f);
		}
	}


	// Affiche le panneau de dialoge au début de niveau (si besoin)
	public void showDialogPanel(){
		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, true);
		nDialog = 0;
		dialogPanel.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = gameData.dialogMessage[0].Item1;
		GameObject imageGO = dialogPanel.transform.Find("Image").gameObject;
		if(gameData.dialogMessage[0].Item2 != null){
			GameObjectManager.setGameObjectState(imageGO,true);
			setImageSprite(imageGO.GetComponent<Image>(), Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels" +
			Path.DirectorySeparatorChar + gameData.levelToLoad.Item1 + Path.DirectorySeparatorChar + "Images" + Path.DirectorySeparatorChar + gameData.dialogMessage[0].Item2);
		}
		else
			GameObjectManager.setGameObjectState(imageGO,false);

		if(gameData.dialogMessage.Count > 1){
			setActiveOKButton(false);
			setActiveNextButton(true);
		}
		else{
			setActiveOKButton(true);
			setActiveNextButton(false);
		}
	}


	// See NextButton in editor
	// Permet d'afficher la suite du dialogue
	public void nextDialog(){
		nDialog++; // On incrémente le nombre de dialogue
		dialogPanel.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = gameData.dialogMessage[nDialog].Item1;
		GameObject imageGO = dialogPanel.transform.Find("Image").gameObject;
		if(gameData.dialogMessage[nDialog].Item2 != null){
			GameObjectManager.setGameObjectState(imageGO,true);
			setImageSprite(imageGO.GetComponent<Image>(), Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels" +
			Path.DirectorySeparatorChar + gameData.levelToLoad.Item1 + Path.DirectorySeparatorChar + "Images" + Path.DirectorySeparatorChar + gameData.dialogMessage[nDialog].Item2);
		}
		else
			GameObjectManager.setGameObjectState(imageGO,false);

		// Si il reste des dialogue à afficher ensuite
		if(nDialog + 1 < gameData.dialogMessage.Count){
			setActiveOKButton(false);
			setActiveNextButton(true);
		}
		else{
			setActiveOKButton(true);
			setActiveNextButton(false);
		}
	}


	// Active ou non le bouton Ok du panel dialogue
	public void setActiveOKButton(bool active){
		GameObjectManager.setGameObjectState(dialogPanel.transform.Find("Buttons").Find("OKButton").gameObject, active);
	}


	// Active ou non le bouton next du panle dialogue
	public void setActiveNextButton(bool active){
		GameObjectManager.setGameObjectState(dialogPanel.transform.Find("Buttons").Find("NextButton").gameObject, active);
	}


	// See OKButton in editor
	// Désactive le panel de dialogue et réinitialise le nombre de dialogue à 0
	public void closeDialogPanel(){
		nDialog = 0;
		gameData.dialogMessage = new List<(string,string)>();
		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, false);
	}


	// Permet de relaner le niveau au début
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
		gameData.totalActionBloc = 0;
		gameData.totalStep = 0;
		gameData.totalExecute = 0;
		gameData.totalCoin = 0;
		gameData.levelToLoadScore = null;
		gameData.dialogMessage = new List<(string, string)>();
		resetGameData();
}


	// See NextLevel button in editor
	// On charge la scéne suivante
	public void nextLevel(){
		// On imcrémente le numéro du niveau
		gameData.levelToLoad.Item2++;
		// On efface l'historique
		gameData.actionsHistory = null;
		// On recharge la scéne (mais avec le nouveau numéro de niveau)
		restartScene();
	}

	// Reset les données du gameData pour la gestion des fonctionalités dans les niveaux
	private void resetGameData()
	{
		gameData.GetComponent<FunctionalityParam>().funcActiveInLevel = new List<string>();
		gameData.GetComponent<GameData>().executeLvlByComp = false;
	}


	// See ReloadLevel and RestartLevel buttons in editor
	// Fait recommencé la scéne mais en gardant l'historique des actions
	public void retry(){
		if (gameData.actionsHistory != null)
			UnityEngine.Object.DontDestroyOnLoad(gameData.actionsHistory);
		restartScene();
	}

	// See ExecuteButton in editor
	// Copie les blocks du panneau d'édition dans le panneau d'exécution
	public void copyEditableScriptsToExecutablePanels(){
		//clean container for each robot and copy the new sequence
		foreach (GameObject robot in playerGO) {
			GameObject executableContainer = robot.GetComponent<ScriptRef>().executableScript;
			// Clean robot container
			for(int i = executableContainer.transform.childCount - 1; i >= 0; i--) {
				Transform child = executableContainer.transform.GetChild(i);
				GameObjectManager.unbind(child.gameObject);
				child.SetParent(null); // beacause destroying is not immediate, we remove this child from its parent, then Unity can take the time he wants to destroy GameObject
				GameObject.Destroy(child.gameObject);
			}

			//copy editable script
			GameObject editableContainer = null;
			// On parcourt les script container pour identifer celui associé au robot 
			foreach (GameObject container in viewportContainer_f)
				// Si le container comporte le même nom que le robot
				if (container.GetComponentInChildren<UIRootContainer>().associedAgentName == robot.GetComponent<AgentEdit>().agentName)
					// On recupére le container qui contient le script à associer au robot
					editableContainer = container.transform.Find("ScriptContainer").gameObject;

			// Si on a bien trouvé un container associé
			if (editableContainer != null)
			{
				// we fill the executable container with actions of the editable container
				EditingUtility.fillExecutablePanel(editableContainer, executableContainer, robot.tag);
				// bind all child
				foreach (Transform child in executableContainer.transform)
				{
					GameObjectManager.bind(child.gameObject);
				}
				// On développe le panneau au cas où il aurait été réduit
				robot.GetComponent<ScriptRef>().executablePanel.transform.Find("Header").Find("Toggle").GetComponent<Toggle>().isOn = true;
			}
		}
		
		// On notifie les systèmes comme quoi le panneau d'execution est rempli
		GameObjectManager.addComponent<ExecutablePanelReady>(MainLoop.instance.gameObject);

		// On harmonise l'affichage de l'UI container des agents
		foreach (GameObject go in agents){
			LayoutRebuilder.ForceRebuildLayoutImmediate(go.GetComponent<ScriptRef>().executablePanel.GetComponent<RectTransform>());
			if(go.CompareTag("Player")){				
				GameObjectManager.setGameObjectState(go.GetComponent<ScriptRef>().executablePanel, true);				
			}
		}
	}

	// Permet d'activé ou désactivé le menu echap
	public void setActiveEscapeMenu()
    {
		// Si le menu est active, le désactive
        if (menuEchap.activeInHierarchy)
        {
			menuEchap.SetActive(false);
		}// Et inversement
        else
        {
			menuEchap.SetActive(true);
		}
    }


}