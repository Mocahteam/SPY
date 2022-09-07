using UnityEngine;
using FYFY;

public class DetectorManager_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void updateDetectors()
	{
		MainLoop.callAppropriateSystemMethod (system, "updateDetectors", null);
	}

}
