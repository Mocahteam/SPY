using UnityEngine;
using UnityEngine.EventSystems;
using FYFY;
using System.Collections;

/// <summary>
/// Manage tooltips for virtual keyboard
/// </summary>
public class TooltipKeyboardSystem : FSystem {
    private Family f_tooltips = FamilyManager.getFamily(new AllOfComponents(typeof(TooltipContent)));

	public Tooltip tooltipUI_Keyboard; // Be sure in inspector that this component is disabled to avoid Update process
    public GameObject tooltipUI_Pointer;

    public EventSystem eventSystem;

    private GameObject lastSelected;

    protected override void onProcess(int familiesUpdateCount)
    {
        bool found = false;
        foreach(GameObject go in f_tooltips)
        {
            if (eventSystem.currentSelectedGameObject == go)
            {
                if (go != lastSelected) {
                    if (tooltipUI_Pointer.activeInHierarchy)
                        tooltipUI_Pointer.GetComponent<Tooltip>().HideTooltip();
                    tooltipUI_Keyboard.ShowTooltip(go.GetComponent<TooltipContent>().text);
                    // recaller la position du tooltip pour qu'il soit dirigé vers le centre de l'écran
                    RectTransform backgroundRectTransform = tooltipUI_Keyboard.transform as RectTransform;
                    Vector3 btnPos = go.transform.position;
                    if (btnPos.x > Screen.width / 2)
                        btnPos.x -= (10 + (backgroundRectTransform.sizeDelta.x * backgroundRectTransform.parent.localScale.x) / 2);
                    else
                        btnPos.x += (10 + (backgroundRectTransform.sizeDelta.x * backgroundRectTransform.parent.localScale.x) / 2);

                    if (btnPos.y > Screen.height / 2)
                        btnPos.y -= (10 + (backgroundRectTransform.sizeDelta.y * backgroundRectTransform.parent.localScale.y) / 2);
                    else
                        btnPos.y += (10 + (backgroundRectTransform.sizeDelta.y * backgroundRectTransform.parent.localScale.y) / 2);
                    backgroundRectTransform.position = btnPos;
                    tooltipUI_Keyboard.StopAllCoroutines();
                    tooltipUI_Keyboard.StartCoroutine(hideInFewSeconds());
                    lastSelected = go;
                }
                found = true;
                break;
            }
        }
        if (!found)
            tooltipUI_Keyboard.HideTooltip();
    }

    private IEnumerator hideInFewSeconds()
    {
        yield return new WaitForSeconds(2);
        tooltipUI_Keyboard.HideTooltip();
    }
}