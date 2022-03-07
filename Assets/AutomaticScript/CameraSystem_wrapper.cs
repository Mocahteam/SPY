using UnityEngine;
using FYFY;

public class CameraSystem_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void setLocateButtons(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "setLocateButtons", go);
	}

	public void ActivatedCameraControl(System.Boolean value)
	{
		MainLoop.callAppropriateSystemMethod (system, "ActivatedCameraControl", value);
	}

}
