using UnityEngine;

public class CameraSystemBridge : MonoBehaviour
{
    // Active ou desactive le systéme
    public void PauseCameraSystem(bool value)
    {
        if (CameraSystem.instance != null) // is null in level editor
            CameraSystem.instance.Pause = value;
		if (EditorCameraSystem.instance != null) // is null in level player
			EditorCameraSystem.instance.Pause = value;
	}

    public void locateAgent(LinkedWith agent)
    {
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.focusOnAgent(agent.target);
    }


    public void focusNextAgent()
    {
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.focusNextAgent();

	}
    public void submitZoom(float value)
    {
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.submitZoom(value);
		if (EditorCameraSystem.instance != null) // is null in level player
			EditorCameraSystem.instance.submitZoom(value);
	}
	public void ToggleOrthographicPerspective()
	{
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.ToggleOrthographicPerspective();
	}

	public void set_UIFrontBack(float value)
	{
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.set_UIFrontBack(value);
		if (EditorCameraSystem.instance != null) // is null in level player
			EditorCameraSystem.instance.set_UIFrontBack(value);
	}

	public void set_UILeftRight(float value)
	{
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.set_UILeftRight(value);
		if (EditorCameraSystem.instance != null) // is null in level player
			EditorCameraSystem.instance.set_UILeftRight(value);
	}

	public void set_UIRotate(float value)
	{
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.set_UIRotate(value);
	}

	public void set_UIZoom(float value)
	{
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.set_UIZoom(value);
		if (EditorCameraSystem.instance != null) // is null in level player
			EditorCameraSystem.instance.set_UIZoom(value);
	}

	public void submitRotate(float value)
	{
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.submitRotate(value);
	}
	public void submitFrontBack(float value)
	{
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.submitFrontBack(value);
		if (EditorCameraSystem.instance != null) // is null in level player
			EditorCameraSystem.instance.submitFrontBack(value);
	}

	public void submitLeftRight(float value)
	{
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.submitLeftRight(value);
		if (EditorCameraSystem.instance != null) // is null in level player
			EditorCameraSystem.instance.submitLeftRight(value);
	}
}
