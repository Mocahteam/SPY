using UnityEngine;
using FYFY;

public class UISystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject buttonExecute;
	public UnityEngine.GameObject buttonPause;
	public UnityEngine.GameObject buttonNextStep;
	public UnityEngine.GameObject buttonContinue;
	public UnityEngine.GameObject buttonSpeed;
	public UnityEngine.GameObject buttonStop;
	public UnityEngine.GameObject menuEchap;
	public UnityEngine.GameObject buttonAddEditableContainer;
	public UnityEngine.GameObject endPanel;
	public UnityEngine.GameObject dialogPanel;
	public UnityEngine.GameObject canvas;
	public UnityEngine.GameObject libraryPanel;
	public UnityEngine.GameObject EditableCanvas;
	public UnityEngine.GameObject prefabViewportScriptContainer;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "buttonExecute", buttonExecute);
		MainLoop.initAppropriateSystemField (system, "buttonPause", buttonPause);
		MainLoop.initAppropriateSystemField (system, "buttonNextStep", buttonNextStep);
		MainLoop.initAppropriateSystemField (system, "buttonContinue", buttonContinue);
		MainLoop.initAppropriateSystemField (system, "buttonSpeed", buttonSpeed);
		MainLoop.initAppropriateSystemField (system, "buttonStop", buttonStop);
		MainLoop.initAppropriateSystemField (system, "menuEchap", menuEchap);
		MainLoop.initAppropriateSystemField (system, "buttonAddEditableContainer", buttonAddEditableContainer);
		MainLoop.initAppropriateSystemField (system, "endPanel", endPanel);
		MainLoop.initAppropriateSystemField (system, "dialogPanel", dialogPanel);
		MainLoop.initAppropriateSystemField (system, "canvas", canvas);
		MainLoop.initAppropriateSystemField (system, "libraryPanel", libraryPanel);
		MainLoop.initAppropriateSystemField (system, "EditableCanvas", EditableCanvas);
		MainLoop.initAppropriateSystemField (system, "prefabViewportScriptContainer", prefabViewportScriptContainer);
	}

	public void startUpdatePlayButton()
	{
		MainLoop.callAppropriateSystemMethod (system, "startUpdatePlayButton", null);
	}

	public void refreshUINameContainer()
	{
		MainLoop.callAppropriateSystemMethod (system, "refreshUINameContainer", null);
	}

	public void setExecutionView(System.Boolean value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setExecutionView", value);
	}

	public void saveHistory()
	{
		MainLoop.callAppropriateSystemMethod (system, "saveHistory", null);
	}

	public void resetScriptContainer(System.Boolean refund)
	{
		MainLoop.callAppropriateSystemMethod (system, "resetScriptContainer", refund);
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

	public void cancelEnd()
	{
		MainLoop.callAppropriateSystemMethod (system, "cancelEnd", null);
	}

	public void fillExecutablePanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "fillExecutablePanel", null);
	}

	public void copyEditableScriptsToExecutablePanels()
	{
		MainLoop.callAppropriateSystemMethod (system, "copyEditableScriptsToExecutablePanels", null);
	}

	public void CleanControlBlock(UnityEngine.Transform specialBlock)
	{
		MainLoop.callAppropriateSystemMethod (system, "CleanControlBlock", specialBlock);
	}

	public void addContainer()
	{
		MainLoop.callAppropriateSystemMethod (system, "addContainer", null);
	}

	public void addSpecificContainer()
	{
		MainLoop.callAppropriateSystemMethod (system, "addSpecificContainer", null);
	}

	public void removeContainer(UnityEngine.GameObject container)
	{
		MainLoop.callAppropriateSystemMethod (system, "removeContainer", container);
	}

	public void selectContainer(UIRootContainer container)
	{
		MainLoop.callAppropriateSystemMethod (system, "selectContainer", container);
	}

	public void newNameContainer(System.String newName)
	{
		MainLoop.callAppropriateSystemMethod (system, "newNameContainer", newName);
	}

	public void setContainerName()
	{
		MainLoop.callAppropriateSystemMethod (system, "setContainerName", null);
	}

	public void setActiveEscapeMenu()
	{
		MainLoop.callAppropriateSystemMethod (system, "setActiveEscapeMenu", null);
	}

}
