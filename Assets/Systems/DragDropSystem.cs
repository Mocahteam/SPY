using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

public class DragDropSystem : FSystem {
	private Family panelPointedGO = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(ElementToDrag), typeof(Image)));
	private Family playerScriptPointed = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(UITypeContainer)));
	private Family playerScriptPointedGO = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(UIActionType), typeof(Image)));
	private Family playerScript = FamilyManager.getFamily(new AllOfComponents(typeof(UITypeContainer)), new AnyOfTags("ScriptConstructor"));
	private GameObject itemDragged;
	private GameObject positionBar;
	private GameData gameData;
	private List<GameObject> limitTexts;

	public DragDropSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		positionBar = GameObject.Find("PositionBar");
		GameObjectManager.setGameObjectState(positionBar, false);
		//LimitTexts
		limitTexts = new List<GameObject>();
		limitTexts.Add(GameObject.Find("ForwardLimit"));
		limitTexts.Add(GameObject.Find("TurnLeftLimit"));
		limitTexts.Add(GameObject.Find("TurnRightLimit"));
		limitTexts.Add(GameObject.Find("WaitLimit"));
		limitTexts.Add(GameObject.Find("ActivateLimit"));
		limitTexts.Add(GameObject.Find("TurnBackLimit"));
		limitTexts.Add(GameObject.Find("IfLimit"));
		limitTexts.Add(GameObject.Find("ForLimit"));

	}

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		bool isActive;
		//Update LimitText
		for(int i = 0; i < limitTexts.Count; i++){
			if(gameData.actionBlocLimit[i] >= 0){
				isActive = gameData.actionBlocLimit[i] == 0 ? false : true;
				GameObjectManager.setGameObjectState(limitTexts[i].transform.parent.gameObject, isActive);
				limitTexts[i].GetComponent<TextMeshProUGUI>().text = "Reste\n" + gameData.actionBlocLimit[i].ToString();
				//desactivate actionBlocs
				if(gameData.actionBlocLimit[i] == 0){
					if(limitTexts[i].transform.parent.gameObject.GetComponent<Available>() != null)
						GameObjectManager.removeComponent<Available>(limitTexts[i].transform.parent.gameObject);
						//limitTexts[i].transform.parent.GetComponent<Image>().raycastTarget = false;
				}
				else{
					if(limitTexts[i].transform.parent.gameObject.GetComponent<Available>() == null)
						GameObjectManager.addComponent<Available>(limitTexts[i].transform.parent.gameObject);
						//limitTexts[i].transform.parent.GetComponent<Image>().raycastTarget = true;
				}
			}

		}

		//Drag
		if(Input.GetMouseButtonDown(0)){
			BaseElement action = null;
			foreach( GameObject go in panelPointedGO){
				GameObject prefab = go.GetComponent<ElementToDrag>().actionPrefab;
				itemDragged = UnityEngine.Object.Instantiate<GameObject>(prefab, go.transform);
				action = itemDragged.GetComponent<BaseElement>();
				if(action.GetType().ToString().Equals("ForAction")){
						TMP_InputField input = itemDragged.GetComponentInChildren<TMP_InputField>();
						input.onEndEdit.AddListener(delegate{onlyPositiveInteger(input);});
				}

				itemDragged.GetComponent<UIActionType>().prefab = prefab;
				itemDragged.GetComponent<UIActionType>().linkedTo = go;
				itemDragged.GetComponent<UIActionType>().action = action;
				action.target = itemDragged;
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
		foreach( GameObject go in playerScriptPointed){
			if(priority == null || priority.GetComponent<UITypeContainer>().layer < go.GetComponent<UITypeContainer>().layer)
				priority = go;
			
		}

		//PositionBar positioning
		if(priority && itemDragged){
			int start = 0;
			if(priority.GetComponent<UITypeContainer>().type != UITypeContainer.Type.Script){
				start++;
			}
			GameObjectManager.setGameObjectState(positionBar, true);
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
			positionBar.transform.SetParent(GameObject.Find("EditableCanvas").transform);
			GameObjectManager.setGameObjectState(positionBar, false);
		}

		//Delete
		if(!itemDragged && Input.GetMouseButtonUp(1)){
			priority = null;
			foreach(GameObject go in playerScriptPointedGO){
				if(!priority || !go.GetComponent<UITypeContainer>() ||
				priority.GetComponent<UITypeContainer>().layer < go.GetComponent<UITypeContainer>().layer){
				//(priority.GetComponent<UITypeContainer>() != null && priority.GetComponent<UITypeContainer>().layer < go.GetComponent<UITypeContainer>().layer)){
					priority = go;
				}
				
			}
			if(priority){
				destroyScript(priority.transform, true);
			}
			priority = null;
		}


		//Drop
		if(Input.GetMouseButtonUp(0)){
			//Drop in script
			if(priority != null && itemDragged != null){
				itemDragged.transform.SetParent(priority.transform);
				itemDragged.transform.localScale = new Vector3(1,1,1);
				itemDragged.transform.SetSiblingIndex(positionBar.transform.GetSiblingIndex());
				itemDragged.GetComponent<Image>().raycastTarget = true;

				//update limit bloc
				GameObjectManager.addComponent<Dropped>(itemDragged);
				//Object.Destroy(actionGO.GetComponent<Available>());
				//ActionManipulator.updateActionBlocLimit(gameData,itemDragged.GetComponent<UIActionType>().type, -1);

				if(itemDragged.GetComponent<UITypeContainer>() != null){
					itemDragged.GetComponent<Image>().raycastTarget = true;
					itemDragged.GetComponent<UITypeContainer>().layer = priority.GetComponent<UITypeContainer>().layer + 1;
				}
				GameObject.Find("EditableCanvas").GetComponent<AudioSource>().Play();
				refreshUI();
			}
			else if( itemDragged != null){
				
				for(int i = 0; i < itemDragged.transform.childCount;i++){
					UnityEngine.Object.Destroy(itemDragged.transform.GetChild(i).gameObject);
				}
				itemDragged.transform.DetachChildren();
				GameObjectManager.unbind(itemDragged);
				UnityEngine.Object.Destroy(itemDragged);
				
			}
			itemDragged = null;
		}
	}

	public void onlyPositiveInteger(TMP_InputField input){
		Debug.Log(input.text);
		if(Int32.Parse(input.text) < 0 ){
			input.text = "0";
		}
	}

	//Recursive script destroyer
	private void destroyScript(Transform go, bool refund = false){
		//refund blocActionLimit
		if(go.gameObject.GetComponent<UIActionType>() != null){
			//gameData.deletedItemLinkedTo.Add(go.GetComponent<UIActionType>().linkedTo);
			GameObjectManager.addComponent<AddOne>(go.GetComponent<UIActionType>().linkedTo);
			if(!refund)
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

	//Refresh Containers size
	private void refreshUI(){
		foreach( GameObject go in playerScript){
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)go.transform );
		}
		
	}

}