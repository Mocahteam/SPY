using UnityEngine;
using FYFY;

public class DragDropSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject mainCanvas;
	public UnityEngine.GameObject positionBar;
	public UnityEngine.GameObject editableContainer;
	public UnityEngine.AudioSource audioSource;
	public System.Single catchTime;
	public UnityEngine.GameObject buttonPlay;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "mainCanvas", mainCanvas);
		MainLoop.initAppropriateSystemField (system, "positionBar", positionBar);
		MainLoop.initAppropriateSystemField (system, "editableContainer", editableContainer);
		MainLoop.initAppropriateSystemField (system, "audioSource", audioSource);
		MainLoop.initAppropriateSystemField (system, "catchTime", catchTime);
		MainLoop.initAppropriateSystemField (system, "buttonPlay", buttonPlay);
	}

	public void dropElementInContainer(UnityEngine.GameObject redBar)
	{
		MainLoop.callAppropriateSystemMethod (system, "dropElementInContainer", redBar);
	}

	public void beginDragElementFromLibrary(UnityEngine.EventSystems.BaseEventData element)
	{
		MainLoop.callAppropriateSystemMethod (system, "beginDragElementFromLibrary", element);
	}

	public void beginDragElementFromEditableScript(UnityEngine.EventSystems.BaseEventData element)
	{
		MainLoop.callAppropriateSystemMethod (system, "beginDragElementFromEditableScript", element);
	}

	public void dragElement()
	{
		MainLoop.callAppropriateSystemMethod (system, "dragElement", null);
	}

	public void endDragElement()
	{
		MainLoop.callAppropriateSystemMethod (system, "endDragElement", null);
	}

	public void doubleClick(UnityEngine.GameObject element)
	{
		MainLoop.callAppropriateSystemMethod (system, "doubleClick", element);
	}

	public void supBlock(UnityEngine.GameObject element)
	{
		MainLoop.callAppropriateSystemMethod (system, "supBlock", element);
	}

	public void pointerDownElement(UnityEngine.GameObject element)
	{
		MainLoop.callAppropriateSystemMethod (system, "pointerDownElement", element);
	}

	public void testObjectpointer()
	{
		MainLoop.callAppropriateSystemMethod (system, "testObjectpointer", null);
	}

}
