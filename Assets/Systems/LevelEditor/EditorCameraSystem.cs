using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// This system manages main camera in editor (movement, zoom)
/// </summary>
public class EditorCameraSystem : FSystem
{

	// Contains current UI focused
	private Family f_UIfocused = FamilyManager.getFamily(new AllOfComponents(typeof(RectTransform), typeof(PointerOver)));

	public Camera mainCamera;

	private float UI_frontBackValue = 0;
	private float UI_leftRightValue = 0;
	private float UI_zoomValue = 0;

	// Distance minimale de zoom
	public float cameraZoomMin;
	// Distance maximale de zoom
	public float cameraZoomMax;

	public static EditorCameraSystem instance;

	public EditorCameraSystem()
    {
		instance = this;
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		// move camera front/back depending on Vertical axis
		if (UI_frontBackValue != 0)
			moveFrontBack(UI_frontBackValue);
		// move camera left/right de pending on Horizontal axis
		if (UI_leftRightValue != 0)
			moveLeftRight(UI_leftRightValue);

		// Move camera with wheel click
		if (Input.GetMouseButton(2) && f_UIfocused.Count == 0)
		{
			float mouseY = Input.GetAxisRaw("Mouse Y");
			float mouseX = Input.GetAxisRaw("Mouse X");

			float dist = Mathf.Abs(Mathf.Abs(mouseX) - Mathf.Abs(mouseY));
			if (Mathf.Abs(mouseY) > Mathf.Abs(mouseX)){
				mouseY = mouseY > 0 ? -1 : (mouseY < 0 ? 1 : 0);
				mouseX = (mouseX > 0 ? -dist : (mouseX < 0 ? dist : 0));
            }
			else if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY)){
				mouseY = mouseY > 0 ? -dist : (mouseY < 0 ? dist : 0);
				mouseX = (mouseX > 0 ? -1 : (mouseX < 0 ? 1 : 0));
			}
			else
			{
				mouseY = mouseY > 0 ? -1 : (mouseY < 0 ? 1 : 0);
				mouseX = (mouseX > 0 ? -1 : (mouseX < 0 ? 1 : 0));
			}

			if (mouseY != 0)
				moveFrontBack(mouseY*2);
			if (mouseX != 0)
				moveLeftRight(mouseX*2);
		}

		// Zoom with scroll wheel only if UI element is not focused
		else if ((Input.GetAxis("Mouse ScrollWheel") < 0 && f_UIfocused.Count == 0) || UI_zoomValue > 0) // Zoom out
		{
			if (UI_zoomValue > 0)
				zoomOut(UI_zoomValue);
			else
				zoomOut(1);
		}
		else if ((Input.GetAxis("Mouse ScrollWheel") > 0 && f_UIfocused.Count == 0) || UI_zoomValue < 0) // Zoom in
		{
			if (UI_zoomValue < 0)
				zoomIn(-UI_zoomValue);
			else
				zoomIn(1);
		}
	}
	
	private void moveFrontBack(float value)
    {
		Transform cameraTarget = mainCamera.transform.parent.parent;
		cameraTarget.Translate(0, 0, -value * Mathf.Max((mainCamera.orthographicSize - cameraZoomMin) / 100, 1) * Time.deltaTime * 120);
	}

	private void moveLeftRight(float value)
    {
		Transform cameraTarget = mainCamera.transform.parent.parent;
		cameraTarget.Translate(value * Mathf.Max((mainCamera.orthographicSize - cameraZoomMin) / 100, 1) * Time.deltaTime * 120, 0, 0);
	}

	private void zoomOut(float value)
    {
		// sync orthographic view
		mainCamera.orthographicSize += value * Time.deltaTime * Mathf.Max((mainCamera.orthographicSize - cameraZoomMin) / 10, 1)* 100; // plus on s'éloigne plus on accélère
		if (mainCamera.orthographicSize > cameraZoomMax)
			mainCamera.orthographicSize = cameraZoomMax;
	}

	private void zoomIn(float value)
    {
		// sync orthographic view
		mainCamera.orthographicSize -= value * Time.deltaTime * Mathf.Max((mainCamera.orthographicSize-cameraZoomMin)/10, 1) * 100; // plus on s'approche plus on ralenti
		if (mainCamera.orthographicSize < cameraZoomMin)
			mainCamera.orthographicSize = cameraZoomMin;
	}

	public void set_UIFrontBack(float value)
	{
		UI_frontBackValue = value;
	}

	public void set_UILeftRight(float value)
	{
		UI_leftRightValue = value;
	}

	public void set_UIZoom(float value)
	{
		UI_zoomValue = value;
	}

	public void submitFrontBack(float value)
	{
		moveFrontBack(value);
	}

	public void submitLeftRight(float value)
	{
		moveLeftRight(value);
	}

	public void submitZoom(float value)
	{
		if (value > 0)
			zoomOut(value);
		else
			zoomIn(-value);
	}
}