using UnityEngine;
using FYFY;

public class CurrentActionManager_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void removeAllCurrentActions()
	{
		MainLoop.callAppropriateSystemMethod (system, "removeAllCurrentActions", null);
	}

}
