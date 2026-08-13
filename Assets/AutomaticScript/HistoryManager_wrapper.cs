using UnityEngine;
using FYFY;

public class HistoryManager_wrapper : BaseWrapper
{
	public UnityEngine.RectTransform EditableContainers;
	public UnityEngine.GameObject libraryFor;
	public UnityEngine.GameObject libraryWait;
	public UnityEngine.GameObject canvas;
	public UnityEngine.GameObject buttonAddEditableContainer;
	public UnityEngine.GameObject buttonExecute;
	public UnityEngine.UI.Button buttonUndo;
	public UnityEngine.UI.Button buttonRedo;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "EditableContainers", EditableContainers);
		MainLoop.initAppropriateSystemField (system, "libraryFor", libraryFor);
		MainLoop.initAppropriateSystemField (system, "libraryWait", libraryWait);
		MainLoop.initAppropriateSystemField (system, "canvas", canvas);
		MainLoop.initAppropriateSystemField (system, "buttonAddEditableContainer", buttonAddEditableContainer);
		MainLoop.initAppropriateSystemField (system, "buttonExecute", buttonExecute);
		MainLoop.initAppropriateSystemField (system, "buttonUndo", buttonUndo);
		MainLoop.initAppropriateSystemField (system, "buttonRedo", buttonRedo);
	}

	public void keepUndoRedoStack()
	{
		MainLoop.callAppropriateSystemMethod (system, "keepUndoRedoStack", null);
	}

	public void saveHistory()
	{
		MainLoop.callAppropriateSystemMethod (system, "saveHistory", null);
	}

	public void undo()
	{
		MainLoop.callAppropriateSystemMethod (system, "undo", null);
	}

	public void redo()
	{
		MainLoop.callAppropriateSystemMethod (system, "redo", null);
	}

}
