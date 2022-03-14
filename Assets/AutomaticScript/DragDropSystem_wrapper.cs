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

	public void beginDragElement(UnityEngine.GameObject element)
	{
		MainLoop.callAppropriateSystemMethod (system, "beginDragElement", element);
	}

	public void endDragElement(UnityEngine.GameObject element)
	{
		MainLoop.callAppropriateSystemMethod (system, "endDragElement", element);
	}

	public void doubleClick(UnityEngine.GameObject element)
	{
		MainLoop.callAppropriateSystemMethod (system, "doubleClick", element);
	}

	public void pointerLeftUpElement(UnityEngine.GameObject element)
	{
		MainLoop.callAppropriateSystemMethod (system, "pointerLeftUpElement", element);
	}

	public void pointerDownElement(UnityEngine.GameObject element)
	{
		MainLoop.callAppropriateSystemMethod (system, "pointerDownElement", element);
	}

	public void dragElement(UnityEngine.GameObject element)
	{
		MainLoop.callAppropriateSystemMethod (system, "dragElement", element);
	}

	public void testObjectpointer()
	{
		MainLoop.callAppropriateSystemMethod (system, "testObjectpointer", null);
	}

}
