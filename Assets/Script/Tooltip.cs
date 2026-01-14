using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    private TMP_Text tooltipText;
    private RectTransform rectTransform;
    private bool state;
    private InputAction navigateAction;
    private InputAction pointActionUI;
    private CanvasScaler canvasScaler;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        tooltipText = GetComponentInChildren<TMP_Text>();
        state = false;
        navigateAction = InputSystem.actions.FindAction("Navigate");
        pointActionUI = EventSystem.current.GetComponent<InputSystemUIInputModule>().point.action;
        canvasScaler = GetComponentInParent<CanvasScaler>(true);
    }

    public void ShowTooltip(string tooltipString)
    {
        if (PlayerPrefs.GetInt("tooltipView") == 1)
        {
            state = true;
            tooltipText.text = tooltipString;
        }
    }

    public void HideTooltip()
    {
        state = false;
    }

    private void LateUpdate()
    {
        GameObject currentSelectedGO = EventSystem.current.currentSelectedGameObject;
        // Si on utilise la navigation clavier on désactive le gestionnaire de pointage (souris, touch...) pour éviter qu'il entre en conflit avec la navigation clavier
        if (navigateAction.WasPressedThisFrame() && pointActionUI.enabled)
        {
            HideTooltip();
            pointActionUI.Disable();
            Cursor.visible = false;
        }
        // Si le pointeur bouge, on réactive le gestionnaire de pointage
        else if (Pointer.current.delta.ReadValue() != Vector2.zero && !pointActionUI.enabled)
        {
            HideTooltip();
            pointActionUI.Enable();
            Cursor.visible = true;
        }
        if (state)
        {
            Vector2 tooltipPos;
            if (pointActionUI.enabled || currentSelectedGO == null)
                tooltipPos = new Vector2(Pointer.current.position.x.value, Pointer.current.position.y.value) / canvasScaler.scaleFactor;
            else
                tooltipPos = new Vector2(currentSelectedGO.transform.position.x, currentSelectedGO.transform.position.y) / canvasScaler.scaleFactor;
            // recaller la position du tooltip pour qu'il soit dirigé vers le centre de l'écran
            if (tooltipPos.x > (canvasScaler.transform as RectTransform).rect.width / 2)
                tooltipPos.x -= (20 / canvasScaler.scaleFactor + rectTransform.rect.width / 2);
            else
                tooltipPos.x += (20 / canvasScaler.scaleFactor + rectTransform.rect.width / 2);

            if (tooltipPos.y > (canvasScaler.transform as RectTransform).rect.height / 2)
                tooltipPos.y -= (20 / canvasScaler.scaleFactor + rectTransform.rect.height / 2);
            else
                tooltipPos.y += (20 / canvasScaler.scaleFactor + rectTransform.rect.height / 2);

            (transform as RectTransform).anchoredPosition = tooltipPos;
        }
        else
            // maintenir le tooltip hors de l'écran
            (transform as RectTransform).anchoredPosition = new Vector3(-100, -100);
    }
}
