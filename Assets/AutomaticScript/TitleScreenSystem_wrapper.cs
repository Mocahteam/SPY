using UnityEngine;
using FYFY;

public class TitleScreenSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject menuCampaigns;
	public UnityEngine.GameObject menuMissions;
	public UnityEngine.GameObject listOfCampaigns;
	public UnityEngine.GameObject listOfLevels;
	public UnityEngine.UI.Button continueButton;
	public UnityEngine.GameObject playButton;
	public UnityEngine.GameObject quitButton;
	public UnityEngine.GameObject detailsCampaign;
	public TMPro.TMP_Text SPYVersion;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "menuCampaigns", menuCampaigns);
		MainLoop.initAppropriateSystemField (system, "menuMissions", menuMissions);
		MainLoop.initAppropriateSystemField (system, "listOfCampaigns", listOfCampaigns);
		MainLoop.initAppropriateSystemField (system, "listOfLevels", listOfLevels);
		MainLoop.initAppropriateSystemField (system, "continueButton", continueButton);
		MainLoop.initAppropriateSystemField (system, "playButton", playButton);
		MainLoop.initAppropriateSystemField (system, "quitButton", quitButton);
		MainLoop.initAppropriateSystemField (system, "detailsCampaign", detailsCampaign);
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

	public void refreshCompetencies(UnityEngine.Transform content)
	{
		MainLoop.callAppropriateSystemMethod (system, "refreshCompetencies", content);
	}

	public void continueScenario()
	{
		MainLoop.callAppropriateSystemMethod (system, "continueScenario", null);
	}

	public void launchLevelEditor()
	{
		MainLoop.callAppropriateSystemMethod (system, "launchLevelEditor", null);
	}

	public void launchScenarioEditor()
	{
		MainLoop.callAppropriateSystemMethod (system, "launchScenarioEditor", null);
	}

	public void quitGame()
	{
		MainLoop.callAppropriateSystemMethod (system, "quitGame", null);
	}

}
