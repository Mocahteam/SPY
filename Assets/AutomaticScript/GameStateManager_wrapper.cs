using UnityEngine;
using FYFY;

public class GameStateManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject playButtonAmount;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "playButtonAmount", playButtonAmount);
	}

	public void LoadState()
	{
		MainLoop.callAppropriateSystemMethod (system, "LoadState", null);
	}

}
