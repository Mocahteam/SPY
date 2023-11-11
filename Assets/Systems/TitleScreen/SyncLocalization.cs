using UnityEngine;
using FYFY;
using UnityEngine.Localization.Settings;
using System.Collections;
using System.Runtime.InteropServices;

/// <summary>
/// This system is in charge to load the correct localization
/// </summary>
public class SyncLocalization : FSystem {

    private Family f_selector = FamilyManager.getFamily(new AllOfComponents(typeof(ItemSelector)));

    /// <summary>
    /// The current item selected
    /// </summary>
    private int currentItemSelected;
    /// <summary>
    /// The list of selectable items
    /// </summary>
    public string[] items;

    [DllImport("__Internal")]
    private static extern string GetBrowserLanguage(); // call javascript

    [DllImport("__Internal")]
    private static extern string UpdateHTMLLanguage(string newLang); // call javascript

    public static SyncLocalization instance;

    public SyncLocalization()
    {
        instance = this;
    }

    protected override void onStart()
    {
        MainLoop.instance.StartCoroutine(WaitLocalizationInitialized());
    }

    private IEnumerator WaitLocalizationInitialized()
    {
        // wait for the Localization initialization operation to complete.
        yield return LocalizationSettings.InitializationOperation;
        // Now, we can switch to appropriate language
        currentItemSelected = 0;
        if (PlayerPrefs.HasKey("localization"))
            currentItemSelected = PlayerPrefs.GetInt("localization");
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            string locale = GetBrowserLanguage();
            if (locale == "fr")
                currentItemSelected = 0;
            else
                currentItemSelected = 1;
        }
        syncLocale();
        // wait again if locale change
        yield return LocalizationSettings.InitializationOperation;

        // Uggly! Force to refresh localization (something wrong on MainMenu in WebGl context...)
        nextItem();
        prevItem();

        GameObjectManager.addComponent<LocalizationLoaded>(MainLoop.instance.gameObject);
    }

    public void syncLocale()
    {
        foreach (GameObject select in f_selector)
        {
            ItemSelector itemSel = select.GetComponent<ItemSelector>();
            itemSel.itemUI.text = items[currentItemSelected];
        }
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[currentItemSelected];
        PlayerPrefs.SetInt("localization", currentItemSelected);
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            UpdateHTMLLanguage(currentItemSelected == 0 ? "fr" : "en");
    }

    /// <summary>
    /// Select the next item in the list (come back at the beginning of the list if the end is reached)
    /// </summary>
    public void nextItem()
    {
        currentItemSelected++;
        if (currentItemSelected >= items.Length)
            currentItemSelected = 0;
        syncLocale();
    }

    /// <summary>
    /// Select the previous item in the list (return the last item if we try to access item before the first item)
    /// </summary>
    public void prevItem()
    {
        currentItemSelected--;
        if (currentItemSelected < 0)
            currentItemSelected = items.Length - 1;
        syncLocale();
    }
}