using UnityEngine;
using FYFY;

public class ButtonConnection_wrapper : BaseWrapper
{
	public UnityEngine.GameObject EditableCanvas;
	public UnityEngine.RectTransform editableContainers;
	public UnityEngine.GameObject mainCanvas;
	public UnityEngine.GameObject dropZone;
	public UnityEngine.GameObject panelPrefab;
	public UnityEngine.Transform contentPanel;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "EditableCanvas", EditableCanvas);
		MainLoop.initAppropriateSystemField (system, "editableContainers", editableContainers);
		MainLoop.initAppropriateSystemField (system, "mainCanvas", mainCanvas);
		MainLoop.initAppropriateSystemField (system, "dropZone", dropZone);
		MainLoop.initAppropriateSystemField (system, "panelPrefab", panelPrefab);
		MainLoop.initAppropriateSystemField (system, "contentPanel", contentPanel);
	}

	public void ChangText(System.String newText)
	{
		MainLoop.callAppropriateSystemMethod (system, "ChangText", newText);
	}

	public void AddActionToDropZone(UnityEngine.GameObject libraryItem)
	{
		MainLoop.callAppropriateSystemMethod (system, "AddActionToDropZone", libraryItem);
	}

}
