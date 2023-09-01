using UnityEngine;
using FYFY;

public class ScriptGenerator_wrapper : BaseWrapper
{
	public UnityEngine.GameObject mainCanvas;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "mainCanvas", mainCanvas);
	}

}
