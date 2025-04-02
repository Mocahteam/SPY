using FYFY;
using FYFY_plugins.PointerManager;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class TTSSystem : FSystem
{
    private Family f_selectableElements = FamilyManager.getFamily(new AnyOfComponents(typeof(Button), typeof(TextMeshProUGUI), typeof(TMP_InputField), typeof(Selectable), typeof(TMP_Dropdown), typeof(Toggle), typeof(Scrollbar)), new NoneOfComponents(typeof(PointerSensitive)));
    private Family f_focused = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver)));
    private Family f_inputFields = FamilyManager.getFamily(new AllOfComponents(typeof(TMP_InputField)));
    private Family f_toggles = FamilyManager.getFamily(new AllOfComponents(typeof(Toggle)));
    private Family f_scrollBars = FamilyManager.getFamily(new AllOfComponents(typeof(Scrollbar)));

    [DllImport("__Internal")]
    private static extern string CallTTS(string txt); // call javascript

    protected override void onStart()
    {
        foreach (GameObject selectable in f_selectableElements)
            GameObjectManager.addComponent<PointerSensitive>(selectable);
        f_selectableElements.addEntryCallback(onNewButton);

        f_focused.addEntryCallback(onNewFocus);

        foreach (GameObject inputField in f_inputFields)
            onNewInputField(inputField);
        f_inputFields.addEntryCallback(onNewInputField);

        foreach (GameObject toggle in f_toggles)
            onNewToggle(toggle);
        f_toggles.addEntryCallback(onNewToggle);

        foreach (GameObject scrollbar in f_scrollBars)
            onNewScrollbar(scrollbar);
        f_scrollBars.addEntryCallback(onNewScrollbar);
    }

    private void onNewButton(GameObject button)
    {
        GameObjectManager.addComponent<PointerSensitive>(button);
    }

    private void onNewFocus(GameObject focused)
    {
        // On ne fait rien si notre parent est un bouton, un inputfield, un drop down ou un toggle, ça sera à eux de faire le job
        if (!focused.transform.parent || !(focused.transform.parent.GetComponentInParent<Button>() || focused.transform.parent.GetComponentInParent<TMP_InputField>() || focused.transform.parent.GetComponentInParent<TMP_Dropdown>() || focused.transform.parent.GetComponentInParent<Toggle>()))
        {
            string prefix = "";
            string content = "";
            if (focused.GetComponent<Button>())
                prefix = LocalizationSettings.SelectedLocale.Identifier.ToString().Contains("(fr)") ? "Bouton, " : "Button, ";
            else if (focused.GetComponent<TMP_InputField>())
            {
                prefix = LocalizationSettings.SelectedLocale.Identifier.ToString().Contains("(fr)") ? "Champ de saisie, " : "Input field, ";
                TMP_InputField inputfield = focused.GetComponent<TMP_InputField>();
                // S'il y a quelque chose dans le inputfield, utiliser cette valeur
                if (inputfield.text != "")
                    content = inputfield.text;
                else // sinon utiliser le premier TMP_Text disponible qui sera le placeholder
                    content = focused.GetComponentInChildren<TMP_Text>().text;
            }
            else if (focused.GetComponent<TMP_Dropdown>())
                prefix = LocalizationSettings.SelectedLocale.Identifier.ToString().Contains("(fr)") ? "Liste déroulante, " : "Dropdown, ";
            else if (focused.GetComponent<Toggle>())
            {
                prefix = LocalizationSettings.SelectedLocale.Identifier.ToString().Contains("(fr)") ? "Case à cocher, " : "Toggle, ";
                Toggle toggle = focused.GetComponent<Toggle>();
                if (toggle.isOn)
                    content = LocalizationSettings.SelectedLocale.Identifier.ToString().Contains("(fr)") ? "cochée, " : "checked, ";
                else
                    content = LocalizationSettings.SelectedLocale.Identifier.ToString().Contains("(fr)") ? "non cochée, " : "unchecked, ";
            }
            else if (focused.GetComponent<Scrollbar>())
            {
                prefix = LocalizationSettings.SelectedLocale.Identifier.ToString().Contains("(fr)") ? "Barre de défilement, valeur : " : "Scrollbar, value";
                Scrollbar scrollbar = focused.GetComponent<Scrollbar>();
                content = "" + scrollbar.value;
            }


            // Cas général : boutons, TMP_Text, Toggle, DropDown
            if (focused.GetComponent<TMP_InputField>() == null)
            {
                TMP_Text text = focused.GetComponentInChildren<TMP_Text>();
                if (text != null)
                    content += text.text;
            }

            // Try to get tooltip to complete description
            TooltipContent tooltip = focused.GetComponentInChildren<TooltipContent>();
            if (tooltip != null)
            {
                if (tooltip.text.Contains("#agentName"))
                    content += (content != "" ? ", " : "") + tooltip.text.Replace("#agentName", tooltip.GetComponent<AgentEdit>().associatedScriptName);
                else
                    content += (content != "" ? ", " : "") + tooltip.text;
            }

            if (content == "")
                content = focused.name;
            else
                content = content.Replace("<br>", " ");

            if (Application.platform == RuntimePlatform.WebGLPlayer)
                CallTTS(prefix + content);
            else
                Debug.Log(prefix + content);
        }
    }

    private void onNewInputField(GameObject inputField_GO)
    {
        TMP_InputField inputF = inputField_GO.GetComponent<TMP_InputField>();
        inputF.onTextSelection.AddListener(delegate (string input, int end, int start)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                CallTTS(input.Substring(Mathf.Min(start, end), Mathf.Max(start, end) - Mathf.Min(start, end)) + (LocalizationSettings.SelectedLocale.Identifier.ToString().Contains("(fr)") ? " sélectionné" : " selected"));
            else
                Debug.Log(input.Substring(Mathf.Min(start, end), Mathf.Max(start, end) - Mathf.Min(start, end)) + (LocalizationSettings.SelectedLocale.Identifier.ToString().Contains("(fr)") ? " sélectionné" : " selected"));
        });

    }

    private void onNewToggle(GameObject toggle_GO)
    {
        Toggle toggle = toggle_GO.GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(delegate (bool state)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                CallTTS(toggle.isOn ? (LocalizationSettings.SelectedLocale.Identifier.ToString().Contains("(fr)") ? "cochée" : "checked") : (LocalizationSettings.SelectedLocale.Identifier.ToString().Contains("(fr)") ? "non cochée" : "unchecked"));
            else
                Debug.Log(toggle.isOn ? (LocalizationSettings.SelectedLocale.Identifier.ToString().Contains("(fr)") ? "cochée" : "checked") : (LocalizationSettings.SelectedLocale.Identifier.ToString().Contains("(fr)") ? "non cochée" : "unchecked"));
        });
    }

    private void onNewScrollbar(GameObject scrollbar_GO)
    {
        Scrollbar scrollbar = scrollbar_GO.GetComponent<Scrollbar>();
        scrollbar.onValueChanged.AddListener(delegate (float value)
        {
            // N'envoyer à la synthèse vocale que si elle a le focus
            if (scrollbar_GO.GetComponent<PointerOver>())
            {
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                    CallTTS(scrollbar.value + "");
                else
                    Debug.Log(scrollbar.value);
            }
        });
    }
}
