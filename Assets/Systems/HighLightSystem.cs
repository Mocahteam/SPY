using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using TMPro;
using System;
using UnityEngine.Events;

public class HighLightSystem : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	private Family highlightableGO = FamilyManager.getFamily(new AnyOfComponents(typeof(Highlightable), typeof(UIActionType))); //has to be defined before nonhighlightedGO because initBaseColor must be called before unHighLightItem
	private Family highlightedGO = FamilyManager.getFamily(new AllOfComponents(typeof(Highlightable), typeof(PointerOver)), new NoneOfComponents(typeof(UIActionType)));
	private Family nonhighlightedGO = FamilyManager.getFamily(new AllOfComponents(typeof(Highlightable)), new NoneOfComponents(typeof(PointerOver), typeof(UIActionType)));
	private Family highlightedAction = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction), typeof(UIActionType)));
	private Family nonhighlightedAction = FamilyManager.getFamily(new AllOfComponents(typeof(UIActionType)), new NoneOfComponents(typeof(CurrentAction)));
    private Family newStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));

	//private Family enemyScriptContainer_f = FamilyManager.getFamily(new NoneOfComponents(typeof(UITypeContainer)), new AnyOfTags("ScriptConstructor"));
	//private GameObject EnemyScriptContainer;

	private GameData gameData;
	
	public HighLightSystem(){
		highlightableGO.addEntryCallback(initBaseColor);
		highlightedGO.addEntryCallback(highLightItem);
		nonhighlightedGO.addEntryCallback(unHighLightItem);
		//highlightedGO.addExitCallback(unHighLightItem);
		highlightedAction.addEntryCallback(highLightItem);
		nonhighlightedAction.addEntryCallback(unHighLightItem);
		//highlightedAction.addExitCallback(unHighLightItem);
        newStep_f.addEntryCallback(onNewStep);
        //highLightedItem = null;
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		//EnemyScriptContainer = enemyScriptContainer_f.First();
	}
	
	private void initBaseColor(GameObject go){
		if(go.GetComponent<BaseElement>()){
			if(go.GetComponent<BasicAction>() && go.GetComponent<Image>()){
				go.GetComponent<BasicAction>().baseColor = go.GetComponent<Image>().color;
			}
			else if(go.GetComponent<ForAction>()){
				go.GetComponent<Highlightable>().baseColor = go.transform.GetChild(0).GetComponent<Image>().color;
			}			
		}

		else{
			go.GetComponent<Highlightable>().baseColor = go.GetComponentInChildren<Renderer>().material.color;
		}
	}
	/*
	public static GameObject getLastGameObjectOf (GameObject gameObject, Action.ActionType type){
		if(gameObject != null && (type == Action.ActionType.For ||type == Action.ActionType.If)){
			gameObject = gameObject.transform.GetChild(gameObject.transform.childCount-1).gameObject;
			return getLastGameObjectOf(gameObject, gameObject.GetComponent<UIActionType>().type);
		}
		return gameObject;		
	}
	
	async static Task removeLastHighLight(GameObject gameObject, Action action){
		await Task.Delay((int)StepSystem.getTimeStep()*1000);		
		if(gameObject != null)
			gameObject = getLastGameObjectOf(gameObject, action.actionType);
		if(gameObject != null)
			gameObject.GetComponent<Image>().color = ActionManipulator.getBaseColor();
	}
	*/
	/*
	//Show the script in the container
	public static void ScriptToContainer(Script script, GameObject container, bool sensitive = false){
		//Debug.Log("SCRIPTtocontainer");
		int i = 0;
		GameObject obj;
		foreach(Action action in script.actions){
			if(i == script.currentAction){
				obj = ActionManipulator.ActionToContainer(action,true);
				obj.transform.SetParent(container.transform, sensitive);
				//if(action == script.actions.Last()){ // action = last action & next action
					//await removeLastHighLight(obj, action);
					//GameObjectManager.unbind(obj);
					//GameObjectManager.removeComponent<HighLight>(obj);
					//Object.Destroy(obj.GetComponent<HighLight>());
				//}
				
			}
			else{
				obj = ActionManipulator.ActionToContainer(action, false);
				obj.transform.SetParent(container.transform, sensitive);
			}
			GameObjectManager.bind(obj);
			i++;
		}

		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)container.transform );

	}*/

    private void onNewStep(GameObject unused)
    {
        //Change the higlighted action every step
		/*
        if (scriptInWindow){
            foreach (GameObject child in EnemyScriptContainer.transform)
            {
				
				//GameObjectManager.unbind(child.gameObject);
                //GameObject.Destroy(child.gameObject);
            }
            //ScriptToContainer(scriptInWindow.GetComponent<Script>(), EnemyScriptContainer);
        }*/
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		GameObject highLightedItem = highlightedGO.First();
		//If click on highlighted item and item has a script, then show script in the 2nd script window
		if(highLightedItem && Input.GetMouseButtonDown(0) && highLightedItem.GetComponent<ScriptRef>()){
			GameObject go = highLightedItem.GetComponent<ScriptRef>().container.transform.parent.transform.parent.gameObject;
			//hide other containers
			foreach(Transform notgo in go.transform.parent.transform){
				if (notgo != go.transform && notgo.gameObject.activeSelf){
					GameObjectManager.setGameObjectState(notgo.gameObject, false);
				}
			}
			GameObjectManager.setGameObjectState(go,true);
			MainLoop.instance.GetComponent<AudioSource>().Play();

		}

	}

	public void highLightItem(GameObject go){
		Debug.Log("highLightItem = "+go.name+"------");
		if(go.GetComponent<BaseElement>()){
			if(go.GetComponent<BasicAction>() && go.GetComponent<Image>()){
				go.GetComponent<Image>().color = go.GetComponent<Highlightable>().highlightedColor;
			}
			else if(go.GetComponent<ForAction>()){
				go.transform.GetChild(0).GetComponent<Image>().color = go.GetComponent<Highlightable>().highlightedColor;
			}			
		}

		else{
			go.GetComponentInChildren<Renderer>().material.color = go.GetComponent<Highlightable>().highlightedColor;
		}
	}

	public void unHighLightItem(GameObject go){
		Debug.Log("------unhighlight");
		if(go.GetComponent<BaseElement>()){
			if(go.GetComponent<BasicAction>() && go.GetComponent<Image>()){
				go.GetComponent<Image>().color = go.GetComponent<Highlightable>().baseColor;
				Debug.Log("unhighlight "+ go.GetComponent<Highlightable>().baseColor.ToString());
			}
			else if(go.GetComponent<ForAction>()){
				//Debug.Log("for basecolor = "+go.GetComponent<Highlightable>().baseColor.ToString());
				go.transform.GetChild(0).GetComponent<Image>().color = go.GetComponent<Highlightable>().baseColor;
			}

		}

		else{
			go.GetComponentInChildren<Renderer>().material.color = go.GetComponent<Highlightable>().baseColor;
		}
		/*
		GameObject prefab = go.GetComponent<UIActionType>().prefab;
		go.GetComponent<Image>().color = prefab.GetComponent<Image>().color;*/
	}

}