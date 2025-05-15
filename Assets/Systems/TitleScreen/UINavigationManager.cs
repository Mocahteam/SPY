using FYFY;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UINavigationManager : FSystem
{
	private Family f_textsUnselectable = FamilyManager.getFamily(new AllOfComponents(typeof(TextMeshProUGUI)), new NoneOfComponents(typeof(Selectable)));
	private Family f_draggedItems = FamilyManager.getFamily(new AllOfComponents(typeof(Dragging)));

	private Family f_buttons = FamilyManager.getFamily(new AllOfComponents(typeof(Button)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	public Color textSelectedColor = new Color(0, 183, 255, 255);
	public List<GameObject> autoFocusProrityOnTab;
	private GameObject lastSelected;
	public EventSystem eventSystem;

	protected override void onStart()
    {
		foreach (GameObject text in f_textsUnselectable)
			onNewUnselectableText(text);
		f_textsUnselectable.addEntryCallback(onNewUnselectableText);
		if (eventSystem == null)
			eventSystem = EventSystem.current;
    }

    protected override void onProcess(int familiesUpdateCount)
	{

		if (Input.GetKeyDown(KeyCode.Home))
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

		// Get the currently selected UI element from the event system.
		GameObject selected = eventSystem.currentSelectedGameObject;

		// ---- Manage keyboard navigation in script ----
		// no item dragged and last selection was an element in script
		if (lastSelected != null && f_draggedItems.Count == 0 && (lastSelected.GetComponentInParent<UIRootContainer>() != null || lastSelected.GetComponentInParent<UIRootExecutor>() != null))
		{
			// press up or down
			if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
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

					if ((Input.GetKeyDown(KeyCode.UpArrow) && id > 0) || (Input.GetKeyDown(KeyCode.DownArrow) && id < (selectables.Count - 1)))
					{
						// get the next one
						GameObject newSelected = selectables[id + (Input.GetKeyDown(KeyCode.UpArrow) ? -1 : 1)].gameObject;
						// set as new selected
						eventSystem.SetSelectedGameObject(newSelected);
						selected = newSelected;
					}
				}
			} 
			// Pas sûr que cette fonctionnalité soit utile... on l'annule pour l'instant
			/*else if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				// Se mettre en navigation automatique pour pouvoir récupérer son voisin de gauche
				Navigation nav = lastSelected.GetComponent<Selectable>().navigation;
				nav.mode = Navigation.Mode.Automatic;
				lastSelected.GetComponent<Selectable>().navigation = nav;
				// wéletionner le voisin de gauche
				selected = lastSelected.GetComponent<Selectable>().FindSelectableOnLeft().gameObject;
				eventSystem.SetSelectedGameObject(selected);
				// Se remettre en navigation non automatique
				nav.mode = Navigation.Mode.None;
				lastSelected.GetComponent<Selectable>().navigation = nav;
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				// Se mettre en navigation automatique pour pouvoir récupérer son voisin de droite
				Navigation nav = lastSelected.GetComponent<Selectable>().navigation;
				nav.mode = Navigation.Mode.Automatic;
				lastSelected.GetComponent<Selectable>().navigation = nav;
				// wéletionner le voisin de droite
				selected = lastSelected.GetComponent<Selectable>().FindSelectableOnRight().gameObject;
				eventSystem.SetSelectedGameObject(selected);
				// Se remettre en navigation non automatique
				nav.mode = Navigation.Mode.None;
				lastSelected.GetComponent<Selectable>().navigation = nav;
			}*/
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

		lastSelected = selected;
	}

	private void onNewUnselectableText(GameObject text)
    {
		if (text.transform.parent && !text.transform.parent.GetComponentInParent<ElementToDrag>(true) && !text.transform.parent.GetComponentInParent<Tooltip>(true) && !text.transform.parent.GetComponentInParent<Selectable>(true))
		{
			Selectable select = text.AddComponent<Selectable>();
			ColorBlock cb = select.colors;
			cb.selectedColor = textSelectedColor;
			select.colors = cb;
			GameObjectManager.refresh(text);
		}
	}
}
