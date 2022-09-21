using UnityEngine;
using FYFY;
using System.Collections;

/// <summary>
/// Manage Doors and Consoles => open/close doors depending on consoles state
/// </summary>
public class DoorAndConsoleManager : FSystem {

	private Family f_door = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
	private Family f_consoleOn = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(AudioSource), typeof(TurnedOn)));
	private Family f_consoleOff = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(AudioSource)), new NoneOfComponents(typeof(TurnedOn)));

	private GameData gameData;

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();
		f_consoleOn.addEntryCallback(onNewConsoleTurnedOn); // Console will enter in this family when TurnedOn component will be added to console (see CurrentActionExecutor)
		f_consoleOff.addEntryCallback(onNewConsoleTurnedOff); // Console will enter in this family when TurnedOn component will be removed from console (see CurrentActionExecutor)
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
				// if slots are equals => disable door
				if (slotGo.GetComponent<ActivationSlot>().slotID == id)
				{
					// hide door
					slotGo.transform.parent.GetComponent<AudioSource>().Play();
					slotGo.transform.parent.GetComponent<Animator>().SetTrigger("Open");
					slotGo.transform.parent.GetComponent<Animator>().speed = gameData.gameSpeed_current;
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
					// display door
					slotGo.transform.parent.GetComponent<AudioSource>().Play();
					slotGo.transform.parent.GetComponent<Animator>().SetTrigger("Close");
					slotGo.transform.parent.GetComponent<Animator>().speed = gameData.gameSpeed_current;
				}
			}
		}
	}
}