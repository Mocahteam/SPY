using UnityEngine;
using FYFY;

public class BriefingEditor_wrapper : BaseWrapper
{
	public UnityEngine.Transform editBriefingPanel;
	public UnityEngine.Transform briefingContent;
	public UnityEngine.GameObject briefingItemPrefab;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "editBriefingPanel", editBriefingPanel);
		MainLoop.initAppropriateSystemField (system, "briefingContent", briefingContent);
		MainLoop.initAppropriateSystemField (system, "briefingItemPrefab", briefingItemPrefab);
	}

	public void SaveBriefings()
	{
		MainLoop.callAppropriateSystemMethod (system, "SaveBriefings", null);
	}

	public void AddNewBriefing(UnityEngine.GameObject parent)
	{
		MainLoop.callAppropriateSystemMethod (system, "AddNewBriefing", parent);
	}

	public void RemoveItemFromParent(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "RemoveItemFromParent", go);
	}

	public void MarkLayoutForRebuild(UnityEngine.RectTransform transform)
	{
		MainLoop.callAppropriateSystemMethod (system, "MarkLayoutForRebuild", transform);
	}

}
