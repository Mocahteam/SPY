using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using System.Collections.Generic;

public class UISystem : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	private GameObject actionContainer;
	private Family PointedGO = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(ElementToDrag)));
	private Family UIScriptPointedGO = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(UIActionType)));

	private Family ContainersGO = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(UITypeContainer)));
	private Family ContainerRefreshGO = FamilyManager.getFamily(new AllOfComponents(typeof(UITypeContainer)));
	private GameObject itemDragged;
	private GameObject positionBar;
	private GameData gameData;
	private GameObject endPanel;
	private GameObject dialogPanel;
	private int nDialog = 0;

	public UISystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		gameData.ButtonExec = GameObject.Find("ExecuteButton");
		gameData.ButtonReset = GameObject.Find("ResetButton");
		positionBar = GameObject.Find("PositionBar");
		positionBar.SetActive(false);
		endPanel = GameObject.Find("EndPanel");
		endPanel.SetActive(false);
		dialogPanel = GameObject.Find("DialogPanel");
		dialogPanel.SetActive(false);
	}
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {

		//Activate EndPanel
		if(gameData.endLevel != 0 && !endPanel.activeSelf){
			endPanel.SetActive(true);
			switch(gameData.endLevel){
				case 1:
					endPanel.transform.GetChild(0).GetComponent<Text>().text = "Vous avez été repéré !";
					endPanel.transform.GetChild(3).gameObject.SetActive(false);
					break;
				case 2:
					endPanel.transform.GetChild(0).GetComponent<Text>().text = "Bravo vous avez gagné !";
					break;
			}
		}

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

		//Drag
		foreach( GameObject go in PointedGO){
			if(Input.GetMouseButtonDown(0)){
				itemDragged = Object.Instantiate<GameObject>(go.GetComponent<ElementToDrag>().actionPrefab, go.transform);
				GameObjectManager.bind(itemDragged);
				itemDragged.GetComponent<Image>().raycastTarget = false;
				break;
			}
		}

		if(itemDragged != null){
			itemDragged.transform.position = Input.mousePosition;
		}

		//Find the container with the last layer 
		GameObject priority = null;
		foreach( GameObject go in ContainersGO){
			if(priority == null || priority.GetComponent<UITypeContainer>().layer < go.GetComponent<UITypeContainer>().layer)
				priority = go;
			
		}

		//PositionBar positioning
		if(priority && itemDragged){
			int start = 0;
			if(priority.GetComponent<UITypeContainer>().type == UITypeContainer.Type.For){
				start++;
			}

			positionBar.SetActive(true);
			positionBar.transform.SetParent(priority.transform);
			positionBar.transform.SetSiblingIndex(priority.transform.childCount-1);
			for(int i = start; i < priority.transform.childCount; i++){
				if(priority.transform.GetChild(i).gameObject != positionBar && Input.mousePosition.y > priority.transform.GetChild(i).position.y){
					positionBar.transform.SetSiblingIndex(i);
					break;
				}
			}
		}
		else{
			positionBar.transform.SetParent(GameObject.Find("PlayerScript").transform);
			positionBar.SetActive(false);
		}

		if(!itemDragged && Input.GetMouseButtonUp(1)){
			priority = null;
			foreach(GameObject go in UIScriptPointedGO){
				if(!priority|| !go.GetComponent<UITypeContainer>() || priority.GetComponent<UITypeContainer>().layer < go.GetComponent<UITypeContainer>().layer){
					priority = go;
				}
				
			}
			if(priority){
				destroyScript(priority.transform);
			}
			priority = null;
		}

		//Drop
		if(Input.GetMouseButtonUp(0)){
			if(priority != null && itemDragged != null){
				itemDragged.transform.SetParent(priority.transform);
				itemDragged.transform.SetSiblingIndex(positionBar.transform.GetSiblingIndex());
				itemDragged.GetComponent<Image>().raycastTarget = true;
				if(itemDragged.GetComponent<UITypeContainer>() != null){
					itemDragged.GetComponent<Image>().raycastTarget = true;
					itemDragged.GetComponent<UITypeContainer>().layer = priority.GetComponent<UITypeContainer>().layer + 1;
				}
				refreshUI();
			}
			else if( itemDragged != null){
				itemDragged.transform.DetachChildren();
				GameObjectManager.unbind(itemDragged);
				Object.Destroy(itemDragged);
				
			}
			itemDragged = null;
		}
	}

	//Refresh Containers size
	private void refreshUI(){
		foreach( GameObject go in ContainerRefreshGO){
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)go.transform );
		}
		
	}

	//Empty the script window
	public void resetScript(){
		GameObject go = GameObject.Find("ScriptContainer");
		for(int i = 0; i < go.transform.childCount; i++){
			destroyScript(go.transform.GetChild(i));
		}
		refreshUI();
	}

	//Recursive script destroyer
	private void destroyScript(Transform go){
		if(go.gameObject.GetComponent<UITypeContainer>() != null){
			for(int i = 0; i < go.childCount; i++){
				destroyScript(go.GetChild(i));
			}
		}
		go.transform.DetachChildren();
		GameObjectManager.unbind(go.gameObject);
		Object.Destroy(go.gameObject);
	}

	public void showDialogPanel(){
		dialogPanel.SetActive(true);
		nDialog = 0;
		dialogPanel.transform.GetChild(0).GetComponent<Text>().text = gameData.dialogMessage[0];
		if(gameData.dialogMessage.Count > 1){
			dialogPanel.transform.GetChild(1).gameObject.SetActive(false);
			dialogPanel.transform.GetChild(2).gameObject.SetActive(true);
		}
		else{
			dialogPanel.transform.GetChild(1).gameObject.SetActive(true);
			dialogPanel.transform.GetChild(2).gameObject.SetActive(false);
		}
	}

	public void nextDialog(){
		nDialog++;
		dialogPanel.transform.GetChild(0).GetComponent<Text>().text = gameData.dialogMessage[nDialog];
		if(nDialog + 1 < gameData.dialogMessage.Count){
			dialogPanel.transform.GetChild(1).gameObject.SetActive(false);
			dialogPanel.transform.GetChild(2).gameObject.SetActive(true);
		}
		else{
			dialogPanel.transform.GetChild(1).gameObject.SetActive(true);
			dialogPanel.transform.GetChild(2).gameObject.SetActive(false);
		}
	}

	public void closeDialogPanel(){
		nDialog = 0;
		gameData.dialogMessage = new List<string>();;
		dialogPanel.SetActive(false);
	}
}