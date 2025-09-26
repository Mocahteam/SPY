using UnityEngine;
using FYFY;

public class InitScenarioEditorManager_wrapper : BaseWrapper
{
	public UnityEngine.UI.Button downloadLevelBt;
	public UnityEngine.GameObject menuCanvas;
	public UnityEngine.GameObject menuEscape;
	public UnityEngine.GameObject closePanelButton;
	public UnityEngine.CanvasGroup[] canvasGroups;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "downloadLevelBt", downloadLevelBt);
		MainLoop.initAppropriateSystemField (system, "menuCanvas", menuCanvas);
		MainLoop.initAppropriateSystemField (system, "menuEscape", menuEscape);
		MainLoop.initAppropriateSystemField (system, "closePanelButton", closePanelButton);
		MainLoop.initAppropriateSystemField (system, "canvasGroups", canvasGroups);
	}

	public void reloadEditor()
	{
		MainLoop.callAppropriateSystemMethod (system, "reloadEditor", null);
	}

	public void returnToLobby()
	{
		MainLoop.callAppropriateSystemMethod (system, "returnToLobby", null);
	}

	public void toggleMainMenu()
	{
		MainLoop.callAppropriateSystemMethod (system, "toggleMainMenu", null);
	}

}
