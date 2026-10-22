using UnityEngine;
using FYFY;

public class TitleScreenSystem_wrapper : BaseWrapper
{
	public UnityEngine.UI.Button continueButton;
	public UnityEngine.GameObject trainingButton;
	public UnityEngine.GameObject gameSelector;
	public UnityEngine.GameObject TileScenarioPrefab;
	public UnityEngine.GameObject TileMissionPrefab;
	public UnityEngine.GameObject quitButton;
	public TMPro.TMP_Text SPYVersion;
	public CurrentSettingsValues currentSettingsValues;
	public UnityEngine.Transform profilPanel;
	public UnityEngine.GameObject newAvatarPanel;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "continueButton", continueButton);
		MainLoop.initAppropriateSystemField (system, "trainingButton", trainingButton);
		MainLoop.initAppropriateSystemField (system, "gameSelector", gameSelector);
		MainLoop.initAppropriateSystemField (system, "TileScenarioPrefab", TileScenarioPrefab);
		MainLoop.initAppropriateSystemField (system, "TileMissionPrefab", TileMissionPrefab);
		MainLoop.initAppropriateSystemField (system, "quitButton", quitButton);
		MainLoop.initAppropriateSystemField (system, "SPYVersion", SPYVersion);
		MainLoop.initAppropriateSystemField (system, "currentSettingsValues", currentSettingsValues);
		MainLoop.initAppropriateSystemField (system, "profilPanel", profilPanel);
		MainLoop.initAppropriateSystemField (system, "newAvatarPanel", newAvatarPanel);
	}

	public void ImportLevelOrScenario(System.String content)
	{
		MainLoop.callAppropriateSystemMethod (system, "ImportLevelOrScenario", content);
	}

	public void DisplayScenarioList()
	{
		MainLoop.callAppropriateSystemMethod (system, "DisplayScenarioList", null);
	}

	public void SetDraggingState(System.Boolean newState)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetDraggingState", newState);
	}

	public void ShowLevels(GameKeys keys)
	{
		MainLoop.callAppropriateSystemMethod (system, "ShowLevels", keys);
	}

	public void ShowDetails(GameKeys keys)
	{
		MainLoop.callAppropriateSystemMethod (system, "ShowDetails", keys);
	}

	public void ContinueScenario()
	{
		MainLoop.callAppropriateSystemMethod (system, "ContinueScenario", null);
	}

	public void LaunchLevel(GameKeys gk)
	{
		MainLoop.callAppropriateSystemMethod (system, "LaunchLevel", gk);
	}

	public void LaunchLevelEditor()
	{
		MainLoop.callAppropriateSystemMethod (system, "LaunchLevelEditor", null);
	}

	public void LaunchScenarioEditor()
	{
		MainLoop.callAppropriateSystemMethod (system, "LaunchScenarioEditor", null);
	}

	public void LaunchConnexionScene()
	{
		MainLoop.callAppropriateSystemMethod (system, "LaunchConnexionScene", null);
	}

	public void QuitGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "QuitGame", null);
	}

	public void SendUserData()
	{
		MainLoop.callAppropriateSystemMethod (system, "SendUserData", null);
	}

}
