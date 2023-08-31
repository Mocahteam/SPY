using UnityEngine;
using FYFY;

public class EditorEscMenu_wrapper : BaseWrapper
{
	public UnityEngine.GameObject buttonMenu;
	public UnityEngine.GameObject menuCanvas;
	public LevelData levelData;
	public PaintableGrid paintableGrid;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "buttonMenu", buttonMenu);
		MainLoop.initAppropriateSystemField (system, "menuCanvas", menuCanvas);
		MainLoop.initAppropriateSystemField (system, "levelData", levelData);
		MainLoop.initAppropriateSystemField (system, "paintableGrid", paintableGrid);
	}

	public void toggleMenu()
	{
		MainLoop.callAppropriateSystemMethod (system, "toggleMenu", null);
	}

	public void closeEditor()
	{
		MainLoop.callAppropriateSystemMethod (system, "closeEditor", null);
	}

}
