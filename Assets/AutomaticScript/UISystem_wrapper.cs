using UnityEngine;
using FYFY;

public class UISystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject LevelGO;
	public UnityEngine.GameObject buttonExecute;
	public UnityEngine.GameObject buttonPause;
	public UnityEngine.GameObject buttonNextStep;
	public UnityEngine.GameObject buttonContinue;
	public UnityEngine.GameObject buttonSpeed;
	public UnityEngine.GameObject buttonStop;
	public UnityEngine.GameObject canvas;
	public UnityEngine.GameObject libraryPanel;
	public CurrentSettingsValues currentSettingsValues;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "LevelGO", LevelGO);
		MainLoop.initAppropriateSystemField (system, "buttonExecute", buttonExecute);
		MainLoop.initAppropriateSystemField (system, "buttonPause", buttonPause);
		MainLoop.initAppropriateSystemField (system, "buttonNextStep", buttonNextStep);
		MainLoop.initAppropriateSystemField (system, "buttonContinue", buttonContinue);
		MainLoop.initAppropriateSystemField (system, "buttonSpeed", buttonSpeed);
		MainLoop.initAppropriateSystemField (system, "buttonStop", buttonStop);
		MainLoop.initAppropriateSystemField (system, "canvas", canvas);
		MainLoop.initAppropriateSystemField (system, "libraryPanel", libraryPanel);
		MainLoop.initAppropriateSystemField (system, "currentSettingsValues", currentSettingsValues);
	}

	public void ReturnToTitleScreen()
	{
		MainLoop.callAppropriateSystemMethod (system, "ReturnToTitleScreen", null);
	}

	public void NextLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "NextLevel", null);
	}

	public void Retry()
	{
		MainLoop.callAppropriateSystemMethod (system, "Retry", null);
	}

}
