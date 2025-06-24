using FYFY;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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

	public EventTrigger cameraRotateLeft;
	public EventTrigger cameraRotateRight;
	public EventTrigger cameraTop;
	public EventTrigger cameraDown;
	public EventTrigger cameraLeft;
	public EventTrigger cameraRight;
	public EventTrigger cameraFocusOn;
	public Button cameraSwitchView;
	public EventTrigger cameraZoomIn;
	public EventTrigger cameraZoomOut;
	public GameObject virtualKeyboard;

	public Button showBriefing;

	private bool cancelNextEscape;

    protected override void onStart()
    {
		cancelNextEscape = false;
        foreach (GameObject go in f_InputFields)
			onNewInputField(go);
		f_InputFields.addEntryCallback(onNewInputField);
    }

	private void onNewInputField(GameObject go)
    {
		// Echap permet de sortir de champ de saisie, c'est traité à la phase "Input events" (cf Unity flowchart) du coup dans l'update le champ de saisie n'aura pas le focus et on affichera automatiquement le menu principal, ce qu'on ne veut pas on souhaite que l'Echap qui annule la saisie termine seulement la saisie sans afficher le menu. C'est le Echap suivant qui devra afficher le menu, d'où ce mécanisme pour annuler le prochaine Echap
		go.GetComponent<TMP_InputField>().onEndEdit.AddListener(delegate (string content)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
				cancelNextEscape = true;
		});
	}

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
	{
		if (inputFieldNotSelected())
		{
			//Active/désactive le menu echap si on appuit sur echap et qu'on n'est pas en train de drag un element et que le clavier virtuel n'est pas ouvert et qu'il ne faut pas l'ignorer
			// Shift + Echap est réservé pour sortir du contexte WebGL et revenir sur la page web (voir html)
			if (mainMenu != null && Input.GetKeyDown(KeyCode.Escape) && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && f_dragging.Count == 0 && f_dropZoneEnabled.Count == 0 && !replacementSlotEnabled() && !virtualKeyboard.activeInHierarchy && !cancelNextEscape)
				mainMenu.onClick.Invoke();
			// Autoriser le prochain Echap
			cancelNextEscape = false;

			// Gestion des actions du contôle de l'exécution
			if (buttonNextStep != null && buttonNextStep.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.Space) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)))
				buttonNextStep.onClick.Invoke();
			else if (buttonStop != null && buttonStop.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.Space) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
				buttonStop.onClick.Invoke();
			else if (buttonSpeed != null && buttonSpeed.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.Space) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
				buttonSpeed.onClick.Invoke();
			else if (buttonPause != null && buttonPause.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.Space))
				buttonPause.onClick.Invoke();
			else if (buttonContinue != null && buttonContinue.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.Space))
				buttonContinue.onClick.Invoke();
			else if (buttonExecute != null && buttonExecute.gameObject.activeInHierarchy && buttonExecute.interactable && Input.GetKeyDown(KeyCode.Space))
				buttonExecute.onClick.Invoke();

			// Gestions des actions du contrôle de la caméra
			// Rotation Gauche
			if (cameraRotateLeft != null && cameraRotateLeft.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.A))
				callEntry(cameraRotateLeft, EventTriggerType.PointerDown);
			if (cameraRotateLeft != null && cameraRotateLeft.gameObject.activeInHierarchy && Input.GetKeyUp(KeyCode.A))
				callEntry(cameraRotateLeft, EventTriggerType.PointerUp);
			// Rotation Droite
			if (cameraRotateRight != null && cameraRotateRight.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.E))
				callEntry(cameraRotateRight, EventTriggerType.PointerDown);
			if (cameraRotateRight != null && cameraRotateRight.gameObject.activeInHierarchy && Input.GetKeyUp(KeyCode.E))
				callEntry(cameraRotateRight, EventTriggerType.PointerUp);
			// Move Top
			if (cameraTop != null && cameraTop.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.Z))
				callEntry(cameraTop, EventTriggerType.PointerDown);
			if (cameraTop != null && cameraTop.gameObject.activeInHierarchy && Input.GetKeyUp(KeyCode.Z))
				callEntry(cameraTop, EventTriggerType.PointerUp);
			// Move Down
			if (cameraDown != null && cameraDown.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.S))
				callEntry(cameraDown, EventTriggerType.PointerDown);
			if (cameraDown != null && cameraDown.gameObject.activeInHierarchy && Input.GetKeyUp(KeyCode.S))
				callEntry(cameraDown, EventTriggerType.PointerUp);
			// Move Left
			if (cameraLeft != null && cameraLeft.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.Q))
				callEntry(cameraLeft, EventTriggerType.PointerDown);
			if (cameraLeft != null && cameraLeft.gameObject.activeInHierarchy && Input.GetKeyUp(KeyCode.Q))
				callEntry(cameraLeft, EventTriggerType.PointerUp);
			// Move Right
			if (cameraRight != null && cameraRight.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.D))
				callEntry(cameraRight, EventTriggerType.PointerDown);
			if (cameraRight != null && cameraRight.gameObject.activeInHierarchy && Input.GetKeyUp(KeyCode.D))
				callEntry(cameraRight, EventTriggerType.PointerUp);
			// Focus on next agent
			if (cameraFocusOn != null && cameraFocusOn.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.C))
				callEntry(cameraFocusOn, EventTriggerType.PointerDown);
			// Switch 2D/3D view
			if (cameraSwitchView != null && cameraSwitchView.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.V))
				cameraSwitchView.onClick.Invoke();
			// Zoom In
			if (cameraZoomIn != null && cameraZoomIn.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.R))
				callEntry(cameraZoomIn, EventTriggerType.PointerDown);
			if (cameraZoomIn != null && cameraZoomIn.gameObject.activeInHierarchy && Input.GetKeyUp(KeyCode.R))
				callEntry(cameraZoomIn, EventTriggerType.PointerUp);
			// Zoom Out
			if (cameraZoomOut != null && cameraZoomOut.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.F))
				callEntry(cameraZoomOut, EventTriggerType.PointerDown);
			if (cameraZoomOut != null && cameraZoomOut.gameObject.activeInHierarchy && Input.GetKeyUp(KeyCode.F))
				callEntry(cameraZoomOut, EventTriggerType.PointerUp);

			// Briefing
			if (showBriefing != null && showBriefing.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.H))
				showBriefing.onClick.Invoke();
		}
	}
	private bool inputFieldNotSelected()
	{
		return EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() == null || !EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>().isFocused;
	}

	private void callEntry(EventTrigger trigger, EventTriggerType type)
	{
		// Création d'un pointer Event par défaut
		PointerEventData pointerData = new PointerEventData(EventSystem.current);
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
			if (replacementSlot.GetComponent<Outline>().enabled)
				return true;
		return false;
	}

}
