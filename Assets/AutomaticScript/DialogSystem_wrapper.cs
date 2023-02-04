using UnityEngine;
using FYFY;

public class DialogSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject LevelGO;
	public UnityEngine.GameObject dialogPanel;
	public UnityEngine.GameObject showDialogsMenu;
	public UnityEngine.GameObject showDialogsBottom;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "LevelGO", LevelGO);
		MainLoop.initAppropriateSystemField (system, "dialogPanel", dialogPanel);
		MainLoop.initAppropriateSystemField (system, "showDialogsMenu", showDialogsMenu);
		MainLoop.initAppropriateSystemField (system, "showDialogsBottom", showDialogsBottom);
	}

	public void showDialogPanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "showDialogPanel", null);
	}

	public void nextDialog()
	{
		MainLoop.callAppropriateSystemMethod (system, "nextDialog", null);
	}

	public void prevDialog()
	{
		MainLoop.callAppropriateSystemMethod (system, "prevDialog", null);
	}

	public void setActiveOKButton(System.Boolean active)
	{
		MainLoop.callAppropriateSystemMethod (system, "setActiveOKButton", active);
	}

	public void setActiveNextButton(System.Boolean active)
	{
		MainLoop.callAppropriateSystemMethod (system, "setActiveNextButton", active);
	}

	public void setActivePrevButton(System.Boolean active)
	{
		MainLoop.callAppropriateSystemMethod (system, "setActivePrevButton", active);
	}

	public void closeDialogPanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "closeDialogPanel", null);
	}

}
