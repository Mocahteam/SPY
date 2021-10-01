using UnityEngine;
using FYFY;
using System.Collections;

/// <summary>
/// Manage Doors and Consoles => open/close doors depending on consoles state
/// </summary>
public class DoorManager : FSystem {

	private Family doorGO = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
	private Family f_consoleOn = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(AudioSource), typeof(TurnedOn)));
	private Family f_consoleOff = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(AudioSource)), new NoneOfComponents(typeof(TurnedOn)));

	public DoorManager()
	{
		if (Application.isPlaying)
		{
			f_consoleOn.addEntryCallback(onNewConsoleTurnedOn); // Console will enter in this family when TurnedOn component will be added to console (see CurrentActionExecutor)
			f_consoleOff.addEntryCallback(onNewConsoleTurnedOff); // Console will enter in this family when TurnedOn component will be removed from console (see CurrentActionExecutor)
		}
	}

	private void onNewConsoleTurnedOn(GameObject consoleGO)
    {
		Activable activable = consoleGO.GetComponent<Activable>();
		// parse all slot controled by this console
		foreach (int id in activable.slotID)
		{
			// parse all doors
			foreach (GameObject slotGo in doorGO)
			{
				// if slots are equals => disable door
				if (slotGo.GetComponent<ActivationSlot>().slotID == id)
				{
					// hide door
					slotGo.GetComponent<Renderer>().enabled = false;
					slotGo.GetComponent<AudioSource>().Play();
					GameObjectManager.setGameObjectState(slotGo, false);
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
			foreach (GameObject slotGo in doorGO)
			{
				// if slots are equals => disable door
				if (slotGo.GetComponent<ActivationSlot>().slotID == id)
				{
					// display door
					slotGo.GetComponent<Renderer>().enabled = true;
					slotGo.GetComponent<AudioSource>().Play();
					GameObjectManager.setGameObjectState(slotGo, true);
				}
			}
		}
	}
}