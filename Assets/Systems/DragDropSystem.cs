using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;

/// <summary>
/// Implement Drag&Drop interaction and dubleclick
/// </summary>
public class DragDropSystem : FSystem
{
    private Family libraryElementPointed_f = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(ElementToDrag), typeof(Image)));
    private Family containerPointed_f = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(UITypeContainer)));
	private Family actionPointed_f = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(UIActionType), typeof(Image)));
	private Family inputUIOver_f = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver)), new AnyOfComponents(typeof(TMP_InputField), typeof(TMP_Dropdown)));
	private Family editableScriptContainer_f = FamilyManager.getFamily(new AllOfComponents(typeof(UITypeContainer)), new AnyOfTags("ScriptConstructor"));
	private Family editableScriptPointed_f = FamilyManager.getFamily(new AllOfComponents(typeof(UITypeContainer), typeof(PointerOver)), new AnyOfTags("ScriptConstructor"));
	private GameObject mainCanvas;
	private GameObject itemDragged;
	private GameObject positionBar;
	private GameObject editableContainer;
	
	//double click
	private float lastClickTime;
	private float catchTime;
	
	private GameObject buttonPlay;
	
	public DragDropSystem()
	{
		if (Application.isPlaying)
		{
			catchTime = 0.25f;
			mainCanvas = GameObject.Find("Canvas");
			editableContainer = editableScriptContainer_f.First();
			positionBar = editableContainer.transform.Find("PositionBar").gameObject;
			buttonPlay = GameObject.Find("ExecuteButton");
		}
	}

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
		//Mouse down
		if(Input.GetMouseButtonDown(0) && !Input.GetMouseButtonUp(0)){ //focus in play mode (unity editor) could be up and down !!! (bug unity)
			//manage click on library
			if(libraryElementPointed_f.Count > 0)
            {
                GameObject go = libraryElementPointed_f.First();
                GameObject prefab = go.GetComponent<ElementToDrag>().actionPrefab;
				// Create a dragged GameObject
				itemDragged = UnityEngine.Object.Instantiate<GameObject>(prefab, go.transform);
                BaseElement action = itemDragged.GetComponent<BaseElement>();
				if(action.GetType().ToString().Equals("ForAction")){
					TMP_InputField input = itemDragged.GetComponentInChildren<TMP_InputField>();
					input.onEndEdit.AddListener(delegate{onlyPositiveInteger(input);});
				}
				itemDragged.GetComponent<UIActionType>().prefab = prefab;
				itemDragged.GetComponent<UIActionType>().linkedTo = go;
				action.target = itemDragged;
				GameObjectManager.bind(itemDragged);
				GameObjectManager.addComponent<Dragged>(itemDragged);
				// exclude this GameObject from the EventSystem
				itemDragged.GetComponent<Image>().raycastTarget = false;
				if(itemDragged.GetComponent<BasicAction>())
					foreach(Image child in itemDragged.GetComponentsInChildren<Image>())
						child.raycastTarget = false;
			}

			// drag in editable script
            if (actionPointed_f.Count > 0 && inputUIOver_f.Count == 0 && editableScriptPointed_f.Count > 0) // cannot drag if inputfield or dropdown pointed
            {
                itemDragged = actionPointed_f.getAt(actionPointed_f.Count-1); // get the last one <=> deeper child PointerOver
				// make this Action draggable
				GameObjectManager.setGameObjectParent(itemDragged, mainCanvas, true);
				itemDragged.transform.localScale = new Vector3(0.8f,0.8f,0.8f);
				GameObjectManager.addComponent<Dragged>(itemDragged);
				// exclude this GameObject from the EventSystem
				itemDragged.GetComponent<Image>().raycastTarget = false;
				if(itemDragged.GetComponent<BasicAction>())
					foreach(Image child in itemDragged.GetComponentsInChildren<Image>())
						child.raycastTarget = false;
				// Restore action and subactions to inventory
				foreach(BaseElement actChild in itemDragged.GetComponentsInChildren<BaseElement>())
					GameObjectManager.addComponent<AddOne>(actChild.gameObject);
				editableContainer.transform.parent.GetComponentInParent<ScrollRect>().enabled = false;
			}

			MainLoop.instance.StartCoroutine(updatePlayButton());
		}

        //Find the deeper container pointed
        GameObject targetContainer = null;
        if (containerPointed_f.Count > 0)
            targetContainer = containerPointed_f.getAt(containerPointed_f.Count - 1);

        if (itemDragged != null)
        {
            itemDragged.transform.position = Input.mousePosition;

            //PositionBar positioning
            if (targetContainer)
            {
                // default put position Bar last
                positionBar.transform.SetParent(targetContainer.transform);
				positionBar.transform.SetSiblingIndex(targetContainer.transform.childCount + 1);
                if (actionPointed_f.Count > 0)
                {
                    // get focused item and adjust position bar depending on mouse position
                    GameObject focusedItemTarget = actionPointed_f.getAt(actionPointed_f.Count - 1);
                    if (focusedItemTarget == targetContainer && Input.mousePosition.y > focusedItemTarget.transform.position.y-30)
                    {
                        targetContainer = targetContainer.transform.parent.gameObject;
                        positionBar.transform.SetParent(targetContainer.transform);
                    }
                    if ((focusedItemTarget.GetComponent<UITypeContainer>() == null && Input.mousePosition.y > focusedItemTarget.transform.position.y) ||
					 (focusedItemTarget.GetComponent<UITypeContainer>() != null && Input.mousePosition.y > focusedItemTarget.transform.position.y-30)){
						 positionBar.transform.SetSiblingIndex(focusedItemTarget.transform.GetSiblingIndex());
					 }
                        
                    else if(focusedItemTarget.GetComponent<UITypeContainer>()){
						positionBar.transform.SetSiblingIndex(focusedItemTarget.transform.GetSiblingIndex() + focusedItemTarget.transform.childCount);
					}
                    else {
						positionBar.transform.SetSiblingIndex(focusedItemTarget.transform.GetSiblingIndex() + 1);
					}
                }
            }
        }
        else
        {
            positionBar.transform.SetParent(editableContainer.transform);
            positionBar.transform.SetSiblingIndex(editableContainer.transform.childCount + 1);
        }	


		// Delete with right click
		if(itemDragged == null && Input.GetMouseButtonUp(1) && actionPointed_f.Count > 0){
			GameObjectManager.addComponent<ResetBlocLimit>(actionPointed_f.getAt(actionPointed_f.Count-1));
			MainLoop.instance.StartCoroutine(updatePlayButton());
		}
		
		// Mouse Up
		if(Input.GetMouseButtonUp(0))
        {
            bool doubleclick = false;
            //check double click
            if (Time.time - lastClickTime < catchTime)
                doubleclick = true;

            //Drop in script
            if (itemDragged != null && (targetContainer != null  || doubleclick)){
				if(doubleclick)
					itemDragged.transform.SetParent(editableContainer.transform);
				else
					itemDragged.transform.SetParent(targetContainer.transform);
				itemDragged.transform.SetSiblingIndex(positionBar.transform.GetSiblingIndex());
				itemDragged.transform.localScale = new Vector3(1,1,1);
				itemDragged.GetComponent<Image>().raycastTarget = true;
				if(itemDragged.GetComponent<BasicAction>())
					foreach(Image child in itemDragged.GetComponentsInChildren<Image>())
						child.raycastTarget = true;

				// update limit bloc
				foreach(BaseElement actChild in itemDragged.GetComponentsInChildren<BaseElement>())
					GameObjectManager.addComponent<Dropped>(actChild.gameObject);

				GameObjectManager.removeComponent<Dragged>(itemDragged);					

				if(itemDragged.GetComponent<UITypeContainer>())
					itemDragged.GetComponent<Image>().raycastTarget = true;
				editableContainer.transform.parent.parent.GetComponent<AudioSource>().Play();
				refreshUI();
			}
            // priority == null, means drop item outside editablePanel
			else if(!doubleclick && itemDragged != null){
                // remove item and all its children
				for(int i = 0; i < itemDragged.transform.childCount;i++)
					UnityEngine.Object.Destroy(itemDragged.transform.GetChild(i).gameObject);
				itemDragged.transform.DetachChildren();
				GameObjectManager.unbind(itemDragged);
				UnityEngine.Object.Destroy(itemDragged);
			}				
			itemDragged = null;
			editableContainer.transform.parent.parent.GetComponent<ScrollRect>().enabled = true;

            lastClickTime = Time.time;
			MainLoop.instance.StartCoroutine(updatePlayButton());
        }
	}

	private IEnumerator updatePlayButton(){
		yield return null;
		buttonPlay.GetComponent<Button>().interactable = !(editableScriptContainer_f.First().transform.childCount < 2);
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
		foreach( GameObject go in editableScriptContainer_f)
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)go.transform );
	}

}