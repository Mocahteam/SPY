using UnityEngine;
using FYFY;

public class ScrollSystem_wrapper : BaseWrapper
{
	public UnityEngine.UI.ScrollRect scrollRect;
	public UnityEngine.RectTransform autoScrollUp;
	public UnityEngine.RectTransform autoScrollDown;
	public UnityEngine.RectTransform autoScrollLeft;
	public UnityEngine.RectTransform autoScrollRight;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "scrollRect", scrollRect);
		MainLoop.initAppropriateSystemField (system, "autoScrollUp", autoScrollUp);
		MainLoop.initAppropriateSystemField (system, "autoScrollDown", autoScrollDown);
		MainLoop.initAppropriateSystemField (system, "autoScrollLeft", autoScrollLeft);
		MainLoop.initAppropriateSystemField (system, "autoScrollRight", autoScrollRight);
	}

	public void setVerticalSpeed(System.Single newSpeed)
	{
		MainLoop.callAppropriateSystemMethod (system, "setVerticalSpeed", newSpeed);
	}

	public void setHorizontalSpeed(System.Single newSpeed)
	{
		MainLoop.callAppropriateSystemMethod (system, "setHorizontalSpeed", newSpeed);
	}

}
