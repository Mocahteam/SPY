using UnityEngine;
using FYFY;

public class EndGameManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject endPanel;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "endPanel", endPanel);
	}

	public void cancelEnd()
	{
		MainLoop.callAppropriateSystemMethod (system, "cancelEnd", null);
	}

}
