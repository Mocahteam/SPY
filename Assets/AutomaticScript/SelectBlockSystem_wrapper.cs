using UnityEngine;
using FYFY;

public class SelectBlockSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject mover;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "mover", mover);
	}

	public void selectBlock(UnityEngine.GameObject obj)
	{
		MainLoop.callAppropriateSystemMethod (system, "selectBlock", obj);
	}

}
