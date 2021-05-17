using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class SaveManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void SaveState(UnityEngine.GameObject buttonStop)
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "SaveState", buttonStop);
	}

	public void LoadState()
	{
		MainLoop.callAppropriateSystemMethod ("SaveManager", "LoadState", null);
	}

}
