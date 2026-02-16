using FYFY;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HotkeySystem : FSystem
{
	private Family f_dropZoneEnabled = FamilyManager.getFamily(new AllOfComponents(typeof(DropZone)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY)); // Les drops zones visibles
	private Family f_dragging = FamilyManager.getFamily(new AllOfComponents(typeof(Dragging)));
	private Family f_replacementSlot = FamilyManager.getFamily(new AllOfComponents(typeof(ReplacementSlot)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family f_InputFields = FamilyManager.getFamily(new AllOfComponents(typeof(TMP_InputField)));
	private Family f_programmingArea = FamilyManager.getFamily(new AllOfComponents(typeof(UIRootContainer)));

	public Button mainMenu;
	public Button closeMainMenu;
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

	public Button showBriefing;
	public Button showMapDesc;
	public Button closeMapDesc;
	public GameObject inventory;
	public Button buttonCopyCode;

	public Button AddContainerButton;
	
	[DllImport("__Internal")]
	private static extern void TryToCopy(string txt); // call javascript => send txt to html to copy in clipboard

	[DllImport("__Internal")]
	private static extern string TryToPaste(); // call javascript => get txt from html clipboard

	private EventSystem eventSystem;

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
	private InputAction paste_act;
	private InputAction focusOnNextProgrammingArea_act;
	private InputAction focusOnInventory_act;

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
		paste_act = InputSystem.actions.FindAction("Paste");
		focusOnNextProgrammingArea_act = InputSystem.actions.FindAction("SelectNextProgrammingArea");
		focusOnInventory_act = InputSystem.actions.FindAction("SelectInventory");

		cancelNextEscape = false;
        foreach (GameObject go in f_InputFields)
			onNewInputField(go);
		f_InputFields.addEntryCallback(onNewInputField);

		eventSystem = EventSystem.current;

	}

	private void onNewInputField(GameObject go)
    {
		// Echap permet de sortir de champ de saisie, c'est traité à la phase "Input events" (cf Unity flowchart) du coup dans l'update le champ de saisie n'aura pas le focus et on affichera automatiquement le menu principal, ce qu'on ne veut pas on souhaite que l'Echap qui annule la saisie termine seulement la saisie sans afficher le menu. C'est le Echap suivant qui devra afficher le menu, d'où ce mécanisme pour annuler le prochaine Echap
		go.GetComponent<TMP_InputField>().onEndEdit.AddListener(delegate (string content)
		{
			if (cancel_act.WasPressedThisFrame() && !exitWebGL_act.WasPressedThisFrame())
				cancelNextEscape = true;
		});
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
		if (Utility.inputFieldNotSelected())
		{
			//Active/désactive le menu echap si on appuit sur echap et qu'on n'est pas en train de drag un element et qu'il ne faut pas l'ignorer
			// Shift + Echap est réservé pour sortir du contexte WebGL et revenir sur la page web (voir html)
			if (mainMenu != null && mainMenu.gameObject.activeInHierarchy && mainMenu.IsInteractable() && cancel_act.WasPressedThisFrame() && !exitWebGL_act.WasPressedThisFrame() && f_dragging.Count == 0 && f_dropZoneEnabled.Count == 0 && !replacementSlotEnabled() && !cancelNextEscape)
				mainMenu.onClick.Invoke();
			else if (closeMainMenu != null && closeMainMenu.gameObject.activeInHierarchy && closeMainMenu.IsInteractable() && cancel_act.WasPressedThisFrame() && !exitWebGL_act.WasPressedThisFrame() && f_dragging.Count == 0 && f_dropZoneEnabled.Count == 0 && !replacementSlotEnabled() && !cancelNextEscape)
				closeMainMenu.onClick.Invoke();
			// Autoriser le prochain Echap
			cancelNextEscape = false;

			// Gestion des actions du contôle de l'exécution
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

			// Gestions des actions du contrôle de la caméra
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
			if (mapDesc_act.WasPressedThisFrame())
			{
				if (closeMapDesc != null && closeMapDesc.gameObject.activeInHierarchy)
					closeMapDesc.onClick.Invoke();
				else if (showMapDesc != null)
					showMapDesc.onClick.Invoke();
			}

			// Copy code
			if (buttonCopyCode != null && buttonCopyCode.gameObject.activeInHierarchy && copy_act.WasPressedThisFrame())
				buttonCopyCode.onClick.Invoke();

			// Select next programmingArea
			if (focusOnNextProgrammingArea_act.WasPressedThisFrame())
            {
				if (f_programmingArea.Count > 0)
				{
					// Vérifier si l'objet actuellement sélectionné est dans la hierarchie d'une zone de programmation
					if (eventSystem.currentSelectedGameObject != null && eventSystem.currentSelectedGameObject.GetComponentInParent<UIRootContainer>() != null)
					{
						// Sélectionner la suivante
						GameObject currentProgrammingArea = eventSystem.currentSelectedGameObject.GetComponentInParent<UIRootContainer>().gameObject;
						for (int i = 0; i < f_programmingArea.Count; i++)
						{
							if (f_programmingArea.getAt(i) == currentProgrammingArea)
							{
								if (i < f_programmingArea.Count - 1)
									eventSystem.SetSelectedGameObject(f_programmingArea.getAt(i + 1).GetComponentInChildren<TMP_InputField>().gameObject);
								else
									eventSystem.SetSelectedGameObject(f_programmingArea.First().GetComponentInChildren<TMP_InputField>().gameObject);
							}

						}
						eventSystem.SetSelectedGameObject(f_programmingArea.First().GetComponentInChildren<TMP_InputField>().gameObject);
					}
					else
					{
						// Sélectionner la première
						eventSystem.SetSelectedGameObject(f_programmingArea.First().GetComponentInChildren<TMP_InputField>().gameObject);
					}
				}
				else if (AddContainerButton != null)
					// select + button
					eventSystem.SetSelectedGameObject(AddContainerButton.gameObject);

			}

			// Select inventory
			if (inventory != null && inventory.activeInHierarchy && focusOnInventory_act.WasPressedThisFrame())
				eventSystem.SetSelectedGameObject(inventory);
		}
		/* Gestion du copier-coller dans les InputField (visiblement les versions des navigateurs au moment de ce test et la version d'Unity 6.3 rendent le copier_coller fonctionnel en natif) 
		else if (eventSystem.currentSelectedGameObject != null && eventSystem.currentSelectedGameObject.GetComponent<TMP_InputField>() != null && eventSystem.currentSelectedGameObject.GetComponent<TMP_InputField>().isFocused && Application.platform == RuntimePlatform.WebGLPlayer)
		{
			TMP_InputField focused_inputField = eventSystem.currentSelectedGameObject.GetComponent<TMP_InputField>();
			int start = Mathf.Min(focused_inputField.selectionStringAnchorPosition, focused_inputField.selectionStringFocusPosition);
			int end = Mathf.Max(focused_inputField.selectionStringAnchorPosition, focused_inputField.selectionStringFocusPosition);
			int length = end - start;

			if (copy_act.WasPressedThisFrame())
			{
				if (length > 0)
				{
					TryToCopy(focused_inputField.text.Substring(start, length));
					// cancel internal copy
					GUIUtility.systemCopyBuffer = "";
				}
			}
			else if (paste_act.WasPressedThisFrame())
			{
				TryToPaste();
			}
		}*/
	}

	// Fonction appelée depuis le javascript (voir Assets/WebGLTemplates/Custom/game.html) via le Wrapper du Système
	public void paste(string content)
	{
		if (eventSystem.currentSelectedGameObject != null && eventSystem.currentSelectedGameObject.GetComponent<TMP_InputField>() != null && eventSystem.currentSelectedGameObject.GetComponent<TMP_InputField>().isFocused && Application.platform == RuntimePlatform.WebGLPlayer)
		{
			TMP_InputField focused_inputField = eventSystem.currentSelectedGameObject.GetComponent<TMP_InputField>();
			int start = Mathf.Min(focused_inputField.selectionStringAnchorPosition, focused_inputField.selectionStringFocusPosition);
			int end = Mathf.Max(focused_inputField.selectionStringAnchorPosition, focused_inputField.selectionStringFocusPosition);
			focused_inputField.text = focused_inputField.text.Substring(0, start) + content + focused_inputField.text.Substring(end, focused_inputField.text.Length - end);
			focused_inputField.caretPosition = start + content.Length;
		}

	}

	private void callEntry(EventTrigger trigger, EventTriggerType type)
	{
		// Création d'un pointer Event par défaut
		PointerEventData pointerData = new PointerEventData(eventSystem);
		// Parcourir les entrée pour chercher le bon type
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
			if (replacementSlot.GetComponentInChildren<Outline>().enabled)
				return true;
		return false;
	}

}
