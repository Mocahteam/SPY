using UnityEngine;
using FYFY;

public class UISystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject LevelGO;
	public UnityEngine.GameObject buttonMenu;
	public UnityEngine.GameObject buttonExecute;
	public UnityEngine.GameObject buttonPause;
	public UnityEngine.GameObject buttonNextStep;
	public UnityEngine.GameObject buttonContinue;
	public UnityEngine.GameObject buttonSpeed;
	public UnityEngine.GameObject buttonStop;
	public UnityEngine.GameObject menuEchap;
	public UnityEngine.GameObject canvas;
	public UnityEngine.GameObject libraryPanel;
	public UnityEngine.GameObject virtualKeyboard;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "LevelGO", LevelGO);
		MainLoop.initAppropriateSystemField (system, "buttonMenu", buttonMenu);
		MainLoop.initAppropriateSystemField (system, "buttonExecute", buttonExecute);
		MainLoop.initAppropriateSystemField (system, "buttonPause", buttonPause);
		MainLoop.initAppropriateSystemField (system, "buttonNextStep", buttonNextStep);
		MainLoop.initAppropriateSystemField (system, "buttonContinue", buttonContinue);
		MainLoop.initAppropriateSystemField (system, "buttonSpeed", buttonSpeed);
		MainLoop.initAppropriateSystemField (system, "buttonStop", buttonStop);
		MainLoop.initAppropriateSystemField (system, "menuEchap", menuEchap);
		MainLoop.initAppropriateSystemField (system, "canvas", canvas);
		MainLoop.initAppropriateSystemField (system, "libraryPanel", libraryPanel);
		MainLoop.initAppropriateSystemField (system, "virtualKeyboard", virtualKeyboard);
	}

	public void setExecutionView(System.Boolean value)
	{
		MainLoop.callAppropriateSystemMethod (system, "setExecutionView", value);
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

	public void setActiveEscapeMenu()
	{
		MainLoop.callAppropriateSystemMethod (system, "setActiveEscapeMenu", null);
	}

}
