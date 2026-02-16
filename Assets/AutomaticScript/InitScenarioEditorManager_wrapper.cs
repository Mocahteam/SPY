using UnityEngine;
using FYFY;

public class InitScenarioEditorManager_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void reloadEditor()
	{
		MainLoop.callAppropriateSystemMethod (system, "reloadEditor", null);
	}

	public void returnToLobby()
	{
		MainLoop.callAppropriateSystemMethod (system, "returnToLobby", null);
	}

}
