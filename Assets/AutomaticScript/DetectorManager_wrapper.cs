using UnityEngine;
using FYFY;

public class DetectorManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject LevelGO;
	public UnityEngine.GameObject RedDetectorPrefab;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "LevelGO", LevelGO);
		MainLoop.initAppropriateSystemField (system, "RedDetectorPrefab", RedDetectorPrefab);
	}

	public void updateDetectors()
	{
		MainLoop.callAppropriateSystemMethod (system, "updateDetectors", null);
	}

}
