using UnityEngine;
using FYFY;

public class CurrentActionManager_wrapper : BaseWrapper
{
	public UnityEngine.Transform editableContainers;
	public UnityEngine.GameObject actionAvailable;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "editableContainers", editableContainers);
		MainLoop.initAppropriateSystemField (system, "actionAvailable", actionAvailable);
	}

}
