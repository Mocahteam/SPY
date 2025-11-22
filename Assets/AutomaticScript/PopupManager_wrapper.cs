using UnityEngine;
using FYFY;

public class PopupManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject panelInfoUser;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "panelInfoUser", panelInfoUser);
	}

	public void turnOnCanvas()
	{
		MainLoop.callAppropriateSystemMethod (system, "turnOnCanvas", null);
	}

}
