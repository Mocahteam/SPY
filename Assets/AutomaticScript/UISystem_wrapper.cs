using UnityEngine;
using FYFY;

public class UISystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject buttonPlay;
	public UnityEngine.GameObject buttonContinue;
	public UnityEngine.GameObject buttonStop;
	public UnityEngine.GameObject buttonPause;
	public UnityEngine.GameObject buttonStep;
	public UnityEngine.GameObject buttonSpeed;
	public UnityEngine.GameObject buttonReset;
	public UnityEngine.GameObject endPanel;
	public UnityEngine.GameObject dialogPanel;
	public UnityEngine.GameObject editableScriptContainer;
	public UnityEngine.GameObject libraryPanel;
	public UnityEngine.GameObject EditableContainer;
	public UnityEngine.GameObject prefabViewportEditableContainer;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "buttonPlay", buttonPlay);
		MainLoop.initAppropriateSystemField (system, "buttonContinue", buttonContinue);
		MainLoop.initAppropriateSystemField (system, "buttonStop", buttonStop);
		MainLoop.initAppropriateSystemField (system, "buttonPause", buttonPause);
		MainLoop.initAppropriateSystemField (system, "buttonStep", buttonStep);
		MainLoop.initAppropriateSystemField (system, "buttonSpeed", buttonSpeed);
		MainLoop.initAppropriateSystemField (system, "buttonReset", buttonReset);
		MainLoop.initAppropriateSystemField (system, "endPanel", endPanel);
		MainLoop.initAppropriateSystemField (system, "dialogPanel", dialogPanel);
		MainLoop.initAppropriateSystemField (system, "editableScriptContainer", editableScriptContainer);
		MainLoop.initAppropriateSystemField (system, "libraryPanel", libraryPanel);
		MainLoop.initAppropriateSystemField (system, "EditableContainer", EditableContainer);
		MainLoop.initAppropriateSystemField (system, "prefabViewportEditableContainer", prefabViewportEditableContainer);
	}

	public void resetScript(System.Boolean refund)
	{
		MainLoop.callAppropriateSystemMethod (system, "resetScript", refund);
	}

	public void setImageSprite()
	{
		MainLoop.callAppropriateSystemMethod (system, "setImageSprite", null);
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

	public void reloadScene()
	{
		MainLoop.callAppropriateSystemMethod (system, "reloadScene", null);
	}

	public void returnToTitleScreen()
	{
		MainLoop.callAppropriateSystemMethod (system, "returnToTitleScreen", null);
	}

	public void nextLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "nextLevel", null);
	}

	public void retry()
	{
		MainLoop.callAppropriateSystemMethod (system, "retry", null);
	}

	public void reloadState()
	{
		MainLoop.callAppropriateSystemMethod (system, "reloadState", null);
	}

	public void stopScript()
	{
		MainLoop.callAppropriateSystemMethod (system, "stopScript", null);
	}

	public void applyScriptToPlayer()
	{
		MainLoop.callAppropriateSystemMethod (system, "applyScriptToPlayer", null);
	}

	public void addContainer()
	{
		MainLoop.callAppropriateSystemMethod (system, "addContainer", null);
	}

}
