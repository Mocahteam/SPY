using UnityEngine;
using FYFY;

public class CameraSystem_wrapper : BaseWrapper
{
	public System.Single cameraMovingSpeed;
	public System.Single cameraZoomSpeed;
	public System.Single cameraZoomMin;
	public System.Single cameraZoomMax;
	public System.Single dragSpeed;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "cameraMovingSpeed", cameraMovingSpeed);
		MainLoop.initAppropriateSystemField (system, "cameraZoomSpeed", cameraZoomSpeed);
		MainLoop.initAppropriateSystemField (system, "cameraZoomMin", cameraZoomMin);
		MainLoop.initAppropriateSystemField (system, "cameraZoomMax", cameraZoomMax);
		MainLoop.initAppropriateSystemField (system, "dragSpeed", dragSpeed);
	}

	public void focusOnAgent(UnityEngine.GameObject agent)
	{
		MainLoop.callAppropriateSystemMethod (system, "focusOnAgent", agent);
	}

}
