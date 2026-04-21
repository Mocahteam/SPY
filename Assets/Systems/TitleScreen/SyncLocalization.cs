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

    public CurrentSettingsValues currentSettingsValues;

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
        Pause = true;
    }

    private IEnumerator WaitLocalizationInitialized()
    {
        // wait for the Localization initialization operation to complete.
        yield return LocalizationSettings.InitializationOperation;
        // Now, we can switch to appropriate language
        if (PlayerPrefs.HasKey("localization"))
            currentSettingsValues.values.currentLanguage = PlayerPrefs.GetInt("localization");
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            string locale = GetBrowserLanguage();
            if (locale == "fr")
                currentSettingsValues.values.currentLanguage = 0;
            else
                currentSettingsValues.values.currentLanguage = 1;
        }
        else
            currentSettingsValues.values.currentLanguage = currentSettingsValues.GetComponent<DefaultSettingsValues>().defaultLanguage;
        syncLocale();
        PlayerPrefs.SetInt("localization", currentSettingsValues.values.currentLanguage);
        PlayerPrefs.Save();

        GameObjectManager.addComponent<LocalizationLoaded>(MainLoop.instance.gameObject);
    }

    public void syncLocale()
    {
        foreach (GameObject option in f_langOptions)
        {
            ColorBlock colors = option.GetComponent<Button>().colors;
            LangOption langOpt = option.GetComponent<LangOption>();
            if ((int)langOpt.lang == currentSettingsValues.values.currentLanguage)
                colors.normalColor = langOpt.on;
            else
                colors.normalColor = langOpt.off;
            option.GetComponent<Button>().colors = colors;
        }
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[currentSettingsValues.values.currentLanguage];
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            UpdateHTMLLanguage(currentSettingsValues.values.currentLanguage == 0 ? "fr" : "en");
    }

    public void changeLang(LangOption.LangType lang)
    {
        currentSettingsValues.values.currentLanguage = (int)lang;
        syncLocale();
        PlayerPrefs.SetInt("localization", currentSettingsValues.values.currentLanguage);
        PlayerPrefs.Save();
    }
}