using UnityEngine;
using FYFY;

public class DetectorManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject endPanel;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "endPanel", endPanel);
	}

	public void detectCollision(System.Boolean on)
	{
		MainLoop.callAppropriateSystemMethod (system, "detectCollision", on);
	}

	public void updateDetector()
	{
		MainLoop.callAppropriateSystemMethod (system, "updateDetector", null);
	}

}
