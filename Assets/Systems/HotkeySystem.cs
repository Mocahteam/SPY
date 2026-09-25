using FYFY;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UI;

public class HotkeySystem : FSystem
{
	private Family f_dropZoneEnabled = FamilyManager.getFamily(new AllOfComponents(typeof(DropZone)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY)); // Les drops zones visibles
	private Family f_dragging = FamilyManager.getFamily(new AllOfComponents(typeof(Dragging)));
	private Family f_replacementSlot = FamilyManager.getFamily(new AllOfComponents(typeof(ReplacementSlot)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family f_InputFields = FamilyManager.getFamily(new AllOfComponents(typeof(TMP_InputField)));
	private Family f_programmingArea = FamilyManager.getFamily(new AllOfComponents(typeof(UIRootContainer)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_memoryArea = FamilyManager.getFamily(new AllOfComponents(typeof(ExecutablePanel)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_localizationLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(LocalizationLoaded)));
	private Family f_localizedStringEvents = FamilyManager.getFamily(new AllOfComponents(typeof(LocalizeStringEvent)));

    public Button mainMenu;
	public Button closeMainMenu;

	public HotKeysSpec hotKeys;

    private EventSystem eventSystem;

	public bool cancelNextCancel_act;

	private InputAction cancel_act;
	private InputAction exitWebGL_act;
    
    const string Us = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private string s_Local = null; // null si non initialisée ou 36 libellés dans l'ordre du layout du clavier, ou "" si le js n'a pu identifier le layout (voir KbLayout_Fetch dans le jslib)

    [DllImport("__Internal")]
    static extern void KbLayout_Fetch();

    // L'instance
    public static HotkeySystem instance;

	public HotkeySystem()
	{
		instance = this;
	}

	protected override void onStart()
	{
		// Récupération des inputActions à partir des clés définies dans l'inspector (voir composant HotKeysSpec)
		foreach (HotKeySpec hotkey in hotKeys.spec)
		{
			hotkey.inputAction = InputSystem.actions.FindAction(hotkey.inputActionName);
			hotkey.hasModifier = HasModifier(hotkey.inputAction);
		}

		cancel_act = InputSystem.actions.FindAction("Cancel");
		exitWebGL_act = InputSystem.actions.FindAction("ExitWebGL");

        cancelNextCancel_act = false;
        foreach (GameObject go in f_InputFields)
			onNewInputField(go);
		f_InputFields.addEntryCallback(onNewInputField);

		eventSystem = EventSystem.current;

		// En WebGL le layout du clavier est abstrait si bien que Unity renverra toujours la configuration QWERTY, pour régler ce problème on passe par le JS pour tenter de récupérer la bonne association touches/symboles
		if (Application.platform == RuntimePlatform.WebGLPlayer)
			KbLayout_Fetch();
		else
			OnKeyboardLayoutDefined(""); // en éditeur le comportement par défaut fonctionne, on ne cherche donc pas à retrouver la bonne association touches/symboles

        MainLoop.instance.StartCoroutine(waitLocalizationAndUpdateShortcuts());
    }

    // Fonction appelée depuis le javascript (voir Assets/Plugins/externalCalls.jslib) via le Wrapper du Système
	// L'objectif ici est de récupérer dans Unity les caractères associés à chaque touche du clavier en prenant en compte les différents layout du clavier (QWERTY, AZERTY, QWERTZ...)
    public void OnKeyboardLayoutDefined(string data)
    {
        s_Local = data != null && data.Length == 36 ? data : "";
    }

    private IEnumerator waitLocalizationAndUpdateShortcuts()
    {
		// Attendre que le système de localization se soit initialisé
        yield return new WaitWhile(() => f_localizationLoaded.Count == 0);
		// Attendre d'avoir obtenu la réponse du JS quand à l'initialisation du s_Local
		yield return new WaitWhile(() => s_Local == null);

        // On parcours tous les champs localisés
        foreach (GameObject localizedGo in f_localizedStringEvents)
        {
            foreach (LocalizeStringEvent lse in localizedGo.GetComponents<LocalizeStringEvent>())
            {
                bool dirty = false;

                // On copie les clés : affecter Value déclenche un ValueChanged
                foreach (string key in new List<string>(lse.StringReference.Keys))
                {
                    if (!key.StartsWith("shortcut")) continue;
                    if (!(lse.StringReference[key] is StringVariable sv)) continue;

                    // On récupère le nom de l'action en supprimant le préfixe "shortcut"
                    InputAction action = InputSystem.actions.FindAction(key.Substring(8));
                    if (action == null)
                    {
                        Debug.LogWarning($"No action \"{key.Substring(8)}\" for the key {key}", localizedGo);
                        continue;
                    }

                    string s = action.GetBindingDisplayString(0);
                    if (string.IsNullOrEmpty(s)) continue;

                    string value = "";
                    // On découpe les [Modifier]+[letter] (ex: Shift+Z)
                    foreach (string token in s.Split("+"))
                    {
                        if (value.Length > 0)
                            value += "+";
                        if (s_Local != null && s_Local != "" && token.Length == 1)
                        {
                            int i = Us.IndexOf(char.ToUpperInvariant(token[0]));
                            value += i < 0 ? token : s_Local[i].ToString();
                        }
                        else
                            value += token;
                    }

                    if (sv.Value == value) continue;

                    sv.Value = value;
                    dirty = true;
                }

                if (dirty) lse.RefreshString();
            }
        }
    }

    private void onNewInputField(GameObject go)
    {
		// Echap permet de sortir de champ de saisie, c'est traité à la phase "Input events" (cf Unity flowchart) du coup dans l'update le champ de saisie n'aura pas le focus et on affichera automatiquement le menu principal, ce qu'on ne veut pas on souhaite que l'Echap qui annule la saisie termine seulement la saisie sans afficher le menu. C'est le Echap suivant qui devra afficher le menu, d'où ce mécanisme pour annuler le prochaine Echap
		go.GetComponent<TMP_InputField>().onEndEdit.AddListener(delegate (string content)
		{
			if (cancel_act.WasPressedThisFrame() && !exitWebGL_act.WasPressedThisFrame())
				cancelNextCancel_act = true;
		});
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
        // Si l'utilisateur choisi de sortir du contexte WebGL et revenir sur la page web (voir html), on ne traite pas plus d'actions cette frame
        if (exitWebGL_act.WasPressedThisFrame())
			return;

		// Si on est dans un inputfield on ne traite pas d'actions cette frame
		if (Utility.inputFieldSelected())
			return;

		// Cas particulier de la gestion de l'action Cancel

		// On appuit sur Echap mais on doit ignorer cette action
		if (cancel_act.WasPressedThisFrame() && cancelNextCancel_act)
		{
			// Autoriser le prochain Cancel
			cancelNextCancel_act = false;
			return;
		}
		// Active/désactive le menu echap si on appuit sur echap et qu'il ne faut pas l'ignorer
		else if (cancel_act.WasPressedThisFrame() && !cancelNextCancel_act)
		{
			// On s'assure qu'on n'est pas en train de drag un element
			if (f_dragging.Count == 0 && f_dropZoneEnabled.Count == 0 && !replacementSlotEnabled())
			{
				// afficher cacher le menu principal en fonction de l'état de l'UI
				if (mainMenu != null && mainMenu.gameObject.activeInHierarchy && mainMenu.IsInteractable())
					mainMenu.onClick.Invoke();
				else if (closeMainMenu != null && closeMainMenu.gameObject.activeInHierarchy && closeMainMenu.IsInteractable())
					closeMainMenu.onClick.Invoke();
			}
			return;
		}

        // Gestion en priorité des actions avec modifier
        foreach (HotKeySpec hotKey in hotKeys.spec)
		{
			// Vérifier qu'on a un modifier
			if (hotKey.hasModifier)
				if (processHotKeySpec(hotKey))
					return;
		}

        // Gestion dans un second temps des actions sans modifier
        foreach (HotKeySpec hotKey in hotKeys.spec)
        {
			// Vérifier qu'il n'y a pas de modifier
			if (!hotKey.hasModifier)
				if (processHotKeySpec(hotKey))
					return;
        }
	}

	private bool processHotKeySpec(HotKeySpec hotKey)
	{
		if (hotKey.UItriggers.Count > 0)
		{
			// On est dans le cas où cette InputAction est associée à au moins un élément d'UI
			foreach (UITrigger trigger in hotKey.UItriggers)
			{
				if (trigger.interactableUI != null && (!trigger.requireActiveInHierarchy || trigger.interactableUI.gameObject.activeInHierarchy) && (!trigger.requireInteractable || (trigger.interactableUI.GetComponent<Selectable>() != null && trigger.interactableUI.GetComponent<Selectable>().IsInteractable())))
				{
					// Adapter le comportement en fonction de si on est sur un Button ou un EventTrigger
					// Cas où on a un Button
					trigger.interactableUI.TryGetComponent<Button>(out Button button);
					if (button != null && hotKey.inputAction.WasPressedThisFrame())
					{
						button.onClick.Invoke();
						return true;
					}
					// Cas où on a un EventTrigger
					trigger.interactableUI.TryGetComponent<EventTrigger>(out EventTrigger eventTrigger);
					if (eventTrigger != null)
					{
						if (hotKey.inputAction.WasPressedThisFrame())
						{
							callEntry(eventTrigger, EventTriggerType.PointerDown);
							return true;
						}
						else if (hotKey.inputAction.WasReleasedThisFrame())
						{
							callEntry(eventTrigger, EventTriggerType.PointerUp);
							return true;
						}
					}
					// On est sur un GO qui n'a ni un bouton ni un EventTrigger, donc on va regarde si on n'est pas dans sur un cas particulier
					// On commence par vérifier si l'action s'est bien déclenchée
					if (hotKey.inputAction.WasPressedThisFrame())
					{
						// Cas de la sélection de l'inventaire
						if (hotKey.inputActionName == "SelectInventory")
						{
							eventSystem.SetSelectedGameObject(trigger.interactableUI.gameObject);
							return true;
						}
						// Cas de la navigation dans les zones de programme et du bouton "+"
						else if (hotKey.inputActionName == "SelectNextProgrammingArea" || hotKey.inputActionName == "SelectPreviousProgrammingArea")
						{
							Button addNewProgramminArea = trigger.interactableUI.GetComponentInChildren<Button>();
							bool addButtonAvailable = addNewProgramminArea != null && addNewProgramminArea.gameObject.activeInHierarchy && addNewProgramminArea.IsInteractable();
							if (f_programmingArea.Count > 0)
							{
								// Vérifier si l'objet actuellement sélectionné est dans la hierarchie d'une zone de programmation
								if (eventSystem.currentSelectedGameObject != null && eventSystem.currentSelectedGameObject.GetComponentInParent<UIRootContainer>() != null)
								{
									// Sélectionner la suivante/précédente
									GameObject currentProgrammingArea = eventSystem.currentSelectedGameObject.GetComponentInParent<UIRootContainer>().gameObject;
									for (int i = 0; i < f_programmingArea.Count; i++)
									{
										if (f_programmingArea.getAt(i) == currentProgrammingArea)
										{
											// gestion de la sélection de la zone précédente
											if (hotKey.inputActionName == "SelectPreviousProgrammingArea")
											{
												// Si une zone de programme nous précède, on la sélectionne
												if (i > 0)
													eventSystem.SetSelectedGameObject(f_programmingArea.getAt(i - 1).GetComponentInChildren<TMP_InputField>().gameObject);
												// si on est sur la première et que le bouton '+' est actif et visible, on le sélectionne
												else if (addButtonAvailable)
													eventSystem.SetSelectedGameObject(addNewProgramminArea.gameObject);
												// sinon on revient à la dernière
												else
													eventSystem.SetSelectedGameObject(f_programmingArea.getAt(f_programmingArea.Count - 1).GetComponentInChildren<TMP_InputField>().gameObject);
											}
											else
											{
												// Si il y a encore une zone de programme, on la sélectionne
												if (i < f_programmingArea.Count - 1)
													eventSystem.SetSelectedGameObject(f_programmingArea.getAt(i + 1).GetComponentInChildren<TMP_InputField>().gameObject);
												// si on est sur la dernière et que le bouton '+' est actif et visible, on le sélectionne
												else if (addButtonAvailable)
													eventSystem.SetSelectedGameObject(addNewProgramminArea.gameObject);
												// sinon on revient au premier
												else
													eventSystem.SetSelectedGameObject(f_programmingArea.First().GetComponentInChildren<TMP_InputField>().gameObject);
											}
										}

									}
								}
								else
								{
									if (hotKey.inputActionName == "SelectPreviousProgrammingArea")
										// Sélectionner la dernière
										eventSystem.SetSelectedGameObject(f_programmingArea.getAt(f_programmingArea.Count - 1).GetComponentInChildren<TMP_InputField>().gameObject);
									else
										// Sélectionner la première
										eventSystem.SetSelectedGameObject(f_programmingArea.First().GetComponentInChildren<TMP_InputField>().gameObject);
								}
							}
							else if (addButtonAvailable)
								// select + button
								eventSystem.SetSelectedGameObject(addNewProgramminArea.gameObject);
							return true;
						}
					}
				}
			}
		}
		else
		{
			// On est dans le cas où cette InputAction n'est associée à aucune UI
			if (hotKey.inputAction.WasPressedThisFrame())
			{
				// Cas de la navigation dans les zones mémoires des robots
				if (hotKey.inputActionName == "SelectNextMemoryArea" || hotKey.inputActionName == "SelectPreviousMemoryArea")
				{
					if (f_memoryArea.Count > 0)
					{
						// Vérifier si l'objet actuellement sélectionné est dans la hierarchie d'une zone de mémoire
						if (eventSystem.currentSelectedGameObject != null && eventSystem.currentSelectedGameObject.GetComponentInParent<ExecutablePanel>() != null)
						{
							// Si on est dans une zone de traçage sélectionner son parent
							if (eventSystem.currentSelectedGameObject.GetComponentInParent<ToggleGroup>() != null)
								eventSystem.SetSelectedGameObject(eventSystem.currentSelectedGameObject.GetComponentInParent<ExecutablePanel>().transform.Find("Header/agentName").gameObject);
							else
							{
								// Sélectionner la suivante/précédente
								GameObject currentMemoryArea = eventSystem.currentSelectedGameObject.GetComponentInParent<ExecutablePanel>().gameObject;
								for (int i = 0; i < f_memoryArea.Count; i++)
								{
									if (f_memoryArea.getAt(i) == currentMemoryArea)
									{
										// gestion de la sélection de la zone précédente
										if (hotKey.inputActionName == "SelectPreviousMemoryArea")
										{
											// Si une mémoire nous précède, on la sélectionne
											if (i > 0)
												eventSystem.SetSelectedGameObject(f_memoryArea.getAt(i - 1).transform.Find("Header/agentName").gameObject);
											// sinon on revient à la dernière
											else
												eventSystem.SetSelectedGameObject(f_memoryArea.getAt(f_memoryArea.Count - 1).transform.Find("Header/agentName").gameObject);
										}
										else
										{
											// Si il y a encore une mémoire, on la sélectionne
											if (i < f_memoryArea.Count - 1)
												eventSystem.SetSelectedGameObject(f_memoryArea.getAt(i + 1).transform.Find("Header/agentName").gameObject);
											// sinon on revient à la première
											else
												eventSystem.SetSelectedGameObject(f_memoryArea.First().transform.Find("Header/agentName").gameObject);
										}
									}
								}
							}
						}
						else
						{
							if (hotKey.inputActionName == "SelectPreviousMemoryArea")
								// Sélectionner la dernière
								eventSystem.SetSelectedGameObject(f_memoryArea.getAt(f_memoryArea.Count - 1).transform.Find("Header/agentName").gameObject);
							else
								// Sélectionner la première
								eventSystem.SetSelectedGameObject(f_memoryArea.First().transform.Find("Header/agentName").gameObject);
						}
					}
					return true;
				}
				// Cas de l'accès à la zone de traçage
				else if (hotKey.inputActionName == "TracingAccess")
				{
                    if (f_memoryArea.Count > 0)
                    {
                        // Vérifier si l'objet actuellement sélectionné est dans la hierarchie d'une zone de mémoire
                        if (eventSystem.currentSelectedGameObject != null && eventSystem.currentSelectedGameObject.GetComponentInParent<ExecutablePanel>() != null)
                        {
                            ExecutablePanel currentMemoryArea = eventSystem.currentSelectedGameObject.GetComponentInParent<ExecutablePanel>();
                            if (currentMemoryArea.GetComponentInChildren<ToggleGroup>() != null)
                                eventSystem.SetSelectedGameObject(currentMemoryArea.GetComponentInChildren<ToggleGroup>().transform.Find("Header").gameObject);
                        }
                    }
					return true;
                }
				// cas du ExitWebGL où on a rien à faire côté Unity et déléguer à la page Html
				else if (hotKey.inputActionName == "ExitWebGL")
					return true;
			}
		}
		return false;
    }

    bool HasModifier(InputAction action)
    {
        foreach (var binding in action.bindings)
        {
            if (binding.effectivePath.Contains("<Keyboard>/ctrl") ||
                binding.effectivePath.Contains("<Keyboard>/shift") ||
                binding.effectivePath.Contains("<Keyboard>/alt"))
            {
                return true;
            }
        }

        return false;
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
