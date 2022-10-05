using UnityEngine;
using FYFY;

public class LevelGenerator_wrapper : BaseWrapper
{
	public UnityEngine.GameObject editableCanvas;
	public UnityEngine.GameObject scriptContainer;
	public UnityEngine.GameObject library;
	public UnityEngine.GameObject EditableContenair;
	public TMPro.TMP_Text levelName;
	public UnityEngine.GameObject canvas;
	public UnityEngine.GameObject buttonExecute;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "editableCanvas", editableCanvas);
		MainLoop.initAppropriateSystemField (system, "scriptContainer", scriptContainer);
		MainLoop.initAppropriateSystemField (system, "library", library);
		MainLoop.initAppropriateSystemField (system, "EditableContenair", EditableContenair);
		MainLoop.initAppropriateSystemField (system, "levelName", levelName);
		MainLoop.initAppropriateSystemField (system, "canvas", canvas);
		MainLoop.initAppropriateSystemField (system, "buttonExecute", buttonExecute);
	}

}
