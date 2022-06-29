using UnityEngine;
using FYFY;

public class ParamCompetenceSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject panelSelectComp;
	public UnityEngine.GameObject panelInfoComp;
	public UnityEngine.GameObject panelInfoUser;
	public System.String pathParamComp;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "panelSelectComp", panelSelectComp);
		MainLoop.initAppropriateSystemField (system, "panelInfoComp", panelInfoComp);
		MainLoop.initAppropriateSystemField (system, "panelInfoUser", panelInfoUser);
		MainLoop.initAppropriateSystemField (system, "pathParamComp", pathParamComp);
	}

	public void openPanelSelectComp()
	{
		MainLoop.callAppropriateSystemMethod (system, "openPanelSelectComp", null);
	}

	public void startLevel()
	{
		MainLoop.callAppropriateSystemMethod (system, "startLevel", null);
	}

	public void infoCompetence(UnityEngine.GameObject comp)
	{
		MainLoop.callAppropriateSystemMethod (system, "infoCompetence", comp);
	}

	public void resetViewInfoCompetence(UnityEngine.GameObject comp)
	{
		MainLoop.callAppropriateSystemMethod (system, "resetViewInfoCompetence", comp);
	}

	public void selectComp()
	{
		MainLoop.callAppropriateSystemMethod (system, "selectComp", null);
	}

	public void unselectComp()
	{
		MainLoop.callAppropriateSystemMethod (system, "unselectComp", null);
	}

	public void addOrRemoveCompSelect()
	{
		MainLoop.callAppropriateSystemMethod (system, "addOrRemoveCompSelect", null);
	}

	public void saveListUser()
	{
		MainLoop.callAppropriateSystemMethod (system, "saveListUser", null);
	}

	public void closeSelectCompetencePanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "closeSelectCompetencePanel", null);
	}

	public void displayMessageUser(System.String message)
	{
		MainLoop.callAppropriateSystemMethod (system, "displayMessageUser", message);
	}

	public void changeSizeButtonCategory()
	{
		MainLoop.callAppropriateSystemMethod (system, "changeSizeButtonCategory", null);
	}

}
