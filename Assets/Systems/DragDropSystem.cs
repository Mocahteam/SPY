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
	private Family tmpPointedGO = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver)), new AnyOfComponents(typeof(TMP_InputField), typeof(TMP_Dropdown)));
	private Family scriptContainers = FamilyManager.getFamily(new AllOfComponents(typeof(UITypeContainer)), new AnyOfTags("ScriptConstructor"));
	private Family editableScriptContainer = FamilyManager.getFamily(new AllOfComponents(typeof(UITypeContainer), typeof(VerticalLayoutGroup), typeof(CanvasRenderer), typeof(PointerSensitive)));
	private Family mainCanvas = FamilyManager.getFamily(new AllOfComponents(typeof(CanvasScaler), typeof(GraphicRaycaster)));
	private GameObject itemDragged;
	private GameObject positionBar;
	private GameData gameData;
	private GameObject editableContainer;
	
	//double click
	private float lastClickTime;
	private float catchTime;
	private bool doubleclick;
	
	public DragDropSystem(){
		catchTime = 0.25f;
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		editableContainer = editableScriptContainer.First();
		positionBar = editableContainer.transform.parent.Find("PositionBar").gameObject;
		GameObjectManager.setGameObjectState(positionBar, false);

		/*
		limitTexts = new List<GameObject>();
		limitTexts.Add(GameObject.Find("ForwardLimit"));
		limitTexts.Add(GameObject.Find("TurnLeftLimit"));
		limitTexts.Add(GameObject.Find("TurnRightLimit"));
		limitTexts.Add(GameObject.Find("WaitLimit"));
		limitTexts.Add(GameObject.Find("ActivateLimit"));
		limitTexts.Add(GameObject.Find("TurnBackLimit"));
		limitTexts.Add(GameObject.Find("IfLimit"));
		limitTexts.Add(GameObject.Find("ForLimit"));
		*/
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
		//Drag
		if(Input.GetMouseButtonDown(0) && !Input.GetMouseButtonUp(0)){ //focus in play mode (unity editor) could be up and down !!! (bug unity)
			//one click
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
				//itemDragged.GetComponent<UIActionType>().action = action;
				action.target = itemDragged;
				GameObjectManager.bind(itemDragged);
				GameObjectManager.addComponent<Dragged>(itemDragged);
				itemDragged.GetComponent<Image>().raycastTarget = false;
				break;
			}

			//double click
			if(itemDragged != null && Time.time - lastClickTime < catchTime)
			{
				Debug.Log("Double click");
				doubleclick = true;

				//drop in editable container
				itemDragged.transform.SetParent(editableContainer.transform);
				itemDragged.transform.localScale = new Vector3(1,1,1);
				//itemDragged.transform.SetSiblingIndex(positionBar.transform.GetSiblingIndex());
				itemDragged.GetComponent<Image>().raycastTarget = true;

				//update limit bloc
				GameObjectManager.addComponent<Dropped>(itemDragged);
				GameObjectManager.removeComponent<Dragged>(itemDragged);

				if(itemDragged.GetComponent<UITypeContainer>()){
					itemDragged.GetComponent<Image>().raycastTarget = true;
					itemDragged.GetComponent<UITypeContainer>().layer = 1;
				}
				editableContainer.transform.parent.parent.GetComponent<AudioSource>().Play();
			}

			//drag in editable script
			if(!doubleclick && itemDragged == null && tmpPointedGO.Count == 0){ //cannot drag if inputfield or dropdown pointed
				GameObject actionPriority = null;
				foreach(GameObject go in playerScriptPointedGO){
					if(actionPriority == null || !go.GetComponent<UITypeContainer>() ||
					 actionPriority.GetComponent<UITypeContainer>().layer < go.GetComponent<UITypeContainer>().layer){
						actionPriority = go;
					}
				}
				if(actionPriority != null){
					itemDragged = actionPriority;
					actionPriority.transform.SetParent(mainCanvas.First().transform);
					itemDragged.transform.localScale = new Vector3(0.8f,0.8f,0.8f);
					GameObjectManager.addComponent<Dragged>(itemDragged);
					itemDragged.GetComponent<Image>().raycastTarget = false;
					GameObjectManager.addComponent<AddOne>(itemDragged);
					editableContainer.transform.parent.GetComponentInParent<ScrollRect>().enabled = false;				
				}

			}

			lastClickTime = Time.time;
		}
		
		if(!doubleclick && itemDragged != null){
			itemDragged.transform.position = Input.mousePosition;
		}

		//Find the container with the last layer 
		GameObject priority = null;
		foreach( GameObject go in playerScriptPointed){
			if(priority == null || priority.GetComponent<UITypeContainer>().layer < go.GetComponent<UITypeContainer>().layer)
				priority = go;
			
		}

		//PositionBar positioning
		if(!doubleclick){
			if(priority && itemDragged){
				int start = 0;
				if(priority.GetComponent<ForAction>() || priority.GetComponent<IfAction>()){
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
				positionBar.transform.SetParent(editableContainer.transform.parent);
				GameObjectManager.setGameObjectState(positionBar, false);
			}			
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
				GameObjectManager.addComponent<ResetBlocLimit>(priority);
				//destroyScript(priority.transform, true);
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
				if(!doubleclick){
					GameObjectManager.addComponent<Dropped>(itemDragged);
					GameObjectManager.removeComponent<Dragged>(itemDragged);					
				}

				if(itemDragged.GetComponent<UITypeContainer>()){
					itemDragged.GetComponent<Image>().raycastTarget = true;
					itemDragged.GetComponent<UITypeContainer>().layer = priority.GetComponent<UITypeContainer>().layer + 1;
				}
				editableContainer.transform.parent.parent.GetComponent<AudioSource>().Play();
				refreshUI();
			}
			else if(!doubleclick && itemDragged != null){
				
				for(int i = 0; i < itemDragged.transform.childCount;i++){
					UnityEngine.Object.Destroy(itemDragged.transform.GetChild(i).gameObject);
				}
				itemDragged.transform.DetachChildren();
				GameObjectManager.unbind(itemDragged);
				UnityEngine.Object.Destroy(itemDragged);
				
			}
			itemDragged = null;
			doubleclick = false;
			editableContainer.transform.parent.GetComponentInParent<ScrollRect>().enabled = true;
		}			

	}

	public void onlyPositiveInteger(TMP_InputField input){
		int res;
		bool success = Int32.TryParse(input.text, out res);
		if(!success || (success && Int32.Parse(input.text) < 0)){
			input.text = "0";
		}
	}

	//Refresh Containers size
	private void refreshUI(){
		foreach( GameObject go in scriptContainers){
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)go.transform );
		}
		
	}

}