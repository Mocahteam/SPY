using UnityEngine;
using FYFY;

public class TooltipKeyboardSystem_wrapper : BaseWrapper
{
	public Tooltip tooltipUI_Keyboard;
	public UnityEngine.GameObject tooltipUI_Pointer;
	public UnityEngine.EventSystems.EventSystem eventSystem;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "tooltipUI_Keyboard", tooltipUI_Keyboard);
		MainLoop.initAppropriateSystemField (system, "tooltipUI_Pointer", tooltipUI_Pointer);
		MainLoop.initAppropriateSystemField (system, "eventSystem", eventSystem);
	}

}
