using UnityEngine;
using FYFY;

public class UINavigationManager_wrapper : BaseWrapper
{
	public System.Collections.Generic.List<UnityEngine.GameObject> autoFocusProrityOnTab;
	public UnityEngine.EventSystems.EventSystem eventSystem;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "autoFocusProrityOnTab", autoFocusProrityOnTab);
		MainLoop.initAppropriateSystemField (system, "eventSystem", eventSystem);
	}

}
