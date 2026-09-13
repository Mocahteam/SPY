using UnityEngine;
using FYFY;

public class ConnexionManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject prefabGameData;
	public UnityEngine.GameObject loadingScreen;
	public TMPro.TMP_Text logs;
	public UnityEngine.GameObject forceLaunchButton;
	public TMPro.TMP_Text progress;
	public TMPro.TMP_Text SPYVersion;
	public UnityEngine.GameObject RightPanel;
	public UnityEngine.GameObject TouchToContinue;
	public UnityEngine.Transform CinematicPanel;
	public CurrentSettingsValues currentSettingsValues;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "prefabGameData", prefabGameData);
		MainLoop.initAppropriateSystemField (system, "loadingScreen", loadingScreen);
		MainLoop.initAppropriateSystemField (system, "logs", logs);
		MainLoop.initAppropriateSystemField (system, "forceLaunchButton", forceLaunchButton);
		MainLoop.initAppropriateSystemField (system, "progress", progress);
		MainLoop.initAppropriateSystemField (system, "SPYVersion", SPYVersion);
		MainLoop.initAppropriateSystemField (system, "RightPanel", RightPanel);
		MainLoop.initAppropriateSystemField (system, "TouchToContinue", TouchToContinue);
		MainLoop.initAppropriateSystemField (system, "CinematicPanel", CinematicPanel);
		MainLoop.initAppropriateSystemField (system, "currentSettingsValues", currentSettingsValues);
	}

	public void continueAfterTouch()
	{
		MainLoop.callAppropriateSystemMethod (system, "continueAfterTouch", null);
	}

	public void forceLaunch()
	{
		MainLoop.callAppropriateSystemMethod (system, "forceLaunch", null);
	}

	public void GetProgression(TMPro.TMP_InputField idSession)
	{
		MainLoop.callAppropriateSystemMethod (system, "GetProgression", idSession);
	}

	public void newGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "newGame", null);
	}

	public void synchUserData()
	{
		MainLoop.callAppropriateSystemMethod (system, "synchUserData", null);
	}

	public void askToLoadLevel(System.String levelToLoad)
	{
		MainLoop.callAppropriateSystemMethod (system, "askToLoadLevel", levelToLoad);
	}

	public void enableSendStatement()
	{
		MainLoop.callAppropriateSystemMethod (system, "enableSendStatement", null);
	}

}
