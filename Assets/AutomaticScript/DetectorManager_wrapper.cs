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

	public void updateDetectors()
	{
		MainLoop.callAppropriateSystemMethod (system, "updateDetectors", null);
	}

}
