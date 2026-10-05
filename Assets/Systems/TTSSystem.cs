using FYFY;
using FYFY_plugins.PointerManager;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TTSSystem : FSystem
{
    private Family f_selectableElements = FamilyManager.getFamily(new AnyOfComponents(typeof(Button), typeof(TMP_InputField), typeof(Selectable), typeof(TMP_Dropdown), typeof(Toggle), typeof(Scrollbar)), new NoneOfComponents(typeof(PointerSensitive)));
    private Family f_focused = FamilyManager.getFamily(new AllOfComponents(typeof(PointerOver)), new AnyOfComponents(typeof(Button), typeof(TMP_InputField), typeof(Selectable), typeof(TMP_Dropdown), typeof(Toggle), typeof(Scrollbar)));
    private Family f_currentAction = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction)));
    private Family f_inputFields = FamilyManager.getFamily(new AllOfComponents(typeof(TMP_InputField)));
    private Family f_toggles = FamilyManager.getFamily(new AllOfComponents(typeof(Toggle)));
    private Family f_scrollBars = FamilyManager.getFamily(new AllOfComponents(typeof(Scrollbar)));
    private Family f_agentSelection = FamilyManager.getFamily(new AnyOfTags("HaloSelection"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_localizationLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(LocalizationLoaded)));


    [DllImport("__Internal")]
    private static extern string CallTTS(string txt); // call javascript => send txt to html to be read by TTS navigator
    [DllImport("__Internal")]
    private static extern void SendToScreenReader(string label); // call javascript

    [DllImport("__Internal")]
    private static extern bool IsTTSEnabled(); // call javascript => return true if "TTS" is checked in html

    [DllImport("__Internal")]
    private static extern bool InstructionOnly(); // call javascript => return true if "Instruction" is checked in html

    private GameData gameData;
    private GameObject previousSelectedGO;
    private GameObject previousFocusedGO;
    private Vector2 previousMousePosition;
    private Dictionary<Scrollbar, float> lastScrollbarNotif;

    private Coroutine currentActionBuilder = null;

    public EventSystem eventSystem;

    protected override void onStart()
    {
        lastScrollbarNotif = new Dictionary<Scrollbar, float>();

        GameObject go = GameObject.Find("GameData");
        if (go != null)
            gameData = go.GetComponent<GameData>();
        else
        {
            Pause = true; // if no GameData we lock this system
            return;
        }

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

        foreach (GameObject selection in f_agentSelection)
            onNewAgentSelected(selection);
        f_agentSelection.addEntryCallback(onNewAgentSelected);

        if (Application.platform == RuntimePlatform.WebGLPlayer)
            // get current state 
            gameData.newTTS_state = IsTTSEnabled();
    }

    // Fonction appelée depuis le javascript (voir Assets/WebGLTemplates/Custom/game.html) via le Wrapper du Système
    public void toggleTTS()
    {
        gameData.newTTS_state = IsTTSEnabled();
    }

    protected override void onProcess(int familiesUpdateCount)
    {
        if (previousSelectedGO != eventSystem.currentSelectedGameObject && eventSystem.currentSelectedGameObject != null)
        {
            previousSelectedGO = eventSystem.currentSelectedGameObject;
            defTTS(previousSelectedGO);
        }

        // send statement if TTS state change in all scenes except ConnexionScene because we don't know the user id
        if (Application.platform == RuntimePlatform.WebGLPlayer && gameData.newTTS_state != gameData.oldTTS_state && SceneManager.GetActiveScene().name != "ConnexionScene")
        {
            gameData.oldTTS_state = gameData.newTTS_state;
            GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
            {
                verb = gameData.newTTS_state ? "enabled" : "disabled",
                objectType = "tts",
                activityExtensions = new Dictionary<string, string>() {
                    { "value", InstructionOnly() ? "instructions" : "all" }
                }
            });
        }
    }

    private void onNewSelectable(GameObject selectable)
    {
        // On exclue le tooltip du système de TTS
        if (!selectable.GetComponentInParent<Tooltip>())
            GameObjectManager.addComponent<PointerSensitive>(selectable);
    }

    private void onNewFocus(GameObject focused)
    {
        // Ne définir le TTS que si la position de la souris a changé
        Vector2Control pointerPos = Pointer.current.position;
        if (previousMousePosition != new Vector2(pointerPos.x.value, pointerPos.y.value))
            defTTS(focused);
    }

    private void onNewAgentSelected(GameObject haloSelection)
    {
        string agentName = haloSelection.transform.parent.GetComponent<ScriptRef>().executablePanel.GetComponentInChildren<UIRootExecutor>(true).scriptName;

        if (Application.platform == RuntimePlatform.WebGLPlayer) { 
            if (!InstructionOnly())
                CallTTS(Utility.GetLocalizedString("FocusOn") + agentName);
            SendToScreenReader(Utility.GetLocalizedString("FocusOn") + agentName);
        }
        else
            Debug.Log(Utility.GetLocalizedString("FocusOn") + agentName);
    }

    private void defTTS(GameObject focused)
    {
        if (gameData == null || f_localizationLoaded.Count == 0)
            return;

        Selectable select = focused.GetComponent<Selectable>();

        string suffix = "";
        string content = "";
        // Cas général : Boutton, TMP_Text, Toggle, DropDown => on va chercher le texte dans ses enfants
        if (focused.GetComponent<TMP_InputField>() == null && focused.GetComponent<LibraryItemRef>() == null)
        {
            TMP_Text text = focused.GetComponentInChildren<TMP_Text>();
            if (text != null)
                content = text.text;
        }

        if (focused.GetComponent<Button>())
            suffix = ", "+ Utility.GetLocalizedString("Button"); // "Boutton" : "Button"
        else if (focused.GetComponent<TMP_InputField>())
        {
            suffix = ", "+ Utility.GetLocalizedString("Inputfield"); // "Champ de saisie" : "Input field"
            TMP_InputField inputfield = focused.GetComponent<TMP_InputField>();
            // S'il y a quelque chose dans le inputfield, utiliser cette valeur
            if (inputfield.text != "")
                content = inputfield.text;
            else // sinon utiliser le premier TMP_Text disponible qui sera le placeholder
                content = inputfield.GetComponentInChildren<TMP_Text>(true).text;
        }
        else if (focused.GetComponent<TMP_Dropdown>())
            suffix = ", "+ Utility.GetLocalizedString("Dropdown"); // "Liste déroulante" : "Dropdown"
        else if (focused.GetComponent<Toggle>())
        {
            suffix = ", " + Utility.GetLocalizedString("Toggle"); // "Case à cocher" : "Toggle"
            Toggle toggle = focused.GetComponent<Toggle>();
            if (toggle.isOn)
                suffix += ", "+ Utility.GetLocalizedString("Checked"); // "cochée" : "checked"
            else
                suffix += ", "+ Utility.GetLocalizedString("Unchecked"); // "non cochée" : "unchecked"
        }
        else if (focused.GetComponent<Scrollbar>())
        {
            Scrollbar scrollbar = focused.GetComponent<Scrollbar>();
            content = Utility.GetLocalizedString("ScrollbarValue") + scrollbar.value; // "Barre de défilement, valeur : " : "Scrollbar, value: "
        }
        else if (focused.GetComponent<CurrentAction>())
        {
            suffix = ", " + Utility.GetLocalizedString("CurrentAction"); // "Action courrante" : "Current action"
        }
        else if (focused.GetComponent<Image>())
        {
            content = Utility.GetLocalizedString("Image"); // "Image" : "Image"
        }

        if (select && !select.IsInteractable())
            suffix += ", " + Utility.GetLocalizedString("Disabled"); // "désactivée" : "disabled";

        // cas du texte de remplacement
        ImgReplacementText replacementText = focused.GetComponentInChildren<ImgReplacementText>();
        if (replacementText != null && replacementText.replacementText != "")
            suffix += ", " + Utility.GetLocalizedString("ReplacementText") + " " + replacementText.replacementText; // "texte de remplacement :" : "replacement text:"

        // Try to get tooltip to complete description
        TooltipContent tooltip = focused.GetComponentInChildren<TooltipContent>();
        if (tooltip != null && tooltip.text != "")
            content += (content != "" ? ", " : "") + tooltip.text;

        if (content == "")
            content = focused.name;
        else
        {
            content = content.Replace("<br>", " ");
            content = content.Replace("\\u00a0", " "); // remplacer le code des espaces insécables par des espaces simples
        }

        if (Application.platform == RuntimePlatform.WebGLPlayer) {
            if (!InstructionOnly() || focused.GetComponentInParent<DialogPanel>() != null)
                CallTTS(content + suffix);
            SendToScreenReader(content + suffix);
        }
        else
            Debug.Log(content + suffix);

        Vector2Control pointerPos = Pointer.current.position;
        previousMousePosition = new Vector2(pointerPos.x.value, pointerPos.y.value);
        previousFocusedGO = focused;
    }

    private void onNewCurrentAction(GameObject unused)
    {
        // Utilisation d'une coroutine parce qu'on ne veut faire se traitement qu'une seule fois pour toutes les CurrentActions
        if (currentActionBuilder == null)
            currentActionBuilder = MainLoop.instance.StartCoroutine(buildTTSForCurrentActions());
    }

    private IEnumerator buildTTSForCurrentActions()
    {
        yield return null;
        string actions = "";
        foreach (GameObject currentAction in f_currentAction)
        {
            actions += currentAction.GetComponent<CurrentAction>().agent.GetComponent<ScriptRef>().executableScript.GetComponent<UIRootExecutor>().scriptName + " " + Utility.GetLocalizedString("CurrentAction") + " " + currentAction.GetComponentInChildren<TooltipContent>().text + ". ";
        }

        if (Application.platform == RuntimePlatform.WebGLPlayer) {
            if (!InstructionOnly())
                CallTTS(actions);
            SendToScreenReader(actions);
        }
        else
            Debug.Log(actions);

        currentActionBuilder = null;
    }

    // Pour vocaliser le texte sélectionné à l'intérieur d'un inputField
    private void onNewInputField(GameObject inputField_GO)
    {
        TMP_InputField inputF = inputField_GO.GetComponent<TMP_InputField>();
        inputF.onTextSelection.AddListener(delegate (string input, int end, int start)
        {
            string output = input.Substring(Mathf.Min(start, end), Mathf.Max(start, end) - Mathf.Min(start, end)) +", "+ Utility.GetLocalizedString("Inputfield") + " " + Utility.GetLocalizedString("Selected");

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                if (!InstructionOnly())
                    CallTTS(output);
                SendToScreenReader(output);
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
            string output = toggle.isOn ? Utility.GetLocalizedString("Checked") : Utility.GetLocalizedString("Unchecked"); // "cochée" ou "non cochée"

            if (Application.platform == RuntimePlatform.WebGLPlayer) {
                if (!InstructionOnly())
                    CallTTS(output);
                SendToScreenReader(output);
            }
            else
                Debug.Log(output);
        });
    }

    // Pour vocaliser le scorll d'une scrollbar
    private void onNewScrollbar(GameObject scrollbar_GO)
    {
        Scrollbar scrollbar = scrollbar_GO.GetComponent<Scrollbar>();
        if (!lastScrollbarNotif.ContainsKey(scrollbar))
            lastScrollbarNotif.Add(scrollbar, scrollbar.value);
        scrollbar.onValueChanged.AddListener(delegate (float value)
        {
            // N'envoyer à la synthèse vocale que si elle a le focus ET que le delta de scroll dépasse le seuil
            if ((scrollbar_GO == previousFocusedGO || scrollbar_GO == eventSystem.currentSelectedGameObject) && Mathf.Abs(lastScrollbarNotif[scrollbar]-scrollbar.value) > 0.1f)
            {
                if (Application.platform == RuntimePlatform.WebGLPlayer) {
                    if (!InstructionOnly())
                        CallTTS(scrollbar.value + "");
                    SendToScreenReader(scrollbar.value + "");
                }
                else
                    Debug.Log(scrollbar.value);

                lastScrollbarNotif[scrollbar] = scrollbar.value;
            }
        });
    }
}
