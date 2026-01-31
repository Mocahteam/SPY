using FYFY;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UINavigationManager : FSystem
{
	private Family f_textsUnselectable = FamilyManager.getFamily(new AllOfComponents(typeof(TextMeshProUGUI)), new NoneOfComponents(typeof(Selectable)));
	private Family f_draggedItems = FamilyManager.getFamily(new AllOfComponents(typeof(Dragging)));
	private Family f_dynamicNavigation = FamilyManager.getFamily(new AllOfComponents(typeof(DynamicNavigation)));

	private Family f_buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	public List<GameObject> autoFocusPrority;
	private GameObject lastSelected;
	public EventSystem eventSystem;

	private InputAction navigateAction;
	private InputAction rightClick;
	private InputAction middleClick;

	protected override void onStart()
    {
		foreach (GameObject text in f_textsUnselectable)
			onNewUnselectableText(text);
		f_textsUnselectable.addEntryCallback(onNewUnselectableText);
		if (eventSystem == null)
			eventSystem = EventSystem.current;

		navigateAction = InputSystem.actions.FindAction("Navigate");
		rightClick = InputSystem.actions.FindAction("RightClick");
		middleClick = InputSystem.actions.FindAction("MiddleClick");
	}

    protected override void onProcess(int familiesUpdateCount)
	{
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

		if (selected == null || !selected.activeInHierarchy || selected.GetComponent<Selectable>() == null)
		{
			// Try to give focus on one of the priority list
			bool focused = false;
			if (autoFocusPrority != null)
				foreach (GameObject target in autoFocusPrority)
					if (target.activeInHierarchy)
					{
						eventSystem.SetSelectedGameObject(target);
						focused = true;
						break;
					}
			// if we can't give focus to the last button available
			if (!focused)
				eventSystem.SetSelectedGameObject(f_buttons.getAt(f_buttons.Count - 1));
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

					if ((navigateValue.y > 0 && id > 0) || (navigateValue.y < 0 && id < (selectables.Count - 1)))
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

		// define next GameObject to focus for dynamic navigation
		foreach (GameObject navGO in f_dynamicNavigation)
		{
			DynamicNavigation nav = navGO.GetComponent<DynamicNavigation>();
			if (selected == navGO && selected == lastSelected)
			{
				if (nav.UpLeft.Length > 0 && (navigateAction.WasPressedThisFrame() && (navigateValue.y > 0 || navigateValue.x < 0)))
				{
					foreach (Selectable sel in nav.UpLeft)
						if (sel != null && sel.gameObject.activeInHierarchy)
						{
							EventSystem.current.SetSelectedGameObject(sel.gameObject);
							break;
						}
				}
				else if (nav.DownRight.Length > 0 && (navigateAction.WasPressedThisFrame() && (navigateValue.y < 0 || navigateValue.x > 0)))
				{
					foreach (Selectable sel in nav.DownRight)
						if (sel != null && sel.gameObject.activeInHierarchy)
						{
							EventSystem.current.SetSelectedGameObject(sel.gameObject);
							break;
						}
				}
			}
		}

		lastSelected = selected;
	}

	private void onNewUnselectableText(GameObject text)
    {
		if (text.transform.parent && !text.transform.parent.GetComponentInParent<ElementToDrag>(true) && !text.transform.parent.GetComponentInParent<Tooltip>(true) && !text.transform.parent.GetComponentInParent<Selectable>(true))
			GameObjectManager.addComponent<Selectable>(text);
	}
}
