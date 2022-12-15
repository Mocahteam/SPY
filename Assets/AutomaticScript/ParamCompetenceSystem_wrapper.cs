using UnityEngine;
using FYFY;

public class ParamCompetenceSystem_wrapper : BaseWrapper
{
	public TMPro.TMP_Dropdown referentialSelector;
	public UnityEngine.GameObject panelInfoComp;
	public UnityEngine.GameObject prefabComp;
	public UnityEngine.GameObject ContentCompMenu;
	public UnityEngine.GameObject compatibleLevelsPanel;
	public UnityEngine.GameObject competenciesPanel;
	public UnityEngine.GameObject levelCompatiblePrefab;
	public UnityEngine.GameObject contentListOfCompatibleLevel;
	public UnityEngine.GameObject contentInfoCompatibleLevel;
	public UnityEngine.GameObject deletableElement;
	public UnityEngine.GameObject contentScenario;
	public UnityEngine.UI.Button testLevel;
	public UnityEngine.UI.Button addToScenario;
	public UnityEngine.GameObject savingPanel;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "referentialSelector", referentialSelector);
		MainLoop.initAppropriateSystemField (system, "panelInfoComp", panelInfoComp);
		MainLoop.initAppropriateSystemField (system, "prefabComp", prefabComp);
		MainLoop.initAppropriateSystemField (system, "ContentCompMenu", ContentCompMenu);
		MainLoop.initAppropriateSystemField (system, "compatibleLevelsPanel", compatibleLevelsPanel);
		MainLoop.initAppropriateSystemField (system, "competenciesPanel", competenciesPanel);
		MainLoop.initAppropriateSystemField (system, "levelCompatiblePrefab", levelCompatiblePrefab);
		MainLoop.initAppropriateSystemField (system, "contentListOfCompatibleLevel", contentListOfCompatibleLevel);
		MainLoop.initAppropriateSystemField (system, "contentInfoCompatibleLevel", contentInfoCompatibleLevel);
		MainLoop.initAppropriateSystemField (system, "deletableElement", deletableElement);
		MainLoop.initAppropriateSystemField (system, "contentScenario", contentScenario);
		MainLoop.initAppropriateSystemField (system, "testLevel", testLevel);
		MainLoop.initAppropriateSystemField (system, "addToScenario", addToScenario);
		MainLoop.initAppropriateSystemField (system, "savingPanel", savingPanel);
	}

	public void loadPanelSelectComp()
	{
		MainLoop.callAppropriateSystemMethod (system, "loadPanelSelectComp", null);
	}

	public void createCompetencies(System.Int32 referentialId)
	{
		MainLoop.callAppropriateSystemMethod (system, "createCompetencies", referentialId);
	}

	public void cleanCompPanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "cleanCompPanel", null);
	}

	public void showCompatibleLevels(System.Boolean filter)
	{
		MainLoop.callAppropriateSystemMethod (system, "showCompatibleLevels", filter);
	}

	public void showLevelInfo(System.String path)
	{
		MainLoop.callAppropriateSystemMethod (system, "showLevelInfo", path);
	}

	public void addCurrentLevelToScenario()
	{
		MainLoop.callAppropriateSystemMethod (system, "addCurrentLevelToScenario", null);
	}

	public void infoCompetence(Competency comp)
	{
		MainLoop.callAppropriateSystemMethod (system, "infoCompetence", comp);
	}

	public void removeLevelFromScenario(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "removeLevelFromScenario", go);
	}

	public void moveLevelInScenario()
	{
		MainLoop.callAppropriateSystemMethod (system, "moveLevelInScenario", null);
	}

	public void refreshUI(UnityEngine.RectTransform competency)
	{
		MainLoop.callAppropriateSystemMethod (system, "refreshUI", competency);
	}

	public void saveScenario(TMPro.TMP_InputField scenarioName)
	{
		MainLoop.callAppropriateSystemMethod (system, "saveScenario", scenarioName);
	}

	public void displaySavingPanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "displaySavingPanel", null);
	}

}
