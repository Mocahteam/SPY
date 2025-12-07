using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class Tooltip : MonoBehaviour
{
    private TMP_Text tooltipText;
    private RectTransform rectTransform;
    private bool state;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        tooltipText = GetComponentInChildren<TMP_Text>();
        state = false;
        
    }

    public void ShowTooltip(string tooltipString)
    {
        Debug.Log("ShowTooltip");
        state = true;
        tooltipText.text = tooltipString;
    }

    public void HideTooltip()
    {
        state = false;
    }

    private void Update()
    {
        if (state)
        {
            Vector2Control pointerPos = Pointer.current.position;

            Vector2 tooltipPos = new Vector2(pointerPos.x.value, pointerPos.y.value);

            // recaller la position du tooltip pour qu'il soit dirigé vers le centre de l'écran
            if (tooltipPos.x > Screen.width / 2)
                tooltipPos.x -= (20 + rectTransform.sizeDelta.x / 2);
            else
                tooltipPos.x += (20 + rectTransform.sizeDelta.x / 2);

            if (tooltipPos.y > Screen.height / 2)
                tooltipPos.y -= (20 + rectTransform.sizeDelta.y / 2);
            else
                tooltipPos.y += (20 + rectTransform.sizeDelta.y / 2);

            transform.position = tooltipPos;
        }
        else
            // maintenir le tooltip hors de l'écran
            transform.position = new Vector3(0, -100);
    }
}
