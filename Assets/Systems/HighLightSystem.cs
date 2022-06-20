using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

/// <summary>
/// Manage highlightable GameObjects
/// </summary>
public class HighLightSystem : FSystem {
	private Family highlightableGO = FamilyManager.getFamily(new AnyOfComponents(typeof(Highlightable), typeof(LibraryItemRef))); //has to be defined before nonhighlightedGO because initBaseColor must be called before unHighLightItem
	private Family highlightedGO = FamilyManager.getFamily(new AllOfComponents(typeof(Highlightable), typeof(PointerOver)), new NoneOfComponents(typeof(LibraryItemRef)));
	private Family nonhighlightedGO = FamilyManager.getFamily(new AllOfComponents(typeof(Highlightable)), new NoneOfComponents(typeof(PointerOver), typeof(LibraryItemRef)));
	private Family highlightedAction = FamilyManager.getFamily(new AllOfComponents(typeof(LibraryItemRef)), new AnyOfComponents( typeof(CurrentAction), typeof(PointerOver)));
	private Family nonCurrentAction = FamilyManager.getFamily(new AllOfComponents(typeof(LibraryItemRef)), new NoneOfComponents(typeof(CurrentAction), typeof(Dragged), typeof(PointerOver)));
	
	protected override void onStart()
    {
		highlightableGO.addEntryCallback(initBaseColor);
		highlightedGO.addEntryCallback(highLightItem);
		nonhighlightedGO.addEntryCallback(unHighLightItem);
		highlightedAction.addEntryCallback(highLightItem);
		nonCurrentAction.addEntryCallback(unHighLightItem);
	}


	private void initBaseColor(GameObject go){
		// check if it is a script instruction
		if((go.GetComponent<BaseElement>() || go.GetComponent<BaseCondition>()) && go.GetComponent<Image>()){
			go.GetComponent<Highlightable>().baseColor = go.GetComponent<Image>().color;
		}
		// check if it is a word object (robot, ground...)
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
		if(highLightedItem && Input.GetMouseButtonUp(0) && highLightedItem.GetComponent<ScriptRef>()){
			GameObject go = highLightedItem.GetComponent<ScriptRef>().uiContainer;
			GameObjectManager.setGameObjectState(go,!go.activeInHierarchy);
			MainLoop.instance.GetComponent<AudioSource>().Play();
		}
	}

	public void highLightItem(GameObject go){
		// first process currentAction in agents panels
		if(go.GetComponent<CurrentAction>())
		{
			go.GetComponent<Image>().color = MainLoop.instance.GetComponent<AgentColor>().currentActionColor;
			Transform parent = go.transform.parent;
			while (parent != null)
            {
				if (parent.GetComponent<ForControl>() || parent.GetComponent<ForeverControl>())
					parent.transform.GetChild(0).GetComponent<Image>().color = MainLoop.instance.GetComponent<AgentColor>().currentActionColor;
				parent = parent.parent;
			}
		}
		// second manage sensitive UI inside editable panel
		else if(go.GetComponent<BaseElement>() && go.GetComponent<PointerOver>())
			go.GetComponent<Image>().color = go.GetComponent<BaseElement>().highlightedColor;
		// third sensitive UI inside library panel
		else if (go.GetComponent<ElementToDrag>() && go.GetComponent<PointerOver>())
			go.GetComponent<Image>().color = go.GetComponent<Highlightable>().highlightedColor;
		// then process world GameObjects (Walls, drone, robots...)
		else if (go.GetComponentInChildren<Renderer>()){
			go.GetComponentInChildren<Renderer>().material.color = go.GetComponent<Highlightable>().highlightedColor;
			if(go.GetComponent<ScriptRef>()){
				Image img = go.GetComponent<ScriptRef>().uiContainer.transform.Find("Container").GetComponent<Image>();
				img.color = img.GetComponent<Highlightable>().highlightedColor;
			}
		}
	}

	public void unHighLightItem(GameObject go){
		// manage the case of script execution
        if (go.GetComponent<BaseElement>()) { 
			go.GetComponent<Image>().color = go.GetComponent<BaseElement>().baseColor;
			Transform parent = go.transform.parent;
			while (parent != null)
			{
				if (parent.GetComponent<ForControl>() || parent.GetComponent<ForeverControl>())
					parent.transform.GetChild(0).GetComponent<Image>().color = MainLoop.instance.GetComponent<AgentColor>().forBaseColor;
				parent = parent.parent;
			}
		}
		// the case of item inside library panel
		else if (go.GetComponent<ElementToDrag>())
			go.GetComponent<Image>().color = go.GetComponent<Highlightable>().baseColor;
		// the case of world GameObjects (robot, ground...)
		else if (go.GetComponentInChildren<Renderer>()){
			go.GetComponentInChildren<Renderer>().material.color = go.GetComponent<Highlightable>().baseColor;
			if(go.GetComponent<ScriptRef>()){
				Image img = go.GetComponent<ScriptRef>().uiContainer.transform.Find("Container").GetComponent<Image>();
				img.color = img.GetComponent<Highlightable>().baseColor;
			}
		}
	}
}