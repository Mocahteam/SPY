using FYFY;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HotkeySystem : FSystem
{
	private Family f_dropZoneEnabled = FamilyManager.getFamily(new AllOfComponents(typeof(DropZone)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY)); // Les drops zones visibles
	private Family f_dragging = FamilyManager.getFamily(new AllOfComponents(typeof(Dragging)));
	private Family f_replacementSlot = FamilyManager.getFamily(new AllOfComponents(typeof(Outline), typeof(ReplacementSlot)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family f_InputFields = FamilyManager.getFamily(new AllOfComponents(typeof(TMP_InputField)));

	public Button mainMenu;
	public Button buttonExecute;
	public Button buttonPause;
	public Button buttonNextStep;
	public Button buttonContinue;
	public Button buttonSpeed;
	public Button buttonStop;

	public Button cameraSwitchView;
	public EventTrigger cameraRotateLeft;
	public EventTrigger cameraRotateRight;
	public EventTrigger cameraTop;
	public EventTrigger cameraDown;
	public EventTrigger cameraLeft;
	public EventTrigger cameraRight;
	public EventTrigger cameraFocusOn;
	public EventTrigger cameraZoomIn;
	public EventTrigger cameraZoomOut;

	public GameObject virtualKeyboard;
	public Button showBriefing;
	public Button showMapDesc;
	public Button buttonCopyCode;

	private bool cancelNextEscape;

	private InputAction cancel_act;
	private InputAction exitWebGL_act;
	private InputAction nextStep_act;
	private InputAction stop_act;
	private InputAction playPause_act;
	private InputAction rotateLeft_act;
	private InputAction rotateRight_act;
	private InputAction moveUp_act;
	private InputAction moveDown_act;
	private InputAction moveLeft_act;
	private InputAction moveRight_act;
	private InputAction focusOnAgent_act;
	private InputAction switchView_act;
	private InputAction zoomIn_act;
	private InputAction zoomOut_act;
	private InputAction showBriefing_act;
	private InputAction mapDesc_act;
	private InputAction copy_act;

	protected override void onStart()
	{
		cancel_act = InputSystem.actions.FindAction("Cancel");
		exitWebGL_act = InputSystem.actions.FindAction("ExitWebGL");
		nextStep_act = InputSystem.actions.FindAction("NextStep");
		stop_act = InputSystem.actions.FindAction("Stop");
		playPause_act = InputSystem.actions.FindAction("PlayPause");
		rotateLeft_act = InputSystem.actions.FindAction("CameraRotateLeft");
		rotateRight_act = InputSystem.actions.FindAction("CameraRotateRight");
		moveUp_act = InputSystem.actions.FindAction("CameraMoveUp");
		moveDown_act = InputSystem.actions.FindAction("CameraMoveDown");
		moveLeft_act = InputSystem.actions.FindAction("CameraMoveLeft");
		moveRight_act = InputSystem.actions.FindAction("CameraMoveRight");
		focusOnAgent_act = InputSystem.actions.FindAction("CameraFocusOnAgent");
		switchView_act = InputSystem.actions.FindAction("CameraSwitchView");
		zoomIn_act = InputSystem.actions.FindAction("CameraZoomIn");
		zoomOut_act = InputSystem.actions.FindAction("CameraZoomOut");
		showBriefing_act = InputSystem.actions.FindAction("ShowBriefing");
		mapDesc_act = InputSystem.actions.FindAction("MapDesc");
		copy_act = InputSystem.actions.FindAction("Copy");

		cancelNextEscape = false;
        foreach (GameObject go in f_InputFields)
			onNewInputField(go);
		f_InputFields.addEntryCallback(onNewInputField);
    }

	private void onNewInputField(GameObject go)
    {
		// Echap permet de sortir de champ de saisie, c'est trait� � la phase "Input events" (cf Unity flowchart) du coup dans l'update le champ de saisie n'aura pas le focus et on affichera automatiquement le menu principal, ce qu'on ne veut pas on souhaite que l'Echap qui annule la saisie termine seulement la saisie sans afficher le menu. C'est le Echap suivant qui devra afficher le menu, d'o� ce m�canisme pour annuler le prochaine Echap
		go.GetComponent<TMP_InputField>().onEndEdit.AddListener(delegate (string content)
		{
			if (cancel_act.WasPressedThisFrame() && !exitWebGL_act.WasPressedThisFrame())
				cancelNextEscape = true;
		});
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
		if (inputFieldNotSelected())
		{
			//Active/d�sactive le menu echap si on appuit sur echap et qu'on n'est pas en train de drag un element et que le clavier virtuel n'est pas ouvert et qu'il ne faut pas l'ignorer
			// Shift + Echap est r�serv� pour sortir du contexte WebGL et revenir sur la page web (voir html)
			if (mainMenu != null && cancel_act.WasPressedThisFrame() && ! exitWebGL_act.WasPressedThisFrame() && f_dragging.Count == 0 && f_dropZoneEnabled.Count == 0 && !replacementSlotEnabled() && !virtualKeyboard.activeInHierarchy && !cancelNextEscape)
				mainMenu.onClick.Invoke();
			// Autoriser le prochain Echap
			cancelNextEscape = false;

			// Gestion des actions du cont�le de l'ex�cution
			if (buttonNextStep != null && buttonNextStep.gameObject.activeInHierarchy && nextStep_act.WasPressedThisFrame())
				buttonNextStep.onClick.Invoke();
			else if (buttonStop != null && buttonStop.gameObject.activeInHierarchy && stop_act.WasPressedThisFrame())
				buttonStop.onClick.Invoke();
			else if (buttonPause != null && buttonPause.gameObject.activeInHierarchy && playPause_act.WasPressedThisFrame())
				buttonPause.onClick.Invoke();
			else if (buttonContinue != null && buttonContinue.gameObject.activeInHierarchy && playPause_act.WasPressedThisFrame())
				buttonContinue.onClick.Invoke();
			else if (buttonExecute != null && buttonExecute.gameObject.activeInHierarchy && buttonExecute.interactable && playPause_act.WasPressedThisFrame())
				buttonExecute.onClick.Invoke();

			// Gestions des actions du contr�le de la cam�ra
			// Rotation Gauche
			if (cameraRotateLeft != null && cameraRotateLeft.gameObject.activeInHierarchy && rotateLeft_act.WasPressedThisFrame())
				callEntry(cameraRotateLeft, EventTriggerType.PointerDown);
			if (cameraRotateLeft != null && cameraRotateLeft.gameObject.activeInHierarchy && rotateLeft_act.WasReleasedThisFrame())
				callEntry(cameraRotateLeft, EventTriggerType.PointerUp);
			// Rotation Droite
			if (cameraRotateRight != null && cameraRotateRight.gameObject.activeInHierarchy && rotateRight_act.WasPressedThisFrame())
				callEntry(cameraRotateRight, EventTriggerType.PointerDown);
			if (cameraRotateRight != null && cameraRotateRight.gameObject.activeInHierarchy && rotateRight_act.WasReleasedThisFrame())
				callEntry(cameraRotateRight, EventTriggerType.PointerUp);
			// Move Up
			if (cameraTop != null && cameraTop.gameObject.activeInHierarchy && moveUp_act.WasPressedThisFrame())
				callEntry(cameraTop, EventTriggerType.PointerDown);
			if (cameraTop != null && cameraTop.gameObject.activeInHierarchy && moveUp_act.WasReleasedThisFrame())
				callEntry(cameraTop, EventTriggerType.PointerUp);
			// Move Down
			if (cameraDown != null && cameraDown.gameObject.activeInHierarchy && moveDown_act.WasPressedThisFrame())
				callEntry(cameraDown, EventTriggerType.PointerDown);
			if (cameraDown != null && cameraDown.gameObject.activeInHierarchy && moveDown_act.WasReleasedThisFrame())
				callEntry(cameraDown, EventTriggerType.PointerUp);
			// Move Left
			if (cameraLeft != null && cameraLeft.gameObject.activeInHierarchy && moveLeft_act.WasPressedThisFrame())
				callEntry(cameraLeft, EventTriggerType.PointerDown);
			if (cameraLeft != null && cameraLeft.gameObject.activeInHierarchy && moveLeft_act.WasReleasedThisFrame())
				callEntry(cameraLeft, EventTriggerType.PointerUp);
			// Move Right
			if (cameraRight != null && cameraRight.gameObject.activeInHierarchy && moveRight_act.WasPressedThisFrame())
				callEntry(cameraRight, EventTriggerType.PointerDown);
			if (cameraRight != null && cameraRight.gameObject.activeInHierarchy && moveRight_act.WasReleasedThisFrame())
				callEntry(cameraRight, EventTriggerType.PointerUp);
			// Focus on next agent
			if (cameraFocusOn != null && cameraFocusOn.gameObject.activeInHierarchy && focusOnAgent_act.WasPressedThisFrame())
				callEntry(cameraFocusOn, EventTriggerType.PointerDown);
			// Switch 2D/3D view
			if (cameraSwitchView != null && cameraSwitchView.gameObject.activeInHierarchy && switchView_act.WasPressedThisFrame())
				cameraSwitchView.onClick.Invoke();
			// Zoom In
			if (cameraZoomIn != null && cameraZoomIn.gameObject.activeInHierarchy && zoomIn_act.WasPressedThisFrame())
				callEntry(cameraZoomIn, EventTriggerType.PointerDown);
			if (cameraZoomIn != null && cameraZoomIn.gameObject.activeInHierarchy && zoomIn_act.WasReleasedThisFrame())
				callEntry(cameraZoomIn, EventTriggerType.PointerUp);
			// Zoom Out
			if (cameraZoomOut != null && cameraZoomOut.gameObject.activeInHierarchy && zoomOut_act.WasPressedThisFrame())
				callEntry(cameraZoomOut, EventTriggerType.PointerDown);
			if (cameraZoomOut != null && cameraZoomOut.gameObject.activeInHierarchy && zoomOut_act.WasReleasedThisFrame())
				callEntry(cameraZoomOut, EventTriggerType.PointerUp);

			// Briefing
			if (showBriefing != null && showBriefing.gameObject.activeInHierarchy && showBriefing_act.WasPressedThisFrame())
				showBriefing.onClick.Invoke();

			// Map description
			if (showMapDesc != null && mapDesc_act.WasPressedThisFrame())
				showMapDesc.onClick.Invoke();

			// Copy code
			if (buttonCopyCode != null && buttonCopyCode.gameObject.activeInHierarchy && copy_act.WasPressedThisFrame())
				buttonCopyCode.onClick.Invoke();
		}
	}
	private bool inputFieldNotSelected()
	{
		return EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() == null || !EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>().isFocused;
	}

	private void callEntry(EventTrigger trigger, EventTriggerType type)
	{
		// Cr�ation d'un pointer Event par d�faut
		PointerEventData pointerData = new PointerEventData(EventSystem.current);
		// Parcourir les entr�e pour chercher le bon type
		foreach (EventTrigger.Entry entry in trigger.triggers)
		{
			if (entry.eventID == type)
				// simulation du clic
				entry.callback.Invoke(pointerData); 
		}
	}

	private bool replacementSlotEnabled()
	{
		foreach (GameObject replacementSlot in f_replacementSlot)
			if (replacementSlot.GetComponent<Outline>().enabled)
				return true;
		return false;
	}

}
