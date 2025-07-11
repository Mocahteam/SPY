using FYFY;
using FYFY_plugins.PointerManager;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TTSSystem : FSystem
{
    private Family f_selectableElements = FamilyManager.getFamily(new AnyOfComponents(typeof(Button), typeof(TMP_InputField), typeof(Selectable), typeof(TMP_Dropdown), typeof(Toggle), typeof(Scrollbar)), new NoneOfComponents(typeof(PointerSensitive)));
    private Family f_focused = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver)), new AnyOfComponents(typeof(Button), typeof(TMP_InputField), typeof(Selectable), typeof(TMP_Dropdown), typeof(Toggle), typeof(Scrollbar)));
    private Family f_currentAction = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction)));
    private Family f_inputFields = FamilyManager.getFamily(new AllOfComponents(typeof(TMP_InputField)));
    private Family f_toggles = FamilyManager.getFamily(new AllOfComponents(typeof(Toggle)));
    private Family f_scrollBars = FamilyManager.getFamily(new AllOfComponents(typeof(Scrollbar)));


    [DllImport("__Internal")]
    private static extern string CallTTS(string txt); // call javascript => send txt to html to be read by TTS navigator


    [DllImport("__Internal")]
    private static extern string SendToScreenReader(string txt); // call javascript => send txt to html to be accessible by screen readers

    [DllImport("__Internal")]
    private static extern bool InstructionOnly(); // call javascript => return true if "Instruction" is checked in html

    private GameData gameData;
    private GameObject previousSelectedGO;
    private GameObject previousFocusedGO;
    private Vector3 previousMousePosition;

    public EventSystem eventSystem;

    protected override void onStart()
    {
        GameObject go = GameObject.Find("GameData");
        if (go != null)
            gameData = go.GetComponent<GameData>();

        foreach (GameObject selectable in f_selectableElements)
            onNewSelectable(selectable);
        f_selectableElements.addEntryCallback(onNewSelectable);

        f_focused.addEntryCallback(onNewFocus);
        f_currentAction.addEntryCallback(onNewCurrentAction);

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

    protected override void onProcess(int familiesUpdateCount)
    {
        if (previousSelectedGO != eventSystem.currentSelectedGameObject && eventSystem.currentSelectedGameObject != null)
        {
            previousSelectedGO = eventSystem.currentSelectedGameObject;
            defTTS(previousSelectedGO);
        }
    }

    private void ifFocused_SendToScreenReader(string content)
    {
        SendToScreenReader(content);
    }

    private void onNewSelectable(GameObject selectable)
    {
        // On exclue le tooltip du système de TTS
        if (!selectable.GetComponentInParent<Tooltip>())
            GameObjectManager.addComponent<PointerSensitive>(selectable);
    }

    private void onNewFocus(GameObject focused)
    {
        // Ne définir le TTS que si la position de la souris à changé
        if (previousMousePosition != Input.mousePosition)
            defTTS(focused);
    }

    private void defTTS(GameObject focused)
    {
        Selectable select = focused.GetComponent<Selectable>();

        string suffix = "";
        string content = "";
        Localization loc = gameData.GetComponent<Localization>();
        // Cas général : Boutton, TMP_Text, Toggle, DropDown => on va chercher le texte dans ses enfants
        if (focused.GetComponent<TMP_InputField>() == null && focused.GetComponent<LibraryItemRef>() == null)
        {
            TMP_Text text = focused.GetComponentInChildren<TMP_Text>();
            if (text != null)
                content = text.text;
        }

        if (focused.GetComponent<Button>())
            suffix = ", "+ loc.localization[22]; // "Boutton" : "Button";
        else if (focused.GetComponent<TMP_InputField>())
        {
            suffix = ", "+ loc.localization[23]; // "Champ de saisie" : "Input field";
            TMP_InputField inputfield = focused.GetComponent<TMP_InputField>();
            // S'il y a quelque chose dans le inputfield, utiliser cette valeur
            if (inputfield.text != "")
                content = inputfield.text;
            else // sinon utiliser le premier TMP_Text disponible qui sera le placeholder
                content = inputfield.GetComponentInChildren<TMP_Text>(true).text;
        }
        else if (focused.GetComponent<TMP_Dropdown>())
            suffix = ", "+ loc.localization[24]; // "Liste déroulante" : "Dropdown";
        else if (focused.GetComponent<Toggle>())
        {
            suffix = ", " + loc.localization[25]; // "Case à cocher" : "Toggle";
            Toggle toggle = focused.GetComponent<Toggle>();
            if (toggle.isOn)
                suffix += ", "+ loc.localization[26]; // "cochée" : "checked";
            else
                suffix += ", "+ loc.localization[27]; // "non cochée" : "unchecked";
        }
        else if (focused.GetComponent<Scrollbar>())
        {
            Scrollbar scrollbar = focused.GetComponent<Scrollbar>();
            content = loc.localization[28] + scrollbar.value; // "Barre de défilement, valeur : " : "Scrollbar, value: ";
        }
        else if (focused.GetComponent<CurrentAction>())
        {
            suffix = ", " + loc.localization[29]; // "Action courrante" : "Current action";
        }
        else if (focused.GetComponent<Image>())
        {
            // cas du texte de remplacement pour les images du briefing
            Transform replacementText = focused.transform.Find("ImgDesc");
            if (replacementText != null)
            {
                content = loc.localization[33]; // "Image" : "Image"
                TMP_Text text = replacementText.GetComponent<TMP_Text>();
                if (text != null && text.text != "")
                    content += ", " + loc.localization[34] + ", " + text.text; // "texte de remplacement" : "replacement text"
            }
        }

        if (select && !select.IsInteractable() && focused.GetComponent<CurrentAction>() != null)
            suffix += ", " + loc.localization[30]; // "désactivée" : "disabled";



        // Try to get tooltip to complete description
        TooltipContent tooltip = focused.GetComponentInChildren<TooltipContent>();
        if (tooltip != null && tooltip.text != "")
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
        {
            if (!InstructionOnly() || focused.transform.parent.gameObject.name == "DialogPanel")
                CallTTS(content + suffix);
            ifFocused_SendToScreenReader(content + suffix);
        }
        else
            Debug.Log(content + suffix);

        previousMousePosition = Input.mousePosition;
        previousFocusedGO = focused;
    }

    private void onNewCurrentAction(GameObject unused)
    {
        string actions = "";
        Localization loc = gameData.GetComponent<Localization>();
        foreach (GameObject currentAction in f_currentAction)
        {
            actions += currentAction.GetComponent<CurrentAction>().agent.GetComponent<ScriptRef>().executableScript.GetComponent<UIRootExecutor>().scriptName + " " + loc.localization[29] + " " + currentAction.GetComponentInChildren<TooltipContent>().text+". ";
        }

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            if (!InstructionOnly())
                CallTTS(actions);
            ifFocused_SendToScreenReader(actions);
        }
        else
            Debug.Log(actions);
    }

    // Pour vocaliser le texte sélectionné à l'intérieur d'un inputField
    private void onNewInputField(GameObject inputField_GO)
    {
        TMP_InputField inputF = inputField_GO.GetComponent<TMP_InputField>();
        inputF.onTextSelection.AddListener(delegate (string input, int end, int start)
        {
            Localization loc = gameData.GetComponent<Localization>();
            string output = input.Substring(Mathf.Min(start, end), Mathf.Max(start, end) - Mathf.Min(start, end)) +", "+ loc.localization[23] + " " + loc.localization[32];
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                
                if (!InstructionOnly())
                    CallTTS(output);
                ifFocused_SendToScreenReader(output);
            }
            else
                Debug.Log(output);
        });

    }

    // Pour vocaliser le changement d'état d'un Toggle
    private void onNewToggle(GameObject toggle_GO)
    {
        Toggle toggle = toggle_GO.GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(delegate (bool state)
        {
            string output = toggle.isOn ? gameData.GetComponent<Localization>().localization[26] : gameData.GetComponent<Localization>().localization[27]; // "cochée" ou "non cochée"
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                if (!InstructionOnly())
                    CallTTS(output);
                ifFocused_SendToScreenReader(output);
            }
            else
                Debug.Log(output);
        });
    }

    // Pour vocaliser le scorll d'une scrollbar
    private void onNewScrollbar(GameObject scrollbar_GO)
    {
        Scrollbar scrollbar = scrollbar_GO.GetComponent<Scrollbar>();
        scrollbar.onValueChanged.AddListener(delegate (float value)
        {
            // N'envoyer à la synthèse vocale que si elle a le focus
            if (scrollbar_GO == previousFocusedGO || scrollbar_GO == eventSystem.currentSelectedGameObject)
            {
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    if (!InstructionOnly())
                        CallTTS(scrollbar.value + "");
                    ifFocused_SendToScreenReader(scrollbar.value + "");
                }
                else
                    Debug.Log(scrollbar.value);
            }
        });
    }
}
