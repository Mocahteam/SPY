using UnityEngine;
using FYFY;

public class MainMenuToggler_wrapper : BaseWrapper
{
	public UnityEngine.GameObject menuCanvas;
	public UnityEngine.CanvasGroup[] canvasGroups;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "menuCanvas", menuCanvas);
		MainLoop.initAppropriateSystemField (system, "canvasGroups", canvasGroups);
	}

	public void ShowMainMenu()
	{
		MainLoop.callAppropriateSystemMethod (system, "ShowMainMenu", null);
	}

	public void HideMainMenu()
	{
		MainLoop.callAppropriateSystemMethod (system, "HideMainMenu", null);
	}

	public void SetCanvasInterractable(System.Boolean state)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetCanvasInterractable", state);
	}

}
