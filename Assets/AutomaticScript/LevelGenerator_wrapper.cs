using UnityEngine;
using FYFY;

public class LevelGenerator_wrapper : BaseWrapper
{
	public UnityEngine.GameObject camera;
	public UnityEngine.GameObject editableScriptContainer;
	public UnityEngine.GameObject scriptContainer;
	public TMPro.TMP_Text levelName;
	public UnityEngine.GameObject canvas;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "camera", camera);
		MainLoop.initAppropriateSystemField (system, "editableScriptContainer", editableScriptContainer);
		MainLoop.initAppropriateSystemField (system, "scriptContainer", scriptContainer);
		MainLoop.initAppropriateSystemField (system, "levelName", levelName);
		MainLoop.initAppropriateSystemField (system, "canvas", canvas);
	}

	public void computeNext(UnityEngine.GameObject container)
	{
		MainLoop.callAppropriateSystemMethod (system, "computeNext", container);
	}

}
