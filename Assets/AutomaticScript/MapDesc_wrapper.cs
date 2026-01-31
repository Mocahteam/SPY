using UnityEngine;
using FYFY;

public class MapDesc_wrapper : BaseWrapper
{
	public UnityEngine.GameObject panel;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "panel", panel);
	}

	public void askToRefreshMapDesc()
	{
		MainLoop.callAppropriateSystemMethod (system, "askToRefreshMapDesc", null);
	}

}
