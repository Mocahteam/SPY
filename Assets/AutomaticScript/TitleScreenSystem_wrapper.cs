using UnityEngine;
using FYFY;

public class TitleScreenSystem_wrapper : BaseWrapper
{
	public UnityEngine.UI.Button continueButton;
	public UnityEngine.GameObject playButton;
	public UnityEngine.GameObject gameSelector;
	public UnityEngine.GameObject TileScenarioPrefab;
	public UnityEngine.GameObject TileMissionPrefab;
	public UnityEngine.GameObject quitButton;
	public TMPro.TMP_Text SPYVersion;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "continueButton", continueButton);
		MainLoop.initAppropriateSystemField (system, "playButton", playButton);
		MainLoop.initAppropriateSystemField (system, "gameSelector", gameSelector);
		MainLoop.initAppropriateSystemField (system, "TileScenarioPrefab", TileScenarioPrefab);
		MainLoop.initAppropriateSystemField (system, "TileMissionPrefab", TileMissionPrefab);
		MainLoop.initAppropriateSystemField (system, "quitButton", quitButton);
		MainLoop.initAppropriateSystemField (system, "SPYVersion", SPYVersion);
	}

	public void importLevelOrScenario(System.String content)
	{
		MainLoop.callAppropriateSystemMethod (system, "importLevelOrScenario", content);
	}

	public void displayScenarioList()
	{
		MainLoop.callAppropriateSystemMethod (system, "displayScenarioList", null);
	}

	public void showLevels(GameKeys keys)
	{
		MainLoop.callAppropriateSystemMethod (system, "showLevels", keys);
	}

	public void showDetails(GameKeys keys)
	{
		MainLoop.callAppropriateSystemMethod (system, "showDetails", keys);
	}

	public void refreshCompetencies()
	{
		MainLoop.callAppropriateSystemMethod (system, "refreshCompetencies", null);
	}

	public void continueScenario()
	{
		MainLoop.callAppropriateSystemMethod (system, "continueScenario", null);
	}

	public void launchLevel(GameKeys gk)
	{
		MainLoop.callAppropriateSystemMethod (system, "launchLevel", gk);
	}

	public void launchLevelEditor()
	{
		MainLoop.callAppropriateSystemMethod (system, "launchLevelEditor", null);
	}

	public void launchScenarioEditor()
	{
		MainLoop.callAppropriateSystemMethod (system, "launchScenarioEditor", null);
	}

	public void launchConnexionScene()
	{
		MainLoop.callAppropriateSystemMethod (system, "launchConnexionScene", null);
	}

	public void quitGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "quitGame", null);
	}

}
