using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

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
        // on récupère l'item texte comme modèle pour calculer la largeur maximale
        TMP_Text itemText = dropdown.itemText;
        if (itemText == null)
            return;

        float maxWidth = 0;

        foreach (TMP_Dropdown.OptionData option in dropdown.options)
        {
            maxWidth = Mathf.Max(
                maxWidth,
                itemText.GetPreferredValues(option.text).x
            );
        }

        float width = maxWidth;

        // prise en compte de la marge gauche
        width += Mathf.Abs((itemText.transform as RectTransform).rect.xMin);

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