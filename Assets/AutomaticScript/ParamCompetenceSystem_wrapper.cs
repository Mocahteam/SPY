using UnityEngine;
using FYFY;

public class ParamCompetenceSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject panelInfoComp;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "panelInfoComp", panelInfoComp);
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

}
