using UnityEngine;
using FYFY;

public class EditorCameraSystem_wrapper : BaseWrapper
{
	public UnityEngine.Camera mainCamera;
	public System.Single cameraZoomMin;
	public System.Single cameraZoomMax;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "mainCamera", mainCamera);
		MainLoop.initAppropriateSystemField (system, "cameraZoomMin", cameraZoomMin);
		MainLoop.initAppropriateSystemField (system, "cameraZoomMax", cameraZoomMax);
	}

	public void set_UIFrontBack(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "set_UIFrontBack", value);
	}

	public void set_UILeftRight(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "set_UILeftRight", value);
	}

	public void set_UIZoom(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "set_UIZoom", value);
	}

	public void submitFrontBack(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "submitFrontBack", value);
	}

	public void submitLeftRight(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "submitLeftRight", value);
	}

	public void submitZoom(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "submitZoom", value);
	}

}
