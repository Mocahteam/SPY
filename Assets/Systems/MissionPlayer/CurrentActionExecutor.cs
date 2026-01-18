using UnityEngine;
using FYFY;
using System.Collections.Generic;

/// <summary>
/// This system executes new currentActions
/// </summary>
public class CurrentActionExecutor : FSystem {
	private Family f_obstacles = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall", "Door"));
	private Family f_activableConsole = FamilyManager.getFamily(new AllOfComponents(typeof(Activable),typeof(Position),typeof(AudioSource)));
    private Family f_newCurrentAction = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction), typeof(BasicAction)));
	private Family f_agent = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)));

	protected override void onStart()
	{
		f_newCurrentAction.addEntryCallback(onNewCurrentAction);
		Pause = true;
	}

	protected override void onProcess(int familiesUpdateCount)
	{
		List<GameObject> agentWillCollide = new List<GameObject>();
		foreach (GameObject agent in f_agent)
		{
			// count inaction if a robot have no CurrentAction
			if (agent.tag == "Player" && agent.GetComponent<ScriptRef>().executableScript.GetComponentInChildren<CurrentAction>(true) == null)
				agent.GetComponent<ScriptRef>().nbOfInactions++;
			// Predict if collision will occurs
			bool predictCollision = false;
			foreach (GameObject agent2 in f_agent) {
				if (agent != agent2 && agent.tag == agent2.tag && agent.tag == "Player")
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
			pos.targetX += pos.x == pos.targetX ? 0 : (pos.x < pos.targetX ? -0.2f : 0.2f);
			pos.targetY += pos.y == pos.targetY ? 0 : (pos.y < pos.targetY ? -0.2f : 0.2f);
		}

		// Record valid movements
		foreach (GameObject robot in f_agent)
		{
			Position pos = robot.GetComponent<Position>();
			if (pos.targetX != -1 && pos.targetY != -1)
			{
				pos.x = pos.targetX;
				pos.y = pos.targetY;
				pos.targetX = -1;
				pos.targetY = -1;
			}
		}
		Pause = true;
	}

	// each time a new currentAction is added, 
	private void onNewCurrentAction(GameObject currentAction) {
		Pause = false; // activates onProcess to identify inactive robots
		
		CurrentAction ca = currentAction.GetComponent<CurrentAction>();	

		// process action depending on action type
		switch (currentAction.GetComponent<BasicAction>().actionType){
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
					if(actGo.GetComponent<Position>().x == agentPos.x && actGo.GetComponent<Position>().y == agentPos.y)
						GameObjectManager.addComponent<Triggered>(actGo);
				if (ca.agent.GetComponent<Animator>())
					ca.agent.GetComponent<Animator>().SetTrigger("Action");
				break;
		}
		ca.StopAllCoroutines();
		if (ca.gameObject.activeInHierarchy)
			ca.StartCoroutine(UtilityGame.pulseItem(ca.gameObject));
		// notify agent moving
		if (ca.agent.CompareTag("Drone") && !ca.agent.GetComponent<Moved>())
			GameObjectManager.addComponent<Moved>(ca.agent);
	}

	private void ApplyForward(GameObject go){
		Position pos = go.GetComponent<Position>();
		switch (go.GetComponent<Direction>().direction){
			case Direction.Dir.North:
				pos.targetX = pos.x;
				pos.targetY = pos.y - 1;
				pos.targetY += checkObstacle((int)pos.x, (int)pos.y - 1);
				break;
			case Direction.Dir.South:
				pos.targetX = pos.x;
				pos.targetY = pos.y + 1;
				pos.targetY -= checkObstacle((int)pos.x, (int)pos.y + 1);
				break;
			case Direction.Dir.East:
				pos.targetX = pos.x + 1;
				pos.targetY = pos.y;
				pos.targetX -= checkObstacle((int)pos.x + 1, (int)pos.y);
				break;
			case Direction.Dir.West:
				pos.targetX = pos.x - 1;
				pos.targetY = pos.y;
				pos.targetX += checkObstacle((int)pos.x - 1, (int)pos.y);
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

	// Retourne la distance à prendre en compte en fonction de la nature de l'obstacle:
	// 0 <=> no obstacles
	// 0.5 <=> Wall
	// 0 <=> closed Door
	private float checkObstacle(int x, int z){
		foreach( GameObject go in f_obstacles){
			if (go.GetComponent<Position>().x == x && go.GetComponent<Position>().y == z) {
				if (go.CompareTag("Wall"))
					return 0.5f;
				// si c'est une porte vérifier si elle est ouverte
				else if (go.CompareTag("Door") && !go.GetComponent<ActivationSlot>().state)
					return 0;
			}
		}
		return 0;
	}
}
