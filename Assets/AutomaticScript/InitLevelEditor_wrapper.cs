using UnityEngine;
using FYFY;

public class InitLevelEditor_wrapper : BaseWrapper
{
	public UnityEngine.GameObject menuCanvas;
	public UnityEngine.UI.Button mapTab;
	public UnityEngine.UI.Button scriptTab;
	public UnityEngine.UI.Button paramTab;
	public UnityEngine.GameObject mapContent;
	public UnityEngine.GameObject scriptContent;
	public UnityEngine.GameObject paramContent;
	public UnityEngine.GameObject initFocused;
	public UnityEngine.GameObject menuEscape;
	public UnityEngine.GameObject closePanelButton;
	public UnityEngine.CanvasGroup[] canvasGroups;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "menuCanvas", menuCanvas);
		MainLoop.initAppropriateSystemField (system, "mapTab", mapTab);
		MainLoop.initAppropriateSystemField (system, "scriptTab", scriptTab);
		MainLoop.initAppropriateSystemField (system, "paramTab", paramTab);
		MainLoop.initAppropriateSystemField (system, "mapContent", mapContent);
		MainLoop.initAppropriateSystemField (system, "scriptContent", scriptContent);
		MainLoop.initAppropriateSystemField (system, "paramContent", paramContent);
		MainLoop.initAppropriateSystemField (system, "initFocused", initFocused);
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
