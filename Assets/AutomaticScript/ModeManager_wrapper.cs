using UnityEngine;
using FYFY;

public class ModeManager_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void SetPlayingMode()
	{
		MainLoop.callAppropriateSystemMethod (system, "SetPlayingMode", null);
	}

	public void SetEditMode()
	{
		MainLoop.callAppropriateSystemMethod (system, "SetEditMode", null);
	}

}
