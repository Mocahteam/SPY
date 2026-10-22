using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_InputField))]
public class DefaultAssociatedButton : MonoBehaviour
{
    public Button button;
    private TMP_InputField inputField;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    private void OnEnable()
    {
        inputField.onSubmit.AddListener(OnSubmit);
    }

    private void OnDisable()
    {
        inputField.onSubmit.RemoveListener(OnSubmit);
    }
    public void OnSubmit(string value)
    {
        if (button != null)
            button.onClick.Invoke();
    }

}
