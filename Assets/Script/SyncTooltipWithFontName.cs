using TMPro;
using UnityEngine;

public class SyncTooltipWithFontName : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<TooltipContent>().text = GetComponentInChildren<TextMeshProUGUI>().text;
    }
}
