using UnityEngine;
using FYFY;

/// <summary>
/// Manage game state
/// </summary>
public class SaveManager : FSystem {

    private Family f_coins = FamilyManager.getFamily(new AnyOfTags("Coin"));
    private Family f_doors = FamilyManager.getFamily(new AnyOfTags("Door"));
    private Family f_directions = FamilyManager.getFamily(new AllOfComponents(typeof(Direction)), new NoneOfComponents(typeof(Detector)));
    private Family f_positions = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new NoneOfComponents(typeof(Detector)));
    private Family f_activables = FamilyManager.getFamily(new AllOfComponents(typeof(Activable)));
    private Family f_currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction)));
    public static SaveManager instance;

    private SaveContent save;

    private string currentContent;

    public SaveManager()
	{
        if (Application.isPlaying)
        {
            save = new SaveContent();
        }
		instance = this;
	}

    // See ExecuteButton in editor
    public void SaveState(GameObject buttonStop)
	{
        if(!buttonStop.activeInHierarchy){
            //reset save
            save.rawSave.coinsState.Clear();
            foreach (GameObject coin in f_coins)
                save.rawSave.coinsState.Add(coin.activeSelf);
            save.rawSave.doorsState.Clear();
            foreach (GameObject door in f_doors)
                save.rawSave.doorsState.Add(door.activeSelf);
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

            currentContent = JsonUtility.ToJson(save.rawSave);
        }

    }

    // See StopButton and ReloadState buttons in editor
    public void LoadState()
    {
        save.rawSave = JsonUtility.FromJson<SaveContent.RawSave>(currentContent);
        for (int i = 0; i < f_coins.Count && i < save.rawSave.coinsState.Count ; i++)
        {
            GameObjectManager.setGameObjectState(f_coins.getAt(i), save.rawSave.coinsState[i]);
            f_coins.getAt(i).GetComponent<Renderer>().enabled = save.rawSave.coinsState[i];
        }
        for (int i = 0; i < f_doors.Count && i < save.rawSave.doorsState.Count ; i++)
        {
            GameObjectManager.setGameObjectState(f_doors.getAt(i), save.rawSave.doorsState[i]);
            f_doors.getAt(i).GetComponent<Renderer>().enabled = save.rawSave.doorsState[i];
        }
        for (int i = 0; i < f_directions.Count && i < save.rawSave.directions.Count ; i++)
            f_directions.getAt(i).GetComponent<Direction>().direction = save.rawSave.directions[i];
        for (int i = 0; i < f_positions.Count && i < save.rawSave.positions.Count ; i++)
        {
            Position pos = f_positions.getAt(i).GetComponent<Position>();
            pos.x = save.rawSave.positions[i].x;
            pos.z = save.rawSave.positions[i].z;
        }
        for (int i = 0; i < f_activables.Count && i < save.rawSave.activables.Count; i++)
        {
            Activable act = f_activables.getAt(i).GetComponent<Activable>();
            act.slotID = save.rawSave.activables[i].slotID;
        }
        foreach(GameObject go in f_currentActions){
            if(go.GetComponent<CurrentAction>().agent.CompareTag("Drone")){
                GameObjectManager.removeComponent<CurrentAction>(go);
            }

        }
        foreach(SaveContent.RawCurrentAction act in save.rawSave.currentDroneActions){
            GameObjectManager.addComponent<CurrentAction>(act.action, new{agent = act.agent});
        }
    }
}
