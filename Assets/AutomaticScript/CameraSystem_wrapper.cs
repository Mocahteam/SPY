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
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "cameraMovingSpeed", cameraMovingSpeed);
		MainLoop.initAppropriateSystemField (system, "cameraRotationSpeed", cameraRotationSpeed);
		MainLoop.initAppropriateSystemField (system, "cameraZoomSpeed", cameraZoomSpeed);
		MainLoop.initAppropriateSystemField (system, "cameraZoomMin", cameraZoomMin);
		MainLoop.initAppropriateSystemField (system, "cameraZoomMax", cameraZoomMax);
		MainLoop.initAppropriateSystemField (system, "dragSpeed", dragSpeed);
	}

	public void ToggleOrthographicPerspective()
	{
		MainLoop.callAppropriateSystemMethod (system, "ToggleOrthographicPerspective", null);
	}

	public void set_UIFrontBack(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "set_UIFrontBack", value);
	}

	public void set_UILeftRight(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "set_UILeftRight", value);
	}

	public void set_UIRotate(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "set_UIRotate", value);
	}

	public void set_UIZoom(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "set_UIZoom", value);
	}

	public void submitRotate(System.Single value)
	{
		MainLoop.callAppropriateSystemMethod (system, "submitRotate", value);
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

	public void focusOnAgent(UnityEngine.GameObject agent)
	{
		MainLoop.callAppropriateSystemMethod (system, "focusOnAgent", agent);
	}

	public void focusNextAgent()
	{
		MainLoop.callAppropriateSystemMethod (system, "focusNextAgent", null);
	}

}
