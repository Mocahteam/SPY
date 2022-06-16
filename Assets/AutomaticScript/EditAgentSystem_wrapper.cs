using UnityEngine;
using FYFY;

public class EditAgentSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject agentSelected;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "agentSelected", agentSelected);
	}

	public void setAgentName(System.String newName)
	{
		MainLoop.callAppropriateSystemMethod (system, "setAgentName", newName);
	}

}
