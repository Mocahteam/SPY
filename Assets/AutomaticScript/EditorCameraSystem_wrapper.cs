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

}
