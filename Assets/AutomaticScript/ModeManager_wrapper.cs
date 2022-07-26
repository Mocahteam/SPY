using UnityEngine;
using FYFY;

public class ModeManager_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
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
