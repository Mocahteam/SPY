using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class TitleScreenSystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void showCampagneMenu()
	{
		MainLoop.callAppropriateSystemMethod ("TitleScreenSystem", "showCampagneMenu", null);
	}

	public void launchLevel(System.Int32 level)
	{
		MainLoop.callAppropriateSystemMethod ("TitleScreenSystem", "launchLevel", level);
	}

}