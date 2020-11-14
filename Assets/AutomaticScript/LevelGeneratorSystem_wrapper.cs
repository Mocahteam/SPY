using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class LevelGeneratorSystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void reloadScene()
	{
		MainLoop.callAppropriateSystemMethod ("LevelGeneratorSystem", "reloadScene", null);
	}

	public void returnToTitleScreen()
	{
		MainLoop.callAppropriateSystemMethod ("LevelGeneratorSystem", "returnToTitleScreen", null);
	}

	public void nextLevel()
	{
		MainLoop.callAppropriateSystemMethod ("LevelGeneratorSystem", "nextLevel", null);
	}

}