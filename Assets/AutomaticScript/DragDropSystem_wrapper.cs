using UnityEngine;
using FYFY;

public class DragDropSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject mainCanvas;
	public UnityEngine.AudioSource audioSource;
	public UnityEngine.RectTransform editableContainers;
	public UnityEngine.AudioClip AddActionSound;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "mainCanvas", mainCanvas);
		MainLoop.initAppropriateSystemField (system, "audioSource", audioSource);
		MainLoop.initAppropriateSystemField (system, "editableContainers", editableContainers);
		MainLoop.initAppropriateSystemField (system, "AddActionSound", AddActionSound);
	}

	public void CheckHighlightDropArea(UnityEngine.GameObject dropArea)
	{
		MainLoop.callAppropriateSystemMethod (system, "CheckHighlightDropArea", dropArea);
	}

	public void UnhighlightDropArea(UnityEngine.GameObject dropArea)
	{
		MainLoop.callAppropriateSystemMethod (system, "UnhighlightDropArea", dropArea);
	}

	public void BeginDragElementFromLibrary(UnityEngine.EventSystems.BaseEventData element)
	{
		MainLoop.callAppropriateSystemMethod (system, "BeginDragElementFromLibrary", element);
	}

	public void BeginDragElementFromEditableScript(UnityEngine.EventSystems.BaseEventData element)
	{
		MainLoop.callAppropriateSystemMethod (system, "BeginDragElementFromEditableScript", element);
	}

	public void DragElement()
	{
		MainLoop.callAppropriateSystemMethod (system, "DragElement", null);
	}

	public void EndDragElement()
	{
		MainLoop.callAppropriateSystemMethod (system, "EndDragElement", null);
	}

	public void DeleteElement(UnityEngine.GameObject elementToDelete)
	{
		MainLoop.callAppropriateSystemMethod (system, "DeleteElement", elementToDelete);
	}

	public void CheckSpecialClick(UnityEngine.EventSystems.BaseEventData element)
	{
		MainLoop.callAppropriateSystemMethod (system, "CheckSpecialClick", element);
	}

	public void SetDefaultDropZone(UnityEngine.GameObject refObject)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetDefaultDropZone", refObject);
	}

	public void LockItem(UnityEngine.Transform transform)
	{
		MainLoop.callAppropriateSystemMethod (system, "LockItem", transform);
	}

	public void UnlockItem(UnityEngine.Transform transform)
	{
		MainLoop.callAppropriateSystemMethod (system, "UnlockItem", transform);
	}

}
