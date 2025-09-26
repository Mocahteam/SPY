using UnityEngine;
using FYFY;

public class CompetenciesLoader_wrapper : BaseWrapper
{
	public UnityEngine.GameObject hiddenCompetencies;
	public UnityEngine.GameObject prefabComp;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "hiddenCompetencies", hiddenCompetencies);
		MainLoop.initAppropriateSystemField (system, "prefabComp", prefabComp);
	}

	public void saveReferetialSelected(System.Int32 referentialId)
	{
		MainLoop.callAppropriateSystemMethod (system, "saveReferetialSelected", referentialId);
	}

}
