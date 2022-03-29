using UnityEngine;
using FYFY;

public class EditAgentSystem_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void agentSelect(UnityEngine.EventSystems.BaseEventData agent)
	{
		MainLoop.callAppropriateSystemMethod (system, "agentSelect", agent);
	}

	public void setAgentName(System.String newName)
	{
		MainLoop.callAppropriateSystemMethod (system, "setAgentName", newName);
	}

	public void majDisplayCardAgent(System.String newName)
	{
		MainLoop.callAppropriateSystemMethod (system, "majDisplayCardAgent", newName);
	}

}
