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

	public void StartNextPathAnimation(UnityEngine.GameObject pathGO)
	{
		MainLoop.callAppropriateSystemMethod (system, "StartNextPathAnimation", pathGO);
	}

	public void ForceDoorSync()
	{
		MainLoop.callAppropriateSystemMethod (system, "ForceDoorSync", null);
	}

}
