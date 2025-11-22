using UnityEngine;
using FYFY;

public class UINavigationManager_wrapper : BaseWrapper
{
	public System.Collections.Generic.List<UnityEngine.GameObject> autoFocusPrority;
	public UnityEngine.EventSystems.EventSystem eventSystem;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "autoFocusPrority", autoFocusPrority);
		MainLoop.initAppropriateSystemField (system, "eventSystem", eventSystem);
	}

}
