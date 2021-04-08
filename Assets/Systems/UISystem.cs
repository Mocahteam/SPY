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

}