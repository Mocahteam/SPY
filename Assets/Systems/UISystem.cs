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
/// </summary>
public class UISystem : FSystem {
	private Family requireEndPanel = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family displayedEndPanel = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd), typeof(AudioSource)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Player"));
	private Family actions = FamilyManager.getFamily(new AllOfComponents(typeof(PointerSensitive), typeof(UIActionType)));
	private Family currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(BasicAction), typeof(UIActionType), typeof(CurrentAction)));
	private Family newEnd_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
	private Family resetBlocLimit_f = FamilyManager.getFamily(new AllOfComponents(typeof(ResetBlocLimit)));
	private Family scriptIsRunning = FamilyManager.getFamily(new AllOfComponents(typeof(PlayerIsMoving)));
	private Family emptyPlayerExecution = FamilyManager.getFamily(new AllOfComponents(typeof(EmptyExecution)));
	private Family agents = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef)));
	private Family viewportContainerPointed_f = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(ViewportContainer))); // Les container contenant les container éditable
	private Family scriptContainer_f = FamilyManager.getFamily(new AllOfComponents(typeof(UITypeContainer))); // Les containers scripts

	private GameData gameData;
	private int nDialog = 0;
	private GameObject lastEditedScript;

	public GameObject buttonPlay;
	public GameObject buttonContinue;
	public GameObject buttonStop;
	public GameObject buttonPause;
	public GameObject buttonStep;
	public GameObject buttonSpeed;
	public GameObject buttonReset;
	public GameObject endPanel;
	public GameObject dialogPanel;
	public GameObject editableScriptContainer;
	public GameObject libraryPanel;
	public GameObject EditableContainer;
	public GameObject prefabViewportScriptContainer;
	private string nameContainerSelected; // Nom du container selectionné
	private GameObject containerSelected; // Le container selectionné

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

		requireEndPanel.addEntryCallback(displayEndPanel);
		displayedEndPanel.addEntryCallback(onDisplayedEndPanel);
		actions.addEntryCallback(linkTo);
		newEnd_f.addEntryCallback(levelFinished);
		resetBlocLimit_f.addEntryCallback(delegate (GameObject go) { destroyScript(go, true); });
		scriptIsRunning.addExitCallback(delegate { setExecutionState(true); });
		scriptIsRunning.addExitCallback(saveHistory);
		emptyPlayerExecution.addEntryCallback(delegate { setExecutionState(true); });
		emptyPlayerExecution.addEntryCallback(delegate { GameObjectManager.removeComponent<EmptyExecution>(MainLoop.instance.gameObject); });

		currentActions.addEntryCallback(onNewCurrentAction);

		lastEditedScript = null;

		loadHistory();
	}

	// Active ou désactive le bouton play si il y a ou non des actions dans un container script
	private IEnumerator updatePlayButton()
	{
		yield return null;
		foreach (GameObject container in scriptContainer_f)
		{
			buttonPlay.GetComponent<Button>().interactable = !(container.transform.childCount < 2);
		}
	}

	// Permet de lancer la corroutine "updatePlayButton" depuis l'extérieur du systéme
	public void startUpdatePlayButton()
    {
		MainLoop.instance.StartCoroutine(updatePlayButton());
	}

	// Rafraichit certain élément de l'UI
	public void refreshUI()
	{
		//Refresh Containers size
		foreach (GameObject container in scriptContainer_f)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(container.GetComponent<RectTransform>());
		}
		MainLoop.instance.StartCoroutine(updatePlayButton());
	}

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

	private void setExecutionState(bool finished){
		buttonReset.GetComponent<Button>().interactable = finished;
		buttonPlay.GetComponent<Button>().interactable = !finished;
		
		GameObjectManager.setGameObjectState(buttonPlay, finished);
		GameObjectManager.setGameObjectState(buttonContinue, !finished);
		GameObjectManager.setGameObjectState(buttonStop, !finished);
		GameObjectManager.setGameObjectState(buttonPause, !finished);
		GameObjectManager.setGameObjectState(buttonStep, !finished);
		GameObjectManager.setGameObjectState(buttonSpeed, !finished);

		GameObjectManager.setGameObjectState(libraryPanel, finished);
		//editable canvas
		GameObjectManager.setGameObjectState(editableScriptContainer.transform.parent.parent.gameObject, finished);
	}
	
	private void saveHistory(int unused = 0){
		if(gameData.actionsHistory == null){
			gameData.actionsHistory = lastEditedScript;
		}
		else{
			foreach(Transform child in lastEditedScript.transform){
				Transform copy = UnityEngine.GameObject.Instantiate(child);
				copy.SetParent(gameData.actionsHistory.transform);
				GameObjectManager.bind(copy.gameObject);				
			}
			GameObjectManager.refresh(gameData.actionsHistory);
		}	
	}

	private void loadHistory(){
		if(gameData != null && gameData.actionsHistory != null){
			for(int i = 0 ; i < gameData.actionsHistory.transform.childCount ; i++){
				Transform child = UnityEngine.GameObject.Instantiate(gameData.actionsHistory.transform.GetChild(i));
				child.SetParent(editableScriptContainer.transform);
				GameObjectManager.bind(child.gameObject);
				GameObjectManager.refresh(editableScriptContainer);
			}
			LevelGenerator.computeNext(gameData.actionsHistory);
			foreach(BaseElement act in editableScriptContainer.GetComponentsInChildren<BaseElement>()){
				GameObjectManager.addComponent<Dropped>(act.gameObject);
			}
			//destroy history
			GameObject.Destroy(gameData.actionsHistory);
			LayoutRebuilder.ForceRebuildLayoutImmediate(editableScriptContainer.GetComponent<RectTransform>());
		}
	}

	private void restoreLastEditedScript(){
		List<Transform> childrenList = new List<Transform>();
		foreach(Transform child in lastEditedScript.transform){
			childrenList.Add(child);
		}
		foreach(Transform child in childrenList){
			child.SetParent(editableScriptContainer.transform);
			GameObjectManager.bind(child.gameObject);
		}
		GameObjectManager.refresh(editableScriptContainer);
	}

	private void levelFinished (GameObject go){
		setExecutionState(true);
		if(go.GetComponent<NewEnd>().endType == NewEnd.Win){
			loadHistory();
			PlayerPrefs.SetInt(gameData.levelToLoad.Item1, gameData.levelToLoad.Item2+1);
			PlayerPrefs.Save();
		}
		else if(go.GetComponent<NewEnd>().endType == NewEnd.Detected){
			//copy player container into editable container
			restoreLastEditedScript();
		}
	}
	private void linkTo(GameObject go){
		if(go.GetComponent<UIActionType>().linkedTo == null){
			if(go.GetComponent<BasicAction>()){
				go.GetComponent<UIActionType>().linkedTo = GameObject.Find(go.GetComponent<BasicAction>().actionType.ToString());
			}			
			else if(go.GetComponent<IfAction>())
				go.GetComponent<UIActionType>().linkedTo = GameObject.Find("If");
			else if(go.GetComponent<ForAction>())
				go.GetComponent<UIActionType>().linkedTo = GameObject.Find("For");
		}
	}

    private void displayEndPanel(GameObject endPanel)
    {
        GameObjectManager.setGameObjectState(endPanel.transform.parent.gameObject, true);
    }

    private void onDisplayedEndPanel (GameObject endPanel)
    { 
        switch (endPanel.GetComponent<NewEnd>().endType)
        {
            case 1:
                endPanel.transform.Find("VerticalCanvas").GetComponentInChildren<TextMeshProUGUI>().text = "Vous avez été repéré !";
                GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
                endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/LoseSound") as AudioClip;
                endPanel.GetComponent<AudioSource>().loop = true;
                endPanel.GetComponent<AudioSource>().Play();
                break;
            case 2:
				int score = (10000 / (gameData.totalActionBloc + 1) + 5000 / (gameData.totalStep + 1) + 6000 / (gameData.totalExecute + 1) + 5000 * gameData.totalCoin);
                Transform verticalCanvas = endPanel.transform.Find("VerticalCanvas");
				verticalCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Bravo vous avez gagné !\nScore: " + score;
                setScoreStars(score, verticalCanvas.Find("ScoreCanvas"));

				endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/VictorySound") as AudioClip;
                endPanel.GetComponent<AudioSource>().loop = false;
                endPanel.GetComponent<AudioSource>().Play();
				GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, true);
                //End
                if (gameData.levelToLoad.Item2 >= gameData.levelList[gameData.levelToLoad.Item1].Count - 1)
                {
                    GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
					GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadState").gameObject, false);
                }
                break;
        }
    }

	private void setScoreStars(int score, Transform scoreCanvas){
		int scoredStars = 0;
		if(gameData.levelToLoadScore != null){
			//check 0, 1, 2 or 3 stars
			if(score >= gameData.levelToLoadScore[0]){
				scoredStars = 3;
			}
			else if(score >= gameData.levelToLoadScore[1]){
				scoredStars = 2;
			}
			else {
				scoredStars = 1;
			}			
		}

		for (int nbStar = 0 ; nbStar < 4 ; nbStar++){
			if(nbStar == scoredStars)
				GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, true);
			else
				GameObjectManager.setGameObjectState(scoreCanvas.GetChild(nbStar).gameObject, false);
		}

		//save score only if better score
		int savedScore = PlayerPrefs.GetInt(gameData.levelToLoad.Item1+Path.DirectorySeparatorChar+gameData.levelToLoad.Item2+gameData.scoreKey, 0);
		if(savedScore < scoredStars){
			PlayerPrefs.SetInt(gameData.levelToLoad.Item1+Path.DirectorySeparatorChar+gameData.levelToLoad.Item2+gameData.scoreKey, scoredStars);
			PlayerPrefs.Save();			
		}
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		//Activate DialogPanel if there is a message
		if(gameData != null && gameData.dialogMessage.Count > 0 && !dialogPanel.transform.parent.gameObject.activeSelf){
			showDialogPanel();
		}
	}


	// Empty the script window
	// See ResetButton in editor
	public void resetScript(bool refund = false){
		// On récupére le contenaire pointer lors du clique poubelle
		GameObject scriptContainerPointer = viewportContainerPointed_f.First().transform.Find("ScriptContainer").gameObject;
		Debug.Log("scriptContainerPointer : " + scriptContainerPointer.name);

		// On parcourt le script container pour détruire toutes les actions
		for (int i = 0 ; i < scriptContainerPointer.transform.childCount ; i++){
			Debug.Log("scriptContainerPointer : " + scriptContainerPointer.transform.GetChild(i).name);
			if (scriptContainerPointer.transform.GetChild(i).GetComponent<BaseElement>()){
				Debug.Log("scriptContainerPointer : " + scriptContainerPointer.transform.GetChild(i).name);
				destroyScript(scriptContainerPointer.transform.GetChild(i).gameObject, refund);				
			}
		
		}
		refreshUI();
	}

	//Recursive script destroyer
	private void destroyScript(GameObject go,  bool refund = false){
		// Que fait cette partie?
		/*
		if(go.GetComponent<UIActionType>() != null){
			if(!refund)
				gameData.totalActionBloc++;
			else
				GameObjectManager.addComponent<AddOne>(go.GetComponent<UIActionType>().linkedTo);
		}
		*/
		
		// Si l'objet passer est un script container
		if(go.GetComponent<UITypeContainer>() != null){
			foreach(Transform child in go.transform){
				if(child.GetComponent<BaseElement>()){
					destroyScript(child.gameObject, refund);
				}
			}
		}

		// Si ce n'est pas le block de fin
		if(go.GetComponent<EndBlockScriptComponent>() == null)
        {
			go.transform.DetachChildren();
			GameObjectManager.unbind(go);
			UnityEngine.Object.Destroy(go);
		}
	}

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
	public void nextDialog(){
		nDialog++;
		dialogPanel.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = gameData.dialogMessage[nDialog].Item1;
		GameObject imageGO = dialogPanel.transform.Find("Image").gameObject;
		if(gameData.dialogMessage[nDialog].Item2 != null){
			GameObjectManager.setGameObjectState(imageGO,true);
			setImageSprite(imageGO.GetComponent<Image>(), Application.streamingAssetsPath + Path.DirectorySeparatorChar + "Levels" +
			Path.DirectorySeparatorChar + gameData.levelToLoad.Item1 + Path.DirectorySeparatorChar + "Images" + Path.DirectorySeparatorChar + gameData.dialogMessage[nDialog].Item2);
		}
		else
			GameObjectManager.setGameObjectState(imageGO,false);

		if(nDialog + 1 < gameData.dialogMessage.Count){
			setActiveOKButton(false);
			setActiveNextButton(true);
		}
		else{
			setActiveOKButton(true);
			setActiveNextButton(false);
		}
	}

	public void setActiveOKButton(bool active){
		GameObjectManager.setGameObjectState(dialogPanel.transform.Find("Buttons").Find("OKButton").gameObject, active);
	}

	public void setActiveNextButton(bool active){
		GameObjectManager.setGameObjectState(dialogPanel.transform.Find("Buttons").Find("NextButton").gameObject, active);
	}

	// See OKButton in editor
	public void closeDialogPanel(){
		nDialog = 0;
		gameData.dialogMessage = new List<(string,string)>();;
		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, false);
	}

	public void reloadScene(){
		gameData.totalActionBloc = 0;
		gameData.totalStep = 0;
		gameData.totalExecute = 0;
		gameData.totalCoin = 0;
		gameData.levelToLoadScore = null;
		gameData.dialogMessage = new List<(string,string)>();
		GameObjectManager.loadScene("MainScene");
	}

	// See TitleScreen and ScreenTitle buttons in editor
	public void returnToTitleScreen(){
		gameData.totalActionBloc = 0;
		gameData.totalStep = 0;
		gameData.totalExecute = 0;
		gameData.totalCoin = 0;
		gameData.levelToLoadScore = null;
		gameData.dialogMessage = new List<(string,string)>();
		gameData.actionsHistory = null;
		GameObjectManager.loadScene("TitleScreen");
	}

	// See NextLevel button in editor
	public void nextLevel(){
		gameData.levelToLoad.Item2++;
		reloadScene();
		gameData.actionsHistory = null;
	}

	// See ReloadLevel and RestartLevel buttons in editor
	public void retry(){
		gameData.totalActionBloc = 0;
		gameData.totalStep = 0;
		gameData.totalExecute = 0;
		gameData.totalCoin = 0;
		gameData.levelToLoadScore = null;
		gameData.dialogMessage = new List<(string,string)>();
		if(gameData.actionsHistory != null)
			UnityEngine.Object.DontDestroyOnLoad(gameData.actionsHistory);
		GameObjectManager.loadScene("MainScene");
	}

	public void reloadState(){
		GameObjectManager.removeComponent<NewEnd>(endPanel);
	}

	// See StopButton in editor
	public void stopScript(){
		restoreLastEditedScript();
		setExecutionState(true);
		CurrentAction act;
		foreach(GameObject go in currentActions){
			act = go.GetComponent<CurrentAction>();
			if(act.agent.CompareTag("Player"))
				GameObjectManager.removeComponent<CurrentAction>(go);
		}		
	}

	// See ExecuteButton in editor
	public void applyScriptToPlayer(){
		//if first click on play button
		if(!buttonStop.activeInHierarchy){
			gameData.totalExecute++;
			//hide panels
			GameObjectManager.setGameObjectState(libraryPanel, false);
			//editable canvas
			GameObjectManager.setGameObjectState(editableScriptContainer.transform.parent.parent.gameObject, false);
			//clean container for each robot
			foreach(GameObject robot in playerGO){
				foreach(Transform child in robot.GetComponent<ScriptRef>().scriptContainer.transform){
					GameObjectManager.unbind(child.gameObject);
					GameObject.Destroy(child.gameObject);
				}
			}
			
			//copy editable script
			lastEditedScript = GameObject.Instantiate(editableScriptContainer);
			foreach(Transform child in lastEditedScript.transform){
				if(child.name.Contains("PositionBar")){
					UnityEngine.GameObject.Destroy(child.gameObject);
				}
			}
			
			GameObject containerCopy = CopyActionsFrom(editableScriptContainer, false, playerGO.First());
			
			foreach( GameObject go in playerGO){
				GameObject targetContainer = go.GetComponent<ScriptRef>().scriptContainer;
				go.GetComponent<ScriptRef>().uiContainer.transform.Find("Header").Find("Toggle").GetComponent<Toggle>().isOn = true;	
				for(int i = 0 ; i < containerCopy.transform.childCount ; i++){
					if(!containerCopy.transform.GetChild(i).name.Contains("PositionBar")){
						Transform child = UnityEngine.GameObject.Instantiate(containerCopy.transform.GetChild(i));
						child.SetParent(targetContainer.transform);
						GameObjectManager.bind(child.gameObject);
						GameObjectManager.refresh(targetContainer);
					}

				}
				LevelGenerator.computeNext(targetContainer);
			}

			UnityEngine.Object.Destroy(containerCopy);

			//empty editable container
			//resetScript();

			buttonPlay.GetComponent<AudioSource>().Play();
			foreach(GameObject go in agents){
				LayoutRebuilder.ForceRebuildLayoutImmediate(go.GetComponent<ScriptRef>().uiContainer.GetComponent<RectTransform>());
				if(go.CompareTag("Player")){				
					GameObjectManager.setGameObjectState(go.GetComponent<ScriptRef>().uiContainer,true);				
				}
			}
		}
	}

    public GameObject CopyActionsFrom(GameObject container, bool isInteractable, GameObject agent){
		GameObject copyGO = GameObject.Instantiate(container); 
		foreach(TMP_Dropdown drop in copyGO.GetComponentsInChildren<TMP_Dropdown>()){
			drop.interactable = isInteractable;
		}
		foreach(TMP_InputField input in copyGO.GetComponentsInChildren<TMP_InputField>()){
			input.interactable = isInteractable;
		}
		foreach(ForAction forAct in copyGO.GetComponentsInChildren<ForAction>()){
			
			if(!isInteractable){
				forAct.nbFor = int.Parse(forAct.transform.GetChild(0).transform.GetChild(1).GetComponent<TMP_InputField>().text);
				forAct.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
			}
				
			else{
				forAct.currentFor = 0;
				forAct.gameObject.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = forAct.nbFor.ToString();
			}

			foreach(BaseElement act in forAct.GetComponentsInChildren<BaseElement>()){
				if(!act.Equals(forAct)){
					forAct.firstChild = act.gameObject;
					break;
				}
			}
		}
		foreach(ForeverAction loopAct in copyGO.GetComponentsInChildren<ForeverAction>()){
			foreach(BaseElement act in loopAct.GetComponentsInChildren<BaseElement>()){
				if(!act.Equals(loopAct)){
					loopAct.firstChild = act.gameObject;
					break;
				}
			}
		}
		foreach(IfAction IfAct in copyGO.GetComponentsInChildren<IfAction>()){
			IfAct.ifEntityType = IfAct.transform.GetChild(0).Find("DropdownEntityType").GetComponent<TMP_Dropdown>().value;
			IfAct.ifDirection = IfAct.transform.GetChild(0).Find("DropdownDirection").GetComponent<TMP_Dropdown>().value;
			IfAct.range = int.Parse(IfAct.transform.GetChild(0).Find("InputFieldRange").GetComponent<TMP_InputField>().text);
			IfAct.ifNot = (IfAct.transform.GetChild(0).Find("DropdownIsOrIsNot").GetComponent<TMP_Dropdown>().value == 1);
			foreach(BaseElement act in IfAct.GetComponentsInChildren<BaseElement>()){
				if(!act.Equals(IfAct)){
					IfAct.firstChild = act.gameObject;
					break;
				}
			}
		}

		foreach(UITypeContainer typeContainer in copyGO.GetComponentsInChildren<UITypeContainer>()){
			typeContainer.enabled = isInteractable;
		}
		foreach(PointerSensitive pointerSensitive in copyGO.GetComponentsInChildren<PointerSensitive>()){
			pointerSensitive.enabled = isInteractable;
		}

		Color actionColor;
		switch(agent.tag){
			case "Player":
				actionColor = MainLoop.instance.GetComponent<AgentColor>().playerAction;
				break;
			case "Drone":
				actionColor = MainLoop.instance.GetComponent<AgentColor>().droneAction;
				break;
			default: // agent by default = robot
				actionColor = MainLoop.instance.GetComponent<AgentColor>().playerAction;
				break;
		}

		foreach(BasicAction act in copyGO.GetComponentsInChildren<BasicAction>()){
			act.gameObject.GetComponent<Image>().color = actionColor;
		}

		return copyGO;
	}

	
	// Ajout un container à la scéne
	public void addContainer()
    {
		// On clone de viewport
		GameObject cloneContainer = Object.Instantiate(prefabViewportScriptContainer);
		// On l'ajoute à l'éditableContainer
		cloneContainer.transform.SetParent(EditableContainer.transform);
		// On regarde conbien de viewport container contient l'éditable pour mettre le nouveau viewport à la bonne position
		cloneContainer.transform.SetSiblingIndex(EditableContainer.GetComponent<EditableCanvacComponent>().nbViewportContainer);
		// On ajoute la caméra dans le bridge
		cloneContainer.GetComponent<CameraSystemBridge>().cameraAssociate = GameObject.Find("Main Camera");
		// Puis on imcrémente le nombre de viewport contenue dans l'éditable
		EditableContainer.GetComponent<EditableCanvacComponent>().nbViewportContainer += 1;
		// On ajoute le nouveau viewport container à FYFY
		GameObjectManager.bind(cloneContainer);

		// Lance le son de l'ajout d'un container
		cloneContainer.GetComponent<AudioSource>().Play();

		// Affiche le bon nom
		bool nameOk = false;
		for(int i = EditableContainer.GetComponent<EditableCanvacComponent>().nbViewportContainer; !nameOk; i++)
        {
			// Si le nom n'est pas déjà utilisé on nomme le nouveau container de cette façon
			if(!nameContainerUsed("Agent" + i))
            {
				cloneContainer.GetComponentInChildren<UITypeContainer>().associedAgentName = "Agent" + i;
				nameOk = true;
			}
		}
		MainLoop.instance.StartCoroutine(updateVerticalName(cloneContainer.GetComponentInChildren<UITypeContainer>().associedAgentName));

	}

	public bool nameContainerUsed(string name) {
		bool nameUsed = false;
		// On regarde en premier lieu si le nom n'existe pas déjà
		foreach (GameObject container in scriptContainer_f)
		{
			if (container.GetComponent<UITypeContainer>().associedAgentName == name)
			{
				nameUsed = true;
			}
		}

		return nameUsed;
	}
	
	// Change le nom du container
	public void newNameContainer(string name)
    {
		Debug.Log("Nom reçue : " + name);
		// Si le nom n'est pas utilisé
        if (!nameContainerUsed(name) && name.Length < 8)
        {
			// On cherche le container
			foreach (GameObject container in scriptContainer_f)
			{
				// Si on trouve celui dont le nom du container selectionné correspond
				if (container.GetComponent<UITypeContainer>().associedAgentName == nameContainerSelected)
				{
					string oldName = container.GetComponent<UITypeContainer>().associedAgentName;
					// On change pour son nouveau nom
					container.GetComponent<UITypeContainer>().associedAgentName = name;
					// Puis on l'affiche verticalement
					verticalName(name);
					// On envoie au systéme sur quel agent on va modifie les données
					bool agentExist = EditAgentSystem.instance.modificationAgent(oldName);
					// Si l'agent existe, on met à jours son lien (on supprime le lien actuelle)
					if (agentExist)
					{
						EditAgentSystem.instance.newScriptContainerLink();
						// Si le changement de nom entre l'agent et le container est automatique, on change aussi le nom de l'agent
						if (container.GetComponent<UITypeContainer>().editNameAuto)
						{
							EditAgentSystem.instance.setAgentName(name);
						}
					}
					else // Sinon on regarde si on agent du même nom existe pour établir un lien
					{
						agentExist = EditAgentSystem.instance.modificationAgent(name);
						if (agentExist)
						{
							EditAgentSystem.instance.newScriptContainerLink();
						}
					}
				}
			}
		}
        else{ // Sinon on annule le changement
			cancelChangeNameContainer(name);
		}
	}

	private IEnumerator updateVerticalName(string name)
	{
		yield return null;
		verticalName(name);
	}

	// Afichage du nom du container à la verticale
	public void verticalName(string name)
    {
		// On recherhe le container qui contient le même nom
		foreach (GameObject container in scriptContainer_f)
        {
			// Si on le trouve, alors on change l'écriture du nom à la vertical
			if (container.GetComponent<UITypeContainer>().associedAgentName == name) {
				// On créer une variable pour stocker les modifications du nom
				string newViewName = "";
				foreach (char l in name)
				{
					newViewName = newViewName + l + "\n";
				}
				// On remplace le nom actuel par le nouveau format
				container.transform.Find("InputField (TMP)").GetComponent<TMP_InputField>().text = newViewName;
			}
        }
	}


	// Affiche le nom du container de manniére horizontal
	public void horizontalName(string name)
    {
		// On recherhe le container qui contient le même nom
		foreach (GameObject container in scriptContainer_f)
		{
			// Si on le trouve, alors on change l'écriture du nom à l'horizontal
			if (container.transform.Find("InputField (TMP)").GetComponent<TMP_InputField>().text == name)
			{
				// On remplace le nom actuel par le nouveau format
				container.transform.Find("InputField (TMP)").GetComponent<TMP_InputField>().text = container.GetComponent<UITypeContainer>().associedAgentName;
				// On enregistre le nom du container selectionné
				nameContainerSelected = container.GetComponent<UITypeContainer>().associedAgentName;
			}
		}
	}

	// Utilisé surtout par les apelles extérieurs au systéme
	// Permet d'enregistrer le nom du container que l'on veux changé
	// Et lui changer son nom 
	public void setContainerName(string oldName, string newName)
    {
		nameContainerSelected = oldName;
		newNameContainer(newName);
	}


	// On annule le nouveau nom
	public void cancelChangeNameContainer(string name)
	{
		foreach (GameObject container in scriptContainer_f)
		{
			// Si le nom afficher du container et le même quand paramétre, mais pas son nom, on a bien le container non modifier
			if (container.transform.Find("InputField (TMP)").GetComponent<TMP_InputField>().text == name)
			{
				// On réaffiche son ancien nom à la vertical
				verticalName(container.GetComponent<UITypeContainer>().associedAgentName);
			}
		}
	}
}