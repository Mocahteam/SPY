using UnityEngine;
using FYFY;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manage Doors and Consoles => open/close doors depending on consoles state
/// </summary>
public class DoorAndConsoleManager : FSystem {

	private Family f_door = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
	private Family f_console = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position)));
	private Family f_consoleTriggered = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(AudioSource), typeof(Triggered)));
	private Family f_doorPath = FamilyManager.getFamily(new AllOfComponents(typeof(DoorPath)));

	private Family f_gameLoaded = FamilyManager.getFamily(new AllOfComponents(typeof(GameLoaded)));

	private Family f_wall = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	private GameData gameData;

	public GameObject LevelGO;
	public GameObject doorPathPrefab;


	public static DoorAndConsoleManager instance;

	public DoorAndConsoleManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();
		f_consoleTriggered.addEntryCallback(onNewConsoleTriggered); // Console will enter in this family when Triggered component will be added to console (see CurrentActionExecutor)
		f_gameLoaded.addEntryCallback(connectDoorsAndConsoles);
		forceDoorSync();
		f_door.addEntryCallback(syncState);
	}

	private IEnumerator animatePath(DoorPath path)
	{
		List<DoorPath> firstChilds = path.nexts;
		if (firstChilds.Count > 0) {
			yield return new WaitForSeconds(Random.Range(0, 10));
			while (true)
			{
				yield return new WaitForSeconds(5);
				foreach (DoorPath child in firstChilds)
				{
					Animator[] anims = child.GetComponentsInChildren<Animator>();
					foreach (Animator anim in anims)
						anim.SetTrigger("Play");
				}
			}
		}
	}

	public void startNextPathAnimation(GameObject pathGO)
	{
		DoorPath path = pathGO.GetComponentInParent<DoorPath>();
		foreach (DoorPath next in path.nexts)
			foreach (Animator anim in next.GetComponentsInChildren<Animator>())
				anim.SetTrigger("Play");
	}

	// Used in StopButton and ReloadState buttons in editor
	public void forceDoorSync()
    {
		foreach (GameObject door in f_door)
			syncState(door);
	}

	private void syncState(GameObject door)
    {
		Animator anim = door.GetComponent<Animator>();
		anim.speed = gameData.gameSpeed_current;
		if (door.GetComponent<ActivationSlot>().state)
			anim.SetTrigger("Open");
        else
			anim.SetTrigger("Close");
    }

	private void onNewConsoleTriggered(GameObject consoleGO)
    {
		Activable activable = consoleGO.GetComponent<Activable>();
		consoleGO.GetComponent<AudioSource>().Play();
		// parse all targets controled by this console
		foreach (ActivationSlot door in activable.targets)
		{
			door.GetComponent<AudioSource>().Play();
			door.state = !door.state;
			syncState(door.gameObject);
		}
		GameObjectManager.removeComponent<Triggered>(consoleGO);
	}

	private DoorPath connectPath(bool isWallInPos, bool isWallInStep, DoorPath previousPath, Transform begin, Transform end)
    {
		// Si on est entre deux murs
		if (isWallInPos && isWallInStep)
		{
			// Ne pas afficher les tronçons verticaux
			begin.Find("onWall").gameObject.SetActive(false);
			end.Find("onWall").gameObject.SetActive(false);
			DoorPath beginPath = begin.Find("Path").GetComponent<DoorPath>();
			DoorPath endPath = end.Find("Path").GetComponent<DoorPath>();
			// Connecter les paths
			previousPath.nexts.Add(beginPath);
			beginPath.nexts.Add(endPath);
			previousPath = endPath;
		}
		// Si on descend d'un mur
		else if (isWallInPos)
		{
			DoorPath beginPath = begin.Find("Path").GetComponent<DoorPath>();
			DoorPath middlePath = begin.Find("onWall").GetComponent<DoorPath>();
			DoorPath endPath = end.Find("Path").GetComponent<DoorPath>();
			// Connecter les paths
			previousPath.nexts.Add(beginPath);
			beginPath.nexts.Add(middlePath);
			middlePath.nexts.Add(endPath);
			previousPath = endPath;
		}
		// Si on monte sur un mur
		else if (isWallInStep)
		{
			DoorPath beginPath = begin.Find("Path").GetComponent<DoorPath>();
			DoorPath middlePath = end.Find("onWall").GetComponent<DoorPath>();
			DoorPath endPath = end.Find("Path").GetComponent<DoorPath>();
			// Connecter les paths
			previousPath.nexts.Add(beginPath);
			beginPath.nexts.Add(middlePath);
			middlePath.nexts.Add(endPath);
			previousPath = endPath;
		}
		// Si on reste sur le sol
		else
		{
			DoorPath beginPath = begin.Find("Path").GetComponent<DoorPath>();
			DoorPath endPath = end.Find("Path").GetComponent<DoorPath>();
			// Connecter les paths
			previousPath.nexts.Add(beginPath);
			beginPath.nexts.Add(endPath);
			previousPath = endPath;
		}
		return previousPath;
	}

	private void connectDoorsAndConsoles(GameObject unused)
    {
		// Hook doors to consoles
		foreach (GameObject console in f_console) {
			Activable act = console.GetComponent<Activable>();
			foreach (GameObject door in f_door)
				if (act.slotID.Contains(door.GetComponent<ActivationSlot>().slotID))
					act.targets.Add(door.GetComponent<ActivationSlot>());
		}

		// Parse all consoles
		foreach (GameObject console in f_console)
		{
			Color randomColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
			Activable consoleSlots = console.GetComponent<Activable>();
			SpriteRenderer[] srs = console.GetComponentsInChildren<SpriteRenderer>();
			foreach (SpriteRenderer sr in srs)
				sr.color = randomColor;
			// Parse all targets
			foreach(ActivationSlot doorSlot in consoleSlots.targets)
			{
				DoorPath previousPath = console.GetComponentInChildren<DoorPath>();
				Position doorPos = doorSlot.GetComponent<Position>();
				// Connect this console with this door
				Position consolePos = console.GetComponent<Position>();
				int xStep = consolePos.x < doorPos.x ? 1 : (consolePos.x == doorPos.x ? 0 : -1);
				int yStep = consolePos.y < doorPos.y ? 1 : (consolePos.y == doorPos.y ? 0 : -1);
				// Commencer par créer un chemin en abscisse
				int x = 0;
				while (consolePos.x + x != doorPos.x)
				{
					GameObject path = Object.Instantiate<GameObject>(doorPathPrefab, LevelGO.transform.position + new Vector3(consolePos.y * 3, 3, (consolePos.x + x + xStep / 2f) * 3), Quaternion.Euler(0, 0, 0), LevelGO.transform);
					bool isWallInPos = isWall(consolePos.x + x, consolePos.y);
					bool isWallInStep = isWall(consolePos.x + x + xStep, consolePos.y);

					Transform begin = null;
					Transform end = null;
					// Si on va d'Ouest en Est
					if (xStep > 0)
                    {
						// S'il y a un mur sur le premier tronçon (d'Ouest au centre)
						if (isWallInPos)
							begin = path.transform.Find("WestUp"); // prendre le segment haut
						else
							begin = path.transform.Find("West"); // prendre le segent bas
						
						// S'il y a un mur sur le second tronçon (du centre à l'Est)
						if (isWallInStep)
							end = path.transform.Find("EastUp"); // prendre le segment haut
						else
							end = path.transform.Find("East"); // prendre le segent bas
						
						// Les tronçons sont déjà bien orienté, rien de plus à faire de ce côté là
					}
					// Si on va d'Est en Ouest
					else if (xStep < 0)
					{
						// S'il y a un mur sur le premier tronçon (d'Est au centre)
						if (isWallInPos)
						{
							begin = path.transform.Find("EastUp"); // prendre le segment haut
							begin.Find("onWall").Rotate(0.0f, 0.0f, 180.0f, Space.Self);
						}
						else
							begin = path.transform.Find("East"); // prendre le segent bas
						begin.Find("Path").Rotate(0.0f, 0.0f, 180.0f, Space.Self);

						// S'il y a un mur sur le second tronçon (du centre à l'Ouest)
						if (isWallInStep)
						{
							end = path.transform.Find("WestUp"); // prendre le segment haut
							end.Find("onWall").Rotate(0.0f, 0.0f, 180.0f, Space.Self);
						}
						else
							end = path.transform.Find("West"); // prendre le segent bas
						end.Find("Path").Rotate(0.0f, 0.0f, 180.0f, Space.Self);
					}
					begin.gameObject.SetActive(true);
					end.gameObject.SetActive(true);

					previousPath = connectPath(isWallInPos, isWallInStep, previousPath, begin, end);

					foreach (SpriteRenderer sr in path.GetComponentsInChildren<SpriteRenderer>())
						sr.color = randomColor;
					GameObjectManager.bind(path);
					x += xStep;
				}

				// Finir par créer le chemin en ordonnée
				int y = 0;
				while (consolePos.y + y != doorPos.y)
				{
					GameObject path = Object.Instantiate<GameObject>(doorPathPrefab, LevelGO.transform.position + new Vector3((consolePos.y + y + yStep / 2f) * 3, 3, (consolePos.x + x) * 3), Quaternion.Euler(0, 0, 0), LevelGO.transform);
					bool isWallInPos = isWall(consolePos.x + x, consolePos.y + y);
					bool isWallInStep = isWall(consolePos.x + x, consolePos.y + y + yStep);

					Transform begin = null;
					Transform end = null;
					// Si on va du Sud au Nord (Attention origine en haut à gauche)
					if (yStep < 0)
					{
						// S'il y a un mur sur le premier tronçon (du Sud au centre)
						if (isWallInPos)
							begin = path.transform.Find("SouthUp"); // prendre le segment haut
						else
							begin = path.transform.Find("South"); // prendre le segent bas

						// S'il y a un mur sur le second tronçon (du centre au Nord)
						if (isWallInStep)
							end = path.transform.Find("NorthUp"); // prendre le segment haut
						else
							end = path.transform.Find("North"); // prendre le segent bas
						
						// Les tronçons sont déjà bien orienté, rien de plus à faire de ce côté là
					}
					// Si on va du Nord au Sud (Attention origine en haut à gauche)
					else if (yStep > 0)
					{
						// S'il y a un mur sur le premier tronçon (du Nord au centre)
						if (isWallInPos)
						{
							begin = path.transform.Find("NorthUp"); // prendre le segment haut
							begin.Find("onWall").Rotate(0.0f, 0.0f, 180.0f, Space.Self);
						}
						else
							begin = path.transform.Find("North"); // prendre le segent bas
						begin.Find("Path").Rotate(0.0f, 0.0f, 180.0f, Space.Self);

						// S'il y a un mur sur le second tronçon (du centre au Sud)
						if (isWallInStep)
						{
							end = path.transform.Find("SouthUp"); // prendre le segment haut
							end.Find("onWall").Rotate(0.0f, 0.0f, 180.0f, Space.Self);
						}
						else
							end = path.transform.Find("South"); // prendre le segent bas
						end.Find("Path").Rotate(0.0f, 0.0f, 180.0f, Space.Self);
					}
					begin.gameObject.SetActive(true);
					end.gameObject.SetActive(true);

					previousPath = connectPath(isWallInPos, isWallInStep, previousPath, begin, end);

					foreach (SpriteRenderer sr in path.GetComponentsInChildren<SpriteRenderer>())
						sr.color = randomColor;
					GameObjectManager.bind(path);
					y += yStep;
				}
            }
		}
		// Start animations
		foreach (GameObject console in f_console)
		{
			DoorPath path = console.GetComponentInChildren<DoorPath>();
			path.StartCoroutine(animatePath(path));
		}
	}

	private bool isWall(float x, float y)
    {
		foreach(GameObject wall in f_wall)
        {
			Position posWall = wall.GetComponent<Position>();
			if (posWall.x == x && posWall.y == y)
				return true;
        }
		return false;
    }
}