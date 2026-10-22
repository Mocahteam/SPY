using UnityEngine;

public class CameraSystemBridge : MonoBehaviour
{
    public void LocateAgent(LinkedWith agent)
    {
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.FocusOnAgent(agent.target);
    }


    public void FocusNextAgent()
    {
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.FocusNextAgent();

	}
    public void SubmitZoom(float value)
    {
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.SubmitZoom(value);
		if (EditorCameraSystem.instance != null) // is null in level player
			EditorCameraSystem.instance.SubmitZoom(value);
	}
	public void ToggleOrthographicPerspective()
	{
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.ToggleOrthographicPerspective();
	}

	public void Set_UIFrontBack(float value)
	{
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.Set_UIFrontBack(value);
		if (EditorCameraSystem.instance != null) // is null in level player
			EditorCameraSystem.instance.Set_UIFrontBack(value);
	}

	public void Set_UILeftRight(float value)
	{
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.Set_UILeftRight(value);
		if (EditorCameraSystem.instance != null) // is null in level player
			EditorCameraSystem.instance.Set_UILeftRight(value);
	}

	public void Set_UIRotate(float value)
	{
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.Set_UIRotate(value);
    }

    public void Set_UIPitching(float value)
    {
        if (CameraSystem.instance != null) // is null in level editor
            CameraSystem.instance.Set_UIPitching(value);
    }

    public void Set_UIZoom(float value)
	{
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.Set_UIZoom(value);
		if (EditorCameraSystem.instance != null) // is null in level player
			EditorCameraSystem.instance.Set_UIZoom(value);
	}

	public void SubmitRotate(float value)
	{
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.SubmitRotate(value);
    }

    public void SubmitPitching(float value)
    {
        if (CameraSystem.instance != null) // is null in level editor
            CameraSystem.instance.SubmitPitching(value);
    }

    public void SubmitFrontBack(float value)
	{
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.SubmitFrontBack(value);
		if (EditorCameraSystem.instance != null) // is null in level player
			EditorCameraSystem.instance.SubmitFrontBack(value);
	}

	public void SubmitLeftRight(float value)
	{
		if (CameraSystem.instance != null) // is null in level editor
			CameraSystem.instance.SubmitLeftRight(value);
		if (EditorCameraSystem.instance != null) // is null in level player
			EditorCameraSystem.instance.SubmitLeftRight(value);
	}
}
