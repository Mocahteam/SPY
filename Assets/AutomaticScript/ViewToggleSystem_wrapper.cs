using UnityEngine;
using FYFY;

public class ViewToggleSystem_wrapper : BaseWrapper
{
	public UnityEngine.Canvas mainCanvas;
	public UnityEngine.Canvas metadataCanvas;
	public PaintableGrid paintableGrid;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "mainCanvas", mainCanvas);
		MainLoop.initAppropriateSystemField (system, "metadataCanvas", metadataCanvas);
		MainLoop.initAppropriateSystemField (system, "paintableGrid", paintableGrid);
	}

	public void toggleCanvas()
	{
		MainLoop.callAppropriateSystemMethod (system, "toggleCanvas", null);
	}

}
