using UnityEngine;
using FYFY;

public class SyncLocalization_wrapper : BaseWrapper
{
	public System.String[] items;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "items", items);
	}

	public void syncLocale()
	{
		MainLoop.callAppropriateSystemMethod (system, "syncLocale", null);
	}

	public void nextItem()
	{
		MainLoop.callAppropriateSystemMethod (system, "nextItem", null);
	}

	public void prevItem()
	{
		MainLoop.callAppropriateSystemMethod (system, "prevItem", null);
	}

}
