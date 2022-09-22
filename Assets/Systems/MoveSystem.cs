using UnityEngine;
using FYFY;

/// <summary>
/// Manage position and Direction component to move agent accordingly
/// </summary>
public class MoveSystem : FSystem {

	private Family f_movable = FamilyManager.getFamily(new AllOfComponents(typeof(Position),typeof(Direction)));

	private bool isWalking;
	private bool isRotating;
	public float turnSpeed;
	public float moveSpeed;
	public AudioClip footSlow;
	public AudioClip footSpeed;
	private GameData gameData;

	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();
		foreach (GameObject movable in f_movable)
			initAgentDirection(movable);
		f_movable.addEntryCallback(initAgentDirection);
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

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {

		foreach( GameObject go in f_movable){
			isWalking = false;
			isRotating = false;
			// Manage position
			if (go.transform.localPosition.z / 3 != go.GetComponent<Position>().x || go.transform.localPosition.x / 3 != go.GetComponent<Position>().y)
			{
				isWalking = true;

				go.transform.localPosition = Vector3.MoveTowards(go.transform.localPosition, new Vector3(go.GetComponent<Position>().y * 3, go.transform.localPosition.y, go.GetComponent<Position>().x * 3), moveSpeed * gameData.gameSpeed_current * Time.deltaTime);
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
				isRotating = true;
			}

			AudioSource audio = go.GetComponent<AudioSource>(); // not included into family because red detector has no audio source
			if (audio != null)
			{
				if (isWalking || isRotating)
				{
					if (!audio.isPlaying)
					{
						if (gameData.gameSpeed_current == gameData.gameSpeed_default)
							audio.clip = footSlow;
						else
							audio.clip = footSpeed;
						audio.Play();
					}
				} else
					audio.Stop();

				if (!isWalking)
				{
					// Stop animation
					if (go.GetComponent<Animator>() && go.tag == "Player")
					{
						go.GetComponent<Animator>().SetFloat("Run", -1f);
						go.GetComponent<Animator>().SetFloat("Walk", -1f);
					}
				}
				if (!isRotating)
				{
					// Stop animation
					if (go.GetComponent<Animator>() && go.tag == "Player")
					{
						go.GetComponent<Animator>().SetFloat("Rotate", -1f);
					}
				}
			}
		}
	}
}