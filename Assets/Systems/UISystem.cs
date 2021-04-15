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
	private GameObject actionContainer;
    private Family requireEndPanel = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)), new NoneOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF));
    private Family displayedEndPanel = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd), typeof(AudioSource)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family playerScript = FamilyManager.getFamily(new AllOfComponents(typeof(UITypeContainer)), new AnyOfTags("ScriptConstructor"));
	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));
	private Family editableScriptContainer = FamilyManager.getFamily(new AllOfComponents(typeof(VerticalLayoutGroup), typeof(CanvasRenderer), typeof(PointerSensitive)));
	private Family agentCanvas = FamilyManager.getFamily(new AllOfComponents(typeof(HorizontalLayoutGroup), typeof(CanvasRenderer)), new NoneOfComponents(typeof(Image)));
	private GameData gameData;
	private GameObject dialogPanel;
	private int nDialog = 0;

	public UISystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		gameData.ButtonExec = GameObject.Find("ExecuteButton");
		gameData.ButtonReset = GameObject.Find("ResetButton");
		GameObject endPanel = GameObject.Find("EndPanel");
		GameObjectManager.setGameObjectState(endPanel, false);
		dialogPanel = GameObject.Find("DialogPanel");
		GameObjectManager.setGameObjectState(dialogPanel, false);
        requireEndPanel.addEntryCallback(displayEndPanel);
        displayedEndPanel.addEntryCallback(onDisplayedEndPanel);
    }

    private void displayEndPanel(GameObject endPanel)
    {
        GameObjectManager.setGameObjectState(endPanel, true);
    }

    private void onDisplayedEndPanel (GameObject endPanel)
    { 
        switch (endPanel.GetComponent<NewEnd>().endType)
        {
            case 1:
                endPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Vous avez été repéré !";
                GameObjectManager.setGameObjectState(endPanel.transform.GetChild(3).gameObject, false);
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
                //End
                if (gameData.levelToLoad >= gameData.levelList.Count - 1)
                {
                    GameObjectManager.setGameObjectState(endPanel.transform.GetChild(3).gameObject, false);
                }
                break;
        }
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {

		//Activate DialogPanel if there is a message
		if(gameData.dialogMessage.Count > 0 && !dialogPanel.activeSelf){
			showDialogPanel();
		}

		//Desactivate Execute & ResetButton if there is a script running
		if(gameData.nbStep>0){
			gameData.ButtonExec.GetComponent<Button>().interactable = false;
			gameData.ButtonReset.GetComponent<Button>().interactable = false;
		}
		else{
			gameData.ButtonExec.GetComponent<Button>().interactable = true;
			gameData.ButtonReset.GetComponent<Button>().interactable = true;
		}

	}

	//Refresh Containers size
	private void refreshUI(){
		foreach( GameObject go in playerScript){
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)go.transform );
		}
		
	}

	//Empty the script window
	public void resetScript(){
		GameObject go = GameObject.Find("ScriptContainer");
		for(int i = 0; i < go.transform.childCount; i++){
			destroyScript(go.transform.GetChild(i), true);
		}
		refreshUI();
	}

	public void resetScriptNoRefund(){
		GameObject go = playerScript.First();
		//add actions to history before destroy
		/*
		List<Action> lastActions = new List<Action>();
		lastActions = ActionManipulator.ScriptContainerToActionList(go);
		foreach(Action action in lastActions){
			gameData.actionsHistory.Add(action);
		}
		*/
		//destroy script
		for(int i = 0; i < go.transform.childCount; i++){
			destroyScript(go.transform.GetChild(i));
		}
		gameData.ButtonExec.GetComponent<AudioSource>().Play();
		refreshUI();
	}

	//Recursive script destroyer
	private void destroyScript(Transform go, bool refund = false){
		//refund blocActionLimit
		if(refund && go.gameObject.GetComponent<UIActionType>() != null){
			//GameObjectManager.removeComponent<Dropped>(go.gameObject);
			//Object.Destroy(go.GetComponent<Available>());
			//ActionManipulator.updateActionBlocLimit(gameData, go.gameObject.GetComponent<UIActionType>().type, 1);
		}
		else if(go.gameObject.GetComponent<UIActionType>() != null){
			gameData.totalActionBloc++;
		}
		
		if(go.gameObject.GetComponent<UITypeContainer>() != null){
			for(int i = 0; i < go.childCount; i++){
				destroyScript(go.GetChild(i));
			}
		}
		for(int i = 0; i < go.transform.childCount;i++){
			UnityEngine.Object.Destroy(go.transform.GetChild(i).gameObject);
		}
		go.transform.DetachChildren();
		GameObjectManager.unbind(go.gameObject);
		UnityEngine.Object.Destroy(go.gameObject);
	}

	public void showDialogPanel(){
		GameObjectManager.setGameObjectState(dialogPanel, true);
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
		GameObjectManager.setGameObjectState(dialogPanel, false);
	}

	public void reloadScene(){
		gameData.nbStep = 0;
		gameData.totalActionBloc = 0;
		gameData.totalStep = 0;
		gameData.totalExecute = 0;
		gameData.totalCoin = 0;
		gameData.dialogMessage = new List<string>();
		GameObjectManager.loadScene("MainScene");
		Debug.Log("reload");
	}

	public void returnToTitleScreen(){
		gameData.nbStep = 0;
		gameData.totalActionBloc = 0;
		gameData.totalStep = 0;
		gameData.totalExecute = 0;
		gameData.totalCoin = 0;
		gameData.dialogMessage = new List<string>();
		GameObjectManager.loadScene("TitleScreen");
	}

	public void nextLevel(){
		gameData.levelToLoad++;
		reloadScene();
		//gameData.actionsHistory.Clear();
	}

	public void retry(){
		gameData.nbStep = 0;
		gameData.totalActionBloc = 0;
		gameData.totalStep = 0;
		gameData.totalExecute = 0;
		gameData.totalCoin = 0;
		
		gameData.dialogMessage = new List<string>();
		/*
		List<Action> l = new List<Action>();
		GameObject scriptComposer = GameObject.Find("ScriptContainer");
		l = ActionManipulator.ScriptContainerToActionList(scriptComposer);
		*/
		GameObjectManager.loadScene("MainScene");
		/*
		//get back actions from history
		Script scriptActions = new Script();
		scriptActions.actions = gameData.actionsHistory;
		scriptActions.currentAction = -1;
		scriptActions.repeat = false;
		GameObject scriptComposer = playerScript.First();
		ActionManipulator.DisplayActionsInContainer(gameData.actionsHistory, scriptComposer);
		*/
		//playerGO.First().GetComponent<Script>().actions = gameData.actionsHistory;
		//ActionManipulator.ScriptToContainer(scriptActions, scriptComposer);	
		/*
		Script scriptActions = new Script();
		scriptActions.actions = l;
		scriptActions.currentAction = 0;
		scriptActions.repeat = false;
		ActionManipulator.ScriptToContainer(scriptActions, scriptComposer);
		*/

		//GameObject endpanel = GameObject.Find("EndPanel");
		//GameObjectManager.removeComponent<NewEnd>(endpanel);
		//endpanel.SetActive(false);
	}

	public void applyScriptToPlayer(){
		GameObject container;
		//add highlight to first action
		GameObject firstAction = getFirstActionOf(editableScriptContainer.First());
		if (firstAction != null){
			firstAction.AddComponent<HighLight>();
			//GameObjectManager.addComponent<HighLight>(firstAction);
			Debug.Log("firstAction = "+firstAction.name);
		}
		List<GameObject> actions = CopyActionsFrom(editableScriptContainer.First());
		GameObject duplicate;
		//bool firstActionHighlighted = false;
		//GameObject firstAction;
		foreach(Transform notgo in agentCanvas.First().transform){
			GameObjectManager.setGameObjectState(notgo.gameObject, false);
		}
		foreach( GameObject go in playerGO){
			GameObjectManager.setGameObjectState(go.GetComponent<ScriptRef>().container, true);
			container = go.GetComponent<ScriptRef>().container.transform.Find("Viewport").Find("ScriptContainer").gameObject;
			foreach(GameObject action in actions){
				duplicate = UnityEngine.Object.Instantiate(action);
				duplicate.transform.SetParent(container.transform);
				
				GameObjectManager.bind(duplicate);
				/*
				if(!firstActionHighlighted){
					if(duplicate.GetComponent<BasicAction>())
						firstAction = duplicate;					
					else if (duplicate.GetComponent<ForAction>() && duplicate.GetComponent<ForAction>().firstChild != null)
						firstAction = duplicate.GetComponent<ForAction>().firstChild

					if (firstAction != null){
						GameObjectManager.addComponent<HighLight>(duplicate);
						firstActionHighlighted = true;	
					}
				}*/
			}
			addNext(container);
			//firstActionHighlighted = false;
			//Debug.Log("actions = "+go.GetComponent<Script>().actions);
            //go.GetComponent<ScriptRef>().currentAction = 0;
            //gameData.nbStep = getNbStep(go.GetComponent<ScriptRef>());
		}

		//applyIfEntityType();
		/*
		if(gameData.nbStep > 0){
			gameData.totalExecute++;
		}*/
	}

	public int getNbStep(GameObject go){
		int nbstep = 0;
		GameObject action = go;
			while(action.GetComponent<BaseElement>() && action.GetComponent<BaseElement>().next != null){
				if(go.GetComponent<ForAction>() && go.GetComponent<ForAction>().nbFor != 0 && go.GetComponent<ForAction>().firstChild != null){
					nbstep += getNbStep(go.GetComponent<ForAction>().firstChild)*go.GetComponent<ForAction>().nbFor;
				}
				else if(go.GetComponent<IfAction>()){ //TO DO + ifvalid
					nbstep += getNbStep(go.GetComponent<IfAction>().firstChild);
				}
				else{ //basicaction
					nbstep++;
				}
				action = go.GetComponent<BaseElement>().next;
			}
		return nbstep;
	}

	
	public GameObject getFirstActionOf (GameObject container){
		GameObject res = null;
		if (container.transform.childCount != 0){
			foreach(Transform go in container.transform){
				//BasicAction
				if(go.GetComponent<BasicAction>()){
					res = go.gameObject;
					break;				
				}
				//For
				else if (go.GetComponent<ForAction>() && go.GetComponent<ForAction>().nbFor != 0 && 
				go.GetComponent<ForAction>().firstChild != null){
					if(go.GetComponent<ForAction>().firstChild.GetComponent<BasicAction>()){
						res = go.GetComponent<ForAction>().firstChild;
						break;						
					}
					else if (go.GetComponent<ForAction>().firstChild.GetComponent<ForAction>()){
						res = getFirstActionOf(go.GetComponent<ForAction>().firstChild);
						break;
					}
					else if (go.GetComponent<ForAction>().firstChild.GetComponent<IfAction>()){
						res = getFirstActionOf(go.GetComponent<IfAction>().firstChild);
						break;
					}
				}
				//If
				else if (go.GetComponent<IfAction>() && go.GetComponent<IfAction>().firstChild != null){
					if(go.GetComponent<IfAction>().firstChild.GetComponent<BasicAction>()){
						res = go.GetComponent<IfAction>().firstChild;
						break;						
					}
					else if (go.GetComponent<IfAction>().firstChild.GetComponent<ForAction>()){
						res = getFirstActionOf(go.GetComponent<IfAction>().firstChild);
						break;
					}
					else if (go.GetComponent<IfAction>().firstChild.GetComponent<IfAction>()){
						res = getFirstActionOf(go.GetComponent<IfAction>().firstChild);
						break;
					}
				}
			}
		}
		return res;
	}
    public List<GameObject> CopyActionsFrom(GameObject container){
		Debug.Log("CopyActionsFrom");
		List<GameObject> l = new List<GameObject>();

		for(int i = 0; i< container.transform.childCount; i++){
			GameObject child = container.transform.GetChild(i).gameObject;
			if(child.GetComponent<ForAction>()){
				ForAction forAct = child.GetComponent<ForAction>();
                forAct.nbFor = int.Parse(child.transform.GetChild(0).transform.GetChild(1).GetComponent<TMP_InputField>().text);
				//loop display
				/*
				if (forAct.nbFor > 0)
					child.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor +1).ToString() + " / " + forAct.nbFor.ToString();
				else*/
				child.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
				//not editable for
				child.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().interactable = false;
				//UnityEngine.Object.Destroy(child.GetComponent<UITypeContainer>());
				GameObjectManager.removeComponent<UITypeContainer>(child);
				
				if(child.transform.childCount != 0){
					List<GameObject> forchildren = CopyActionsFrom(child);
					foreach(GameObject forchild in forchildren){
						if (forchild.GetComponent<BaseElement>()){
							forAct.firstChild = forchild;
							Debug.Log("firstchild for = "+forchild.name);
							break;							
						}

					}
				}
					
			}
			else if(child.GetComponent<IfAction>()){
				IfAction IfAct = child.GetComponent<IfAction>();
				IfAct.ifEntityType = child.transform.GetChild(0).Find("DropdownEntityType").GetComponent<TMP_Dropdown>().value;
				IfAct.ifDirection = child.transform.GetChild(0).Find("DropdownDirection").GetComponent<TMP_Dropdown>().value;
				IfAct.range = int.Parse(child.transform.GetChild(0).Find("InputFieldRange").GetComponent<TMP_InputField>().text);
				IfAct.ifNot = (child.transform.GetChild(0).Find("DropdownIsOrIsNot").GetComponent<TMP_Dropdown>().value == 1);
				
				//not editable if
				child.transform.GetChild(0).Find("DropdownEntityType").GetComponent<TMP_Dropdown>().interactable = false;
				child.transform.GetChild(0).Find("DropdownDirection").GetComponent<TMP_Dropdown>().interactable = false;
				child.transform.GetChild(0).Find("InputFieldRange").GetComponent<TMP_InputField>().interactable = false;
				child.transform.GetChild(0).Find("DropdownIsOrIsNot").GetComponent<TMP_Dropdown>().interactable = false;
				//UnityEngine.Object.Destroy(child.GetComponent<UITypeContainer>());
				GameObjectManager.removeComponent<UITypeContainer>(child);

				if(child.transform.childCount != 0){
					List<GameObject> ifchildren = CopyActionsFrom(child);
					foreach(GameObject ifchild in ifchildren){
						if (ifchild.GetComponent<BaseElement>()){
							IfAct.firstChild = ifchild;
							Debug.Log("firstchild if = "+ifchild.name);
							break;							
						}
					}
				}
			}
			l.Add(child); 
			//not editable action
			//UnityEngine.Object.Destroy(child.GetComponent<PointerSensitive>());
			if(child.GetComponent<PointerSensitive>())
				GameObjectManager.removeComponent<PointerSensitive>(child);
		}
		return l;
	}
	private void addNext(GameObject container){
		int i = 1;
		//for each child, next = next child
		foreach(Transform child in container.transform){
			Debug.Log(child.gameObject.name);
			if(i < container.transform.childCount && child.GetComponent<BaseElement>()){
				child.GetComponent<BaseElement>().next = container.transform.GetChild(i).gameObject;
			}
			//if or for action
			if(child.GetComponent<IfAction>() || child.GetComponent<ForAction>())
				addNext(child.gameObject);
			i++;
		}
		//last child's next = parent 
		if(container.transform.childCount != 0 && (container.transform.GetComponent<IfAction>() || container.transform.GetComponent<ForAction>()) &&
		container.transform.GetChild(container.transform.childCount-1).GetComponent<BaseElement>())
			container.transform.GetChild(container.transform.childCount-1).GetComponent<BaseElement>().next = container;		
	}

}