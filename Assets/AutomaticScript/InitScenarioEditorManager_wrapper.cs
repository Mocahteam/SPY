using UnityEngine;
using FYFY;

public class InitScenarioEditorManager_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void ReloadEditor()
	{
		MainLoop.callAppropriateSystemMethod (system, "ReloadEditor", null);
	}

	public void ReturnToLobby()
	{
		MainLoop.callAppropriateSystemMethod (system, "ReturnToLobby", null);
	}

}
