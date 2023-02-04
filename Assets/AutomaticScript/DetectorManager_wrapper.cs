using UnityEngine;
using FYFY;

public class DetectorManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject LevelGO;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "LevelGO", LevelGO);
	}

	public void updateDetectors()
	{
		MainLoop.callAppropriateSystemMethod (system, "updateDetectors", null);
	}

}
