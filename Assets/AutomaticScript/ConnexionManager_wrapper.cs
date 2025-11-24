using UnityEngine;
using FYFY;

public class ConnexionManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject prefabGameData;
	public UnityEngine.GameObject loadingScreen;
	public TMPro.TMP_Text logs;
	public TMPro.TMP_Text progress;
	public TMPro.TMP_Text SPYVersion;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "prefabGameData", prefabGameData);
		MainLoop.initAppropriateSystemField (system, "loadingScreen", loadingScreen);
		MainLoop.initAppropriateSystemField (system, "logs", logs);
		MainLoop.initAppropriateSystemField (system, "progress", progress);
		MainLoop.initAppropriateSystemField (system, "SPYVersion", SPYVersion);
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

	public void synchUserData(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "synchUserData", go);
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
