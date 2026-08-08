using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropDownAutoWidth : MonoBehaviour
{
    [SerializeField] private float extraPadding = 20f;

    public TMP_Dropdown dropdown;

    private void OnEnable()
    {
        StartCoroutine(WaitAndResize());
    }

    private System.Collections.IEnumerator WaitAndResize()
    {
        yield return null;
        yield return null;
        ResizeDropDown();
    }

    private void ResizeDropDown()
    {
        CanvasScaler canvasScaler = GetComponentInParent<CanvasScaler>(true);
        // on récupère le premier item de la liste déroulante comme modèle pour calculer la largeur maximale de toutes les options
        TMP_Text itemText = transform.GetComponentInChildren<TMP_Text>();
        if (itemText == null)
            return;

        float maxWidth = 0;
        
        // si l'auto sizing est activé, on le désactive temporairement pour calculer la largeur maximale des options, sinon le GetPreferredValues renverra toujours la largeur de l'option avec la taille de police maximale, ce qui n'est pas ce que nous voulons.
        bool autoSizing = itemText.enableAutoSizing;
        itemText.enableAutoSizing = false;
        foreach (TMP_Dropdown.OptionData option in dropdown.options)
        {
            maxWidth = Mathf.Max(
                maxWidth,
                itemText.GetPreferredValues(option.text).x
            );
        }
        // on restaure l'état de l'auto sizing
        itemText.enableAutoSizing = autoSizing;

        float width = maxWidth;

        // prise en compte de la marge gauche
        width += (itemText.transform as RectTransform).offsetMin.x;

        // prise en compte de la marge droite
        width += -(itemText.transform as RectTransform).offsetMax.x;

        // Largeur de la scrollbar
        if (transform.TryGetComponent(out ScrollRect scrollRect) && scrollRect.verticalScrollbar != null)
            width += scrollRect.verticalScrollbar.GetComponent<RectTransform>().rect.width;

        // au minimum prendre la taille du dropdown
        width = Mathf.Max(width, dropdown.GetComponent<RectTransform>().rect.width);

        width += extraPadding;

        RectTransform dropdownRT = transform as RectTransform;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
                    canvasScaler.GetComponent<Canvas>().renderMode == RenderMode.ScreenSpaceCamera ? canvasScaler.GetComponent<Canvas>().worldCamera : null,
                    dropdownRT.position);

        bool leftSide = screenPos.x / canvasScaler.scaleFactor < (canvasScaler.transform as RectTransform).rect.width / 2;

        if (leftSide)
        {
            // La liste s'ouvre vers la droite
            dropdownRT.anchorMin = dropdownRT.anchorMax = new Vector2(0f, 0f);
            dropdownRT.pivot = new Vector2(0f, 1f);
            dropdownRT.anchoredPosition = new Vector2(0f, dropdownRT.anchoredPosition.y);
        }
        else
        {
            // La liste s'ouvre vers la gauche
            dropdownRT.anchorMin = dropdownRT.anchorMax = new Vector2(1f, 0f);
            dropdownRT.pivot = new Vector2(1f, 1f);
            dropdownRT.anchoredPosition = new Vector2(0f, dropdownRT.anchoredPosition.y);
        }

        dropdownRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }
}