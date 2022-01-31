using UnityEngine;
using FYFY;

public class SaveManager_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void SaveState(UnityEngine.GameObject buttonStop)
	{
		MainLoop.callAppropriateSystemMethod (system, "SaveState", buttonStop);
	}

	public void LoadState()
	{
		MainLoop.callAppropriateSystemMethod (system, "LoadState", null);
	}

}
