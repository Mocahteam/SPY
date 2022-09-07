using UnityEngine;
using FYFY;

public class AutoScrollSystem_wrapper : BaseWrapper
{
	public UnityEngine.UI.ScrollRect scrollRect;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "scrollRect", scrollRect);
	}

	public void setVerticalSpeed(System.Single newSpeed)
	{
		MainLoop.callAppropriateSystemMethod (system, "setVerticalSpeed", newSpeed);
	}

	public void setHorizontalSpeed(System.Single newSpeed)
	{
		MainLoop.callAppropriateSystemMethod (system, "setHorizontalSpeed", newSpeed);
	}

	public void onScroll()
	{
		MainLoop.callAppropriateSystemMethod (system, "onScroll", null);
	}

}
