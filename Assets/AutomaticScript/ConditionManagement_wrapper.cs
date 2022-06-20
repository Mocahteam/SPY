using UnityEngine;
using FYFY;

public class ConditionManagement_wrapper : BaseWrapper
{
	public UnityEngine.GameObject endPanel;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "endPanel", endPanel);
	}

	public void convertionConditionSequence()
	{
		MainLoop.callAppropriateSystemMethod (system, "convertionConditionSequence", null);
	}

}
