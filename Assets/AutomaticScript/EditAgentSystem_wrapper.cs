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

	public void changeName(System.String newName)
	{
		MainLoop.callAppropriateSystemMethod (system, "changeName", newName);
	}

	public void changeNameAgentExterneElement()
	{
		MainLoop.callAppropriateSystemMethod (system, "changeNameAgentExterneElement", null);
	}

	public void linkScriptContainer(UnityEngine.GameObject agent)
	{
		MainLoop.callAppropriateSystemMethod (system, "linkScriptContainer", agent);
	}

	public void dislinkOrLinkAgent()
	{
		MainLoop.callAppropriateSystemMethod (system, "dislinkOrLinkAgent", null);
	}

	public void dislinkScriptContainer(UnityEngine.GameObject agent)
	{
		MainLoop.callAppropriateSystemMethod (system, "dislinkScriptContainer", agent);
	}

	public void changeNameAssociedElement()
	{
		MainLoop.callAppropriateSystemMethod (system, "changeNameAssociedElement", null);
	}

}
