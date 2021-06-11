using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class UISystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void resetScript(System.Boolean refund)
	{
		MainLoop.callAppropriateSystemMethod ("UISystem", "resetScript", refund);
	}

	public void showDialogPanel()
	{
		MainLoop.callAppropriateSystemMethod ("UISystem", "showDialogPanel", null);
	}

	public void nextDialog()
	{
		MainLoop.callAppropriateSystemMethod ("UISystem", "nextDialog", null);
	}

	public void setActiveOKButton(System.Boolean active)
	{
		MainLoop.callAppropriateSystemMethod ("UISystem", "setActiveOKButton", active);
	}

	public void setActiveNextButton(System.Boolean active)
	{
		MainLoop.callAppropriateSystemMethod ("UISystem", "setActiveNextButton", active);
	}

	public void closeDialogPanel()
	{
		MainLoop.callAppropriateSystemMethod ("UISystem", "closeDialogPanel", null);
	}

	public void reloadScene()
	{
		MainLoop.callAppropriateSystemMethod ("UISystem", "reloadScene", null);
	}

	public void returnToTitleScreen()
	{
		MainLoop.callAppropriateSystemMethod ("UISystem", "returnToTitleScreen", null);
	}

	public void nextLevel()
	{
		MainLoop.callAppropriateSystemMethod ("UISystem", "nextLevel", null);
	}

	public void retry()
	{
		MainLoop.callAppropriateSystemMethod ("UISystem", "retry", null);
	}

	public void reloadState()
	{
		MainLoop.callAppropriateSystemMethod ("UISystem", "reloadState", null);
	}

	public void stopScript()
	{
		MainLoop.callAppropriateSystemMethod ("UISystem", "stopScript", null);
	}

	public void applyScriptToPlayer()
	{
		MainLoop.callAppropriateSystemMethod ("UISystem", "applyScriptToPlayer", null);
	}

}
