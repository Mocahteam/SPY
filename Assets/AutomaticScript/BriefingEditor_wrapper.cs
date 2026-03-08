using UnityEngine;
using FYFY;

public class BriefingEditor_wrapper : BaseWrapper
{
	public UnityEngine.Transform editBriefingPanel;
	public UnityEngine.GameObject briefingItemPrefab;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "editBriefingPanel", editBriefingPanel);
		MainLoop.initAppropriateSystemField (system, "briefingItemPrefab", briefingItemPrefab);
	}

	public void saveBriefings()
	{
		MainLoop.callAppropriateSystemMethod (system, "saveBriefings", null);
	}

	public void addNewBriefing(UnityEngine.GameObject parent)
	{
		MainLoop.callAppropriateSystemMethod (system, "addNewBriefing", parent);
	}

	public void removeItemFromParent(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "removeItemFromParent", go);
	}

	public void markLayoutForRebuild(UnityEngine.RectTransform transform)
	{
		MainLoop.callAppropriateSystemMethod (system, "markLayoutForRebuild", transform);
	}

}
