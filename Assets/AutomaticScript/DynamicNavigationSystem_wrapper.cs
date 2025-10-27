using UnityEngine;
using FYFY;

public class DynamicNavigationSystem_wrapper : BaseWrapper
{
	public UnityEngine.EventSystems.EventSystem current;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "current", current);
	}

}
