using UnityEngine;
using FYFY;

public class ModeManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject playButtonAmount;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "playButtonAmount", playButtonAmount);
	}

	public void setPlayingMode()
	{
		MainLoop.callAppropriateSystemMethod (system, "setPlayingMode", null);
	}

	public void setEditMode()
	{
		MainLoop.callAppropriateSystemMethod (system, "setEditMode", null);
	}

}
