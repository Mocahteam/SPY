using UnityEngine;
using FYFY;

public class LevelGenerator_wrapper : BaseWrapper
{
	public UnityEngine.GameObject editableScriptContainer;
	public UnityEngine.GameObject scriptContainer;
	public TMPro.TMP_Text levelName;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "editableScriptContainer", editableScriptContainer);
		MainLoop.initAppropriateSystemField (system, "scriptContainer", scriptContainer);
		MainLoop.initAppropriateSystemField (system, "levelName", levelName);
	}

	public void computeNext(UnityEngine.GameObject container)
	{
		MainLoop.callAppropriateSystemMethod (system, "computeNext", container);
	}

}
