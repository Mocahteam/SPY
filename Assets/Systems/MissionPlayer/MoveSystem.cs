using UnityEngine;
using FYFY;
using FYFY_plugins.CollisionManager;
using System.Collections;

/// <summary>
/// Manage position and Direction component to move agent accordingly
/// </summary>
public class MoveSystem : FSystem {

	private Family f_movable = FamilyManager.getFamily(new AllOfComponents(typeof(Position),typeof(Direction)));
	private Family f_end = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
	private Family f_positionCorrected = FamilyManager.getFamily(new AllOfComponents(typeof(PositionCorrected)));

	public float turnSpeed;
	public float moveSpeed;
	public AudioClip footSlow;
	public AudioClip footSpeed;
	private GameData gameData;

	public static MoveSystem instance;

	public MoveSystem()
    {
		instance = this;
    }

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();
		foreach (GameObject movable in f_movable)
			initAgentDirection(movable);
		f_movable.addEntryCallback(initAgentDirection);
		f_end.addEntryCallback(onNewEnd);

		// Lorsqu'on reçoit du CurrentActionExecutor la notif comme quoi les positions ont été corrigées, on lance le onProcess pour lancer les animation
		f_positionCorrected.addEntryCallback(delegate { Pause = false; });

		Pause = true;
	}

	private void onNewEnd(GameObject end)
    {
		// En cas de victoire
		if (end.GetComponent<NewEnd>().endType == NewEnd.Win)
		{
			foreach (GameObject go in f_movable)
			{
				if (go.GetComponent<Animator>() && go.CompareTag("Player"))
					go.GetComponent<Animator>().SetInteger("Danse", Mathf.FloorToInt(Random.Range(1, 11)));
			}
		}
		// for other end type, nothing to do more
	}

	private void initAgentDirection(GameObject agent)
    {
		switch (agent.GetComponent<Direction>().direction)
		{
			case Direction.Dir.North:
				agent.transform.rotation = Quaternion.Euler(0, -90, 0);
				break;
			case Direction.Dir.East:
				agent.transform.rotation = Quaternion.Euler(0, 0, 0);
				break;
			case Direction.Dir.West:
				agent.transform.rotation = Quaternion.Euler(0, 180, 0);
				break;
			case Direction.Dir.South:
				agent.transform.rotation = Quaternion.Euler(0, 90, 0);
				break;
		}
	}

	private int verticalStrength(Position pos)
    {
		if (pos.transform.localPosition.x - (pos.y * 3) > 0.01f)
			return 1;
		else if (pos.transform.localPosition.x - (pos.y * 3) < -0.01f)
			return -1;
		else
			return 0;
	}

	private int horizontalStrength(Position pos)
	{
		if (pos.transform.localPosition.z - (pos.x * 3) > 0.01f)
			return 1;
		else if (pos.transform.localPosition.z - (pos.x * 3) < -0.01f)
			return -1;
		else
			return 0;
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		int movingCpt = 0;
		foreach (GameObject go in f_movable)
		{
			// Manage position
			Position pos = go.GetComponent<Position>();
			if (Mathf.Abs(go.transform.localPosition.z / 3 - go.GetComponent<Position>().x) > 0.01f || Mathf.Abs(go.transform.localPosition.x / 3 - go.GetComponent<Position>().y) > 0.01f)
			{
				// calcul de la position de départ
				Vector3 startPosition = new Vector3(3 * (pos.y + verticalStrength(pos)), go.transform.localPosition.y, 3 * (pos.x + horizontalStrength(pos)));
				go.transform.localPosition = Vector3.MoveTowards(startPosition, new Vector3(pos.y * 3, go.transform.localPosition.y, pos.x * 3), moveSpeed * gameData.gameSpeed_current * Mathf.Min(1f / gameData.gameSpeed_current, Time.time - gameData.startStepTime));
				if (go.GetComponent<Animator>() && go.CompareTag("Player"))
                {
					if (gameData.gameSpeed_current == gameData.gameSpeed_default)
					{
						go.GetComponent<Animator>().SetFloat("Walk", 1f);
						go.GetComponent<Animator>().SetFloat("Run", -1f);
					}
					else
					{
						go.GetComponent<Animator>().SetFloat("Walk", -1f);
						go.GetComponent<Animator>().SetFloat("Run", 1f);
					}
					go.GetComponent<ScriptRef>().StopAllCoroutines();
				}
				movingCpt++;
			}
			else
			{
				// Position atteinte
				go.transform.localPosition = new Vector3(pos.y * 3, go.transform.localPosition.y, pos.x * 3);
				if (go.GetComponent<Animator>() && go.CompareTag("Player"))
				{
					// On stope l'animation dans une coroutine pour éviter de voir le robot faire une micro pause entre deux steps, ainsi le onProcess stoppera cette coroutine si l'animation doit continuer
					go.GetComponent<ScriptRef>().StartCoroutine(delayStopAnim(go.GetComponent<Animator>()));
				}
			}

			// Manage orientation
			Quaternion target = Quaternion.Euler(0, 0, 0);
			switch (go.GetComponent<Direction>().direction)
			{
				case Direction.Dir.North:
					target = Quaternion.Euler(0, -90, 0);
					break;
				case Direction.Dir.East:
					target = Quaternion.Euler(0, 0, 0);
					break;
				case Direction.Dir.West:
					target = Quaternion.Euler(0, 180, 0);
					break;
				case Direction.Dir.South:
					target = Quaternion.Euler(0, 90, 0);
					break;
			}
			if (target.eulerAngles.y != go.transform.eulerAngles.y)
			{
				go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, target, turnSpeed * gameData.gameSpeed_current * Time.deltaTime);
				if (go.GetComponent<Animator>() && go.CompareTag("Player"))
					go.GetComponent<Animator>().SetFloat("Rotate", 1f);
				movingCpt++;
			}
			else
			{
				go.transform.rotation = target;
				if (go.GetComponent<Animator>() && go.CompareTag("Player"))
					go.GetComponent<Animator>().SetFloat("Rotate", -1f);
			}
		}
		if (movingCpt == 0)
			Pause = true;
	}

	public  void syncAnimations()
	{
		foreach (GameObject go in f_movable)
			if (go.GetComponent<Animator>() && go.GetComponent<ScriptRef>() && !go.GetComponent<ScriptRef>().isBroken)
			{
				go.GetComponent<Animator>().SetTrigger("Idle");
				go.GetComponent<Collider>().enabled = true;
			}
	}

	private IEnumerator delayStopAnim(Animator anim)
    {
		// On laisse passé quelques frames avant de stopper l'animation pour laisser le temps de stopper cette coroutine si l'animation doit continuer
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		Debug.Log("Stop animations");
		anim.SetFloat("Walk", -1f);
		anim.SetFloat("Run", -1f);
	}
}