using FYFY;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.Networking;
using static UnityEngine.AdaptivePerformance.Provider.AdaptivePerformanceSubsystemDescriptor;

public class ButtonConnection : FSystem
{

    private Family f_actionForLRS= FamilyManager.getFamily(new AllOfComponents(typeof(ActionPerformedForLRS)));

    public GameObject EditableCanvas;
    public RectTransform editableContainers;
    public List<(ActionPerformedForLRS, string)> LogHistory = new List<(ActionPerformedForLRS, string)>();

    public GameObject mainCanvas; // Le canvas principal
    public GameObject dropZone; // La dernière dropzone utilisée

    private Family f_doors = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
    private Family f_coins = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Coin"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_players = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family f_drone = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Drone"));
    private Family f_redDetector = FamilyManager.getFamily(new AllOfComponents(typeof(Rigidbody), typeof(Detector), typeof(Position)));
    private Family f_draggableElement = FamilyManager.getFamily(new AllOfComponents(typeof(ElementToDrag)));

    private GameData gameData;

    protected override void onStart()
    {
        GameObject gd = GameObject.Find("GameData");
        if (gd != null)
            gameData = gd.GetComponent<GameData>();
    }

    public void ChangText(string newText)
    {

        /*
        Debug.Log("Show the LRS data in the console : ");

        Debug.Log(LogHistory);
        foreach (ActionPerformedForLRS ap in LogHistory)
        {
            Debug.Log("verb : " + ap.verb);
            Debug.Log("objectType : " + ap.objectType);
            Debug.Log("result : " + ap.result);
            Debug.Log("completed : " + ap.completed);
            Debug.Log("success : " + ap.success);
            Debug.Log("response : " + ap.response);
            Debug.Log("score : " + ap.score);
            Debug.Log("duration : " + ap.duration);
            foreach (KeyValuePair<string, string> entry in ap.activityExtensions)
            {
                Debug.Log($"Key: {entry.Key}, Value: {entry.Value}");
            }
        }
        ;
        */


        // Supprime les exemples de scripts précédents
        GameObject exempleScript = GameObject.Find("MainCanvas/ExamplePanel");

        List<GameObject> childrenToDestroy = new List<GameObject>();

        foreach (Transform child in exempleScript.transform){
            childrenToDestroy.Add(child.gameObject);
        }

        foreach (GameObject childObj in childrenToDestroy){
            GameObjectManager.unbind(childObj);
            Object.Destroy(childObj);
        }


        string scriptsContent = "";
        foreach (Transform viewportScriptContainer in editableContainers)
            scriptsContent += exportEditableScriptToString(viewportScriptContainer.Find("ScriptContainer"), null);

        string xmlPayload = BuildXmlPayload(scriptsContent);

        MainLoop.instance.StartCoroutine(PostXML(xmlPayload));

    }


    private void showExampleScript(string response)
    {
        Debug.Log(response);

        string pattern = @"<script\b[^>]*>(.*?)<\/script>";
        MatchCollection matches = Regex.Matches(response, pattern, RegexOptions.Singleline);

        if (matches.Count == 0)
        {
            Debug.LogWarning("No <script> tags found in the provided text.");
            return;
        }

        string filePath = Path.Combine(Application.streamingAssetsPath, "Levels/Infiltration/Niveau05.xml");
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Level file not found at: {filePath}");
            return;
        }

        for (int i = 0; i < matches.Count; i++)
        {
            Match match = matches[i];
            string xmlString = match.Value;
            xmlString = xmlString.Replace("action_llm", "action");

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(xmlString);
                Debug.Log($"Loaded script block {i + 1} successfully!");

                XmlNode child = xmlDoc.DocumentElement;
                XmlElement rootElement = xmlDoc.DocumentElement;

                ScriptToLoad stl = MainLoop.instance.gameObject.AddComponent<ScriptToLoad>();
                stl.scriptNode = child;

                // 1. Read the outputLine attribute from the <script> tag
                if (rootElement != null && rootElement.HasAttribute("outputLine"))
                {
                    stl.scriptName = rootElement.GetAttribute("outputLine");
                }
                else
                {
                    // Fallback in case a script tag doesn't have the attribute
                    stl.scriptName = "API_HARMONY";
                }

                Debug.Log($"Assigned Script Name: {stl.scriptName}");

                stl.editMode = UIRootContainer.EditMode.Locked;
                stl.type = UIRootContainer.SolutionType.NonOptimal;
            }
            catch (XmlException e)
            {
                Debug.LogError($"Script block {i + 1} found XML but it's malformed: {e.Message}");
            }
        }

        MainLoop.instance.StartCoroutine(delayRefreshMainLoop());
    }

    private Dictionary<string, string> BuildCategoryExtension(){
        // Prepare the dictionary
        Dictionary<string, string> activityExtensions = new Dictionary<string, string>();

        // 1. Coins Coordinates
        List<string> coinCoords = new List<string>();
        foreach (GameObject coin in f_coins)
        {
            Position pos = coin.GetComponent<Position>();
            coinCoords.Add($"({pos.x},{pos.y})");
        }
        activityExtensions.Add("coins_positions", string.Join(";", coinCoords));

        // 2. Players Coordinates & Info
        List<string> playerInfo = new List<string>();
        foreach (GameObject player in f_players)
        {
            Position pos = player.GetComponent<Position>();
            string name = player.GetComponent<AgentEdit>().associatedScriptName;
            playerInfo.Add($"({pos.x},{pos.y}):{name}:{player.GetComponent<Direction>().direction}");
        }
        activityExtensions.Add("players_positions", string.Join(";", playerInfo));

        // 3. Doors Coordinates & State
        List<string> doorInfo = new List<string>();
        foreach (GameObject door in f_doors)
        {
            Position pos = door.GetComponent<Position>();
            ActivationSlot act = door.GetComponent<ActivationSlot>();
            int state = act.state ? 1 : 0;
            doorInfo.Add($"({pos.x},{pos.y}):ID{act.slotID}:{door.GetComponent<Direction>().direction}:{state}");
        }
        activityExtensions.Add("doors_positions", string.Join(";", doorInfo));

        // 4. Red Detectors (Danger Areas)
        List<string> detectorCoords = new List<string>();
        foreach (GameObject redArea in f_redDetector)
        {
            Position pos = redArea.GetComponent<Position>();
            if (pos.x != -1 && pos.y != -1)
            {
                detectorCoords.Add($"({pos.x},{pos.y})");
            }
        }
        activityExtensions.Add("danger_zones", string.Join(";", detectorCoords));

        // 5. Drones Coordinates & Associated Script
        List<string> droneCoords = new List<string>();
        foreach (GameObject drone in f_drone)
        {

            Position pos = drone.GetComponent<Position>();
            if (pos.x != -1 && pos.y != -1)
            {
                ScriptRef scriptRef = drone.GetComponent<ScriptRef>();
                string droneName = scriptRef.executablePanel.transform.Find("Header/agentName").GetComponent<TMP_Text>().text;
                droneCoords.Add($"({pos.x},{pos.y}):{droneName}:{drone.GetComponent<Direction>().direction}");
            }

        }
        activityExtensions.Add("drones_positions", string.Join(";", droneCoords));

        return activityExtensions;
    }

    public string GetInventoryLimitsString()
    {
        StringBuilder sb = new StringBuilder();

        foreach (GameObject go in f_draggableElement)
        {
            if (go.activeSelf)
            {
                string blockName = go.name;
                int limit = -1;

                if (gameData.actionBlockLimit.ContainsKey(blockName))
                {
                    limit = gameData.actionBlockLimit[blockName];
                    
                }

                // Format the limit display
                string limitText = (limit < 0) ? "Unlimited" : limit.ToString();

                sb.AppendLine($"{blockName}: {limitText}");
            }
        }

        return sb.ToString();
    }

    private string BuildXmlPayload(string content)
    {
        ActionPerformedForLRS NewScript = new ActionPerformedForLRS
        {
            verb = "asked-help",
            objectType = content,
            activityExtensions = BuildCategoryExtension(),
        };

        LogHistory.Add((NewScript, Time.timeSinceLevelLoad.ToString("F2")));

        StringBuilder sb = new StringBuilder();
        sb.Append("<activities>");
        sb.Append("<studentID>"+ GBL_Interface.userUUID + "</studentID>");
        sb.Append("<sendStatement>" + gameData.sendStatementEnabled + "</sendStatement>");
        foreach (var log in LogHistory)
        {
            sb.Append("<activity>");
            sb.Append($"<type>{log.Item1.verb}</type>");
            sb.Append($"<object>{log.Item1.objectType}</object>");
            sb.Append($"<result>{log.Item1.result}</result>");

            if (log.Item1.activityExtensions != null){
                if(log.Item1.activityExtensions.ContainsKey("level_timestamp"))
                {
                    sb.Append($"<timestamp>{log.Item2}</timestamp>");
                }
                if (log.Item1.activityExtensions.ContainsKey("value")){
                    string val = log.Item1.activityExtensions["value"];
                    sb.Append($"<activityExtensions>{val}</activityExtensions>");
                }
                if (log.Item1.activityExtensions.ContainsKey("content"))
                {
                    string val = log.Item1.activityExtensions["content"];
                    sb.Append($"<activityExtensions>{val}</activityExtensions>");
                }
                if(log.Item1.verb == "asked-help")
                {
                    string val = log.Item1.activityExtensions["coins_positions"];
                    sb.Append($"<activityExtensions><coins_positions>{val}</coins_positions>");
                    val = log.Item1.activityExtensions["players_positions"];
                    sb.Append($"<players_positions>{val}</players_positions>");
                    val = log.Item1.activityExtensions["doors_positions"];
                    sb.Append($"<doors_positions>{val}</doors_positions>");
                    val = log.Item1.activityExtensions["danger_zones"];
                    sb.Append($"<danger_zones>{val}</danger_zones>");
                    val = log.Item1.activityExtensions["drones_positions"];
                    sb.Append($"<drones_positions>{val}</drones_positions>");

                    string inventoryLimits = GetInventoryLimitsString();
                    sb.Append($"<usableBlocksLeft>{inventoryLimits}</usableBlocksLeft></activityExtensions>");
                }
            }

            if (log.Item1.resultExtensions != null){
                string extensionsStr = (log.Item1.resultExtensions != null)
                    ? string.Join(", ", log.Item1.resultExtensions.Select(kvp => $"{kvp.Key}: {kvp.Value}"))
                    : "null";
                    sb.Append($"<resultExtensions>{extensionsStr}</resultExtensions>");
            }

            sb.Append("</activity>");
        }
        sb.Append("</activities>");
        return sb.ToString();
    }

    public GameObject panelPrefab;
    public Transform contentPanel;

    IEnumerator PostXML(string xmlData)
    {
        string url = "http://localhost:5000/api/receive-xml";
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(xmlData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/xml");

            yield return request.SendWebRequest();

            Debug.Log(request.downloadHandler.text);

            showExampleScript(request.downloadHandler.text);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                Debug.Log("Response: " + request.downloadHandler.text);
                GameObject newPanel = Object.Instantiate(panelPrefab, contentPanel);
                RectTransform rect = newPanel.GetComponent<RectTransform>();
                rect.localScale = Vector3.one;
                rect.anchoredPosition = Vector2.zero;


                string pattern = @"<script\b[^>]*>.*?</script>";
                string shownText = Regex.Replace(request.downloadHandler.text, pattern, "", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                // 2. Access the Text component and set the string
                TMP_Text textComponent = newPanel.GetComponentInChildren<TMP_Text>();
                textComponent.text = shownText;
                Debug.Log($"Spawned Panel Parent: {newPanel.transform.parent.name}");
                Debug.Log($"Text Component found? {textComponent != null}. Text applied length: {textComponent.text.Length}");

                //TextField.text = 
            }
        }
    }



    IEnumerator PostCompletion(string score)
    {
        string url = "http://localhost:5000/api/receive-xml";
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(score);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/xml");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }



    protected override void onProcess(int familiesUpdateCount)
    {
        foreach (GameObject go in f_actionForLRS)
        {
            OnNewAction(go);
        }
    }

    private string exportEditableScriptToString(Transform scriptContainer, GameObject focusedArea)
    {
        string scriptsContent = scriptContainer.Find("Header").GetComponentInChildren<TMP_InputField>().text + " {";
        for (int i = 0; i < scriptContainer.childCount; i++)
        {
            if (scriptContainer.GetChild(i).GetComponent<Highlightable>())
                scriptsContent += " " + UtilityGame.exportBlockToString(scriptContainer.GetChild(i).GetComponent<Highlightable>(), focusedArea);
        }
        if (scriptContainer.GetChild(scriptContainer.childCount - 1).gameObject == focusedArea)
            scriptsContent += " ####";
        scriptsContent += " }\n";
        return scriptsContent;
    }

    private void OnNewAction(GameObject go)
    {
        if (go == null)
        {
            Debug.LogWarning("OnNewAction received a null GameObject!");
            return;
        }

        ActionPerformedForLRS[] listAP = go.GetComponents<ActionPerformedForLRS>();


        foreach (ActionPerformedForLRS ap in listAP)
        {

            if (ap.activityExtensions == null)
                ap.activityExtensions = new Dictionary<string, string>();

            if (ap.verb == "completed" && ap.success == -1)
            {
                string abandonMsg = "failed_attempt:ID" + GBL_Interface.userUUID + "<>:Post?" + gameData.sendStatementEnabled;
                MainLoop.instance.StartCoroutine(PostCompletion(abandonMsg));
            }

            Debug.LogWarning(ap.verb);
            Debug.LogWarning(Time.timeSinceLevelLoad.ToString("F2"));

            LogHistory.Add((ap, Time.timeSinceLevelLoad.ToString("F2")));

            string resultExtensionsStr = (ap.resultExtensions != null)
                ? string.Join(", ", ap.resultExtensions.Select(kvp => $"{kvp.Key}: {kvp.Value}"))
                : "null";

            string activityExtensionsStr = (ap.activityExtensions != null)
                ? string.Join(", ", ap.activityExtensions.Select(kvp => $"{kvp.Key}: {kvp.Value}"))
                : "null";

            if (ap.resultExtensions != null)
            {
                if (ap.resultExtensions.ContainsKey("score"))
                {
                    // 1. Check for unique "executed" verb
                    // Count how many times the verb "executed" appears in the total history
                    int executionCount = LogHistory.Count(log => log.Item1.verb == "executed");

                    MainLoop.instance.StartCoroutine(PostCompletion("score:" + ap.resultExtensions["score"] + ":ID" + GBL_Interface.userUUID + "<tv time>" + ":Post?" + gameData.sendStatementEnabled + ":executionsNb" + executionCount));
                }
            }

            if (ap.verb == "exited"){
                var lastCompletion = LogHistory.LastOrDefault(log => log.Item1.verb == "completed");
                
                if (lastCompletion.Item1 == null){
                    string exitMessage = "exit:ID" + GBL_Interface.userUUID + "<>:Post?" + gameData.sendStatementEnabled;
                    MainLoop.instance.StartCoroutine(PostCompletion(exitMessage));
                }
            }

            if (ap.activityExtensions != null && ap.verb == "launched")
            {
                go = GameObject.Find("GameData");
                gameData = go.GetComponent<GameData>();

                List<string> dialogStrings = gameData.scenarios[gameData.selectedScenario]
                    .levels[gameData.levelToLoad]
                    .overridedDialogs
                    .Select(d => d.text)
                    .ToList();

                if (ap.activityExtensions.ContainsKey("value"))
                {
                    string dialogContent = string.Join(";", dialogStrings);
                    if(dialogContent == ""){
                        dialogContent = "no dialog";
                    }
                    MainLoop.instance.StartCoroutine(PostCompletion("level:" + ap.activityExtensions["value"] + ":ID" + GBL_Interface.userUUID + "<" + dialogContent  + ">" + ":Post?" + gameData.sendStatementEnabled));
                }
            }


            string info = $"[LRS Log] Verb: {ap.verb} | Success: {ap.success} | Result: {resultExtensionsStr} | Activity: {activityExtensionsStr}";
            Debug.Log(info);
        }
    }

    public void AddActionToDropZone(GameObject libraryItem)
    {
        GameObject itemDragged = UtilityGame.createEditableBlockFromLibrary(libraryItem, mainCanvas);

        GameObjectManager.bind(itemDragged);

        //var actionScript = mainCanvas.GetComponent<DragDropSystem>();

        // Si le bloc ajouté est une structure de contrôle, définir son premier ReplacementSlot comme le prochain à utiliser pour ajouter de nouveaux blocs au double-click
        if (itemDragged.GetComponent<ControlElement>() != null)
        {
            ReplacementSlot[] dropAreas = itemDragged.GetComponentsInChildren<ReplacementSlot>(true);
            foreach (ReplacementSlot rs in dropAreas)
                if (rs.slotType == ReplacementSlot.SlotType.BaseElement)
                {
                    // L'ajout de l'item dragged ajoutera un Selected sur le replacementSlot utilisé, il faut donc attendre une frame que le Selected par défaut soit défini pour ajouter celui là qui nous intéresse (Le ReplacementSlot de notre structure de contrôle)
                    rs.StartCoroutine(delaySlotSelection(rs));
                    break;
                }
        }

        GameObjectManager.addComponent<NeedRefreshPlayButton>(MainLoop.instance.gameObject);


        GameObjectManager.addComponent<ScrollOn>(itemDragged);
        MainLoop.instance.StartCoroutine(UtilityGame.pulseItem(itemDragged));

        // Refresh UI
        GameObjectManager.addComponent<NeedRefreshPlayButton>(MainLoop.instance.gameObject);

        itemDragged = null;

    }

    private IEnumerator delaySlotSelection(ReplacementSlot rs)
    {
        yield return null;
        GameObjectManager.addComponent<Selected>(rs.gameObject);
    }

    private IEnumerator delayRefreshMainLoop()
    {
        yield return null;
        GameObjectManager.refresh(MainLoop.instance.gameObject);
    }
}
