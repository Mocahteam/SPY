using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// This manager enables to save the game state and to restore it on demand for instance when the player is detected by drones, he can reset the game on a state just before the previous execution
/// </summary>
public class GameStateManager : FSystem {

    private Family f_coins = FamilyManager.getFamily(new AnyOfTags("Coin"));
    private Family f_directions = FamilyManager.getFamily(new AllOfComponents(typeof(Direction)), new NoneOfComponents(typeof(Detector)));
    private Family f_positions = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new NoneOfComponents(typeof(Detector)));
    private Family f_doors = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot)));
    private Family f_scriptRefs = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef)));
    private Family f_currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction)));
    private Family f_forControls = FamilyManager.getFamily(new AllOfComponents(typeof(ForControl)));

    private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));

    private SaveContent save;

    private string currentContent;
    private GameData gameData;

    public GameObject playButtonAmount;

    public GameObject level;

    public static GameStateManager instance;

    public GameStateManager()
	{
		instance = this;
	}
    protected override void onStart()
    {
        GameObject go = GameObject.Find("GameData");
        if (go != null)
            gameData = go.GetComponent<GameData>();
        save = new SaveContent();
        f_playingMode.addEntryCallback(delegate { SaveState(); });

        Pause = true;
    }

    // Save data of all interactable objects in scene
    private void SaveState()
	{
        //reset save

        save.rawSave.totalCoin = gameData.totalCoin;
        save.rawSave.coinsState.Clear();
        foreach (GameObject coin in f_coins)
            save.rawSave.coinsState.Add(coin.activeSelf);
        save.rawSave.directions.Clear();
        foreach (GameObject dir in f_directions)
            save.rawSave.directions.Add(dir.GetComponent<Direction>().direction);
        save.rawSave.positions.Clear();
        foreach (GameObject pos in f_positions)
            save.rawSave.positions.Add(new SaveContent.RawPosition(pos.GetComponent<Position>()));
        save.rawSave.doors.Clear();
        foreach (GameObject door in f_doors)
            save.rawSave.doors.Add(new SaveContent.RawActivationSlot(door.GetComponent<ActivationSlot>()));
        save.rawSave.scriptRefs.Clear();
        foreach (GameObject scriptRef in f_scriptRefs)
            save.rawSave.scriptRefs.Add(new SaveContent.RawScriptRef(scriptRef.GetComponent<ScriptRef>()));
        save.rawSave.currentDroneActions.Clear();    
        foreach(GameObject go in f_currentActions)
            if(go.GetComponent<CurrentAction>().agent.CompareTag("Drone"))
                save.rawSave.currentDroneActions.Add(new SaveContent.RawCurrentAction(go));
        save.rawSave.currentLoopParams.Clear();
        foreach (GameObject go in f_forControls)
            save.rawSave.currentLoopParams.Add(new SaveContent.RawLoop(go.GetComponent<ForControl>()));

        currentContent = JsonUtility.ToJson(save.rawSave);

        // If amount enabled, reduce by 1
        if (playButtonAmount.activeSelf)
        {
            TMP_Text amountText = playButtonAmount.GetComponentInChildren<TMP_Text>();
            amountText.text = "" + (int.Parse(amountText.text) - 1);
        }
    }

    // Used in StopButton and ReloadState buttons in editor
    // Load saved state an restore data on interactable game objects
    public void LoadState()
    {
        save.rawSave = JsonUtility.FromJson<SaveContent.RawSave>(currentContent);

        gameData.totalCoin = save.rawSave.totalCoin;
        for (int i = 0; i < f_coins.Count && i < save.rawSave.coinsState.Count ; i++)
        {
            GameObject coin_go = f_coins.getAt(i);
            GameObjectManager.setGameObjectState(coin_go, save.rawSave.coinsState[i]);
            coin_go.GetComponent<Renderer>().enabled = save.rawSave.coinsState[i];
            coin_go.GetComponent<Collider>().enabled = save.rawSave.coinsState[i];
        }
        for (int i = 0; i < f_directions.Count && i < save.rawSave.directions.Count; i++)
        {
            GameObject go = f_directions.getAt(i);
            go.GetComponent<Direction>().direction = save.rawSave.directions[i];
            // Orienter correctement l'objet
            switch (save.rawSave.directions[i]) 
            {
                case Direction.Dir.North:
                    go.transform.rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case Direction.Dir.East:
                    go.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case Direction.Dir.West:
                    go.transform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case Direction.Dir.South:
                    go.transform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
            }
        }
        for (int i = 0; i < f_positions.Count && i < save.rawSave.positions.Count ; i++)
        {
            Position pos = f_positions.getAt(i).GetComponent<Position>();
            pos.x = save.rawSave.positions[i].x;
            pos.y = save.rawSave.positions[i].y;
            // Téléport object to the right position
            pos.transform.position = level.transform.position + new Vector3(pos.y * 3, pos.transform.position.y - level.transform.position.y, pos.x * 3);
        }
        for (int i = 0; i < f_doors.Count && i < save.rawSave.doors.Count; i++)
        {
            ActivationSlot act = f_doors.getAt(i).GetComponent<ActivationSlot>();
            act.state = save.rawSave.doors[i].state;
        }
        for (int i = 0; i < f_scriptRefs.Count && i < save.rawSave.scriptRefs.Count; i++)
        {
            ScriptRef scriptRef = f_scriptRefs.getAt(i).GetComponent<ScriptRef>();
            scriptRef.scriptFinished = save.rawSave.scriptRefs[i].scriptFinished;
            scriptRef.isBroken = save.rawSave.scriptRefs[i].isBroken;
        }
        foreach (GameObject go in f_currentActions)
            if(go.GetComponent<CurrentAction>().agent.CompareTag("Drone"))
                GameObjectManager.removeComponent<CurrentAction>(go);
        foreach(SaveContent.RawCurrentAction act in save.rawSave.currentDroneActions)
            // on demande d'ajouter la CurrentAction à la prochaine frame => A cet frame on bascule en EditMode et pour ignorer l'exécution de la restauration de cette current action, il faut attendre que le basculement en EditMode soit acté
            MainLoop.instance.StartCoroutine(delayAddCurrentAction(act.action, act.agent));
        for (int i = 0; i < f_forControls.Count && i < save.rawSave.currentLoopParams.Count; i++)
        {
            ForControl fc = f_forControls.getAt(i).GetComponent<ForControl>();
            fc.currentFor = save.rawSave.currentLoopParams[i].currentFor;
            fc.nbFor = save.rawSave.currentLoopParams[i].nbFor;
            ScrollRect parentScrollRect = fc.GetComponentInParent<ScrollRect>();
            if (parentScrollRect != null)
            {
                LinkedWith lw = parentScrollRect.transform.parent.GetComponentInChildren<LinkedWith>();
                if (lw != null) // "for" of a drone in the executable panel
                    fc.GetComponentInChildren<TMP_InputField>(true).text = fc.currentFor + " / " + fc.nbFor.ToString();
                else // "for" in an editable panel
                    fc.GetComponentInChildren<TMP_InputField>(true).text = fc.nbFor.ToString();
            }
        }

        // If amount enabled, reduce by 1
        if (playButtonAmount.activeSelf)
        {
            TMP_Text amountText = playButtonAmount.GetComponentInChildren<TMP_Text>();
            amountText.text = "" + (int.Parse(amountText.text) + 1);
        }
    }

    private IEnumerator delayAddCurrentAction(GameObject nextAction, GameObject agent)
    {
        yield return null; // on restaure une CurrentAction mais on ne veut pas l'exécuter car on bascule en EditMode, donc on attend d'être bien en EditMode avant d'ajouter les CurrentActions
        GameObjectManager.addComponent<CurrentAction>(nextAction, new { agent = agent });
    }
}
