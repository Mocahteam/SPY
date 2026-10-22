using UnityEngine;
using FYFY;

public class CameraSystem_wrapper : BaseWrapper
{
	public System.Single cameraMovingSpeed;
	public System.Single cameraRotationSpeed;
	public System.Single cameraZoomSpeed;
	public System.Single cameraZoomMin;
	public System.Single cameraZoomMax;
	public System.Single dragSpeed;
	public CurrentSettingsValues currentSettingsValues;
	public UnityEngine.GameObject dialogPanel;
	public UnityEngine.RectTransform LeftPanel;
	public UnityEngine.RectTransform ExecutableCanvas;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "cameraMovingSpeed", cameraMovingSpeed);
		MainLoop.initAppropriateSystemField (system, "cameraRotationSpeed", cameraRotationSpeed);
		MainLoop.initAppropriateSystemField (system, "cameraZoomSpeed", cameraZoomSpeed);
		MainLoop.initAppropriateSystemField (system, "cameraZoomMin", cameraZoomMin);
		MainLoop.initAppropriateSystemField (system, "cameraZoomMax", cameraZoomMax);
		MainLoop.initAppropriateSystemField (system, "dragSpeed", dragSpeed);
		MainLoop.initAppropriateSystemField (system, "currentSettingsValues", currentSettingsValues);
		MainLoop.initAppropriateSystemField (system, "dialogPanel", dialogPanel);
		MainLoop.initAppropriateSystemField (system, "LeftPanel", LeftPanel);
		MainLoop.initAppropriateSystemField (system, "ExecutableCanvas", ExecutableCanvas);
	}

	public void ToggleOrthographicPerspective()
	{
		MainLoop.callAppropriateSystemMethod (system, "ToggleOrthographicPerspective", null);
	}

	public void SetOrthographicView(System.Boolean state)
	{
		MainLoop.callAppropriateSystemMethod (system, "SetOrthographicView", state);
	}

	public void Set_UIFrontBack(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "Set_UIFrontBack", value);
	}

	public void Set_UILeftRight(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "Set_UILeftRight", value);
	}

	public void Set_UIRotate(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "Set_UIRotate", value);
	}

	public void Set_UIPitching(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "Set_UIPitching", value);
	}

	public void Set_UIZoom(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "Set_UIZoom", value);
	}

	public void SubmitRotate(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SubmitRotate", value);
	}

	public void SubmitPitching(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "SubmitPitching", value);
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

	public void FocusNextAgent()
	{
		MainLoop.callAppropriateSystemMethod (system, "FocusNextAgent", null);
	}

}
