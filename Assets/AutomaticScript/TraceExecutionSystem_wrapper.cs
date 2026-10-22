using UnityEngine;
using FYFY;

public class TraceExecutionSystem_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void OnActionSelected()
	{
		MainLoop.callAppropriateSystemMethod (system, "OnActionSelected", null);
	}

}
