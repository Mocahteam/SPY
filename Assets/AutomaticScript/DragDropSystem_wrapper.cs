using UnityEngine;
using FYFY;

public class DragDropSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject mainCanvas;
	public UnityEngine.GameObject positionBar;
	public UnityEngine.GameObject lastEditableContainer;
	public UnityEngine.AudioSource audioSource;
	public System.Single catchTime;
	public UnityEngine.GameObject buttonPlay;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "mainCanvas", mainCanvas);
		MainLoop.initAppropriateSystemField (system, "positionBar", positionBar);
		MainLoop.initAppropriateSystemField (system, "lastEditableContainer", lastEditableContainer);
		MainLoop.initAppropriateSystemField (system, "audioSource", audioSource);
		MainLoop.initAppropriateSystemField (system, "catchTime", catchTime);
		MainLoop.initAppropriateSystemField (system, "buttonPlay", buttonPlay);
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

	public void dropElementInContainer(UnityEngine.GameObject redBar)
	{
		MainLoop.callAppropriateSystemMethod (system, "dropElementInContainer", redBar);
	}

	public void deleteElement(UnityEngine.GameObject element)
	{
		MainLoop.callAppropriateSystemMethod (system, "deleteElement", element);
	}

	public void clickLibraryElementForAddInContainer(UnityEngine.EventSystems.BaseEventData element)
	{
		MainLoop.callAppropriateSystemMethod (system, "clickLibraryElementForAddInContainer", element);
	}

	public void testObjectpointer()
	{
		MainLoop.callAppropriateSystemMethod (system, "testObjectpointer", null);
	}

}
