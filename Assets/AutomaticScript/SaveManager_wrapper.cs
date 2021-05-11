using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class SaveManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void SaveState()
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "SaveState", null);
	}

	public void saveStateIfFirstStep()
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "saveStateIfFirstStep", null);
	}

	public void LoadState()
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "LoadState", null);
	}

}
