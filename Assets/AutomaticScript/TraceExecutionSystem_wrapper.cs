using UnityEngine;
using FYFY;

public class TraceExecutionSystem_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void onActionSelected()
	{
		MainLoop.callAppropriateSystemMethod (system, "onActionSelected", null);
	}

}
