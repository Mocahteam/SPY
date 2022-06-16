using UnityEngine;
using FYFY;

public class CameraSystem_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void focusOnAgent(UnityEngine.GameObject agent)
	{
		MainLoop.callAppropriateSystemMethod (system, "focusOnAgent", agent);
	}

}
