using UnityEngine;
using FYFY;

public class TitleScreenSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject prefabGameData;
	public UnityEngine.GameObject mainCanvas;
	public UnityEngine.GameObject mainMenu;
	public UnityEngine.GameObject compLevelButton;
	public UnityEngine.GameObject listOfCampaigns;
	public UnityEngine.GameObject listOfLevels;
	public UnityEngine.GameObject playButton;
	public UnityEngine.GameObject quitButton;
	public UnityEngine.GameObject levelEditorButton;
	public UnityEngine.GameObject loadingScreen;
	public UnityEngine.GameObject sessionIdPanel;
	public UnityEngine.GameObject deletableElement;
	public TMPro.TMP_InputField scenarioName;
	public TMPro.TMP_InputField scenarioAbstract;
	public UnityEngine.GameObject detailsCampaign;
	public UnityEngine.GameObject virtualKeyboard;
	public TMPro.TMP_Text progress;
	public TMPro.TMP_Text logs;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "prefabGameData", prefabGameData);
		MainLoop.initAppropriateSystemField (system, "mainCanvas", mainCanvas);
		MainLoop.initAppropriateSystemField (system, "mainMenu", mainMenu);
		MainLoop.initAppropriateSystemField (system, "compLevelButton", compLevelButton);
		MainLoop.initAppropriateSystemField (system, "listOfCampaigns", listOfCampaigns);
		MainLoop.initAppropriateSystemField (system, "listOfLevels", listOfLevels);
		MainLoop.initAppropriateSystemField (system, "playButton", playButton);
		MainLoop.initAppropriateSystemField (system, "quitButton", quitButton);
		MainLoop.initAppropriateSystemField (system, "levelEditorButton", levelEditorButton);
		MainLoop.initAppropriateSystemField (system, "loadingScreen", loadingScreen);
		MainLoop.initAppropriateSystemField (system, "sessionIdPanel", sessionIdPanel);
		MainLoop.initAppropriateSystemField (system, "deletableElement", deletableElement);
		MainLoop.initAppropriateSystemField (system, "scenarioName", scenarioName);
		MainLoop.initAppropriateSystemField (system, "scenarioAbstract", scenarioAbstract);
		MainLoop.initAppropriateSystemField (system, "detailsCampaign", detailsCampaign);
		MainLoop.initAppropriateSystemField (system, "virtualKeyboard", virtualKeyboard);
		MainLoop.initAppropriateSystemField (system, "progress", progress);
		MainLoop.initAppropriateSystemField (system, "logs", logs);
	}

	public void closeSettingsAndSelectNextFocusedButton(UnityEngine.GameObject settingsWindows)
	{
		MainLoop.callAppropriateSystemMethod (system, "closeSettingsAndSelectNextFocusedButton", settingsWindows);
	}

	public void synchUserData(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "synchUserData", go);
	}

	public void GetProgression(TMPro.TMP_InputField idSession)
	{
		MainLoop.callAppropriateSystemMethod (system, "GetProgression", idSession);
	}

	public void forceLaunch()
	{
		MainLoop.callAppropriateSystemMethod (system, "forceLaunch", null);
	}

	public void importLevelOrScenario(System.String content)
	{
		MainLoop.callAppropriateSystemMethod (system, "importLevelOrScenario", content);
	}

	public void displayScenarioList()
	{
		MainLoop.callAppropriateSystemMethod (system, "displayScenarioList", null);
	}

	public void delayRefreshCompetencies(UnityEngine.Transform content)
	{
		MainLoop.callAppropriateSystemMethod (system, "delayRefreshCompetencies", content);
	}

	public void launchLevelEditor()
	{
		MainLoop.callAppropriateSystemMethod (system, "launchLevelEditor", null);
	}

	public void askToLoadLevel(System.String levelToLoad)
	{
		MainLoop.callAppropriateSystemMethod (system, "askToLoadLevel", levelToLoad);
	}

	public void enableSendStatement()
	{
		MainLoop.callAppropriateSystemMethod (system, "enableSendStatement", null);
	}

	public void quitGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "quitGame", null);
	}

}
