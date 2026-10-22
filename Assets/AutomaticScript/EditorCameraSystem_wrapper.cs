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

	public void Set_UIFrontBack(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "Set_UIFrontBack", value);
	}

	public void Set_UILeftRight(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "Set_UILeftRight", value);
	}

	public void Set_UIZoom(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "Set_UIZoom", value);
	}

	public void SubmitFrontBack(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SubmitFrontBack", value);
	}

	public void SubmitLeftRight(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SubmitLeftRight", value);
	}

	public void SubmitZoom(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SubmitZoom", value);
	}

}
