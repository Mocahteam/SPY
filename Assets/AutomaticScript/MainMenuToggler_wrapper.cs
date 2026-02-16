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

	public void toggleMainMenu()
	{
		MainLoop.callAppropriateSystemMethod (system, "toggleMainMenu", null);
	}

	public void setCanvasInterractable(System.Boolean state)
	{
		MainLoop.callAppropriateSystemMethod (system, "setCanvasInterractable", state);
	}

}
