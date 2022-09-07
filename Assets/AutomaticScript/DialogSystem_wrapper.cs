using UnityEngine;
using FYFY;

public class DialogSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject dialogPanel;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "dialogPanel", dialogPanel);
	}

	public void showDialogPanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "showDialogPanel", null);
	}

	public void nextDialog()
	{
		MainLoop.callAppropriateSystemMethod (system, "nextDialog", null);
	}

	public void setActiveOKButton(System.Boolean active)
	{
		MainLoop.callAppropriateSystemMethod (system, "setActiveOKButton", active);
	}

	public void setActiveNextButton(System.Boolean active)
	{
		MainLoop.callAppropriateSystemMethod (system, "setActiveNextButton", active);
	}

	public void closeDialogPanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "closeDialogPanel", null);
	}

	public void setImageSprite()
	{
		MainLoop.callAppropriateSystemMethod (system, "setImageSprite", null);
	}

}
