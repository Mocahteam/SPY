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
	public UnityEngine.GameObject MainPanel;
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
		MainLoop.initAppropriateSystemField (system, "MainPanel", MainPanel);
		MainLoop.initAppropriateSystemField (system, "TouchToContinue", TouchToContinue);
		MainLoop.initAppropriateSystemField (system, "CinematicPanel", CinematicPanel);
		MainLoop.initAppropriateSystemField (system, "currentSettingsValues", currentSettingsValues);
	}

	public void ForceLaunch()
	{
		MainLoop.callAppropriateSystemMethod (system, "ForceLaunch", null);
	}

	public void GetProgression(TMPro.TMP_InputField idSession)
	{
		MainLoop.callAppropriateSystemMethod (system, "GetProgression", idSession);
	}

	public void NewGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "NewGame", null);
	}

	public void SynchUserData()
	{
		MainLoop.callAppropriateSystemMethod (system, "SynchUserData", null);
	}

	public void AskToLoadLevel(System.String levelToLoad)
	{
		MainLoop.callAppropriateSystemMethod (system, "AskToLoadLevel", levelToLoad);
	}

	public void EnableSendStatement()
	{
		MainLoop.callAppropriateSystemMethod (system, "EnableSendStatement", null);
	}

}
