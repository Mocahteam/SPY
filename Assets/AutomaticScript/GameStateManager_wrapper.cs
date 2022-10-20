using UnityEngine;
using FYFY;

public class GameStateManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject playButtonAmount;
	public UnityEngine.GameObject level;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "playButtonAmount", playButtonAmount);
		MainLoop.initAppropriateSystemField (system, "level", level);
	}

	public void LoadState()
	{
		MainLoop.callAppropriateSystemMethod (system, "LoadState", null);
	}

}
