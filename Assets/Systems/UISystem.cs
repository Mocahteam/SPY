using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

public class UISystem : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	//private GameObject actionContainer;
    private Family requireEndPanel = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family displayedEndPanel = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd), typeof(AudioSource)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));
	private Family editableScriptContainer = FamilyManager.getFamily(new AllOfComponents(typeof(UITypeContainer), typeof(VerticalLayoutGroup), typeof(CanvasRenderer), typeof(PointerSensitive)));
	private Family agentCanvas = FamilyManager.getFamily(new AllOfComponents(typeof(HorizontalLayoutGroup), typeof(CanvasRenderer)), new NoneOfComponents(typeof(Image)));
	private Family actions = FamilyManager.getFamily(new AllOfComponents(typeof(PointerSensitive), typeof(UIActionType)));
    private Family currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(BasicAction),typeof(UIActionType), typeof(CurrentAction)));
	private Family actionsPanel = FamilyManager.getFamily(new AllOfComponents(typeof(VerticalLayoutGroup), typeof(Image)));
	private Family newEnd_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
	private Family resetBlocLimit_f = FamilyManager.getFamily(new AllOfComponents(typeof(ResetBlocLimit)));
	private Family scriptIsRunning = FamilyManager.getFamily(new AllOfComponents(typeof(PlayerIsMoving)));
	private Family emptyPlayerExecution = FamilyManager.getFamily(new AllOfComponents(typeof(EmptyExecution))); 
	private GameData gameData;
	private GameObject dialogPanel;
	private int nDialog = 0;
	private GameObject buttonPlay;
	private GameObject buttonContinue;
	private GameObject buttonStop;
	private GameObject buttonReset;
	private GameObject buttonPause;
	private GameObject buttonStep;
	private GameObject lastEditedScript;
	private GameObject endPanel;
	private GameObject executionCanvas;

	public UISystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();

		executionCanvas = GameObject.Find("ExecutionCanvas");
		buttonPlay = executionCanvas.transform.Find("ExecuteButton").gameObject;
		buttonContinue = executionCanvas.transform.Find("ContinueButton").gameObject;
		buttonStop = executionCanvas.transform.Find("StopButton").gameObject;
		buttonPause = executionCanvas.transform.Find("PauseButton").gameObject;
		buttonStep = executionCanvas.transform.Find("NextStepButton").gameObject;

		buttonReset = GameObject.Find("ResetButton");
		endPanel = GameObject.Find("EndPanel");
		GameObjectManager.setGameObjectState(endPanel.transform.parent.gameObject, false);
		dialogPanel = GameObject.Find("DialogPanel");
		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, false);
        requireEndPanel.addEntryCallback(displayEndPanel);
        displayedEndPanel.addEntryCallback(onDisplayedEndPanel);
		actions.addEntryCallback(linkTo);
		newEnd_f.addEntryCallback(levelFinished);
		resetBlocLimit_f.addEntryCallback(delegate(GameObject go){destroyScript(go, true);});
		//scriptIsRunning.addEntryCallback(delegate{setExecutionState(false);});
		scriptIsRunning.addExitCallback(delegate{setExecutionState(true);});
		scriptIsRunning.addExitCallback(saveHistory);
		//0 execution on firstaction
		emptyPlayerExecution.addEntryCallback(delegate{setExecutionState(true);});
		emptyPlayerExecution.addEntryCallback(delegate{GameObjectManager.removeComponent<EmptyExecution>(MainLoop.instance.gameObject);});
		lastEditedScript = null;

		loadHistory();
    }
	private void setExecutionState(bool finished){
		Debug.Log("setExecutionState "+finished);
		buttonReset.GetComponent<Button>().interactable = finished;
		
		GameObjectManager.setGameObjectState(buttonPlay, finished);
		GameObjectManager.setGameObjectState(buttonContinue, !finished);
		GameObjectManager.setGameObjectState(buttonStop, !finished);
		GameObjectManager.setGameObjectState(buttonPause, !finished);
		GameObjectManager.setGameObjectState(buttonStep, !finished);

		GameObjectManager.setGameObjectState(actionsPanel.First(), finished);
		//editable canvas
		GameObjectManager.setGameObjectState(editableScriptContainer.First().transform.parent.parent.gameObject, finished);
	}
	
	private void saveHistory(int unused = 0){
		if(gameData.actionsHistory == null){
			gameData.actionsHistory = lastEditedScript;
		}
		else{
			foreach(Transform child in lastEditedScript.transform){
				Transform copy = UnityEngine.GameObject.Instantiate(child);
				Debug.Log("copy = "+copy.name);
				Debug.Log("actionsHistory = "+gameData.actionsHistory.name);
				Debug.Log("child = "+child.name);
				copy.SetParent(gameData.actionsHistory.transform);
				GameObjectManager.bind(copy.gameObject);				
			}
			GameObjectManager.refresh(gameData.actionsHistory);
		}	
	}

	private void loadHistory(){
		if(gameData.actionsHistory != null){
			GameObject editableCanvas = editableScriptContainer.First();
			for(int i = 0 ; i < gameData.actionsHistory.transform.childCount ; i++){
				Transform child = UnityEngine.GameObject.Instantiate(gameData.actionsHistory.transform.GetChild(i));
				//GameObjectManager.setGameObjectParent(child.gameObject, editableCanvas, true);
				//child.gameObject.AddComponent<Dropped>();
				child.SetParent(editableCanvas.transform);
				GameObjectManager.bind(child.gameObject);
				GameObjectManager.refresh(editableCanvas);
			}
			addNext(gameData.actionsHistory);
			foreach(BaseElement act in editableCanvas.GetComponentsInChildren<BaseElement>()){
				GameObjectManager.addComponent<Dropped>(act.gameObject);
			}
			//destroy history
			//GameObjectManager.unbind(gameData.actionsHistory);
			GameObject.Destroy(gameData.actionsHistory);
			LayoutRebuilder.ForceRebuildLayoutImmediate(editableCanvas.GetComponent<RectTransform>());
		}
		//Canvas.ForceUpdateCanvases();
		
	}

	private void restoreLastEditedScript(){
		List<Transform> childrenList = new List<Transform>();
		foreach(Transform child in lastEditedScript.transform){
			childrenList.Add(child);
		}
		GameObject container = editableScriptContainer.First();
		foreach(Transform child in childrenList){
			//Debug.Log(child.name);
			child.SetParent(container.transform);
			GameObjectManager.bind(child.gameObject);
		}
		GameObjectManager.refresh(container);
	}

	private void levelFinished (GameObject go){
		setExecutionState(true);
		if(go.GetComponent<NewEnd>().endType == NewEnd.Win){
			Debug.Log("level finished avant");
			//saveHistory();
			loadHistory();
			Debug.Log("level finished après");
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
                endPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Vous avez été repéré !";
                GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
                endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/LoseSound") as AudioClip;
                endPanel.GetComponent<AudioSource>().loop = true;
                endPanel.GetComponent<AudioSource>().Play();
                break;
            case 2:
                //endPanel.transform.GetChild(0).GetComponent<Text>().text = "Bravo vous avez gagné !\n Nombre d'instructions: "+ 
                //gameData.totalActionBloc + "\nNombre d'étape: " + gameData.totalStep +"\nPièces récoltées:" + gameData.totalCoin;

                endPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Bravo vous avez gagné !\nScore: " + (10000 / (gameData.totalActionBloc + 1) + 5000 / (gameData.totalStep + 1) + 6000 / (gameData.totalExecute + 1) + 5000 * gameData.totalCoin);
                endPanel.GetComponent<AudioSource>().clip = Resources.Load("Sound/VictorySound") as AudioClip;
                endPanel.GetComponent<AudioSource>().loop = false;
                endPanel.GetComponent<AudioSource>().Play();
				GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, true);
                //End
                if (gameData.levelToLoad >= gameData.levelList.Count - 1)
                {
                    GameObjectManager.setGameObjectState(endPanel.transform.Find("NextLevel").gameObject, false);
					GameObjectManager.setGameObjectState(endPanel.transform.Find("ReloadState").gameObject, false);
                }
                break;
        }
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {

		//Activate DialogPanel if there is a message
		if(gameData.dialogMessage.Count > 0 && !dialogPanel.transform.parent.gameObject.activeSelf){
			showDialogPanel();
		}

	}

	//Refresh Containers size
	private void refreshUI(){
		foreach( GameObject go in editableScriptContainer){
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)go.transform );
		}
		
	}

	//Empty the script window
	public void resetScript(){
		GameObject editableContainer = editableScriptContainer.First();
		/*
		foreach(BaseElement go in editableContainer.GetComponentsInChildren<BaseElement>()){
			Debug.Log("baseelement go = "+ go.name);
			destroyScript(go.gameObject);
		}*/
		for (int i = 0 ; i < editableContainer.transform.childCount ; i++){
			if(editableContainer.transform.GetChild(i).GetComponent<BaseElement>()){
				//Debug.Log("baseelement go = "+ editableContainer.transform.GetChild(i).name);
				destroyScript(editableContainer.transform.GetChild(i).gameObject);				
			}
		
		}
		refreshUI();
	}

	//Recursive script destroyer  bool refund = false
	private void destroyScript(GameObject go,  bool refund = false){
		//refund blocActionLimit
		//if(refund && go.gameObject.GetComponent<UIActionType>() != null){
			//GameObjectManager.removeComponent<Dropped>(go.gameObject);
			//Object.Destroy(go.GetComponent<Available>());
			//ActionManipulator.updateActionBlocLimit(gameData, go.gameObject.GetComponent<UIActionType>().type, 1);
		//}
		if(go.GetComponent<UIActionType>() != null){
			if(!refund)
				gameData.totalActionBloc++;
			else
				GameObjectManager.addComponent<AddOne>(go.GetComponent<UIActionType>().linkedTo);
		}
		
		if(go.GetComponent<UITypeContainer>() != null){
			/*
			for(int i = 0; i < go.transform.childCount; i++){
				destroyScript(go.transform.GetChild(i).gameObject, refund);
			}*/
			foreach(Transform child in go.transform){
				if(child.GetComponent<BaseElement>()){
					//Debug.Log("child of uitypecontainer = "+child.name);
					destroyScript(child.gameObject, refund);
				}
			}
		}
		/*
		for(int i = 0; i < go.transform.childCount;i++){
			UnityEngine.Object.Destroy(go.transform.GetChild(i).gameObject);
		}*/
		go.transform.DetachChildren();
		GameObjectManager.unbind(go);
		UnityEngine.Object.Destroy(go);
	}

	public void showDialogPanel(){
		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, true);
		nDialog = 0;
		dialogPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = gameData.dialogMessage[0];
		if(gameData.dialogMessage.Count > 1){
			setActiveOKButton(false);
			setActiveNextButton(true);
		}
		else{
			setActiveOKButton(true);
			setActiveNextButton(false);
		}
	}

	public void nextDialog(){
		nDialog++;
		dialogPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = gameData.dialogMessage[nDialog];
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
		GameObjectManager.setGameObjectState(dialogPanel.transform.GetChild(1).gameObject, active);
	}

	public void setActiveNextButton(bool active){
		GameObjectManager.setGameObjectState(dialogPanel.transform.GetChild(2).gameObject, active);
	}

	public void closeDialogPanel(){
		nDialog = 0;
		gameData.dialogMessage = new List<string>();;
		GameObjectManager.setGameObjectState(dialogPanel.transform.parent.gameObject, false);
	}

	public void reloadScene(){
		gameData.totalActionBloc = 0;
		gameData.totalStep = 0;
		gameData.totalExecute = 0;
		gameData.totalCoin = 0;
		gameData.dialogMessage = new List<string>();
		GameObjectManager.loadScene("MainScene");
		Debug.Log("reload");
	}

	public void returnToTitleScreen(){
		gameData.totalActionBloc = 0;
		gameData.totalStep = 0;
		gameData.totalExecute = 0;
		gameData.totalCoin = 0;
		gameData.dialogMessage = new List<string>();
		gameData.actionsHistory = null;
		GameObjectManager.loadScene("TitleScreen");
	}

	public void nextLevel(){
		gameData.levelToLoad++;
		reloadScene();
		gameData.actionsHistory = null;
	}

	public void retry(){
		gameData.totalActionBloc = 0;
		gameData.totalStep = 0;
		gameData.totalExecute = 0;
		gameData.totalCoin = 0;
		
		gameData.dialogMessage = new List<string>();
		if(gameData.actionsHistory != null)
			UnityEngine.Object.DontDestroyOnLoad(gameData.actionsHistory);
		GameObjectManager.loadScene("MainScene");
	}

	public void reloadState(){
		GameObjectManager.removeComponent<NewEnd>(endPanel);
	}

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

	public void applyScriptToPlayer(){
		//if first click on play button
		if(!buttonStop.activeInHierarchy){
			//hide panels
			GameObjectManager.setGameObjectState(actionsPanel.First(), false);
			//editable canvas
			GameObjectManager.setGameObjectState(editableScriptContainer.First().transform.parent.parent.gameObject, false);
			//clean container for each robot
			foreach(GameObject robot in playerGO){
				foreach(Transform child in robot.GetComponent<ScriptRef>().scriptContainer.transform){
					GameObjectManager.unbind(child.gameObject);
					GameObject.Destroy(child.gameObject);
				}
			}
			
			//copy editable script
			lastEditedScript = GameObject.Instantiate(editableScriptContainer.First());
			foreach(Transform child in lastEditedScript.transform){
				if(child.name.Contains("PositionBar")){
					UnityEngine.GameObject.Destroy(child.gameObject);
				}
			}

			GameObject containerCopy = CopyActionsFrom(editableScriptContainer.First(), false);

			/*
			foreach(Transform notgo in agentCanvas.First().transform){
				GameObjectManager.setGameObjectState(notgo.gameObject, false);
			}
			*/
			
			foreach( GameObject go in playerGO){
				GameObject targetContainer = go.GetComponent<ScriptRef>().scriptContainer;
				GameObjectManager.setGameObjectState(go.GetComponent<ScriptRef>().uiContainer, true);
				go.GetComponent<ScriptRef>().uiContainer.transform.Find("Toggle").GetComponent<Toggle>().isOn = true;	
				for(int i = 0 ; i < containerCopy.transform.childCount ; i++){
					if(!containerCopy.transform.GetChild(i).name.Contains("PositionBar")){
						Transform child = UnityEngine.GameObject.Instantiate(containerCopy.transform.GetChild(i));
						child.SetParent(targetContainer.transform);
						GameObjectManager.bind(child.gameObject);
						GameObjectManager.refresh(targetContainer);
					}

				}
				addNext(targetContainer);
			}

			UnityEngine.Object.Destroy(containerCopy);

			//empty editable container
			resetScript();

			buttonPlay.GetComponent<AudioSource>().Play();
		}

	}

    public GameObject CopyActionsFrom(GameObject container, bool isInteractable){
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
		foreach(LoopAction loopAct in copyGO.GetComponentsInChildren<LoopAction>()){
			foreach(BaseElement act in loopAct.GetComponentsInChildren<BaseElement>()){
				if(!act.Equals(loopAct)){
					loopAct.firstChild = act.gameObject;
					break;
				}
			}
		}
		foreach(IfAction IfAct in copyGO.GetComponentsInChildren<IfAction>()){
			Debug.Log("childoutofbounds "+IfAct.gameObject.name);
			Debug.Log("childoutofbounds "+IfAct.transform.GetChild(0).name);
			Debug.Log("childoutofbounds "+IfAct.transform.GetChild(0).Find("DropdownEntityType").name);
			Debug.Log("childoutofbounds "+IfAct.transform.GetChild(0).Find("DropdownEntityType").GetComponent<TMP_Dropdown>().name);
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


		return copyGO;
	}

	private void addNext(GameObject container){
		for(int i = 0 ; i < container.transform.childCount ; i++){
			Transform child = container.transform.GetChild(i);
			if(i < container.transform.childCount-1 && child.GetComponent<BaseElement>()){
				child.GetComponent<BaseElement>().next = container.transform.GetChild(i+1).gameObject;
			}
			else if(i == container.transform.childCount-1 && child.GetComponent<BaseElement>() && container.GetComponent<BaseElement>()){
				if(container.GetComponent<ForAction>() || container.GetComponent<LoopAction>()){
					child.GetComponent<BaseElement>().next = container;
				}
				else if(container.GetComponent<IfAction>()){
					child.GetComponent<BaseElement>().next = container.GetComponent<BaseElement>().next;
				}
				
			}
			//if or for action
			if(child.GetComponent<IfAction>() || child.GetComponent<ForAction>() || child.GetComponent<LoopAction>()){
				addNext(child.gameObject);
			}
		}
	}

	
}