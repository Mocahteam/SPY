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
						(r2Pos.targetX == -1 && r2Pos.targetY == -1 && r1Pos.targetX == r2Pos.startX && r1Pos.targetY == r2Pos.startY) ||
						// the two robot want to exchange their position => forbiden
						(r1Pos.targetX == r2Pos.startX && r1Pos.targetY == r2Pos.startY && r1Pos.startX == r2Pos.targetX && r1Pos.startY == r2Pos.targetY);
					if (predictCollision)
						break;
				}
			}
			if (predictCollision)
				agentWillCollide.Add(agent);
		}

		// Reduce move if collision between agents
		foreach(GameObject agent in agentWillCollide)
        {
			Position pos = agent.GetComponent<Position>();
			pos.targetX += pos.startX == pos.targetX ? 0 : (pos.startX < pos.targetX ? -0.15f : 0.15f);
			pos.targetY += pos.startY == pos.targetY ? 0 : (pos.startY < pos.targetY ? -0.15f : 0.15f);
		}

		// validate new position based on rounding corrected target position
		foreach (GameObject agent in f_agent)
		{
			Position pos = agent.GetComponent<Position>();
			if (pos.targetX != -1 && pos.targetY != -1)
			{
				pos.x = Mathf.RoundToInt(pos.targetX);
				pos.y = Mathf.RoundToInt(pos.targetY);
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

			// reset data used to move
			Position pos = ca.agent.GetComponent<Position>();
			pos.startX = -1;
			pos.startY = -1;
			pos.targetX = -1;
			pos.targetY = -1;

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
		pos.startX = pos.x;
		pos.startY = pos.y;
		switch (go.GetComponent<Direction>().direction){
			case Direction.Dir.North:
				pos.targetX = pos.startX;
				pos.targetY = pos.startY - checkObstacle(go, pos.startX, pos.startY - 1);
				break;
			case Direction.Dir.South:
				pos.targetX = pos.startX;
				pos.targetY = pos.startY + checkObstacle(go, pos.startX, pos.startY + 1);
				break;
			case Direction.Dir.East:
				pos.targetX = pos.startX + checkObstacle(go, pos.startX + 1, pos.startY);
				pos.targetY = pos.startY;
				break;
			case Direction.Dir.West:
				pos.targetX = pos.startX - checkObstacle(go, pos.startX - 1, pos.startY);
				pos.targetY = pos.startY;
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

	// Retourne la distance à parcourir en fonction de la nature de l'obstacle:
	// No obstacles => 1
	// Wall => 0.49 (volontairement inférieur à 0.5 pour que l'arrondi nous donne la position de la case de départ)
	// Furniture => player:0.8, drone:1
	// Door closed => 1
	private float checkObstacle(GameObject agent, int simTargetX, int simTargetY){
		foreach( GameObject obstacle in f_obstacles){
			if (obstacle.GetComponent<Position>().x == simTargetX && obstacle.GetComponent<Position>().y == simTargetY) {
				if (obstacle.CompareTag("Wall"))
					return 0.49f;
				else if (obstacle.CompareTag("Furniture"))
					return (agent.CompareTag("Player") ? 0.8f : 1f);
				// si c'est une porte vérifier si elle est ouverte
				else if (obstacle.CompareTag("Door") && !obstacle.GetComponent<ActivationSlot>().state)
					return 1f;
			}
		}
		return 1f;
	}
}
