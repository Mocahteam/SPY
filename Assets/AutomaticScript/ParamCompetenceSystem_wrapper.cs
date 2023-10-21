using UnityEngine;
using FYFY;

public class ParamCompetenceSystem_wrapper : BaseWrapper
{
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
	public UnityEngine.UI.Button testLevelBt;
	public UnityEngine.UI.Button downloadLevelBt;
	public UnityEngine.UI.Button addToScenario;
	public UnityEngine.GameObject savingPanel;
	public UnityEngine.GameObject editBriefingPanel;
	public UnityEngine.GameObject briefingItemPrefab;
	public TMPro.TMP_InputField scenarioAbstract;
	public TMPro.TMP_InputField scenarioName;
	public UnityEngine.GameObject scenarioContent;
	public UnityEngine.GameObject loadingScenarioContent;
	public UnityEngine.GameObject mainCanvas;
	public TMPro.TMP_InputField levelFilterByName;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
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
		MainLoop.initAppropriateSystemField (system, "testLevelBt", testLevelBt);
		MainLoop.initAppropriateSystemField (system, "downloadLevelBt", downloadLevelBt);
		MainLoop.initAppropriateSystemField (system, "addToScenario", addToScenario);
		MainLoop.initAppropriateSystemField (system, "savingPanel", savingPanel);
		MainLoop.initAppropriateSystemField (system, "editBriefingPanel", editBriefingPanel);
		MainLoop.initAppropriateSystemField (system, "briefingItemPrefab", briefingItemPrefab);
		MainLoop.initAppropriateSystemField (system, "scenarioAbstract", scenarioAbstract);
		MainLoop.initAppropriateSystemField (system, "scenarioName", scenarioName);
		MainLoop.initAppropriateSystemField (system, "scenarioContent", scenarioContent);
		MainLoop.initAppropriateSystemField (system, "loadingScenarioContent", loadingScenarioContent);
		MainLoop.initAppropriateSystemField (system, "mainCanvas", mainCanvas);
		MainLoop.initAppropriateSystemField (system, "levelFilterByName", levelFilterByName);
	}

	public void refreshCompetencies()
	{
		MainLoop.callAppropriateSystemMethod (system, "refreshCompetencies", null);
	}

	public void selectCompetencies(System.Int32 referentialId)
	{
		MainLoop.callAppropriateSystemMethod (system, "selectCompetencies", referentialId);
	}

	public void traceLoadindScenarioEditor()
	{
		MainLoop.callAppropriateSystemMethod (system, "traceLoadindScenarioEditor", null);
	}

	public void showCompatibleLevels()
	{
		MainLoop.callAppropriateSystemMethod (system, "showCompatibleLevels", null);
	}

	public void resetFilters()
	{
		MainLoop.callAppropriateSystemMethod (system, "resetFilters", null);
	}

	public void filterCompatibleLevels(System.Boolean nameFiltering)
	{
		MainLoop.callAppropriateSystemMethod (system, "filterCompatibleLevels", nameFiltering);
	}

	public void displayLoadingPanel(System.String filter)
	{
		MainLoop.callAppropriateSystemMethod (system, "displayLoadingPanel", filter);
	}

	public void loadScenario()
	{
		MainLoop.callAppropriateSystemMethod (system, "loadScenario", null);
	}

	public void onScenarioSelected(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "onScenarioSelected", go);
	}

	public void addCurrentLevelToScenario()
	{
		MainLoop.callAppropriateSystemMethod (system, "addCurrentLevelToScenario", null);
	}

	public void infoCompetence(Competency comp)
	{
		MainLoop.callAppropriateSystemMethod (system, "infoCompetence", comp);
	}

	public void removeItemFromParent(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "removeItemFromParent", go);
	}

	public void refreshUI(UnityEngine.RectTransform competency)
	{
		MainLoop.callAppropriateSystemMethod (system, "refreshUI", competency);
	}

	public void saveScenario(TMPro.TMP_InputField scenarioName)
	{
		MainLoop.callAppropriateSystemMethod (system, "saveScenario", scenarioName);
	}

	public void displaySavingPanel(TMPro.TMP_InputField scenarName)
	{
		MainLoop.callAppropriateSystemMethod (system, "displaySavingPanel", scenarName);
	}

	public void showBriefingOverride(DataLevelBehaviour dataLevel)
	{
		MainLoop.callAppropriateSystemMethod (system, "showBriefingOverride", dataLevel);
	}

	public void saveBriefingOverride()
	{
		MainLoop.callAppropriateSystemMethod (system, "saveBriefingOverride", null);
	}

	public void addNewBriefing(UnityEngine.GameObject parent)
	{
		MainLoop.callAppropriateSystemMethod (system, "addNewBriefing", parent);
	}

	public void testLevel(DataLevelBehaviour dlb)
	{
		MainLoop.callAppropriateSystemMethod (system, "testLevel", dlb);
	}

	public void downloadLevel(DataLevelBehaviour dlb)
	{
		MainLoop.callAppropriateSystemMethod (system, "downloadLevel", dlb);
	}

}
