using UnityEngine;
using FYFY;
using System.Collections;

/// <summary>
/// Manage Doors and Consoles => open/close doors depending on consoles state
/// </summary>
public class DoorAndConsoleManager : FSystem {

	private Family f_door = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
	private Family f_console = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position)));
	private Family f_consoleOn = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(AudioSource), typeof(TurnedOn)));
	private Family f_consoleOff = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(AudioSource)), new NoneOfComponents(typeof(TurnedOn)));
	private Family f_doorPath = FamilyManager.getFamily(new AllOfComponents(typeof(DoorPath)));

	private Family f_gameLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded)));

	private GameData gameData;

	public GameObject doorPathPrefab;
	public Color pathOn;
	public Color pathOff;

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();
		f_consoleOn.addEntryCallback(onNewConsoleTurnedOn); // Console will enter in this family when TurnedOn component will be added to console (see CurrentActionExecutor)
		f_consoleOff.addEntryCallback(onNewConsoleTurnedOff); // Console will enter in this family when TurnedOn component will be removed from console (see CurrentActionExecutor)
		f_gameLoaded.addEntryCallback(connectDoorsAndConsoles);
	}

	private void onNewConsoleTurnedOn(GameObject consoleGO)
    {
		Activable activable = consoleGO.GetComponent<Activable>();
		// parse all slot controled by this console
		foreach (int id in activable.slotID)
		{
			// parse all doors
			foreach (GameObject slotGo in f_door)
			{
				// if slots are equals => enable door
				if (slotGo.GetComponent<ActivationSlot>().slotID == id)
				{
					// display door
					slotGo.transform.parent.GetComponent<AudioSource>().Play();
					slotGo.transform.parent.GetComponent<Animator>().SetTrigger("Close");
					slotGo.transform.parent.GetComponent<Animator>().speed = gameData.gameSpeed_current;
					updatePathColor(id, true);
				}
			}
		}
	}

	private void onNewConsoleTurnedOff(GameObject consoleGO)
	{
		Activable activable = consoleGO.GetComponent<Activable>();
		// parse all slot controled by this console
		foreach (int id in activable.slotID)
		{
			// parse all doors
			foreach (GameObject slotGo in f_door)
			{
				// if slots are equals => disable door
				if (slotGo.GetComponent<ActivationSlot>().slotID == id)
				{
					// hide door
					slotGo.transform.parent.GetComponent<AudioSource>().Play();
					slotGo.transform.parent.GetComponent<Animator>().SetTrigger("Open");
					slotGo.transform.parent.GetComponent<Animator>().speed = gameData.gameSpeed_current;
					updatePathColor(id, false);
				}
			}
		}
	}

	private void updatePathColor(int slotId, bool state)
    {
		foreach(GameObject path in f_doorPath)
			if (path.GetComponent<DoorPath>().slotId == slotId)
				foreach (SpriteRenderer sr in path.GetComponentsInChildren<SpriteRenderer>())
					sr.color = state ? pathOn : pathOff;
    }

	private void connectDoorsAndConsoles(GameObject unused)
    {
		// Parse all doors
		foreach (GameObject door in f_door)
        {
			Position doorPos = door.GetComponent<Position>();
			ActivationSlot doorSlot = door.GetComponent<ActivationSlot>();
			// Parse all consoles
			foreach(GameObject console in f_console)
            {
				// Check if door is controlled by this console
				Activable consoleSlots = console.GetComponent<Activable>();
				bool isOn = console.GetComponent<TurnedOn>() != null;
				if (consoleSlots.slotID.Contains(doorSlot.slotID))
				{
					// Connect this console with this door
					Position consolePos = console.GetComponent<Position>();
					int xStep = consolePos.x < doorPos.x ? 1 : (consolePos.x == doorPos.x ? 0 : -1);
					int yStep = consolePos.y < doorPos.y ? 1 : (consolePos.y == doorPos.y ? 0 : -1);
					int x = 0;
					while (consolePos.x + x != doorPos.x)
					{
						GameObject path = Object.Instantiate<GameObject>(doorPathPrefab, gameData.LevelGO.transform.position + new Vector3(consolePos.y * 3, 3, (consolePos.x + x + xStep / 2f) * 3), Quaternion.Euler(0, 0, 0), gameData.LevelGO.transform);
						path.transform.Find("West").gameObject.SetActive(true);
						path.transform.Find("East").gameObject.SetActive(true);
						path.GetComponent<DoorPath>().slotId = doorSlot.slotID;
						foreach (SpriteRenderer sr in path.GetComponentsInChildren<SpriteRenderer>())
							sr.color = isOn ? pathOn : pathOff;
						GameObjectManager.bind(path);
						x = x + xStep;
					}
					int y = 0;
					while (consolePos.y + y != doorPos.y)
					{
						GameObject path = Object.Instantiate<GameObject>(doorPathPrefab, gameData.LevelGO.transform.position + new Vector3((consolePos.y + y + yStep / 2f) * 3, 3, (consolePos.x + x) * 3), Quaternion.Euler(0, 0, 0), gameData.LevelGO.transform);
						path.transform.Find("South").gameObject.SetActive(true);
						path.transform.Find("North").gameObject.SetActive(true);
						path.GetComponent<DoorPath>().slotId = doorSlot.slotID;
						foreach (SpriteRenderer sr in path.GetComponentsInChildren<SpriteRenderer>())
							sr.color = isOn ? pathOn : pathOff;
						GameObjectManager.bind(path);
						y = y + yStep;
					}
				}

            }
        }
    }
}