using UnityEngine;
using FYFY;

public class DoorAndConsoleManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject LevelGO;
	public UnityEngine.GameObject doorPathPrefab;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "LevelGO", LevelGO);
		MainLoop.initAppropriateSystemField (system, "doorPathPrefab", doorPathPrefab);
	}

	public void startNextPathAnimation(UnityEngine.GameObject pathGO)
	{
		MainLoop.callAppropriateSystemMethod (system, "startNextPathAnimation", pathGO);
	}

	public void forceDoorSync()
	{
		MainLoop.callAppropriateSystemMethod (system, "forceDoorSync", null);
	}

}
