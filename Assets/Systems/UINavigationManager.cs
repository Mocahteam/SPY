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

	public List<GameObject> autoFocusProrityOnTab;
	private GameObject lastSelected;
	public EventSystem eventSystem;

	private InputAction navigateAction;
	private InputAction focusOnFirstUI;

	protected override void onStart()
    {
		foreach (GameObject text in f_textsUnselectable)
			onNewUnselectableText(text);
		f_textsUnselectable.addEntryCallback(onNewUnselectableText);
		if (eventSystem == null)
			eventSystem = EventSystem.current;

		navigateAction = InputSystem.actions.FindAction("Navigate");
		focusOnFirstUI = InputSystem.actions.FindAction("FocusOnFirstUI");
	}

    protected override void onProcess(int familiesUpdateCount)
	{
		// Récupérer la valeur Vector2 de Navigate
		Vector2 navigateValue = navigateAction.ReadValue<Vector2>();
		if (navigateAction.WasPressedThisFrame())
			Debug.Log(navigateValue);

		// Get the currently selected UI element from the event system.
		GameObject selected = eventSystem.currentSelectedGameObject;

		if (focusOnFirstUI.WasPressedThisFrame())
		{
			// Try to give focus on one of the priority list
			bool focused = false;
			if (autoFocusProrityOnTab != null)
				foreach (GameObject target in autoFocusProrityOnTab)
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

		// ---- On Scroll View, auto scroll on item selection ----
		// Do nothing if there are none OR if the selected game object is not a child of a scroll rect OR if the selected game object is the same as it was last frame,
		// meaning we haven't to move.
		if (selected != null && selected.GetComponentInParent<ScrollRect>() != null && selected != lastSelected)
		{
			// Get the content
			RectTransform viewport = selected.GetComponentInParent<ScrollRect>().viewport;
			RectTransform contentPanel = selected.GetComponentInParent<ScrollRect>().content;

			float selectedInContent_Y = Mathf.Abs(contentPanel.InverseTransformPoint(selected.transform.position).y);

			Vector2 targetAnchoredPosition = new Vector2(contentPanel.anchoredPosition.x, contentPanel.anchoredPosition.y);
			// we auto focus on selected object only if it is not visible
			if (selectedInContent_Y - contentPanel.anchoredPosition.y < 0 || (selectedInContent_Y + (selected.transform as RectTransform).rect.height) - contentPanel.anchoredPosition.y > viewport.rect.height)
			{
				// check if selected object is too high
				if (selectedInContent_Y - contentPanel.anchoredPosition.y < 0)
				{
					targetAnchoredPosition = new Vector2(
						targetAnchoredPosition.x,
						selectedInContent_Y
					);
				}
				// selected object is too low
				else
				{
					targetAnchoredPosition = new Vector2(
						targetAnchoredPosition.x,
						-viewport.rect.height + selectedInContent_Y + (selected.transform as RectTransform).rect.height
					);
				}

				contentPanel.anchoredPosition = targetAnchoredPosition;
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
						if (sel.gameObject.activeInHierarchy)
						{
							EventSystem.current.SetSelectedGameObject(sel.gameObject);
							break;
						}
				}
				else if (nav.DownRight.Length > 0 && (navigateAction.WasPressedThisFrame() && (navigateValue.y < 0 || navigateValue.x > 0)))
				{
					foreach (Selectable sel in nav.DownRight)
						if (sel.gameObject.activeInHierarchy)
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
