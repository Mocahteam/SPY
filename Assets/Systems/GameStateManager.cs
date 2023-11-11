using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// This manager enables to save the game state and to restore it on demand for instance when the player is detected by drones, he can reset the game on a state just before the previous execution
/// </summary>
public class GameStateManager : FSystem {

    private Family f_coins = FamilyManager.getFamily(new AnyOfTags("Coin"));
    private Family f_directions = FamilyManager.getFamily(new AllOfComponents(typeof(Direction)), new NoneOfComponents(typeof(Detector)));
    private Family f_positions = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new NoneOfComponents(typeof(Detector)));
    private Family f_activables = FamilyManager.getFamily(new AllOfComponents(typeof(Activable)));
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
        save.rawSave.activables.Clear();
        foreach (GameObject act in f_activables)
            save.rawSave.activables.Add(new SaveContent.RawActivable(act.GetComponent<Activable>()));
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
        for (int i = 0; i < f_directions.Count && i < save.rawSave.directions.Count ; i++)
            f_directions.getAt(i).GetComponent<Direction>().direction = save.rawSave.directions[i];
        for (int i = 0; i < f_positions.Count && i < save.rawSave.positions.Count ; i++)
        {
            Position pos = f_positions.getAt(i).GetComponent<Position>();
            pos.x = save.rawSave.positions[i].x;
            pos.y = save.rawSave.positions[i].y;
            // Téléport object to the right position
            pos.transform.position = level.transform.position + new Vector3(pos.y * 3, pos.transform.position.y - level.transform.position.y, pos.x * 3);
        }
        for (int i = 0; i < f_activables.Count && i < save.rawSave.activables.Count; i++)
        {
            Activable act = f_activables.getAt(i).GetComponent<Activable>();
            act.slotID = save.rawSave.activables[i].slotID;
            if (save.rawSave.activables[i].state)
            {
                if (act.GetComponent<TurnedOn>() == null)
                    GameObjectManager.addComponent<TurnedOn>(act.gameObject);
            }
            else
            {
                if (act.GetComponent<TurnedOn>() != null)
                    GameObjectManager.removeComponent<TurnedOn>(act.gameObject);
            }
        }
        foreach(GameObject go in f_currentActions)
            if(go.GetComponent<CurrentAction>().agent.CompareTag("Drone"))
                GameObjectManager.removeComponent<CurrentAction>(go);
        foreach(SaveContent.RawCurrentAction act in save.rawSave.currentDroneActions)
            GameObjectManager.addComponent<CurrentAction>(act.action, new{agent = act.agent});
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
                    fc.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = fc.currentFor + " / " + fc.nbFor.ToString();
                else // "for" in an editable panel
                    fc.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = fc.nbFor.ToString();
            }
        }

        // If amount enabled, reduce by 1
        if (playButtonAmount.activeSelf)
        {
            TMP_Text amountText = playButtonAmount.GetComponentInChildren<TMP_Text>();
            amountText.text = "" + (int.Parse(amountText.text) + 1);
        }
    }
}
