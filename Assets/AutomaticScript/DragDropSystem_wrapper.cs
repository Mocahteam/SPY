using UnityEngine;
using FYFY;

public class DragDropSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject mainCanvas;
	public UnityEngine.GameObject positionBar;
	public UnityEngine.GameObject editableContainer;
	public System.Single catchTime;
	public UnityEngine.GameObject buttonPlay;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "mainCanvas", mainCanvas);
		MainLoop.initAppropriateSystemField (system, "positionBar", positionBar);
		MainLoop.initAppropriateSystemField (system, "editableContainer", editableContainer);
		MainLoop.initAppropriateSystemField (system, "catchTime", catchTime);
		MainLoop.initAppropriateSystemField (system, "buttonPlay", buttonPlay);
	}

	public void onlyPositiveInteger(TMPro.TMP_InputField input)
	{
		MainLoop.callAppropriateSystemMethod (system, "onlyPositiveInteger", input);
	}

}
