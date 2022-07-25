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
	private Family actions = FamilyManager.getFamily(new AllOfComponents(typeof(PointerSensitive), typeof(LibraryItemRef)));
	private Family currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(BasicAction), typeof(LibraryItemRef), typeof(CurrentAction)));
	private Family newEnd_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
	private Family resetBlocLimit_f = FamilyManager.getFamily(new AllOfComponents(typeof(ResetBlocLimit)));
	private Family playerScriptEnds = FamilyManager.getFamily(new NoneOfComponents(typeof(Moved)), new AnyOfTags("Player"));
	private Family emptyPlayerExecution = FamilyManager.getFamily(new AllOfComponents(typeof(EmptyExecution)));
	private Family agents = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef)));
	private Family viewportContainerPointed_f = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(ViewportContainer))); // Les container contenant les container éditable
	private Family viewportContainer_f = FamilyManager.getFamily(new AllOfComponents(typeof(ViewportContainer))); // Les containers viewport
	private Family scriptContainer_f = FamilyManager.getFamily(new AllOfComponents(typeof(UIRootContainer)), new AnyOfTags("ScriptConstructor")); // Les containers de scripts
	private Family agent_f = FamilyManager.getFamily(new AllOfComponents(typeof(AgentEdit), typeof(ScriptRef))); // On récupére les agents pouvant être édité
	private Family resetButton_f = FamilyManager.getFamily(new AnyOfTags("ResetButton")); // Les boutons reset

	private Family inventoryBlocks = FamilyManager.getFamily(new AllOfComponents(typeof(ElementToDrag)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	private GameData gameData;
	private int nDialog = 0;

	public GameObject buttonPlay;
	public GameObject buttonContinue;
	public GameObject buttonStop;
	public GameObject buttonPause;
	public GameObject buttonStep;
	public GameObject buttonSpeed;
	public GameObject menuEchap;
	public GameObject endPanel;
	public GameObject dialogPanel;
	public GameObject canvas;
	public GameObject libraryPanel;
	public GameObject EditableCanvas;
	public GameObject prefabViewportScriptContainer;
	private UIRootContainer containerSelected; // Le container selectionné

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
		resetBlocLimit_f.addEntryCallback(delegate (GameObject go) {
			destroyScript(go, true);
			GameObjectManager.unbind(go);
			UnityEngine.Object.Destroy(go);
		});
		playerScriptEnds.addEntryCallback(delegate { setExecutionView(false); });
		playerScriptEnds.addEntryCallback(saveHistory);
		emptyPlayerExecution.addEntryCallback(delegate { setExecutionView(true); });
		emptyPlayerExecution.addEntryCallback(delegate { GameObjectManager.removeComponent<EmptyExecution>(MainLoop.instance.gameObject); });
		
		currentActions.addEntryCallback(onNewCurrentAction);

		inventoryBlocks.addEntryCallback(delegate { MainLoop.instance.StartCoroutine(forceLibraryRefresh()); });
		inventoryBlocks.addExitCallback(delegate { MainLoop.instance.StartCoroutine(forceLibraryRefresh()); });

		loadHistory();

		// Afin de mettre en rouge les noms qui ne sont pas en lien dés le début
		MainLoop.instance.StartCoroutine(tcheckLinkName());
		MainLoop.instance.StartCoroutine(forceLibraryRefresh());
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
		buttonPlay.GetComponent<Button>().interactable = false;
		foreach (GameObject container in scriptContainer_f)
		{
			if (container.GetComponentsInChildren<BaseElement>().Length > 0)
			{
				buttonPlay.GetComponent<Button>().interactable = true;
			}
		}
	}


	// Vérifie si les noms des containers correspond à un agent et vice-versa
	// Si non, Fait apparaitre le nom en rouge
	private IEnumerator tcheckLinkName()
	{
		yield return null;

		// On parcours les containers et si aucun nom ne correspond alors on met leur nom en gras rouge
		foreach (GameObject container in scriptContainer_f)
		{
			bool nameSame = false;
			foreach (GameObject agent in agent_f)
				if (container.GetComponent<UIRootContainer>().associedAgentName == agent.GetComponent<AgentEdit>().agentName)
					nameSame = true;

			// Si même nom trouver on met l'arriére plan blanc
			if (nameSame)
				container.transform.Find("ContainerName").GetComponent<TMP_InputField>().image.color = Color.white;
			else // sinon rouge 
				container.transform.Find("ContainerName").GetComponent<TMP_InputField>().image.color = new Color(1f, 0.4f, 0.28f, 1f);
		}

		// On fait la même chose pour les agents
		foreach (GameObject agent in agent_f)
		{
			bool nameSame = false;
			foreach (GameObject container in scriptContainer_f)
				if (container.GetComponent<UIRootContainer>().associedAgentName == agent.GetComponent<AgentEdit>().agentName)
					nameSame = true;

			// Si même nom trouver on met l'arriére transparent
			if (nameSame)
				agent.GetComponent<ScriptRef>().executablePanel.GetComponentInChildren<TMP_InputField>().image.color = new Color(1f, 1f, 1f, 1f);
			else // sinon rouge 
				agent.GetComponent<ScriptRef>().executablePanel.GetComponentInChildren<TMP_InputField>().image.color = new Color(1f, 0.4f, 0.28f, 1f);
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


	// Permet de lancer la corroutine "updatePlayButton" depuis l'extérieur du systéme
	public void startUpdatePlayButton()
    {
		MainLoop.instance.StartCoroutine(updatePlayButton());
	}

	// Rafraichit le nom des containers
	public void refreshUINameContainer()
	{
		MainLoop.instance.StartCoroutine(tcheckLinkName());
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
		// Toggle execution panel
		GameObjectManager.setGameObjectState(canvas.transform.Find("ExecutableCanvas").gameObject, value);
	}
	

	// Add the executed scripts to the containers history
	private void saveHistory(GameObject unused){
		if(gameData.actionsHistory == null){
			// set history as a copy of editable canvas
			gameData.actionsHistory = GameObject.Instantiate(EditableCanvas.transform.GetChild(0).transform).gameObject;
			gameData.actionsHistory.SetActive(false); // keep this gameObject as a ghost
			// We don't bind the history to FYFY
		}
		else{
			// parse all containers inside editable canvas
			for(int containerCpt = 0; containerCpt < EditableCanvas.transform.GetChild(0).childCount; containerCpt++){
				Transform viewportForEditableContainer = EditableCanvas.transform.GetChild(0).GetChild(containerCpt);
				// the first child is the script container that contains script elements
				foreach (Transform child in viewportForEditableContainer.GetChild(0))
                {
					if (child.GetComponent<BaseElement>())
                    {
						// copy this child inside the appropriate history
						GameObject.Instantiate(child, gameData.actionsHistory.transform.GetChild(containerCpt));
						// We don't bind the history to FYFY
					}
				}
			}
		}
		// Erase all editable containers
		foreach (Transform viewportForEditableContainer in EditableCanvas.transform.GetChild(0))
			foreach (Transform child in viewportForEditableContainer.GetChild(0))
				if (child.GetComponent<BaseElement>())
				{
					GameObjectManager.unbind(child.gameObject);
					GameObject.Destroy(child);
				}
	}


	// Restore saved scripts in history inside editable script containers
	private void loadHistory(){
		if(gameData != null && gameData.actionsHistory != null)
		{
			// For security, erase all editable containers
			foreach (Transform viewportForEditableContainer in EditableCanvas.transform.GetChild(0))
				foreach (Transform child in viewportForEditableContainer.GetChild(0))
					if (child.GetComponent<BaseElement>())
					{
						GameObjectManager.unbind(child.gameObject);
						GameObject.Destroy(child);
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
						Transform history_childCopy = GameObject.Instantiate(history_child, EditableCanvas.transform.GetChild(0).GetChild(i));
						// Place this child copy at the end of the container
						history_childCopy.SetAsFirstSibling();
						history_childCopy.SetSiblingIndex(history_childCopy.parent.childCount - 2);
						GameObjectManager.bind(history_childCopy.gameObject);
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
		}
	}


	// ????
	private void restoreLastEditedScript(){
		/* TODO => reprendre ça
		List<Transform> childrenList = new List<Transform>();
		if(lastEditedScript != null)
        {
			foreach (Transform child in lastEditedScript.transform)
			{
				childrenList.Add(child);
			}
			foreach (Transform child in childrenList)
			{
				child.SetParent(editableScriptContainer.transform);
				GameObjectManager.bind(child.gameObject);
			}
			GameObjectManager.refresh(editableScriptContainer);
		}*/
	}


	// Lors d'une fin d'exécution de séquence, gére les différents éléments à ré-afficher ou si il faut sauvegarder que la progression du joueur
	private void levelFinished (GameObject go){
		// On réaffiche les différent panel pour la création de séquence
		setExecutionView(true);

		// En cas de fin de niveau
		if(go.GetComponent<NewEnd>().endType == NewEnd.Win){
			// Affichage de l'historique de l'ensemble des actions exécutés
			loadHistory();
			// Sauvegarde de l'état d'avancement des niveaux pour le jour (niveau et étoile)
			PlayerPrefs.SetInt(gameData.levelToLoad.Item1, gameData.levelToLoad.Item2+1);
			PlayerPrefs.Save();
		}
		else if(go.GetComponent<NewEnd>().endType == NewEnd.Detected){
			//copy player container into editable container
			restoreLastEditedScript();
		}
		else if (go.GetComponent<NewEnd>().endType == NewEnd.BadCondition)
		{
			//copy player container into editable container
			restoreLastEditedScript();
		}
	}


	// Find item in library to hook to this GameObject
	private void linkTo(GameObject go){
		if(go.GetComponent<LibraryItemRef>().linkedTo == null){
			if(go.GetComponent<BasicAction>())
				go.GetComponent<LibraryItemRef>().linkedTo = GameObject.Find(go.GetComponent<BasicAction>().actionType.ToString());
			else if (go.GetComponent<BaseCaptor>())
				go.GetComponent<LibraryItemRef>().linkedTo = GameObject.Find(go.GetComponent<BaseCaptor>().captorType.ToString());
			else if (go.GetComponent<BaseOperator>())
				go.GetComponent<LibraryItemRef>().linkedTo = GameObject.Find(go.GetComponent<BaseOperator>().operatorType.ToString());
			else if (go.GetComponent<WhileControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = GameObject.Find("While");
			else if (go.GetComponent<ForeverControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = GameObject.Find("Forever");
			else if (go.GetComponent<ForControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = GameObject.Find("For");
			else if (go.GetComponent<IfElseControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = GameObject.Find("IfElse");
			else if(go.GetComponent<IfControl>())
				go.GetComponent<LibraryItemRef>().linkedTo = GameObject.Find("If");
		}
	}


	// Permet la gestion de l'affiche du panel de fin de niveau
    private void displayEndPanel(GameObject endPanel)
    {
        GameObjectManager.setGameObjectState(endPanel.transform.parent.gameObject, true);
    }


	// Permet de switcher entre les affichages différents de fin de niveau
	// Cas 1 : Un ennemie à repéré le robot
	// Cas 2 : Le robot est sortie du labyrinth
	// Cas 3 : Le joueur à mal remplit une condition
    private void onDisplayedEndPanel (GameObject endPanel)
    { 
        switch (endPanel.GetComponent<NewEnd>().endType)
        {
            case 1:
                endPanel.transform.Find("VerticalCanvas").GetComponentInChildren<TextMeshProUGUI>().text = "Vous avez été repéré !";
                GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
				GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadLevel").gameObject, true);
				GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadState").gameObject, true);
				GameObjectManager.setGameObjectState(endPanel.transform.Find("ScreenTitle").gameObject, true);
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
				GameObjectManager.setGameObjectState(endPanel.transform.Find("ScreenTitle").gameObject, true);
				//End
				if (gameData.levelToLoad.Item2 >= gameData.levelList[gameData.levelToLoad.Item1].Count - 1)
                {
                    GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
					GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadLevel").gameObject, true);
					GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadState").gameObject, false);
				}
                break;
			case 3:
				endPanel.transform.Find("VerticalCanvas").GetComponentInChildren<TextMeshProUGUI>().text = "Une condition est mal remplie !";
				GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
				GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadLevel").gameObject, false);
				GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadState").gameObject, true);
				GameObjectManager.setGameObjectState(endPanel.transform.Find("ScreenTitle").gameObject, false);
				endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/LoseSound") as AudioClip;
				endPanel.GetComponent<AudioSource>().loop = true;
				endPanel.GetComponent<AudioSource>().Play();
				break;
        }
    }

	// Gére le nombre d'étoile à afficher selon le score obtenue
	private void setScoreStars(int score, Transform scoreCanvas){
		// Détermine le nombre d'étoile à afficher
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
		
		// Affiche le nombre d'étoile désiré
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


	// Empty the script window
	// See ResetButton in editor
	public void resetScriptContainer(bool refund = false){
		// On récupére le contenair pointé lors du clique de la balayette
		GameObject scriptContainerPointer = viewportContainerPointed_f.First().transform.Find("ScriptContainer").gameObject;

		// On parcourt le script container pour détruire toutes les instructions
		for (int i = scriptContainerPointer.transform.childCount-1; i >= 0 ; i--){
			if (scriptContainerPointer.transform.GetChild(i).GetComponent<BaseElement>()){
				DragDropSystem.instance.deleteElement(scriptContainerPointer.transform.GetChild(i).gameObject);				
			}
		}
		// Enable the last emptySlot and disable dropZone
		GameObjectManager.setGameObjectState(scriptContainerPointer.transform.GetChild(scriptContainerPointer.transform.childCount - 1).gameObject, true);
		GameObjectManager.setGameObjectState(scriptContainerPointer.transform.GetChild(scriptContainerPointer.transform.childCount - 2).gameObject, false);
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


	// See TitleScreen and ScreenTitle buttons in editor
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
	

	// ????
	public void reloadState(){
		GameObjectManager.removeComponent<NewEnd>(endPanel);
	}

	public void fillExecutablePanel(GameObject srcScript, GameObject targetContainer, string agentTag)
    {
		// On va copier la sequence créé par le joueur dans le container de la fenêtre du robot
		// On commence par créer une copie du container ou se trouve la sequence
		GameObject containerCopy = CopyActionsFromAndInitFirstChild(srcScript, false, agentTag);
		// On copie les actions dedans 
		for (int i = 0; i < containerCopy.transform.childCount; i++)
		{
			// On ne conserve que les BaseElement et on les nettoie
			if (containerCopy.transform.GetChild(i).GetComponent<BaseElement>())
			{
				Transform child = UnityEngine.GameObject.Instantiate(containerCopy.transform.GetChild(i)); ;
				// Si c'est un block special (for, if...)
				if (child.GetComponent<ControlElement>())
					CleanControlBlock(child);
				child.SetParent(targetContainer.transform, false);
			}
		}
		// Va linker les blocs ensemble
		// C'est à dire qu'il va définir pour chaque bloc, qu'elle est le suivant à exécuter
		LevelGenerator.computeNext(targetContainer);
		// On détruit la copy de la sequence d'action
		UnityEngine.Object.Destroy(containerCopy);
		// On notifie les systèmes comme quoi le panneau d'execution est rempli
		GameObjectManager.addComponent<ExecutablePanelReady>(MainLoop.instance.gameObject);
	}

	// See ExecuteButton in editor
	// Copie les blocks du panneau d'édition dans le panneau d'exécution
	public void copyEditableScriptsToExecutablePanels(){
		//clean container for each robot and copy the new sequence
		foreach (GameObject robot in playerGO) {
			GameObject executableContainer = robot.GetComponent<ScriptRef>().executableScript;
			// Clean robot container
			foreach (Transform child in executableContainer.transform) {
				GameObjectManager.unbind(child.gameObject);
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
				fillExecutablePanel(editableContainer, executableContainer, robot.tag);
				// bind all child
				foreach (Transform child in executableContainer.transform)
					GameObjectManager.bind(child.gameObject);
				// On développe le panneau au cas où il aurait été réduit
				robot.GetComponent<ScriptRef>().executablePanel.transform.Find("Header").Find("Toggle").GetComponent<Toggle>().isOn = true;
			}
		}
		// On harmonise l'affichage de l'UI container des agents
		foreach(GameObject go in agents){
			LayoutRebuilder.ForceRebuildLayoutImmediate(go.GetComponent<ScriptRef>().executablePanel.GetComponent<RectTransform>());
			if(go.CompareTag("Player")){				
				GameObjectManager.setGameObjectState(go.GetComponent<ScriptRef>().executablePanel, true);				
			}
		}
	}

	/**
	 * On copie le container qui contient la sequence d'actions et on initialise les firstChild
	 * Param:
	 *	Container (GameObject) : Le container qui contient le script à copier
	 *	isInteractable (bool) : Si le script copié peut contenir des éléments interactable (sinon l'interaction sera desactivé)
	 *	agent (GameObject) : L'agent sur qui l'on va copier la sequence (pour définir la couleur)
	 * 
	 **/
	public GameObject CopyActionsFromAndInitFirstChild(GameObject container, bool isInteractable, string agentTag){
		// On va travailler avec une copy du container
		GameObject copyGO = GameObject.Instantiate(container); 
		//Pour tous les élément interactible, on va les désactiver/activer selon le paramétrage
		foreach(TMP_Dropdown drop in copyGO.GetComponentsInChildren<TMP_Dropdown>()){
			drop.interactable = isInteractable;
		}
		foreach(TMP_InputField input in copyGO.GetComponentsInChildren<TMP_InputField>()){
			input.interactable = isInteractable;
		}

		// Pour chaque bloc for
		foreach(ForControl forAct in copyGO.GetComponentsInChildren<ForControl>()){
			// Si activé, on note le nombre de tour de boucle à faire
			if(!isInteractable && !forAct.gameObject.GetComponent<WhileControl>())
			{
				forAct.nbFor = int.Parse(forAct.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text);
				forAct.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();		
			}// Sinon on met tout à 0
			else if(isInteractable && !forAct.gameObject.GetComponent<WhileControl>())
			{
				forAct.currentFor = 0;
				forAct.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = forAct.nbFor.ToString();
			}
			else if (forAct is WhileControl)
            {
				if (forAct.transform.Find("ConditionContainer").GetChild(0).gameObject.GetComponent<BaseCondition>())
				{
					// On traduit la condition en string
					((WhileControl)forAct).condition = new List<string>();
					ConditionManagement.instance.convertionConditionSequence(forAct.gameObject.transform.Find("ConditionContainer").GetChild(0).gameObject, ((WhileControl)forAct).condition);
				}
				else
				{
					GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.BadCondition });
				}
			}
			// On parcourt les éléments présent dans le block action
			foreach(BaseElement act in forAct.GetComponentsInChildren<BaseElement>()){
				// Si ce n'est pas un bloc action alors on le note comme premier élément puis on arrête le parcourt des éléments
				if(!act.Equals(forAct)){
					forAct.firstChild = act.gameObject;
					break;
				}
			}
		}
		// Pour chaque block de boucle infini
		foreach (ForeverControl loopAct in copyGO.GetComponentsInChildren<ForeverControl>()){
			foreach (BaseElement act in loopAct.GetComponentsInChildren<BaseElement>())
			{
				if (!act.Equals(loopAct))
				{
					loopAct.firstChild = act.gameObject;
					break;
				}
			}
		}
		// Pour chaque block if
		foreach(IfControl ifAct in copyGO.GetComponentsInChildren<IfControl>()){
			//On vérifie que le bloc condition comporte un élément ou un opérator
			if (ifAct.transform.Find("ConditionContainer").GetChild(0).gameObject.GetComponent<BaseCondition>())
			{
				// On traduit la condition en string
				ifAct.condition = new List<string>();
				ConditionManagement.instance.convertionConditionSequence(ifAct.gameObject.transform.Find("ConditionContainer").GetChild(0).gameObject, ifAct.condition);
			}
            else
            {
				GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.BadCondition });
			}

			GameObject thenContainer = ifAct.transform.Find("Container").gameObject;
			BaseElement firstThen = thenContainer.GetComponentInChildren<BaseElement>();
			if (firstThen)
				ifAct.firstChild = firstThen.gameObject;
			//Si c'est un elseAction
			if (ifAct is IfElseControl)
            {
				GameObject elseContainer = ifAct.transform.Find("ElseContainer").gameObject;
				BaseElement firstElse = elseContainer.GetComponentInChildren<BaseElement>();
				if (firstElse)
					((IfElseControl)ifAct).elseFirstChild = firstElse.gameObject;
			}
		}

		foreach(PointerSensitive pointerSensitive in copyGO.GetComponentsInChildren<PointerSensitive>())
			pointerSensitive.enabled = isInteractable;

		foreach (Selectable selectable in copyGO.GetComponentsInChildren<Selectable>())
			selectable.interactable = isInteractable;

		// On défini la couleur de l'action selon l'agent à qui appartiendra la script
		Color actionColor;
		switch(agentTag)
		{
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

	/**
	 * Nettoie le bloc de controle (On supprime les end-zones, on met les conditions sous forme d'un seul bloc)
	 * Param:
	 *	specialBlock (GameObject) : Container qu'il faut nettoyer
	 * 
	 **/
	public void CleanControlBlock(Transform specialBlock)
    {
		// Vérifier que c'est bien un block de controle
		if (specialBlock.GetComponent<ControlElement>())
		{
			// Récupérer le container des actions
			Transform container = specialBlock.transform.Find("Container");
			// remove the last child, the emptyZone
			GameObject emptySlot = container.GetChild(container.childCount - 1).gameObject;
			if (GameObjectManager.isBound(emptySlot))
				GameObjectManager.unbind(emptySlot);
			emptySlot.transform.SetParent(null);
			GameObject.Destroy(emptySlot);
			// remove the new last child, the dropzone
			GameObject dropZone = container.GetChild(container.childCount - 1).gameObject;
			if (GameObjectManager.isBound(emptySlot))
				GameObjectManager.unbind(dropZone);
			dropZone.transform.SetParent(null);
			GameObject.Destroy(dropZone);

			// Si c'est un block if on garde le container des actions (sans le emptyslot et la dropzone) mais la condition est traduite dans IfAction
			if (specialBlock.GetComponent<IfElseControl>())
			{
				// get else container
				Transform elseContainer = specialBlock.transform.Find("ElseContainer");
				// remove the last child, the emptyZone
				emptySlot = elseContainer.GetChild(elseContainer.childCount - 1).gameObject;
				if (GameObjectManager.isBound(emptySlot))
					GameObjectManager.unbind(emptySlot);
				emptySlot.transform.SetParent(null);
				GameObject.Destroy(emptySlot);
				// remove the new last child, the dropzone
				dropZone = elseContainer.GetChild(elseContainer.childCount - 1).gameObject;
				if (GameObjectManager.isBound(emptySlot))
					GameObjectManager.unbind(dropZone);
				dropZone.transform.SetParent(null);
				GameObject.Destroy(dropZone);
			}

			// On parcourt les blocks qui composent le container afin de les nettoyer également
			foreach (Transform block in container)
				// Si c'est le cas on fait un appel récursif
				if (block.GetComponent<ControlElement>())
					CleanControlBlock(block);
		}
	}

	// used on + button
	public void addContainer()
    {
		addSpecificContainer();
	}

	// Ajouter un container à la scéne
	public void addSpecificContainer(string name = "", AgentEdit.EditMode editState = AgentEdit.EditMode.Editable, List<GameObject> script = null)
	{
		// On clone le prefab
		GameObject cloneContainer = Object.Instantiate(prefabViewportScriptContainer);
		Transform editableContainers = EditableCanvas.transform.Find("EditableContainers");
		// On l'ajoute à l'éditableContainer
		cloneContainer.transform.SetParent(editableContainers, false);
		// We secure the scale
		cloneContainer.transform.localScale = new Vector3(1, 1, 1);
		// On regarde combien de viewport container contient l'éditable pour mettre le nouveau viewport à la bonne position
		cloneContainer.transform.SetSiblingIndex(EditableCanvas.GetComponent<EditableCanvacComponent>().nbViewportContainer);
		// Puis on imcrémente le nombre de viewport contenue dans l'éditable
		EditableCanvas.GetComponent<EditableCanvacComponent>().nbViewportContainer += 1;

		// Lance le son de l'ajout d'un container
		cloneContainer.GetComponent<AudioSource>().Play();

		// Affiche le bon nom
		if (name != "")
		{
			// On définie son nom à celui de l'agent
			cloneContainer.GetComponentInChildren<UIRootContainer>().associedAgentName = name;
			// On affiche le bon nom sur le container
			cloneContainer.GetComponentInChildren<TMP_InputField>().text = name;
		}
		else
		{
			bool nameOk = false;
			for (int i = EditableCanvas.GetComponent<EditableCanvacComponent>().nbViewportContainer; !nameOk; i++)
			{
				// Si le nom n'est pas déjà utilisé on nomme le nouveau container de cette façon
				if (!nameContainerUsed("Script" + i))
				{
					cloneContainer.GetComponentInChildren<UIRootContainer>().associedAgentName = "Script" + i;
					// On affiche le bon nom sur le container
					cloneContainer.GetComponentInChildren<TMP_InputField>().text = "Script" + i;
					nameOk = true;
				}
			}
			MainLoop.instance.StartCoroutine(tcheckLinkName());
		}

		// Si on est en mode Lock, on bloque l'édition et on interdit de supprimer le script
		if (editState == AgentEdit.EditMode.Locked)
		{
			cloneContainer.GetComponentInChildren<TMP_InputField>().interactable = false;
			cloneContainer.transform.Find("ScriptContainer").Find("Header").Find("CloseButton").GetComponent<Button>().interactable = false;
		}

		// ajout du script par défaut
		GameObject dropArea = cloneContainer.GetComponentInChildren<ReplacementSlot>().gameObject;
		if (script != null && dropArea != null)
			for (int k = 0; k < script.Count; k++)
			{
				DragDropSystem.instance.addItemOnDropArea(script[k], dropArea);
				// refresh all the hierarchy of parent containers
				DragDropSystem.instance.refreshHierarchyContainers(dropArea);
			}

		// On ajoute le nouveau viewport container à FYFY
		GameObjectManager.bind(cloneContainer);

		// Update size of parent GameObject
		MainLoop.instance.StartCoroutine(setEditableSize());
	}

	public IEnumerator setEditableSize()
    {
		yield return null;
		yield return null;
		RectTransform editableContainers = (RectTransform)EditableCanvas.transform.Find("EditableContainers");
		// Resolve bug when creating the first editable component, it is the child of the verticalLayout but not included inside!!!
		// We just disable and enable it and force update rect
		if (editableContainers.childCount > 0)
		{
			editableContainers.GetChild(0).gameObject.SetActive(false);
			editableContainers.GetChild(0).gameObject.SetActive(true);
		}
		editableContainers.ForceUpdateRectTransforms();
		yield return null;
		// compute new size
		((RectTransform)EditableCanvas.transform.parent).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(215, editableContainers.rect.width));
	}

	public void removeContainer(GameObject container)
    {
		GameObjectManager.unbind(container);
		Object.Destroy(container);
		// Update size of parent GameObject
		MainLoop.instance.StartCoroutine(setEditableSize());
	}

	public void selectContainer(UIRootContainer container)
    {
		containerSelected = container;
	}

	public UIRootContainer selectContainerByName(string name)
	{
		foreach (GameObject container in scriptContainer_f)
		{
			UIRootContainer uiContainer = container.GetComponent<UIRootContainer>();
			if (uiContainer.associedAgentName == name)
				return uiContainer;
		}

		return null;
	}


	// Vérifie si le nom proposé existe déjà ou non pour un script container
	public bool nameContainerUsed(string nameTested) {
		// On regarde en premier lieu si le nom n'existe pas déjà
		foreach (GameObject container in scriptContainer_f)
			if (container.GetComponent<UIRootContainer>().associedAgentName == nameTested)
				return true;

		return false;
	}
	
	// Change le nom du container
	public void newNameContainer(string newName)
	{
		string oldName = containerSelected.associedAgentName;
		if (oldName != newName)
		{
			// Si le nom n'est pas utilisé
			if (!nameContainerUsed(newName))
			{
				// On tente de récupérer un agent lié à l'ancien nom
				AgentEdit linkedAgent = EditAgentSystem.instance.selectLinkedAgentByName(oldName);
				// Si l'agent existe, on met à jour son lien (on supprime le lien actuelle)
				if (linkedAgent)
					EditAgentSystem.instance.setAgentName(newName);
				// On change pour son nouveau nom
				containerSelected.associedAgentName = newName;
				containerSelected.transform.Find("ContainerName").GetComponent<TMP_InputField>().text = newName;
			}
			else
			{ // Sinon on annule le changement
				containerSelected.transform.Find("ContainerName").GetComponent<TMP_InputField>().text = oldName;
			}
		}
		MainLoop.instance.StartCoroutine(tcheckLinkName());
	}

	// Utilisé surtout par les appels extérieurs au systéme
	// Permet d'enregistrer le nom du container que l'on veux changé
	// Et lui changer son nom 
	public void setContainerName(string oldName, string newName)
	{
		UIRootContainer uiContainer = selectContainerByName(oldName);
		if (uiContainer != null)
		{
			containerSelected = uiContainer;
			newNameContainer(newName);
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