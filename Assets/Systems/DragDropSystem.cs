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
    private Family containerPointed_f = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(UITypeContainer))); // Les container éditable
	private Family actionPointed_f = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(UIActionType), typeof(Image)));
	private Family inputUIOver_f = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver)), new AnyOfComponents(typeof(TMP_InputField), typeof(TMP_Dropdown)));
	private Family editableScriptPointed_f = FamilyManager.getFamily(new AllOfComponents(typeof(UITypeContainer), typeof(PointerOver)), new AnyOfTags("ScriptConstructor"));
	public GameObject mainCanvas;
	private GameObject itemDragged;
	public GameObject positionBar;
	public GameObject editableContainer;
	
	//double click
	private float lastClickTime;
	public float catchTime;
	bool doubleclick = false;

	public GameObject buttonPlay;

	public static DragDropSystem instance;

	public DragDropSystem()
    {
		instance = this;
    }

	protected override void onStart()
    {
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		//Mouse down
		if (Input.GetMouseButtonDown(0) && !Input.GetMouseButtonUp(0)) { //focus in play mode (unity editor) could be up and down !!! (bug unity)
																		 //manage click on library
			if (libraryElementPointed_f.Count > 0)
			{
				/*
				GameObject go = libraryElementPointed_f.First();
				GameObject prefab = go.GetComponent<ElementToDrag>().actionPrefab;
				// Create a dragged GameObject
				itemDragged = UnityEngine.Object.Instantiate<GameObject>(prefab, go.transform);
				BaseElement action = itemDragged.GetComponent<BaseElement>();
				if (action.GetType().ToString().Equals("ForAction")) {
					TMP_InputField input = itemDragged.GetComponentInChildren<TMP_InputField>();
					input.onEndEdit.AddListener(delegate { onlyPositiveInteger(input); });
				}
				itemDragged.GetComponent<UIActionType>().prefab = prefab;
				itemDragged.GetComponent<UIActionType>().linkedTo = go;
				action.target = itemDragged;
				GameObjectManager.bind(itemDragged);
				GameObjectManager.addComponent<Dragged>(itemDragged);
				// exclude this GameObject from the EventSystem
				itemDragged.GetComponent<Image>().raycastTarget = false;
				if (itemDragged.GetComponent<BasicAction>())
					foreach (Image child in itemDragged.GetComponentsInChildren<Image>())
						child.raycastTarget = false;
				*/
			}

			// drag in editable script
			if (actionPointed_f.Count > 0 && inputUIOver_f.Count == 0 && editableScriptPointed_f.Count > 0) // cannot drag if inputfield or dropdown pointed
			{
				itemDragged = actionPointed_f.getAt(actionPointed_f.Count - 1); // get the last one <=> deeper child PointerOver
																				// make this Action draggable
				GameObjectManager.setGameObjectParent(itemDragged, mainCanvas, true);
				itemDragged.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
				GameObjectManager.addComponent<Dragged>(itemDragged);
				// exclude this GameObject from the EventSystem
				itemDragged.GetComponent<Image>().raycastTarget = false;
				if (itemDragged.GetComponent<BasicAction>())
					foreach (Image child in itemDragged.GetComponentsInChildren<Image>())
						child.raycastTarget = false;
				// Restore action and subactions to inventory
				foreach (BaseElement actChild in itemDragged.GetComponentsInChildren<BaseElement>())
					GameObjectManager.addComponent<AddOne>(actChild.gameObject);
				editableContainer.transform.parent.GetComponentInParent<ScrollRect>().enabled = false;
			}

			MainLoop.instance.StartCoroutine(updatePlayButton());
		}

		//Find the deeper container pointed
		GameObject targetContainer = null;
		// Si un container editable est pointé
		if (containerPointed_f.Count > 0) {
			//Debug.Log(containerPointed_f.Count);
			targetContainer = containerPointed_f.getAt(containerPointed_f.Count - 1);
		}
		/*
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
		*/


		// Delete with right click
		if(itemDragged == null && Input.GetMouseButtonUp(1) && actionPointed_f.Count > 0){
			GameObjectManager.addComponent<ResetBlocLimit>(actionPointed_f.getAt(actionPointed_f.Count-1));
			MainLoop.instance.StartCoroutine(updatePlayButton());
		}
		
		// Mouse Up
		if(Input.GetMouseButtonUp(0))
        {
			/*
            doubleclick = false;
            //check double click
            if (Time.time - lastClickTime < catchTime)
                doubleclick = true;
			
			//Drop in script
			if (itemDragged != null && (targetContainer != null  || doubleclick)){
				if (doubleclick)
				{
					dropElementInContainer(itemDragged, editableContainer);
					//itemDragged.transform.SetParent(editableContainer.transform);
				}
			*/
				/*
				else
					itemDragged.transform.SetParent(targetContainer.transform);
				
			itemDragged.transform.SetSiblingIndex(positionBar.transform.GetSiblingIndex());
			itemDragged.transform.localScale = new Vector3(1,1,1);
			itemDragged.GetComponent<Image>().raycastTarget = true;
				
			if(itemDragged.GetComponent<BasicAction>())
				foreach(Image child in itemDragged.GetComponentsInChildren<Image>())
					child.raycastTarget = true;
				
			// update limit bloc
			foreach (BaseElement actChild in itemDragged.GetComponentsInChildren<BaseElement>())
				GameObjectManager.addComponent<Dropped>(actChild.gameObject);

			GameObjectManager.removeComponent<Dragged>(itemDragged);					

			if(itemDragged.GetComponent<UITypeContainer>())
				itemDragged.GetComponent<Image>().raycastTarget = true;
			editableContainer.transform.parent.parent.GetComponent<AudioSource>().Play();
			refreshUI();
			*/
			//}
			/*
            // priority == null, means drop item outside editablePanel
			else if(!doubleclick && itemDragged != null){
				Debug.Log("Object laché : " + itemDragged.name);
				dropOutDoorContainer(itemDragged);

				Debug.Log("arreter drag dans mauvais zone");
                // remove item and all its children
				for(int i = 0; i < itemDragged.transform.childCount;i++)
					UnityEngine.Object.Destroy(itemDragged.transform.GetChild(i).gameObject);
				itemDragged.transform.DetachChildren();
				GameObjectManager.unbind(itemDragged);

			}
			*/

			itemDragged = null;
			editableContainer.transform.parent.parent.GetComponent<ScrollRect>().enabled = true;

            lastClickTime = Time.time;
			MainLoop.instance.StartCoroutine(updatePlayButton());
        }
	}

	private IEnumerator updatePlayButton(){
		yield return null;
		buttonPlay.GetComponent<Button>().interactable = !(editableContainer.transform.childCount < 2);
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
		LayoutRebuilder.ForceRebuildLayoutImmediate(editableContainer.GetComponent<RectTransform>());
	}

	// Place l'element dans la place ciblé du container editable (zone de sequence d'action)
	private void dropElementInContainer(GameObject element, GameObject targetContainer)
    {
		Debug.Log("drop element in container : " + element.name);

		// On associe l'element au container
		element.transform.SetParent(targetContainer.transform);
		// On met l'élément à la position voulue
		element.transform.SetSiblingIndex(positionBar.transform.GetSiblingIndex());
		// On le met à la taille voulue
		element.transform.localScale = new Vector3(1, 1, 1);
		// Pour réactivé la selection posible
		element.GetComponent<Image>().raycastTarget = true;
		if (element.GetComponent<BasicAction>())
		{
			foreach (Image child in element.GetComponentsInChildren<Image>())
			{
				child.raycastTarget = true;
			}
		}
		
		// update limit bloc
		foreach (BaseElement actChild in element.GetComponentsInChildren<BaseElement>())
			GameObjectManager.addComponent<Dropped>(actChild.gameObject);

		//GameObjectManager.removeComponent<Dragged>(element);

		if (element.GetComponent<UITypeContainer>())
			element.GetComponent<Image>().raycastTarget = true;

		// Lance le son de dépôt du block d'action
		targetContainer.transform.parent.parent.GetComponent<AudioSource>().Play();
		
		refreshUI();
	}

	// Désabonne l'élément des familles du systéme et le détruit l'element (ainsi que ces enfants)
	private void dropOutDoorContainer(GameObject element)
    {
		Debug.Log("arreter drag dans mauvais zone (mais par gestion de fonction)");
		// remove item and all its children
		for (int i = 0; i < element.transform.childCount; i++)
			UnityEngine.Object.Destroy(element.transform.GetChild(i).gameObject);
		element.transform.DetachChildren();
		GameObjectManager.unbind(element);
		UnityEngine.Object.Destroy(element);
		Debug.Log("Object détruit");
		Debug.Log("itemDragged : " + itemDragged.name);

		refreshUI();
	}

	public void beginDragElement(GameObject element)
    {
		GameObject itemDragged2;
		GameObject go = libraryElementPointed_f.First();
		GameObject prefab = go.GetComponent<ElementToDrag>().actionPrefab;
		// Create a dragged GameObject
		itemDragged2 = UnityEngine.Object.Instantiate<GameObject>(prefab, go.transform);
		BaseElement action = itemDragged2.GetComponent<BaseElement>();
		if (action.GetType().ToString().Equals("ForAction"))
		{
			TMP_InputField input = itemDragged.GetComponentInChildren<TMP_InputField>();
			input.onEndEdit.AddListener(delegate { onlyPositiveInteger(input); });
		}
		itemDragged2.GetComponent<UIActionType>().prefab = prefab;
		itemDragged2.GetComponent<UIActionType>().linkedTo = go;
		action.target = itemDragged2;
		GameObjectManager.bind(itemDragged2);
		GameObjectManager.addComponent<Dragged>(itemDragged2);
		// exclude this GameObject from the EventSystem
		itemDragged2.GetComponent<Image>().raycastTarget = false;
		if (itemDragged2.GetComponent<BasicAction>())
			foreach (Image child in itemDragged2.GetComponentsInChildren<Image>())
				child.raycastTarget = false;
	}

	// Determine si l'element associer à l'évenement Pointer Up se trouvé dans une zone de container ou non
	// Dirige vers la bonne fonction selon le cas
	public void endDragElement(GameObject element)
	{
		Debug.Log("Objet laché fonction");
		// On commence par regarder si il y a un container pointé et si oui, on le récupére
		GameObject targetContainer = null;
		if (containerPointed_f.Count > 0)
		{
			//Debug.Log(containerPointed_f.Count);
			targetContainer = containerPointed_f.getAt(containerPointed_f.Count - 1);
		}

		// Si aucun container n'est pointé
		if (targetContainer == null)
		{
			Debug.Log("Pas de container");
			dropOutDoorContainer(element);

		}
		else // Sinon cela veux dire qu'il y a au moins un container de pointé
		{
			Debug.Log("container présent");
			dropElementInContainer(element, targetContainer);
		}

	}

	public void doubleClick(GameObject element)
	{
		// Vérifier si double clique ou non
		doubleclick = false;
		//check double click
		if (Time.time - lastClickTime < catchTime)
			doubleclick = true;
		// On met à jours le timer du dernier clique
		lastClickTime = Time.time;

		if (doubleclick)
		{
			dropElementInContainer(element, editableContainer);
		}
		else
		{
			dropOutDoorContainer(element);
		}

		// On fois la fonction terminer on désactive de nouveau le doubleclick
		doubleclick = false;
	}

	// Supprime l'element
	public void pointerLeftUpElement(GameObject element)
    {

    }

	public void pointerDownElement(GameObject element)
    {
		Debug.Log("Pointer down");
		
	}

	public void dragElement(GameObject element)
	{
		Debug.Log("Pointer drag");

		itemDragged.transform.position = Input.mousePosition;
	}


	// Fonction de test pour voir si l'event associé est le bon
	public void testObjectpointer()
	{
		Debug.Log("Fonction test d'objet pointé");
		if (actionPointed_f.Count > 0)
		{
			foreach (GameObject go in actionPointed_f)
			{
				Debug.Log("Objet pointé : " + go.name);
			}
		}
	}

}