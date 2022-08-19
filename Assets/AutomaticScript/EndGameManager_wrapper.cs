using UnityEngine;
using FYFY;

public class EndGameManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject endPanel;
	public UnityEngine.GameObject badEndPanel;
	public UnityEngine.GameObject menuPanel;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "endPanel", endPanel);
		MainLoop.initAppropriateSystemField (system, "badEndPanel", badEndPanel);
		MainLoop.initAppropriateSystemField (system, "menuPanel", menuPanel);
	}

	public void badEnd()
	{
		MainLoop.callAppropriateSystemMethod (system, "badEnd", null);
	}

}
