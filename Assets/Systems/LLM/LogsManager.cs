using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using TinCan;
using UnityEngine;
using UnityEngine.Networking;
using static DIG.GBLXAPI.Builders.StatementBuilder;
using static UnityEditor.Profiling.RawFrameDataView;

[XmlRoot("GameLog")]
public class GameLog
{
    [XmlArray("activities")]
    public List<activity> activity = new List<activity>();
}

public class activity
{
    [XmlAttribute] public string timestamp;
    [XmlAttribute] public string type;
    public Program code;
}

public class Program
{
    [XmlAttribute] public string outputLine;
    public List<action> script;
}

public class action
{
    [XmlAttribute] public string type;
    public string value;
}

public class LogsManager : MonoBehaviour
{
    private GameLog _currentLog = new GameLog();
    public GameObject EditableCanvas;

    public void LogButtonClick(string buttonName)
    {
        _currentLog.activity.Add(new activity
        {
            timestamp = System.DateTime.Now.ToString("HH:mm:ss"),
            type = "launched-program"
        });
        Debug.Log($"Log enregistré : {buttonName}");
    }

    private List<action> newBlockList = new List<action>();
    // Capture l'état actuel des blocs (votre historique FYFY)
    public void CaptureFinalBlocks()
    {
        newBlockList = new List<action>();
        Transform containerParent = EditableCanvas.transform.GetChild(0);

        foreach (Transform viewportContainer in containerParent)
        {

            foreach (Transform block in viewportContainer.GetChild(0))
            {
                if (block.GetComponent<BaseElement>())
                {
                    newBlockList.Add(new action
                    {
                        type = block.name.Replace("(Clone)", ""),
                        value = GetBlockValue(block)
                    });
                }
            }
        }
        UIRootContainer uiRoot = EditableCanvas.transform.GetComponentInChildren<UIRootContainer>();
        string agentName = uiRoot != null ? uiRoot.scriptName : "Inconnu";

        _currentLog.activity.Add(new activity
        {
            timestamp = System.DateTime.Now.ToString("HH:mm:ss"),
            type = "asked-help",
            code = new Program
            {
                outputLine = agentName,
                script = newBlockList
            }
        });
    }

    private string GetBlockValue(Transform block)
    {
        // Optionnel : extraire la valeur si c'est un bloc 'For'
        var forControl = block.GetComponent<ForControl>();
        return forControl != null ? forControl.nbFor.ToString() : "";
    }

    public void ExportToXML()
    {
        CaptureFinalBlocks();

        string path = Path.Combine(Application.persistentDataPath, "HistoryLog.xml");
        XmlSerializer serializer = new XmlSerializer(typeof(GameLog));

        using (StreamWriter writer = new StreamWriter(path))
        {
            serializer.Serialize(writer, _currentLog);
        }

        // Affiche aussi dans la console pour débugger
        StringWriter sw = new StringWriter();
        serializer.Serialize(sw, _currentLog);
        string xmlPayload = sw.ToString();

        StartCoroutine(PostAndReadXMLCoroutine(xmlPayload));
    }

    private IEnumerator PostAndReadXMLCoroutine(string xmlContent)
    {
        // If running Flask locally, use "http://localhost:5000/api/randomize-xml" 
        // or "http://127.0.0.1:5000/api/randomize-xml"
        string url = "http://127.0.0.1:5000/api/randomize-xml";

        UnityWebRequest www = new UnityWebRequest(url, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(xmlContent);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer(); // Essential to read the response

        www.SetRequestHeader("Content-Type", "application/xml");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Server Error: {www.error}");
        }
        else
        {
            // 3. Capture the randomized XML from the server
            string receivedXml = www.downloadHandler.text;

            Debug.Log("<color=green>Success! Received Randomized XML:</color>");
            Debug.Log(receivedXml);

            // Optional: If you want to turn that XML back into a GameLog object:
            // DeserializeReceivedLog(receivedXml);
        }
    }
}