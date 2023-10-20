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

	// Distance minimale de zoom
	public float cameraZoomMin;
	// Distance maximale de zoom
	public float cameraZoomMax;

	public static EditorCameraSystem instance;

	public EditorCameraSystem()
    {
		instance = this;
    }

	private bool inputFieldNotSelected()
    {
		return EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() == null;
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		// move camera front/back depending on Vertical axis
		if ((Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.S)) && inputFieldNotSelected())
		{
			moveFrontBack(Input.GetKey(KeyCode.Z) ? 1 : -1);
		}
		// move camera left/right de pending on Horizontal axis
		if ((Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.D)) && inputFieldNotSelected())
		{
			moveLeftRight(Input.GetKey(KeyCode.Q) ? -1 : 1);
		}

		// Zoom in/out with keyboard
		if (Input.GetKey(KeyCode.R) && inputFieldNotSelected())
			zoomIn(0.1f);
		else if (Input.GetKey(KeyCode.F) && inputFieldNotSelected())
			zoomOut(0.1f);

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
		else if (Input.GetAxis("Mouse ScrollWheel") < 0 && f_UIfocused.Count == 0) // Zoom out
		{
			zoomOut(1);
		}
		else if (Input.GetAxis("Mouse ScrollWheel") > 0 && f_UIfocused.Count == 0) // Zoom in
		{
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
}