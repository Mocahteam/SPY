using UnityEngine;
using FYFY;

public class GameStateManager_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void LoadState()
	{
		MainLoop.callAppropriateSystemMethod (system, "LoadState", null);
	}

}
