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
	public UnityEngine.GameObject endPanel;
	public UnityEngine.GameObject dialogPanel;
	public UnityEngine.GameObject editableScriptContainer;
	public UnityEngine.GameObject libraryPanel;
	public UnityEngine.GameObject EditableContainer;
	public UnityEngine.GameObject EditableCanvas;
	public UnityEngine.GameObject prefabViewportScriptContainer;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "buttonPlay", buttonPlay);
		MainLoop.initAppropriateSystemField (system, "buttonContinue", buttonContinue);
		MainLoop.initAppropriateSystemField (system, "buttonStop", buttonStop);
		MainLoop.initAppropriateSystemField (system, "buttonPause", buttonPause);
		MainLoop.initAppropriateSystemField (system, "buttonStep", buttonStep);
		MainLoop.initAppropriateSystemField (system, "buttonSpeed", buttonSpeed);
		MainLoop.initAppropriateSystemField (system, "endPanel", endPanel);
		MainLoop.initAppropriateSystemField (system, "dialogPanel", dialogPanel);
		MainLoop.initAppropriateSystemField (system, "editableScriptContainer", editableScriptContainer);
		MainLoop.initAppropriateSystemField (system, "libraryPanel", libraryPanel);
		MainLoop.initAppropriateSystemField (system, "EditableContainer", EditableContainer);
		MainLoop.initAppropriateSystemField (system, "EditableCanvas", EditableCanvas);
		MainLoop.initAppropriateSystemField (system, "prefabViewportScriptContainer", prefabViewportScriptContainer);
	}

	public void startUpdatePlayButton()
	{
		MainLoop.callAppropriateSystemMethod (system, "startUpdatePlayButton", null);
	}

	public void refreshUIButton()
	{
		MainLoop.callAppropriateSystemMethod (system, "refreshUIButton", null);
	}

	public void refreshUINameContainer()
	{
		MainLoop.callAppropriateSystemMethod (system, "refreshUINameContainer", null);
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

	public void restartScene()
	{
		MainLoop.callAppropriateSystemMethod (system, "restartScene", null);
	}

	public void returnToTitleScreen()
	{
		MainLoop.callAppropriateSystemMethod (system, "returnToTitleScreen", null);
	}

	public void initZeroVariableLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "initZeroVariableLevel", null);
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

	public void newNameContainer(System.String name)
	{
		MainLoop.callAppropriateSystemMethod (system, "newNameContainer", name);
	}

	public void verticalName(System.String name)
	{
		MainLoop.callAppropriateSystemMethod (system, "verticalName", name);
	}

	public void horizontalName(System.String name)
	{
		MainLoop.callAppropriateSystemMethod (system, "horizontalName", name);
	}

	public void setContainerName()
	{
		MainLoop.callAppropriateSystemMethod (system, "setContainerName", null);
	}

	public void noChangeName(System.String name)
	{
		MainLoop.callAppropriateSystemMethod (system, "noChangeName", name);
	}

	public void cancelChangeNameContainer(System.String name)
	{
		MainLoop.callAppropriateSystemMethod (system, "cancelChangeNameContainer", name);
	}

}
