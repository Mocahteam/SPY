using UnityEngine;
using FYFY;

public class CurrentActionManager_wrapper : BaseWrapper
{
	public UnityEngine.Transform editableContainers;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "editableContainers", editableContainers);
	}

}
