using FYFY;
using FYFY_plugins.PointerManager;
using System;
using System.Collections.Generic;
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

    private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));
    private Family f_gameLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded)));

    private Family f_walls = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"));
    private Family f_grounds = FamilyManager.getFamily(new AnyOfTags("Ground"));
    private Family f_doors = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
    private Family f_consoles = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(AudioSource)));
    private Family f_coins = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Coin"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_players = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family f_drone = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Drone"));
    private Family f_redDetector = FamilyManager.getFamily(new AllOfComponents(typeof(Rigidbody), typeof(Detector), typeof(Position)));
    private Family f_exit = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Exit"));
    private Dictionary<int, Dictionary<int, string>> staticMap;


    [DllImport("__Internal")]
    private static extern string CallTTS(string txt); // call javascript => send txt to html to be read by TTS navigator


    [DllImport("__Internal")]
    private static extern string SendToScreenReader(string txt); // call javascript => send txt to html to be accessible by screen readers

    [DllImport("__Internal")]
    private static extern string UpdateMap(string txt); // call javascript => send txt to html to made map accessible by text

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

        if (Application.platform == RuntimePlatform.WebGLPlayer)
            f_gameLoaded.addEntryCallback(buildStaticMap);
    }

    protected override void onProcess(int familiesUpdateCount)
    {
        if (previousSelectedGO != eventSystem.currentSelectedGameObject && eventSystem.currentSelectedGameObject != null)
        {
            previousSelectedGO = eventSystem.currentSelectedGameObject;
            defTTS(previousSelectedGO);
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
        // Ne définir le TTS que si la position de la souris à changé
        if (previousMousePosition != Input.mousePosition)
            defTTS(focused);
    }

    private void defTTS(GameObject focused)
    {
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
            suffix = ", "+gameData.localization[54]; // "Boutton" : "Button";
        else if (focused.GetComponent<TMP_InputField>())
        {
            suffix = ", "+gameData.localization[55]; // "Champ de saisie" : "Input field";
            TMP_InputField inputfield = focused.GetComponent<TMP_InputField>();
            // S'il y a quelque chose dans le inputfield, utiliser cette valeur
            if (inputfield.text != "")
                content = inputfield.text;
            else // sinon utiliser le premier TMP_Text disponible qui sera le placeholder
                content = inputfield.GetComponentInChildren<TMP_Text>(true).text;
        }
        else if (focused.GetComponent<TMP_Dropdown>())
            suffix = ", "+gameData.localization[56]; // "Liste déroulante" : "Dropdown";
        else if (focused.GetComponent<Toggle>())
        {
            suffix = ", " + gameData.localization[57]; // "Case à cocher" : "Toggle";
            Toggle toggle = focused.GetComponent<Toggle>();
            if (toggle.isOn)
                suffix += ", "+gameData.localization[58]; // "cochée" : "checked";
            else
                suffix += ", "+gameData.localization[59]; // "non cochée" : "unchecked";
        }
        else if (focused.GetComponent<Scrollbar>())
        {
            Scrollbar scrollbar = focused.GetComponent<Scrollbar>();
            content = gameData.localization[60] + scrollbar.value; // "Barre de défilement, valeur : " : "Scrollbar, value: ";
        }
        else if (focused.GetComponent<CurrentAction>())
        {
            suffix = ", " + gameData.localization[61]; // "Action courrante" : "Current action";
        }

        if (select && !select.IsInteractable() && focused.GetComponent<CurrentAction>() != null)
            suffix += ", " + gameData.localization[62]; // "désactivée" : "disabled";



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
        {
            if (!InstructionOnly() || focused.transform.parent.gameObject.name == "DialogPanel")
                CallTTS(content + suffix);
            SendToScreenReader(content + suffix);
        }
        else
            Debug.Log(content + suffix);

        previousMousePosition = Input.mousePosition;
        previousFocusedGO = focused;
    }

    private void onNewCurrentAction(GameObject unused)
    {
        string actions = "";
        foreach (GameObject currentAction in f_currentAction)
        {
            actions += currentAction.GetComponent<CurrentAction>().agent.GetComponent<ScriptRef>().executableScript.GetComponent<UIRootExecutor>().scriptName + " " + gameData.localization[63] + " " + currentAction.GetComponentInChildren<TooltipContent>().text+". ";
        }

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            if (!InstructionOnly())
                CallTTS(actions);
            SendToScreenReader(actions);
            updateMap(null);
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
            string output = input.Substring(Mathf.Min(start, end), Mathf.Max(start, end) - Mathf.Min(start, end)) +", "+ gameData.localization[55] + " " + gameData.localization[64];
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
            string output = toggle.isOn ? gameData.localization[58] : gameData.localization[59]; // "cochée" ou "non cochée"
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
                    SendToScreenReader(scrollbar.value + "");
                }
                else
                    Debug.Log(scrollbar.value);
            }
        });
    }


    private void buildStaticMap(GameObject unused)
    {
        staticMap = new Dictionary<int, Dictionary<int, string>>();
        foreach (GameObject ground in f_grounds)
        {
            int x = (int)(ground.transform.localPosition.z / 3); // C'est bien le z du transform dans le x de la map du jeu
            int y = (int)(ground.transform.localPosition.x / 3); // et le x du transform dans le y de la map du jeu
            if (!staticMap.ContainsKey(x))
                staticMap[x] = new Dictionary<int, string>();
            staticMap[x][y] = gameData.localization[66];
        }
        foreach (GameObject wall in f_walls)
        {
            Position pos = wall.GetComponent<Position>();
            if (!staticMap.ContainsKey(pos.x))
                staticMap[pos.x] = new Dictionary<int, string>();
            staticMap[pos.x][pos.y] = gameData.localization[67];
        }
        foreach (GameObject teleport in f_exit)
        {
            Position pos = teleport.GetComponent<Position>();
            // On ignore les "Spawn" pas utile pour la description de la carte, juste de la déco
            staticMap[pos.x][pos.y] = teleport.tag == "Exit" ? gameData.localization[68] : staticMap[pos.x][pos.y];
        }

        f_editingMode.addEntryCallback(updateMap);
        updateMap(null);
    }

    private void updateMap(GameObject unused)
    {
        // Calcul du nombre maximal de y
        int yMax = 0;
        foreach (int key in staticMap.Keys)
            yMax = staticMap[key].Count > yMax ? staticMap[key].Count : yMax;
        // Construction d'une martice de la bonne taille
        string [,] exportMap = new string[staticMap.Count, yMax];
        // Recopie de la partie statique dans la map d'export
        foreach (KeyValuePair<int, Dictionary<int, string>> xKvp in staticMap)
            foreach (KeyValuePair<int, string> yKvp in xKvp.Value)
                exportMap[xKvp.Key, yKvp.Key] = yKvp.Value;

        foreach (GameObject coin in f_coins)
        {
            Position pos = coin.GetComponent<Position>();
            exportMap[pos.x, pos.y] += "/"+ gameData.localization[69];
        }

        foreach (GameObject player in f_players)
        {
            Position pos = player.GetComponent<Position>();
            exportMap[pos.x, pos.y] += "/"+ gameData.localization[70] + ": " + player.GetComponent<AgentEdit>().associatedScriptName + " (" + player.GetComponent<Direction>().direction + ")";
        }

        foreach (GameObject drone in f_drone)
        {
            Position pos = drone.GetComponent<Position>();
            // récupération du nom du drone
            ScriptRef scriptRef = drone.GetComponent<ScriptRef>();
            string droneName = scriptRef.executablePanel.transform.Find("Header").Find("agentName").GetComponent<TMP_Text>().text;
            exportMap[pos.x, pos.y] += "/"+ gameData.localization[71] + ": " + droneName + " (" + drone.GetComponent<Direction>().direction + ")";
        }

        foreach (GameObject redArea in f_redDetector)
        {
            Position pos = redArea.GetComponent<Position>();
            exportMap[pos.x, pos.y] += "/"+ gameData.localization[72];
        }

        foreach (GameObject door in f_doors)
        {
            Position pos = door.GetComponent<Position>();
            exportMap[pos.x, pos.y] += "/"+ gameData.localization[73] + door.GetComponent<ActivationSlot>().slotID+" "+(door.activeInHierarchy ? "("+ gameData.localization[74] + ")" : "("+ gameData.localization[75] + ")");
        }

        foreach (GameObject console in f_consoles)
        {
            Position pos = console.GetComponent<Position>();
            exportMap[pos.x, pos.y] += "/"+ gameData.localization[76] + " (" + String.Join(",", console.GetComponent<Activable>().slotID) + ")";
        }

        // Convert map to string
        string stringExport = gameData.localization[77]+"<br>";
        for (int j = 0; j < exportMap.GetLength(1); j++)  {
            for (int i = 0; i < exportMap.GetLength(0); i++)
                stringExport += exportMap[i, j];
            stringExport += "<br>";
        }
        stringExport += gameData.localization[78] + "<br>";

        UpdateMap(stringExport);
    }
}
