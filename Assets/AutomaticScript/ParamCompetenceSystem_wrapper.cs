using UnityEngine;
using FYFY;

public class ParamCompetenceSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject panelInfoComp;
	public UnityEngine.GameObject panelInfoUser;
	public UnityEngine.GameObject prefabComp;
	public UnityEngine.GameObject ContentCompMenu;
	public TMPro.TMP_Text messageForUser;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "panelInfoComp", panelInfoComp);
		MainLoop.initAppropriateSystemField (system, "panelInfoUser", panelInfoUser);
		MainLoop.initAppropriateSystemField (system, "prefabComp", prefabComp);
		MainLoop.initAppropriateSystemField (system, "ContentCompMenu", ContentCompMenu);
		MainLoop.initAppropriateSystemField (system, "messageForUser", messageForUser);
	}

	public void openPanelSelectComp()
	{
		MainLoop.callAppropriateSystemMethod (system, "openPanelSelectComp", null);
	}

	public void cleanCompPanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "cleanCompPanel", null);
	}

	public void showCompatibleLevels()
	{
		MainLoop.callAppropriateSystemMethod (system, "showCompatibleLevels", null);
	}

	public void infoCompetence(Competency comp)
	{
		MainLoop.callAppropriateSystemMethod (system, "infoCompetence", comp);
	}

	public void displayMessageUser(System.String message)
	{
		MainLoop.callAppropriateSystemMethod (system, "displayMessageUser", message);
	}

	public void refreshUI(UnityEngine.RectTransform competency)
	{
		MainLoop.callAppropriateSystemMethod (system, "refreshUI", competency);
	}

}
