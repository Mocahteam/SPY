using UnityEngine;
using FYFY;

public class UndoRedoManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject EditableContainers;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "EditableContainers", EditableContainers);
	}

}
