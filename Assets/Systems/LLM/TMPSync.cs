using TMPro;
using UnityEngine;

public class TMPSync : MonoBehaviour
{
    public TextMeshProUGUI source;
    public TextMeshProUGUI target;

    private string _lastText;
    private Color _lastColor;

    void Update()
    {
        // Only update when something actually changed
        if (source.text != _lastText || source.color != _lastColor)
        {
            target.text = source.text;
            target.color = source.color;
            _lastText = source.text;
            _lastColor = source.color;
        }
    }
}