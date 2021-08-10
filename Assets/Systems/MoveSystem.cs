using UnityEngine;
using FYFY;

/// <summary>
/// Manage position and Direction component to move agent accordingly
/// </summary>
public class MoveSystem : FSystem {

	private float turnSpeed = 150f;
	private float moveSpeed = 7f;
	private Family f_movable = FamilyManager.getFamily(new AllOfComponents(typeof(Position),typeof(Direction)));
	private bool isMoving;

	public MoveSystem()
	{
		if (Application.isPlaying)
		{
			foreach (GameObject go in f_movable)
				initAgentDirection(go);
			f_movable.addEntryCallback(initAgentDirection);
		}
    }

	private void initAgentDirection(GameObject agent)
    {
		switch (agent.GetComponent<Direction>().direction)
		{
			case Direction.Dir.East:
				agent.transform.rotation = Quaternion.Euler(0, 90, 0);
				break;
			case Direction.Dir.North:
				agent.transform.rotation = Quaternion.Euler(0, 0, 0);
				break;
			case Direction.Dir.South:
				agent.transform.rotation = Quaternion.Euler(0, 180, 0);
				break;
			case Direction.Dir.West:
				agent.transform.rotation = Quaternion.Euler(0, -90, 0);
				break;
		}
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {

		foreach( GameObject go in f_movable){
			isMoving = false; 
			// Manage position
			if(go.transform.localPosition.x/3 != go.GetComponent<Position>().x || go.transform.localPosition.z/3 != go.GetComponent<Position>().z ||
			 go.GetComponent<Position>().animate){
				if(go.GetComponent<Animator>()){
					go.GetComponent<Animator>().SetFloat("Run", 1f);	
				}
				go.GetComponent<Position>().animate = false;
				isMoving = true;
				
				go.transform.localPosition = Vector3.MoveTowards(go.transform.localPosition, new Vector3(go.GetComponent<Position>().x*3,go.transform.localPosition.y,go.GetComponent<Position>().z*3), moveSpeed* Time.deltaTime);
			}
			else{
				if(go.GetComponent<Animator>())
					go.GetComponent<Animator>().SetFloat("Run", -1f);
			}

			// Manage orientation
			Quaternion target = Quaternion.Euler(0, 0, 0);
			switch(go.GetComponent<Direction>().direction){
				case Direction.Dir.East:
					target = Quaternion.Euler(0, 90, 0);
					break;
				case Direction.Dir.North:
					target = Quaternion.Euler(0, 0, 0);
					break;
				case Direction.Dir.South:
					target = Quaternion.Euler(0, 180, 0);
					break;
				case Direction.Dir.West:
					target = Quaternion.Euler(0, -90, 0);
					break;
			}
			if(target.eulerAngles.y != go.transform.eulerAngles.y){
				go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, target, turnSpeed*Time.deltaTime);
				if(go.GetComponent<Animator>())
					go.GetComponent<Animator>().SetFloat("Turn", 1f);
				isMoving = true;
				
			}
			else{
				if(go.GetComponent<Animator>())
					go.GetComponent<Animator>().SetFloat("Turn", -1f);
			}

			AudioSource audio = go.GetComponent<AudioSource>(); // not included into family because red detector has no audio source
			if(audio != null){
				if(isMoving){
					if(!audio.isPlaying)
						audio.Play();
				}
				else{
					audio.Stop();
				}
			}

		}
	}
}