using UnityEngine;
using FYFY;

public class HistoryManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject EditableCanvas;
	public UnityEngine.GameObject libraryFor;
	public UnityEngine.GameObject libraryWait;
	public UnityEngine.GameObject canvas;
	public UnityEngine.GameObject buttonAddEditableContainer;
	public UnityEngine.GameObject buttonExecute;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "EditableCanvas", EditableCanvas);
		MainLoop.initAppropriateSystemField (system, "libraryFor", libraryFor);
		MainLoop.initAppropriateSystemField (system, "libraryWait", libraryWait);
		MainLoop.initAppropriateSystemField (system, "canvas", canvas);
		MainLoop.initAppropriateSystemField (system, "buttonAddEditableContainer", buttonAddEditableContainer);
		MainLoop.initAppropriateSystemField (system, "buttonExecute", buttonExecute);
	}

	public void saveHistory()
	{
		MainLoop.callAppropriateSystemMethod (system, "saveHistory", null);
	}

}
