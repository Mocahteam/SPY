using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

public class UISystem : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	private GameObject actionContainer;
	private Family PointedGO = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(ElementToDrag)));

	private Family ContainersGO = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver), typeof(UITypeContainer)));
	private Family ContnainerRefreshGO = FamilyManager.getFamily(new AllOfComponents(typeof(UITypeContainer)));
	private GameObject itemDragged;
	private GameData gameData;
	public UISystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
	}
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {

		if(gameData.scriptRunning){
			gameData.ButtonExec.GetComponent<Button>().interactable = false;
			gameData.ButtonReset.GetComponent<Button>().interactable = false;
		}
		else{
			gameData.ButtonExec.GetComponent<Button>().interactable = true;
			gameData.ButtonReset.GetComponent<Button>().interactable = true;
		}

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

		GameObject priority = null;
		foreach( GameObject go in ContainersGO){
			if(priority == null || priority.GetComponent<UITypeContainer>().layer < go.GetComponent<UITypeContainer>().layer)
				priority = go;
			
		}
		//if(priority != null)
			//Debug.Log(priority.GetComponent<UITypeContainer>().type);


		if(Input.GetMouseButtonUp(0)){
			if(priority != null && itemDragged != null){
				//Object.Instantiate(itemDragged, priority.transform);
				itemDragged.transform.SetParent(priority.transform);
				//GameObjectManager.bind(itemDragged);
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

	private void refreshUI(){
		//Canvas.ForceUpdateCanvases();
		foreach( GameObject go in ContnainerRefreshGO){
			go.GetComponent<VerticalLayoutGroup>().enabled = false;
		}

		Canvas.ForceUpdateCanvases();

		foreach( GameObject go in ContnainerRefreshGO){
			go.GetComponent<VerticalLayoutGroup>().enabled = true;
		}
	}

	public void resetScript(){
		GameObject go = GameObject.Find("ScriptContainer");
		for(int i = 0; i < go.transform.childCount; i++){
			destroyScript(go.transform.GetChild(i));
		}
		refreshUI();
	}

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
}