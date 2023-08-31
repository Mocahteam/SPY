using UnityEngine;
using FYFY;

public class EditorGridPaintSystem_wrapper : BaseWrapper
{
	public PaintableGrid paintableGrid;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "paintableGrid", paintableGrid);
	}

	public void setBrush(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "setBrush", go);
	}

}
