using UnityEngine;
using FYFY;

public class HotkeySystem_wrapper : BaseWrapper
{
	public UnityEngine.UI.Button mainMenu;
	public UnityEngine.UI.Button closeMainMenu;
	public HotKeysSpec hotKeys;
	public System.Boolean cancelNextCancel_act;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "mainMenu", mainMenu);
		MainLoop.initAppropriateSystemField (system, "closeMainMenu", closeMainMenu);
		MainLoop.initAppropriateSystemField (system, "hotKeys", hotKeys);
		MainLoop.initAppropriateSystemField (system, "cancelNextCancel_act", cancelNextCancel_act);
	}

	public void OnKeyboardLayoutDefined(System.String data)
	{
		MainLoop.callAppropriateSystemMethod (system, "OnKeyboardLayoutDefined", data);
	}

}
