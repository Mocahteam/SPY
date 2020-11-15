using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class UISystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void resetScript()
	{
		MainLoop.callAppropriateSystemMethod ("UISystem", "resetScript", null);
	}

	public void showDialogPanel(System.String text)
	{
		MainLoop.callAppropriateSystemMethod ("UISystem", "showDialogPanel", text);
	}

	public void closeDialogPanel()
	{
		MainLoop.callAppropriateSystemMethod ("UISystem", "closeDialogPanel", null);
	}

}