using UnityEngine;
using FYFY;
using System.Collections.Generic;

/// <summary>
/// This system executes new currentActions
/// </summary>
public class CurrentActionExecutor : FSystem {
	private Family f_obstacles = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall", "Furniture", "Door"));
	private Family f_activableConsole = FamilyManager.getFamily(new AllOfComponents(typeof(Activable),typeof(Position),typeof(AudioSource)));
    private Family f_newCurrentAction = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction), typeof(BasicAction)));
	private Family f_agent = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)));
	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family f_positionCorrected = FamilyManager.getFamily(new AllOfComponents(typeof(PositionCorrected)));

	protected override void onStart()
	{
		f_newCurrentAction.addEntryCallback(onNewCurrentAction);
		f_positionCorrected.addEntryCallback(delegate (GameObject go) { GameObjectManager.removeComponent<PositionCorrected>(go); });
		Pause = true;
	}

	protected override void onProcess(int familiesUpdateCount)
	{
		List<GameObject> agentWillCollide = new List<GameObject>();
		foreach (GameObject agent in f_agent)
		{
			// count inaction if a robot have no CurrentAction
			if (agent.CompareTag("Player") && agent.GetComponent<ScriptRef>().executableScript.GetComponentInChildren<CurrentAction>(true) == null)
				agent.GetComponent<ScriptRef>().nbOfInactions++;
			// Predict if collision will occurs between agent to adapt target ccordinates
			bool predictCollision = false;
			foreach (GameObject agent2 in f_agent) {
				if (agent != agent2 && agent.tag == agent2.tag)
				{
					Position r1Pos = agent.GetComponent<Position>();
					Position r2Pos = agent2.GetComponent<Position>();
					// check if the two robots move on the same position => forbiden
					predictCollision = (r2Pos.targetX != -1 && r2Pos.targetY != -1 && r1Pos.targetX == r2Pos.targetX && r1Pos.targetY == r2Pos.targetY) ||
						// one robot doesn't move and the other try to move on its position => forbiden
						(r2Pos.targetX == -1 && r2Pos.targetY == -1 && r1Pos.targetX == r2Pos.x && r1Pos.targetY == r2Pos.y) ||
						// the two robot want to exchange their position => forbiden
						(r1Pos.targetX == r2Pos.x && r1Pos.targetY == r2Pos.y && r1Pos.x == r2Pos.targetX && r1Pos.y == r2Pos.targetY);
					if (predictCollision)
						break;
				}
			}
			if (predictCollision)
				agentWillCollide.Add(agent);
		}

		// Reduce move if collision
		foreach(GameObject agent in agentWillCollide)
        {
			Position pos = agent.GetComponent<Position>();
			pos.targetX += pos.x == pos.targetX ? 0 : (pos.x < pos.targetX ? -0.15f : 0.15f);
			pos.targetY += pos.y == pos.targetY ? 0 : (pos.y < pos.targetY ? -0.15f : 0.15f);
		}

		// Record valid movements
		foreach (GameObject agent in f_agent)
		{
			Position pos = agent.GetComponent<Position>();
			if (pos.targetX != -1 && pos.targetY != -1)
			{
				pos.x = pos.targetX;
				pos.y = pos.targetY;
				pos.targetX = -1;
				pos.targetY = -1;
			}
		}
		// maintenant que les positions prennent en compte les futures collisions, lancer les systèmes dépendants
		GameObjectManager.addComponent<PositionCorrected>(MainLoop.instance.gameObject);
		Pause = true;
	}

	// each time a new currentAction is added, 
	private void onNewCurrentAction(GameObject currentAction) {
		// On ne traite les CurrentAction qu'en mode play
		if (f_playingMode.Count > 0)
		{
			Pause = false; // activates onProcess to manage future collisions and to count inactions

			CurrentAction ca = currentAction.GetComponent<CurrentAction>();

			// process action depending on action type
			switch (currentAction.GetComponent<BasicAction>().actionType)
			{
				case BasicAction.ActionType.Forward:
					ApplyForward(ca.agent);
					break;
				case BasicAction.ActionType.TurnLeft:
					ApplyTurnLeft(ca.agent);
					break;
				case BasicAction.ActionType.TurnRight:
					ApplyTurnRight(ca.agent);
					break;
				case BasicAction.ActionType.TurnBack:
					ApplyTurnBack(ca.agent);
					break;
				case BasicAction.ActionType.Wait:
					break;
				case BasicAction.ActionType.Activate:
					Position agentPos = ca.agent.GetComponent<Position>();
					foreach (GameObject actGo in f_activableConsole)
						if (actGo.GetComponent<Position>().x == agentPos.x && actGo.GetComponent<Position>().y == agentPos.y)
							GameObjectManager.addComponent<Triggered>(actGo);
					if (ca.agent.GetComponent<Animator>())
						ca.agent.GetComponent<Animator>().SetTrigger("Action");
					break;
			}
			ca.StopAllCoroutines();
			if (ca.gameObject.activeInHierarchy)
				ca.StartCoroutine(UtilityGame.pulseItem(ca.gameObject));
		}
	}

	private void ApplyForward(GameObject go){
		Position pos = go.GetComponent<Position>();
		switch (go.GetComponent<Direction>().direction){
			case Direction.Dir.North:
				pos.targetX = pos.x;
				pos.targetY = pos.y - 1;
				pos.targetY += checkObstacle(go, Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y) - 1);
				break;
			case Direction.Dir.South:
				pos.targetX = pos.x;
				pos.targetY = pos.y + 1;
				pos.targetY -= checkObstacle(go, Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y) + 1);
				break;
			case Direction.Dir.East:
				pos.targetX = pos.x + 1;
				pos.targetY = pos.y;
				pos.targetX -= checkObstacle(go, Mathf.RoundToInt(pos.x) + 1, Mathf.RoundToInt(pos.y));
				break;
			case Direction.Dir.West:
				pos.targetX = pos.x - 1;
				pos.targetY = pos.y;
				pos.targetX += checkObstacle(go, Mathf.RoundToInt(pos.x) - 1, Mathf.RoundToInt(pos.y));
				break;
		}
	}

	private void ApplyTurnLeft(GameObject go){
		switch (go.GetComponent<Direction>().direction){
			case Direction.Dir.North:
				go.GetComponent<Direction>().direction = Direction.Dir.West;
				break;
			case Direction.Dir.South:
				go.GetComponent<Direction>().direction = Direction.Dir.East;
				break;
			case Direction.Dir.East:
				go.GetComponent<Direction>().direction = Direction.Dir.North;
				break;
			case Direction.Dir.West:
				go.GetComponent<Direction>().direction = Direction.Dir.South;
				break;
		}
	}

	private void ApplyTurnRight(GameObject go){
		switch (go.GetComponent<Direction>().direction){
			case Direction.Dir.North:
				go.GetComponent<Direction>().direction = Direction.Dir.East;
				break;
			case Direction.Dir.South:
				go.GetComponent<Direction>().direction = Direction.Dir.West;
				break;
			case Direction.Dir.East:
				go.GetComponent<Direction>().direction = Direction.Dir.South;
				break;
			case Direction.Dir.West:
				go.GetComponent<Direction>().direction = Direction.Dir.North;
				break;
		}
	}

	private void ApplyTurnBack(GameObject go){
		switch (go.GetComponent<Direction>().direction){
			case Direction.Dir.North:
				go.GetComponent<Direction>().direction = Direction.Dir.South;
				break;
			case Direction.Dir.South:
				go.GetComponent<Direction>().direction = Direction.Dir.North;
				break;
			case Direction.Dir.East:
				go.GetComponent<Direction>().direction = Direction.Dir.West;
				break;
			case Direction.Dir.West:
				go.GetComponent<Direction>().direction = Direction.Dir.East;
				break;
		}
	}

	// Retourne la pénalité de distance à prendre en compte en fonction de la nature de l'obstacle:
	// No obstacles => 0
	// Wall => 0.51 (volontairement à 0.51 pour qu'avec l'arondi l'agent soit considéré sur la case avant le déplacement)
	// Furniture => player:0.2, drone:0
	// Door closed => 0
	private float checkObstacle(GameObject agent, int x, int z){
		foreach( GameObject obstacle in f_obstacles){
			if (obstacle.GetComponent<Position>().x == x && obstacle.GetComponent<Position>().y == z) {
				if (obstacle.CompareTag("Wall"))
					return 0.51f;
				else if (obstacle.CompareTag("Furniture"))
					return (agent.CompareTag("Player") ? 0.2f : 0f);
				// si c'est une porte vérifier si elle est ouverte
				else if (obstacle.CompareTag("Door") && !obstacle.GetComponent<ActivationSlot>().state)
					return 0f;
			}
		}
		return 0f;
	}
}
