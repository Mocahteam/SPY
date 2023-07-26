using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

/// <summary>
/// This component enables to select an item inside a list of elements
/// </summary>
public class ItemSelector : MonoBehaviour
{
    /// <summary>
    /// The current item selected
    /// </summary>
    public int currentItem = 0;

    /// <summary>
    /// The list of selectable items
    /// </summary>
    public string[] items;

    /// <summary>
    /// The Text component that display the item selected
    /// </summary>
    public TextMeshProUGUI itemUI;

    // Start is called before the first frame update
    void Start()
    {
        itemUI.text = items[currentItem];
    }

    /// <summary>
    /// Select the next item in the list (come back at the beginning of the list if the end is reached)
    /// </summary>
    public void nextItem()
    {
        currentItem++;
        if (currentItem >= items.Length)
            currentItem = 0;
        itemUI.text = items[currentItem];
        changeLocale();
    }

    /// <summary>
    /// Select the previous item in the list (return the last item if we try to access item before the first item)
    /// </summary>
    public void prevItem()
    {
        currentItem--;
        if (currentItem < 0)
            currentItem = items.Length-1;
        itemUI.text = items[currentItem];
        changeLocale();
    }

    /// <summary>
    /// Select locale accrodingly to the langage selector
    /// </summary>
    public void changeLocale()
    {
        if (currentItem == 0)
            LocalizationSettings.Instance.SetSelectedLocale(LocalizationSettings.Instance.GetAvailableLocales().GetLocale("fr"));
        if (currentItem == 1)
            LocalizationSettings.Instance.SetSelectedLocale(LocalizationSettings.Instance.GetAvailableLocales().GetLocale("en"));
        PlayerPrefs.SetString("locale", LocalizationSettings.Instance.GetSelectedLocale().Identifier.Code);
    }

    public void switchTo (string locale)
    {
        if (locale == "fr")
            currentItem = 0;
        else
            currentItem = 1;
        itemUI.text = items[currentItem];
        changeLocale();
    }
}
