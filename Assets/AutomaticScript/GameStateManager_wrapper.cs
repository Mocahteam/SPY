using UnityEngine;
using FYFY;

public class GameStateManager_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void SaveState()
	{
		MainLoop.callAppropriateSystemMethod (system, "SaveState", null);
	}

	public void LoadState()
	{
		MainLoop.callAppropriateSystemMethod (system, "LoadState", null);
	}

}
