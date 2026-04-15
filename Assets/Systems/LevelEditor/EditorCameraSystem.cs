using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Localization.Components;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// This system manages main camera in editor (movement, zoom)
/// </summary>
public class EditorCameraSystem : FSystem
{

	// Contains current UI focused
	private Family f_UIfocused = FamilyManager.getFamily(new AllOfComponents(typeof(RectTransform), typeof(PointerOver)));
	private Family f_dragging = FamilyManager.getFamily(new AllOfComponents(typeof(Dragging)));

	public Camera mainCamera;

	private float UI_frontBackValue = 0;
	private float UI_leftRightValue = 0;
	private float UI_zoomValue = 0;

	private InputAction middleClick;

	// Distance minimale de zoom
	public float cameraZoomMin;
	// Distance maximale de zoom
	public float cameraZoomMax;

	public LocalizeStringEvent lseTurnLeft;
	public LocalizeStringEvent lseMoveUp;
	public LocalizeStringEvent lseMoveLeft;

	public static EditorCameraSystem instance;

	public EditorCameraSystem()
    {
		instance = this;
    }

	protected override void onStart()
	{
		middleClick = InputSystem.actions.FindAction("MiddleClick");

		// synchronise le contenu des tooltip en fonction du clavier utilisé (azerty vs qwerty'
		lseTurnLeft.StringReference.Arguments = new[] { new { shortcut = InputSystem.actions.FindAction("CameraRotateLeft").GetBindingDisplayString(0) } };
		lseTurnLeft.RefreshString();
		lseMoveUp.StringReference.Arguments = new[] { new { shortcut = InputSystem.actions.FindAction("CameraMoveUp").GetBindingDisplayString(0) } };
		lseMoveUp.RefreshString();
		lseMoveLeft.StringReference.Arguments = new[] { new { shortcut = InputSystem.actions.FindAction("CameraMoveLeft").GetBindingDisplayString(0) } };
		lseMoveLeft.RefreshString();

		EnhancedTouchSupport.Enable();
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
		if (middleClick.IsPressed() && f_UIfocused.Count == 0)
		{
			DeltaControl delta = Pointer.current.delta;

			moveFrontBack(-delta.y.value);
			moveLeftRight(-delta.x.value);
		}

		// Zoom with scroll wheel only if UI element is not focused
		else if ((Mouse.current.scroll.y.value < 0 && f_UIfocused.Count == 0) || UI_zoomValue > 0) // Zoom out
		{
			if (UI_zoomValue > 0)
				zoomOut(UI_zoomValue);
			else
				zoomOut(0.5f) ;
		}
		else if ((Mouse.current.scroll.y.value > 0 && f_UIfocused.Count == 0) || UI_zoomValue < 0) // Zoom in
		{
			if (UI_zoomValue < 0)
				zoomIn(-UI_zoomValue);
			else
				zoomIn(0.5f);
		}

		// Gestion du déplacement de la camera au tactile
		if (f_UIfocused.Count == 0 && f_dragging.Count == 0)
		{
			var touches = Touch.activeTouches;

			if (touches.Count == 2)
			{
				// Distance actuelle et précédente
				float currentDistance = Vector2.Distance(touches[0].screenPosition, touches[1].screenPosition);
				float previousDistance = Vector2.Distance(
					touches[0].screenPosition - touches[0].delta,
					touches[1].screenPosition - touches[1].delta
				);

				float distanceDelta = currentDistance - previousDistance;

				// Zoom (pincement)
				if (Mathf.Abs(distanceDelta) > 0.01f)
				{
					if (distanceDelta > 0)
					{
						zoomIn(distanceDelta / 32);
					}
					else
					{
						zoomOut(-distanceDelta / 32);
					}
				}

				// Pan (déplacement à deux doigts)
				Vector2 move1 = touches[0].delta;
				Vector2 move2 = touches[1].delta;

				// Vérifie que les doigts bougent dans une direction similaire
				if (Vector2.Dot(move1.normalized, move2.normalized) > 0.7f)
				{
					Vector2 averageMove = (move1 + move2) / 2f;
					moveFrontBack(-averageMove.y / 4);
					moveLeftRight(-averageMove.x / 4);
				}
			}
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