using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

/// <summary>
/// Manage highlightable GameObjects
/// </summary>
public class HighLightSystem : FSystem {
	private Family highlightableGO = FamilyManager.getFamily(new AnyOfComponents(typeof(Highlightable), typeof(UIActionType))); //has to be defined before nonhighlightedGO because initBaseColor must be called before unHighLightItem
	private Family highlightedGO = FamilyManager.getFamily(new AllOfComponents(typeof(Highlightable), typeof(PointerOver)), new NoneOfComponents(typeof(UIActionType)));
	private Family nonhighlightedGO = FamilyManager.getFamily(new AllOfComponents(typeof(Highlightable)), new NoneOfComponents(typeof(PointerOver), typeof(UIActionType)));
	private Family highlightedAction = FamilyManager.getFamily(new AllOfComponents(typeof(UIActionType)), new AnyOfComponents( typeof(CurrentAction), typeof(PointerOver)));
	private Family nonCurrentAction = FamilyManager.getFamily(new AllOfComponents(typeof(UIActionType)), new NoneOfComponents(typeof(CurrentAction), typeof(Dragged), typeof(PointerOver)));

	private GameData gameData;
	
	public HighLightSystem()
	{
		if (Application.isPlaying)
		{
			highlightableGO.addEntryCallback(initBaseColor);
			highlightedGO.addEntryCallback(highLightItem);
			nonhighlightedGO.addEntryCallback(unHighLightItem);
			highlightedAction.addEntryCallback(highLightItem);
			nonCurrentAction.addEntryCallback(unHighLightItem);
			gameData = GameObject.Find("GameData").GetComponent<GameData>();
		}
	}
	
	private void initBaseColor(GameObject go){
		if(go.GetComponent<BaseElement>() && go.GetComponent<Image>()){
			go.GetComponent<Highlightable>().baseColor = go.GetComponent<Image>().color;
		}
		if (go.GetComponentInChildren<Renderer>()){
			go.GetComponent<Highlightable>().baseColor = go.GetComponentInChildren<Renderer>().material.color;
			if(go.GetComponent<ScriptRef>()){
				Image img = go.GetComponent<ScriptRef>().uiContainer.transform.Find("Container").GetComponent<Image>();
				img.GetComponent<Highlightable>().baseColor = img.color;	
			}			
		}
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		GameObject highLightedItem = highlightedGO.First();
		//If click on highlighted item and item has a script, then show its script in the 2nd script window
		if(highLightedItem && Input.GetMouseButtonDown(0) && highLightedItem.GetComponent<ScriptRef>()){
			GameObject go = highLightedItem.GetComponent<ScriptRef>().uiContainer;
			if(go.activeInHierarchy)
				GameObjectManager.setGameObjectState(go,false);
			else
				GameObjectManager.setGameObjectState(go,true);
			MainLoop.instance.GetComponent<AudioSource>().Play();
		}
	}

	public void highLightItem(GameObject go){
		if(go.GetComponent<CurrentAction>()){
			if(go.GetComponent<BasicAction>() && go.GetComponent<Image>()){
				go.GetComponent<Image>().color = MainLoop.instance.GetComponent<AgentColor>().currentActionColor;
			}
			else if(go.GetComponent<ForAction>() || go.GetComponent<ForeverAction>()){
				go.transform.GetChild(0).GetComponent<Image>().color = MainLoop.instance.GetComponent<AgentColor>().currentActionColor;
			}				
		}
		else if(go.GetComponent<BaseElement>()){ //pointed action
			if((go.GetComponent<BasicAction>() || go.GetComponent<IfAction>()) && go.GetComponent<Image>()){
				go.GetComponent<Image>().color = go.GetComponent<BaseElement>().highlightedColor;
			}
			else if(go.GetComponent<ForAction>() || go.GetComponent<ForeverAction>()){
				go.GetComponent<Image>().color = go.GetComponent<BaseElement>().highlightedColor;
			}			
		}
		else if(go.GetComponentInChildren<Renderer>()){
			go.GetComponentInChildren<Renderer>().material.color = go.GetComponent<Highlightable>().highlightedColor;
			if(go.GetComponent<ScriptRef>()){
				Image img = go.GetComponent<ScriptRef>().uiContainer.transform.Find("Container").GetComponent<Image>();
				img.color = img.GetComponent<Highlightable>().highlightedColor;
			}
		}
		else if(go.GetComponent<ElementToDrag>() && go.GetComponent<Image>()){
			go.GetComponent<Image>().color = go.GetComponent<Highlightable>().highlightedColor;
		}
	}

	public void unHighLightItem(GameObject go){
		if(go.GetComponent<BaseElement>()){
			if((go.GetComponent<BasicAction>() || go.GetComponent<IfAction>()) && go.GetComponent<Image>()){
				go.GetComponent<Image>().color = go.GetComponent<BaseElement>().baseColor;
			}
			else if(go.GetComponent<ForAction>() || go.GetComponent<ForeverAction>()){
				if(go.GetComponent<Image>().color.Equals(go.GetComponent<BaseElement>().baseColor)){
					go.transform.GetChild(0).GetComponent<Image>().color = MainLoop.instance.GetComponent<AgentColor>().forBaseColor;
				}
				else{
					go.GetComponent<Image>().color = go.GetComponent<BaseElement>().baseColor;
				}	
			}
		}
		else if (go.GetComponentInChildren<Renderer>()){
			go.GetComponentInChildren<Renderer>().material.color = go.GetComponent<Highlightable>().baseColor;
			if(go.GetComponent<ScriptRef>()){
				Image img = go.GetComponent<ScriptRef>().uiContainer.transform.Find("Container").GetComponent<Image>();
				img.color = img.GetComponent<Highlightable>().baseColor;
			}
		}

		else if(go.GetComponent<ElementToDrag>()){
			go.GetComponent<Image>().color = go.GetComponent<Highlightable>().baseColor;
		}
	}
}