using UnityEngine;
using FYFY;

public class EndGameManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject playButtonAmount;
	public UnityEngine.GameObject endPanel;
	public UnityEngine.AudioClip LoseSound;
	public UnityEngine.AudioClip VictorySound;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "playButtonAmount", playButtonAmount);
		MainLoop.initAppropriateSystemField (system, "endPanel", endPanel);
		MainLoop.initAppropriateSystemField (system, "LoseSound", LoseSound);
		MainLoop.initAppropriateSystemField (system, "VictorySound", VictorySound);
	}

	public void cancelEnd()
	{
		MainLoop.callAppropriateSystemMethod (system, "cancelEnd", null);
	}

}
