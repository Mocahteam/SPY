using DIG.GBLXAPI;
using FYFY;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class UINavigationManager : FSystem
{
	private Family f_textsUnselectable = FamilyManager.getFamily(new AllOfComponents(typeof(TextMeshProUGUI)), new NoneOfComponents(typeof(Selectable)));
	private Family f_draggedItems = FamilyManager.getFamily(new AllOfComponents(typeof(Dragging)));
	private Family f_InputFields = FamilyManager.getFamily(new AllOfComponents(typeof(TMP_InputField)));

	private Family f_buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	public List<GameObject> autoFocusPrority;
	private GameObject lastSelected;
	public EventSystem eventSystem;

	private InputAction navigateAction;
    private InputAction homeAction;
    private InputAction rightClick;
	private InputAction middleClick;

    private UserData userData;

    [DllImport("__Internal")]
	private static extern void QuitFullScreen(); // call javascript
	[DllImport("__Internal")]
	private static extern void ResetFullScreen(); // call javascript
	[DllImport("__Internal")]
	private static extern bool IsUnityCanvasFocused(); // call javascript


    // L'instance
    public static UINavigationManager instance;

    public UINavigationManager()
    {
        instance = this;
    }
    protected override void onStart()
    {
        GameObject go = GameObject.Find("GameData");
		if (go != null)
		{
			userData = go.GetComponent<UserData>();

            if (Application.platform == RuntimePlatform.WebGLPlayer)
                MainLoop.instance.StartCoroutine(catchApplicationState(IsUnityCanvasFocused()));
            else
                MainLoop.instance.StartCoroutine(catchApplicationState(Application.isFocused));
        }

        foreach (GameObject text in f_textsUnselectable)
			onNewUnselectableText(text);
		f_textsUnselectable.addEntryCallback(onNewUnselectableText);
		if (eventSystem == null)
			eventSystem = EventSystem.current;

		navigateAction = InputSystem.actions.FindAction("Navigate");
        homeAction = InputSystem.actions.FindAction("Home");
        rightClick = InputSystem.actions.FindAction("RightClick");
		middleClick = InputSystem.actions.FindAction("MiddleClick");

		EnhancedTouchSupport.Enable();

		// Add callback in all inputField
		foreach (GameObject input in f_InputFields)
			onNewInputField(input);
		f_InputFields.addEntryCallback(onNewInputField);

    }

    protected override void onProcess(int familiesUpdateCount)
	{
		if (homeAction.WasPressedThisFrame())
		{
            // Définir le currentSelectedGameObject à null pour qu'au prochain update l'UI le plus prioritaire soit automatiquement sélectionnée. Permet ainsi même si on est sur l'objet le plus prioritaire de refaire vocaliser cet élément.
            eventSystem.SetSelectedGameObject(null);
			lastSelected = null;
			return;
        }
		
		if (eventSystem.alreadySelecting)
            return;

        // Récupérer la valeur Vector2 de Navigate
        Vector2 navigateValue = navigateAction.ReadValue<Vector2>();

        // Get the currently selected UI element from the event system.
        GameObject selected = eventSystem.currentSelectedGameObject;

		// Par défaut un clic-droit ou un clic-molette déclenche un PointerDown. A chaque PointerDown l'EventSystem regarde s'il doit sélectionner un objet, sur un clic droit il considère que non et donc rend le currentSelectedGameObject à null. Comme on utilise le clic-droit pour supprimer des blocs, si le currentSelectedGameObject devient null à chaque suppression, le UINavigationManager sélectionne alors automatiquement le prochain gameObject ce qui nous fait sortir de la zone d'édition. On annule donc se comportement en maintenant le currentSelectedGameObject au précédent connu.
		if (selected == null && lastSelected != null && (rightClick.WasPressedThisFrame() || middleClick.WasPressedThisFrame()))
		{
			selected = lastSelected;
			eventSystem.SetSelectedGameObject(selected);
		}

		// On va chercher à donner le focus à notre liste de priorité si on n'a pas d'objet sélectionné ou qu'il n'est pas réellement interactif et qu'il ne s'agit pas de cas particuliers (on tolère les objets non interactable comme l'objets dragged, les objets dans une zone de programme, les tiles de sélection dans TitleScreen et les vignettes d'avatars bloquées, dans ces cas on leur laisse le focus dessus, se sont les seuls objets inactif que l'on va autoriser à sélectionner)
		if (selected == null || (!IsReallyInteractable(selected) && selected.GetComponent<Dragging>() == null && selected.GetComponentInParent<UIRootContainer>() == null && selected.GetComponentInParent<UIRootExecutor>() == null && selected.transform.parent.name != "GameList" && !selected.CompareTag("UI_Avatar")))
		{
			// Try to give focus on one of the priority list
			bool focused = false;
			if (autoFocusPrority != null)
				foreach (GameObject target in autoFocusPrority)
					if (IsReallyInteractable(target))
					{
						eventSystem.SetSelectedGameObject(target);
						focused = true;
						break;
					}
			// if we can't, give focus to the last button available
			if (!focused && f_buttons.Count > 0)
			{
				eventSystem.SetSelectedGameObject(f_buttons.getAt(f_buttons.Count - 1));
			}
		}

		// ---- Manage keyboard navigation in script ----
		// no item dragged and last selection was an element in script
		if (lastSelected != null && f_draggedItems.Count == 0 && (lastSelected.GetComponentInParent<UIRootContainer>() != null || lastSelected.GetComponentInParent<UIRootExecutor>() != null))
		{
            // press up or down
            if (navigateAction.WasPressedThisFrame() && navigateValue.y != 0)
			{
				// do nothing if last selected object is a focused inputfield (case of for blocks)
				TMP_InputField input = lastSelected.GetComponent<TMP_InputField>();
				if (input == null || !input.isFocused)
				{
					// get all child selectable (automatically sorted top down)
					List<Selectable> selectables;
					if (lastSelected.GetComponentInParent<UIRootContainer>())
						selectables = new List<Selectable>(lastSelected.GetComponentInParent<UIRootContainer>().GetComponentsInChildren<Selectable>());
					else
						selectables = new List<Selectable>(lastSelected.GetComponentInParent<UIRootExecutor>().GetComponentsInChildren<Selectable>());
					// remove all untagged buttons (collapse buttons) and all GameObjects not active in hierarchy
					for (int i = selectables.Count - 1; i >= 0; i--)
					{
						if (selectables[i].GetComponent<Button>() || !selectables[i].gameObject.activeInHierarchy)
							selectables.RemoveAt(i);
					}

					// get id of last selected object
					int id = selectables.IndexOf(lastSelected.GetComponent<Selectable>());

					Navigation nav = lastSelected.GetComponent<Selectable>().navigation;
                    // si l'objet sélectionné a un navigation explicite en Up ou down, on ne fait rien, sinon on gère la navigation par script
                    if ((navigateValue.y > 0 && id > 0 && nav.selectOnUp == null) || (navigateValue.y < 0 && id < (selectables.Count - 1) && nav.selectOnDown == null))
					{
						// get the next one
						GameObject newSelected = selectables[id + (navigateValue.y > 0 ? -1 : 1)].gameObject;
						// set as new selected
						eventSystem.SetSelectedGameObject(newSelected);
						selected = newSelected;
					}
				}
			}
		}
        // ---- end ----

        // ---- Manage SiblingNavigation and DynamicNavigation ----
        if (selected != null)
		{
			// define next GameObject to focus for sibling navigation
			SiblingNavigation sibNav = selected.GetComponent<SiblingNavigation>();
			if (sibNav != null && selected == lastSelected)
			{
				if (navigateAction.WasPressedThisFrame() && (navigateValue.y > 0 || navigateValue.x < 0) && selected.transform.GetSiblingIndex() - 1 >= 0)
					EventSystem.current.SetSelectedGameObject(selected.transform.parent.GetChild(selected.transform.GetSiblingIndex() - 1).gameObject);
				else if (navigateAction.WasPressedThisFrame() && (navigateValue.y < 0 || navigateValue.x > 0) && selected.transform.GetSiblingIndex() + 1 < selected.transform.parent.childCount)
					EventSystem.current.SetSelectedGameObject(selected.transform.parent.GetChild(selected.transform.GetSiblingIndex() + 1).gameObject);
			}

			// define next GameObject to focus for dynamic navigation
			DynamicNavigation dynNav = selected.GetComponent<DynamicNavigation>();
			if (dynNav != null && selected == lastSelected)
			{
				if (dynNav.UpLeft.Length > 0 && (navigateAction.WasPressedThisFrame() && (navigateValue.y > 0 || navigateValue.x < 0)))
				{
					foreach (Selectable sel in dynNav.UpLeft)
						if (sel != null && sel.gameObject.activeInHierarchy && sel.interactable)
						{
							EventSystem.current.SetSelectedGameObject(sel.gameObject);
							break;
						}
				}
				else if (dynNav.DownRight.Length > 0 && (navigateAction.WasPressedThisFrame() && (navigateValue.y < 0 || navigateValue.x > 0)))
				{
					foreach (Selectable sel in dynNav.DownRight)
						if (sel != null && sel.gameObject.activeInHierarchy && sel.interactable)
						{
							EventSystem.current.SetSelectedGameObject(sel.gameObject);
							break;
						}
				}
			}
		}
        // ---- end ----

        // En tactile un bouton peut ne pas recevoir de onExit, dans ce cas, le bouton est toujours considéré comme le bouton actif et si on clique ailleurs l'évènement est envoyé à ce GameObject au lieu du nouveau. Pour contrer ça, si une nouvelle phase de touch commence (Began) et que l'objet actif n'est pas celui sous le doigt, on déselectionne l'objet
        ReadOnlyArray<Touch> activeTouches = Touch.activeTouches;
		for (int i = 0; i < activeTouches.Count; i++)
		{
			if (activeTouches[i].phase == TouchPhase.Began)
			{
				int fingerId = activeTouches[i].finger.index;
				if (EventSystem.current.currentSelectedGameObject != null)
				{
					if (!EventSystem.current.IsPointerOverGameObject(fingerId))
					{
						EventSystem.current.SetSelectedGameObject(null);
						selected = null;
					}
				}
			}
		}

		lastSelected = selected;
	}

	private bool IsReallyInteractable(GameObject obj)
	{
		if (!obj.activeInHierarchy)
			return false;

		// Button
		Selectable sel = obj.GetComponent<Selectable>();
		if (sel != null && !sel.IsInteractable())
			return false;

		// CanvasGroup
		CanvasGroup[] groups = obj.GetComponentsInParent<CanvasGroup>();
		foreach (CanvasGroup g in groups)
		{
			if (!g.interactable || !g.blocksRaycasts)
				return false;
			if (g.ignoreParentGroups)
				break;
		}

		// Graphic
		var graphic = obj.GetComponent<Graphic>();
		if (graphic != null && !graphic.raycastTarget)
			return false;

		return true;
	}

	private void onNewUnselectableText(GameObject text)
    {
		if (text.transform.parent && !text.transform.parent.GetComponentInParent<ElementToDrag>(true) && !text.transform.parent.GetComponentInParent<Tooltip>(true) && !text.transform.parent.GetComponentInParent<Selectable>(true))
			GameObjectManager.addComponent<Selectable>(text);
	}

	private void onNewInputField(GameObject go)
	{
		// Pour le tactile si on est en mode plein écran on force la sortie du plein écran quand on entre dans un InputField et on le restaure quand on en ressort
		go.GetComponent<TMP_InputField>().onSelect.AddListener(delegate (string content)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer && Touch.activeTouches.Count > 0)
                QuitFullScreen();
        });

		go.GetComponent<TMP_InputField>().onEndEdit.AddListener(delegate (string content)
		{
            if (Application.platform == RuntimePlatform.WebGLPlayer)
				ResetFullScreen();
        });
    }

    public IEnumerator catchApplicationState(bool hasFocus)
    {
        if (!hasFocus)
        {
            // player click outside the game
            userData.lastFocusOut = DateTime.Now.ToUniversalTime().Ticks;

#if UNITY_WEBGL && !UNITY_EDITOR
			// WebGLInput n'est inclus dans le using UnityEngine que si le profil de compilation est WebGL, donc on ne peut pas l'utiliser dans le code C# directement. On doit donc utiliser la directive de compilation pour l'utiliser uniquement dans le profil WebGL.
			// disable WebGLInput.captureAllKeyboardInput so elements in web page can handle keyboard inputs (usefull for Tab navigation)
			WebGLInput.captureAllKeyboardInput = false;
#endif
        }
        else // player come back in the game
        {

#if UNITY_WEBGL && !UNITY_EDITOR
			// WebGLInput n'est inclus dans le using UnityEngine que si le profil de compilation est WebGL, donc on ne peut pas l'utiliser dans le code C# directement. On doit donc utiliser la directive de compilation pour l'utiliser uniquement dans le profil WebGL.
			// disable WebGLInput.captureAllKeyboardInput so elements in web page can handle keyboard inputs (usefull for Tab navigation)
			WebGLInput.captureAllKeyboardInput = true;
#endif

            if (userData.lastFocusOut != -1 && new TimeSpan(DateTime.Now.ToUniversalTime().Ticks - userData.lastFocusOut).Minutes >= 10)
            {
                GameObject xAPI = GameObject.Find("GBLXAPI");
                if (xAPI != null)
                {
                    GameObject.Destroy(xAPI);
                    GBLXAPI.IsInit = false;
                }
                GameData gd = GameObject.Find("GameData").GetComponent<GameData>();
                gd.selectedScenario = "";
                gd.actionsHistory = null;
                yield return null;
                yield return null;
                GameObjectManager.addComponent<AskToLoadScene>(MainLoop.instance.gameObject, new { sceneName = "ConnexionScene" });
            }
            userData.lastFocusOut = -1;

        }
    }

    // Fonction appelée depuis le javascript (voir Assets/WebGLTemplates/Custom/game.html) via le Wrapper du Système
    public void HTMLcanvasFocus(int hasFocus)
	{
        MainLoop.instance.StartCoroutine(catchApplicationState(hasFocus == 1));
    }
}
