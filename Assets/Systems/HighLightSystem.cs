using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

/// <summary>
/// Manage highlightable GameObjects (word object as robots, drones, ground and UI object as current action executed or library items
/// </summary>
public class HighLightSystem : FSystem {
	private Family f_highlightable = FamilyManager.getFamily(new AnyOfComponents(typeof(Highlightable), typeof(LibraryItemRef))); //has to be defined before nonhighlightedGO because initBaseColor must be called before unHighLightItem
	private Family f_highlighted = FamilyManager.getFamily(new AllOfComponents(typeof(Highlightable), typeof(PointerOver)), new NoneOfComponents(typeof(LibraryItemRef)));
	private Family f_nonhighlighted = FamilyManager.getFamily(new AllOfComponents(typeof(Highlightable)), new NoneOfComponents(typeof(PointerOver), typeof(LibraryItemRef)));
	private Family f_highlightedAction = FamilyManager.getFamily(new AllOfComponents(typeof(LibraryItemRef)), new AnyOfComponents( typeof(CurrentAction), typeof(PointerOver)));
	private Family f_nonCurrentAction = FamilyManager.getFamily(new AllOfComponents(typeof(LibraryItemRef)), new NoneOfComponents(typeof(CurrentAction), typeof(PointerOver), typeof(Dragging)));

	private Family f_focusedSyncColors = FamilyManager.getFamily(new AllOfComponents(typeof(SynColors), typeof(Selectable), typeof(PointerOver)));
	private Family f_unfocusedSyncColors = FamilyManager.getFamily(new AllOfComponents(typeof(SynColors), typeof(Selectable)), new NoneOfComponents(typeof(PointerOver)));

	public GameObject dialogPanel;

	protected override void onStart()
    {
		f_highlightable.addEntryCallback(initBaseColor);
		f_highlighted.addEntryCallback(highLightItem);
		f_nonhighlighted.addEntryCallback(unHighLightItem);
		f_highlightedAction.addEntryCallback(highLightItem);
		f_nonCurrentAction.addEntryCallback(unHighLightItem);

		f_focusedSyncColors.addEntryCallback(delegate (GameObject go) {
			propagateColor(go, true);
		});
		f_unfocusedSyncColors.addEntryCallback(delegate (GameObject go) {
			propagateColor(go, false);
		});
	}

	private void propagateColor(GameObject go, bool useHighlightColor)
    {
		Selectable sel = go.GetComponent<Selectable>();
		Color color = useHighlightColor ? sel.colors.highlightedColor : sel.colors.normalColor;
		if (!sel.interactable)
			color = sel.colors.disabledColor;
		foreach (Image img in go.GetComponent<SynColors>().targets)
			img.color = color;
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		GameObject highLightedItem = f_highlighted.First();
		//If click on highlighted item and item has a script, then show its script in the 2nd script window
		if(highLightedItem && Input.GetMouseButtonUp(0) && highLightedItem.GetComponent<ScriptRef>() && dialogPanel.activeInHierarchy == false)
		{
			GameObject go = highLightedItem.GetComponent<ScriptRef>().executablePanel;
			GameObjectManager.setGameObjectState(go,!go.activeInHierarchy);
			MainLoop.instance.GetComponent<AudioSource>().Play();
		}
	}

	private void initBaseColor(GameObject go)
	{
		// check if it is a script instruction
		if ((go.GetComponent<BaseElement>() || go.GetComponent<BaseCondition>()) && go.GetComponent<Selectable>())
		{
			Selectable sel = go.GetComponent<Selectable>();
			if (sel.interactable)
				go.GetComponent<Highlightable>().baseColor = sel.colors.normalColor;
			else
				go.GetComponent<Highlightable>().baseColor = sel.colors.disabledColor;
		}
		// check if it is a word object (robot, ground...)
		if (go.GetComponentInChildren<Renderer>(true))
		{
			go.GetComponent<Highlightable>().baseColor = go.GetComponentInChildren<Renderer>(true).material.color;
			if (go.GetComponent<ScriptRef>())
			{
				Image img = go.GetComponent<ScriptRef>().executablePanel.transform.Find("Scroll View").GetComponent<Image>();
				img.GetComponent<Highlightable>().baseColor = img.color;
			}
		}
	}

	public void highLightItem(GameObject go){
		// first process currentAction in agents panels (execution mode)
		if(go.GetComponent<CurrentAction>())
		{
			Selectable sel = go.GetComponent<Selectable>();
			ColorBlock cb = sel.colors;
			if (sel.interactable)
				cb.normalColor = go.GetComponent<Highlightable>().highlightedColor;
			else
				cb.disabledColor = go.GetComponent<Highlightable>().highlightedColor;
			sel.colors = cb;

			Transform parent = go.transform.parent;
			while (parent != null)
            {
				if (parent.GetComponent<ForControl>() || parent.GetComponent<ForeverControl>())
				{
					sel = parent.GetComponent<Selectable>();
					cb = sel.colors;
					if (sel.interactable)
						cb.normalColor = parent.GetComponent<Highlightable>().highlightedColor;
					else
						cb.disabledColor = parent.GetComponent<Highlightable>().highlightedColor;
					sel.colors = cb;
					propagateColor(parent.gameObject, false);
				}
				parent = parent.parent;
			}
		}
		// then process world GameObjects (Walls, drone, robots...)
		else if (go.GetComponentInChildren<Renderer>(true)){
			go.GetComponentInChildren<Renderer>(true).material.color = go.GetComponent<Highlightable>().highlightedColor;
			if(go.GetComponent<ScriptRef>()){
				Image img = go.GetComponent<ScriptRef>().executablePanel.transform.Find("Scroll View").GetComponent<Image>();
				img.color = img.GetComponent<Highlightable>().highlightedColor;
			}
		}
	}

	public void unHighLightItem(GameObject go){
		// manage the case of items in executable panels
		ScrollRect parentScroll = go.GetComponentInParent<ScrollRect>();
		if ((go.GetComponent<BaseElement>() || go.GetComponent<BaseCondition>()) && parentScroll && parentScroll.transform.parent.name.Contains("ExecutablePanel")) {
			Selectable sel = go.GetComponent<Selectable>();
			ColorBlock cb = sel.colors;
			if (sel.interactable)
				cb.normalColor = go.GetComponent<Highlightable>().baseColor;
			else
				cb.disabledColor = go.GetComponent<Highlightable>().baseColor;
			sel.colors = cb;

			Transform parent = go.transform.parent;
			while (parent != null)
			{
				if (parent.GetComponent<ForControl>() || parent.GetComponent<ForeverControl>())
				{
					sel = parent.GetComponent<Selectable>();
					cb = sel.colors;
					if (sel.interactable)
						cb.normalColor = parent.GetComponent<Highlightable>().baseColor;
					else
						cb.disabledColor = parent.GetComponent<Highlightable>().baseColor;
					sel.colors = cb;
					propagateColor(parent.gameObject, false);
				}
				parent = parent.parent;
			}
		}
		// the case of world GameObjects (robot, ground...)
		else if (go.GetComponentInChildren<Renderer>(true))
		{
			go.GetComponentInChildren<Renderer>(true).material.color = go.GetComponent<Highlightable>().baseColor;
			if (go.GetComponent<ScriptRef>())
			{
				Image img = go.GetComponent<ScriptRef>().executablePanel.transform.Find("Scroll View").GetComponent<Image>();
				img.color = img.GetComponent<Highlightable>().baseColor;
			}
		}
	}
}