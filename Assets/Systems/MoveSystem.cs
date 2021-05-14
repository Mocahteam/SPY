using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;

public class MoveSystem : FSystem {

	private float turnSpeed = 150f;
	private float moveSpeed = 7f;
	private Family agents = FamilyManager.getFamily(new AllOfComponents(typeof(Position),typeof(Direction), typeof(Animator), typeof(AudioSource)));
	private GameData gameData;
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	public MoveSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();

        foreach (GameObject go in agents)
        {
            switch (go.GetComponent<Direction>().direction)
            {
                case Direction.Dir.East:
                    go.transform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
                case Direction.Dir.North:
                    go.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case Direction.Dir.South:
                    go.transform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case Direction.Dir.West:
                    go.transform.rotation = Quaternion.Euler(0, -90, 0);
                    break;
            }
        }
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {

		foreach( GameObject go in agents){

			bool isMoving = false;

			if(go.transform.localPosition.x/3 != go.GetComponent<Position>().x || go.transform.localPosition.z/3 != go.GetComponent<Position>().z){
				if(go.GetComponent<Animator>())
					go.GetComponent<Animator>().SetFloat("Run", 1f);
				isMoving = true;
				
				go.transform.localPosition = Vector3.MoveTowards(go.transform.localPosition, new Vector3(go.GetComponent<Position>().x*3,go.transform.localPosition.y,go.GetComponent<Position>().z*3), moveSpeed* Time.deltaTime);
			}
			else{
				if(go.GetComponent<Animator>())
					go.GetComponent<Animator>().SetFloat("Run", -1f);
			}

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
			//Debug.Log("target " + target.eulerAngles.y);
			//Debug.Log("player " +go.transform.eulerAngles.y);
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

			if(isMoving){
				if(!go.GetComponent<AudioSource>().isPlaying)
					go.GetComponent<AudioSource>().Play();
			}
			else{
				go.GetComponent<AudioSource>().Stop();
			}
		}
	}
}