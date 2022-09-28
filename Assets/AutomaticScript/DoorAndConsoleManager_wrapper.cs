using UnityEngine;
using FYFY;

public class DoorAndConsoleManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject doorPathPrefab;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "doorPathPrefab", doorPathPrefab);
	}

}
