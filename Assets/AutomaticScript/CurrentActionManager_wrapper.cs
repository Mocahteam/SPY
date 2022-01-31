using UnityEngine;
using FYFY;

public class CurrentActionManager_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void firstAction(UnityEngine.GameObject buttonStop)
	{
		MainLoop.callAppropriateSystemMethod (system, "firstAction", buttonStop);
	}

}
