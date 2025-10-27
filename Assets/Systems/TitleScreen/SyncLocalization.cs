using UnityEngine;
using FYFY;
using UnityEngine.Localization.Settings;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// This system is in charge to load the correct localization
/// </summary>
public class SyncLocalization : FSystem {

    private Family f_langOptions = FamilyManager.getFamily(new AllOfComponents(typeof(LangOption), typeof(Button)));

    /// <summary>
    /// The current language selected
    /// </summary>
    private int currentLangSelected;

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
        currentLangSelected = 0;
        if (PlayerPrefs.HasKey("localization"))
            currentLangSelected = PlayerPrefs.GetInt("localization");
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            string locale = GetBrowserLanguage();
            if (locale == "fr")
                currentLangSelected = 0;
            else
                currentLangSelected = 1;
        }
        syncLocale();
        PlayerPrefs.SetInt("localization", currentLangSelected);
        PlayerPrefs.Save();

        GameObjectManager.addComponent<LocalizationLoaded>(MainLoop.instance.gameObject);
    }

    public void syncLocale()
    {
        foreach (GameObject option in f_langOptions)
        {
            ColorBlock colors = option.GetComponent<Button>().colors;
            LangOption langOpt = option.GetComponent<LangOption>();
            if ((int)langOpt.lang == currentLangSelected)
                colors.normalColor = langOpt.on;
            else
                colors.normalColor = langOpt.off;
            option.GetComponent<Button>().colors = colors;
        }
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[currentLangSelected];
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            UpdateHTMLLanguage(currentLangSelected == 0 ? "fr" : "en");
    }

    public void changeLang(LangOption.LangType lang)
    {
        currentLangSelected = (int)lang;
        syncLocale();
        PlayerPrefs.SetInt("localization", currentLangSelected);
        PlayerPrefs.Save();
    }
}