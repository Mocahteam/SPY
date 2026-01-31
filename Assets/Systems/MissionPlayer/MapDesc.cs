using FYFY;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapDesc : FSystem
{
    private Family f_gameLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded)));
    private Family f_onNewStep = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private Family f_editMode = FamilyManager.getFamily(new AnyOfComponents(typeof(EditMode)));

    private Family f_walls = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"));
    private Family f_furnitures = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Furniture"));
    private Family f_grounds = FamilyManager.getFamily(new AnyOfTags("Ground"));
    private Family f_doors = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
    private Family f_consoles = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(AudioSource)));
    private Family f_coins = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Coin"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_players = FamilyManager.getFamily(new AnyOfTags("Player"));
    private Family f_drone = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Drone"));
    private Family f_redDetector = FamilyManager.getFamily(new AllOfComponents(typeof(Rigidbody), typeof(Detector), typeof(Position)));
    private Family f_exit = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Exit"));
    private Dictionary<int, Dictionary<int, string>> staticMap;

    public GameObject panel;
    private Transform lineModel;

    private Localization gameDataLoc;
    private GameData gameData;

    public static DetectorManager instance;

    protected override void onStart()
    {
        GameObject go = GameObject.Find("GameData");
        if (go != null)
        {
            gameDataLoc = go.GetComponent<Localization>();
            gameData = go.GetComponent<GameData>();
        }

        f_gameLoaded.addEntryCallback(delegate { buildStaticMap(); });
        f_editMode.addEntryCallback(delegate { askToRefreshMapDesc(); });
        f_onNewStep.addEntryCallback(delegate { askToRefreshMapDesc(); });

        lineModel = panel.transform.Find("Model");

        Pause = true;
    }

    // Used by ReloadState and StopButton UIs
    public void askToRefreshMapDesc()
    {
        Pause = false;
    }

    protected override void onProcess(int familiesUpdateCount)
    {
        MainLoop.instance.StartCoroutine(delayUpdateMap());
        Pause = true;
    }

    private IEnumerator delayUpdateMap()
    {
        // On attend quelques frames pour laisser le temps au DetectorManager de recalculer les zones observées, dans le cas général d'un NewStep la mis à jour des zones surveillées survient en T+2 d'où le fait de laisser passer ses 3 frames
        yield return null;
        yield return null;
        yield return null;
        updateMap();
    }

    private void buildStaticMap()
    {
        staticMap = new Dictionary<int, Dictionary<int, string>>();
        foreach (GameObject ground in f_grounds)
        {
            int x = (int)(ground.transform.localPosition.z / 3); // C'est bien le z du transform dans le x de la map du jeu
            int y = (int)(ground.transform.localPosition.x / 3); // et le x du transform dans le y de la map du jeu
            if (!staticMap.ContainsKey(x))
                staticMap[x] = new Dictionary<int, string>();
            staticMap[x][y] = gameDataLoc.localization[35];
        }
        foreach (GameObject wall in f_walls)
        {
            Position pos = wall.GetComponent<Position>();
            if (!staticMap.ContainsKey(Mathf.RoundToInt(pos.x)))
                staticMap[Mathf.RoundToInt(pos.x)] = new Dictionary<int, string>();
            staticMap[Mathf.RoundToInt(pos.x)][Mathf.RoundToInt(pos.y)] = gameDataLoc.localization[36];
        }
        foreach (GameObject furniture in f_furnitures)
        {
            Position pos = furniture.GetComponent<Position>();
            if (!staticMap.ContainsKey(Mathf.RoundToInt(pos.x)))
                staticMap[Mathf.RoundToInt(pos.x)] = new Dictionary<int, string>();
            staticMap[Mathf.RoundToInt(pos.x)][Mathf.RoundToInt(pos.y)] = gameDataLoc.localization[48];
        }
        if (!gameData.hideExit && !gameData.fogEnabled)
        {
            foreach (GameObject teleport in f_exit)
            {
                Position pos = teleport.GetComponent<Position>();
                // On ignore les "Spawn" pas utile pour la description de la carte, juste de la déco
                staticMap[Mathf.RoundToInt(pos.x)][Mathf.RoundToInt(pos.y)] = teleport.CompareTag("Exit") ? gameDataLoc.localization[37] : staticMap[Mathf.RoundToInt(pos.x)][Mathf.RoundToInt(pos.y)];
            }
        }

        askToRefreshMapDesc();
    }

    private void updateMap()
    {
        if (staticMap != null)
        {
            // Calcul du nombre maximal de y
            int yMax = 0;
            foreach (int key in staticMap.Keys)
                yMax = staticMap[key].Count > yMax ? staticMap[key].Count : yMax;
            // Construction d'une martice de la bonne taille
            string[,] exportMap = new string[staticMap.Count, yMax];
            // Recopie de la partie statique dans la map d'export
            foreach (KeyValuePair<int, Dictionary<int, string>> xKvp in staticMap)
                foreach (KeyValuePair<int, string> yKvp in xKvp.Value)
                    exportMap[xKvp.Key, yKvp.Key] = yKvp.Value;

            foreach (GameObject coin in f_coins)
            {
                Position pos = coin.GetComponent<Position>();
                exportMap[Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)] += "+" + gameDataLoc.localization[38];
            }

            foreach (GameObject player in f_players)
            {
                Position pos = player.GetComponent<Position>();
                exportMap[Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)] += "+(" + gameDataLoc.localization[39] + ": " + player.GetComponent<AgentEdit>().associatedScriptName + " " + (player.GetComponent<ScriptRef>().isBroken ? gameDataLoc.localization[6] : player.GetComponent<Direction>().direction) + ")";
            }

            foreach (GameObject drone in f_drone)
            {
                Position pos = drone.GetComponent<Position>();
                // récupération du nom du drone
                ScriptRef scriptRef = drone.GetComponent<ScriptRef>();
                string droneName = scriptRef.executablePanel.transform.Find("Header/agentName").GetComponent<TMP_Text>().text;
                exportMap[Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)] += "+(" + gameDataLoc.localization[40] + ": " + droneName + " " + (scriptRef.isBroken ? gameDataLoc.localization[6] : drone.GetComponent<Direction>().direction) + ")";
            }

            foreach (GameObject redArea in f_redDetector)
            {
                Position pos = redArea.GetComponent<Position>();
                // Ajouter l'observation si ce n'est pas déjà présent sur cette case
                if (pos.x != -1 && pos.y != -1 && !exportMap[Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)].Contains("+" + gameDataLoc.localization[41]))
                    exportMap[Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)] += "+" + gameDataLoc.localization[41];
            }

            foreach (GameObject door in f_doors)
            {
                Position pos = door.GetComponent<Position>();
                ActivationSlot act = door.GetComponent<ActivationSlot>();
                exportMap[Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)] += "+(" + gameDataLoc.localization[42] + act.slotID + " " + (act.state ? gameDataLoc.localization[44] : gameDataLoc.localization[43]) + ")";
            }

            foreach (GameObject console in f_consoles)
            {
                Position pos = console.GetComponent<Position>();
                exportMap[Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)] += "+(" + gameDataLoc.localization[45] + " " + String.Join(",", console.GetComponent<Activable>().slotID) + ")";
            }

            // Affichage de la taille de la carte
            panel.transform.Find("MapSize").GetComponent<TMP_Text>().text = Utility.getFormatedText(gameDataLoc.localization[46], exportMap.GetLength(1), exportMap.GetLength(0)); // Taille carte

            // Vérifier si l'UI avec le focus ne serait pas un des éléments de la liste
            int childSelectedPos = -1;
            if (EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.transform.parent == panel.transform)
                childSelectedPos = EventSystem.current.currentSelectedGameObject.transform.GetSiblingIndex();

            // Nettoyage de l'ancienne description, toutes les lignes comprises entre la 4ème et l'avant-avant-dernière
            for (int i = 3; i < panel.transform.childCount - 2; i++)
            {
                Transform lineToRemove = panel.transform.GetChild(i);
                lineToRemove.gameObject.SetActive(false);
                GameObjectManager.unbind(lineToRemove.gameObject);
                UnityEngine.Object.Destroy(lineToRemove.gameObject);
            }
            // Ajout de la nouvelle description
            for (int j = 0; j < exportMap.GetLength(1); j++)
            {
                string line = gameDataLoc.localization[47] + " " + (j + 1) + ": "; // Ligne X:
                for (int i = 0; i < exportMap.GetLength(0); i++)
                    line += exportMap[i, j] + " "; // export de la ligne X
                                                   // Création d'une nouvelle ligne à partir du modèle
                GameObject newLine = GameObject.Instantiate<GameObject>(lineModel.gameObject, lineModel.parent);
                newLine.GetComponent<TMP_Text>().text = line;
                // Insertion de la ligne juste après la ligne indiquant le début de la description (position 3) + les lignes précédemment insérées
                newLine.transform.SetSiblingIndex(3 + j);
                newLine.SetActive(true);
                GameObjectManager.bind(newLine);
            }
            // si l'objet avec le focus était dans la liste, redonner le focus à la ligne le remplacant
            if (childSelectedPos != -1 && childSelectedPos < panel.transform.childCount)
                EventSystem.current.SetSelectedGameObject(panel.transform.GetChild(childSelectedPos).gameObject);
        }
    }
}
