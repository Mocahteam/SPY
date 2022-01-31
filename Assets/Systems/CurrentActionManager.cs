using UnityEngine;
using FYFY;
using TMPro;
using System.Collections;


/// <summary>
/// Manage CurrentAction components, parse scripts and define next CurrentActions
/// </summary>
public class CurrentActionManager : FSystem
{
	private Family firstStep = FamilyManager.getFamily(new AllOfComponents(typeof(FirstStep)));
	private Family newStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private Family currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(BasicAction),typeof(UIActionType), typeof(CurrentAction)));
	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));
	private Family wallGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"));
	private Family droneGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Drone"));
	private Family doorGO = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
	private Family redDetectorGO = FamilyManager.getFamily(new AllOfComponents(typeof(Rigidbody), typeof(Detector), typeof(Position)));
	private Family coinGO = FamilyManager.getFamily(new AllOfComponents(typeof(CapsuleCollider), typeof(Position), typeof(ParticleSystem)), new AnyOfTags("Coin"));
	private Family activableConsoleGO = FamilyManager.getFamily(new AllOfComponents(typeof(Activable),typeof(Position),typeof(AudioSource)));
	private Family scriptIsRunning = FamilyManager.getFamily(new AllOfComponents(typeof(PlayerIsMoving)));

	protected override void onStart()
    {
		firstStep.addEntryCallback(initFirstActions);
		newStep_f.addEntryCallback(delegate (GameObject unused) { onNewStep(); });
		scriptIsRunning.addExitCallback(removePlayersCurrentActions);
	}

	// See ExecuteButton in editor (launch execution process by adding FirstStep)
	public void firstAction(GameObject buttonStop)
	{
		if (!buttonStop.activeInHierarchy)
			GameObjectManager.addComponent<FirstStep>(MainLoop.instance.gameObject);
	}

	private void initFirstActions(GameObject unused)
	{
		MainLoop.instance.StartCoroutine(delayInitFirstsActions());
	}

	private IEnumerator delayInitFirstsActions()
	{
		yield return null; // wait editable script was copied to executable panels

		// init currentAction on the first action of players
		foreach (GameObject player in playerGO)
			addCurrentActionOnFirstAction(player);

		// init currentAction on the first action of ennemies
		bool forceNewStep = false;
		foreach (GameObject drone in droneGO)
			if (!drone.GetComponent<ScriptRef>().scriptContainer.GetComponentInChildren<CurrentAction>() && !drone.GetComponent<ScriptRef>().scriptFinished)
				addCurrentActionOnFirstAction(drone);
			else
				forceNewStep = true; // will move currentAction on next action

		if (forceNewStep)
			onNewStep();
	}

	private void addCurrentActionOnFirstAction(GameObject agent)
    {
		GameObject firstAction = null;
		// try to get the first action
		Transform container = agent.GetComponent<ScriptRef>().scriptContainer.transform;
		if (container.childCount > 0)
			firstAction = getFirstActionOf(container.GetChild(0).gameObject, agent);

		if (firstAction != null)
		{
			// Set this action as CurrentAction
			GameObjectManager.addComponent<CurrentAction>(firstAction, new { agent = agent });

			// parse parents to init ForAction blocs
			Transform parentAction = firstAction.transform.parent;
			while (parentAction != null)
			{
				if (parentAction.GetComponent<ForAction>())
				{
					ForAction forAct = parentAction.GetComponent<ForAction>();
					forAct.currentFor++;
					forAct.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
				}
				parentAction = parentAction.parent;
			}
		}
		else
			GameObjectManager.addComponent<EmptyExecution>(MainLoop.instance.gameObject);
	}

	// get first action inside "action", it could be control structure (if, for...) => recursive call
	public GameObject getFirstActionOf(GameObject action, GameObject agent)
	{
		if (action == null)
			return null;
		if (action.GetComponent<BasicAction>())
			return action;
		else
		{
			// check if action is a ForAction
			if (action.GetComponent<ForAction>())
			{
				// check if this ForAction include a child and nb iteration != 0
				if (action.GetComponent<ForAction>().firstChild != null && action.GetComponent<ForAction>().nbFor != 0)
					// get first action of its first child (could be if, for...)
					return getFirstActionOf(action.GetComponent<ForAction>().firstChild, agent);
				else
					// this for doesn't contain action or nb iteration == 0 => get first action of next action (could be if, for...)
					return getFirstActionOf(action.GetComponent<ForAction>().next, agent);
			}
			// check if action is a IfAction
			else if (action.GetComponent<IfAction>())
			{
				// check if this IfAction include a child and if condition is evaluated to true
				if (action.GetComponent<IfAction>().firstChild != null && ifValid(action.GetComponent<IfAction>(), agent))
					// get first action of its first child (could be if, for...)
					return getFirstActionOf(action.GetComponent<IfAction>().firstChild, agent);
				else
					// this if doesn't contain action or its condition is false => get first action of next action (could be if, for...)
					return getFirstActionOf(action.GetComponent<IfAction>().next, agent);
			}
			// check if action is a ForeverAction
			else if (action.GetComponent<ForeverAction>())
				// always return firstchild of this ForeverAction
				return getFirstActionOf(action.GetComponent<ForeverAction>().firstChild, agent);
		}
		return null;
	}

	public bool ifValid(IfAction ifAction, GameObject scripted)
	{
		bool ifok = false;
		// get absolute target position depending on player orientation and relative direction to observe
		Vector2 vec = new Vector2();
		switch (getDirection(scripted.GetComponent<Direction>().direction, ifAction.ifDirection))
		{
			case Direction.Dir.North:
				vec = new Vector2(0, ifAction.range);
				break;
			case Direction.Dir.South:
				vec = new Vector2(0, -ifAction.range);
				break;
			case Direction.Dir.East:
				vec = new Vector2(ifAction.range, 0);
				break;
			case Direction.Dir.West:
				vec = new Vector2(-ifAction.range, 0);
				break;
		}

		// check target position
		switch (ifAction.ifEntityType)
		{
			case 0: // walls
				foreach (GameObject go in wallGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = !ifAction.ifNot;
				break;
			case 1: // doors
				foreach (GameObject go in doorGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = !ifAction.ifNot;
				break;
			case 2: // ennemies
				foreach (GameObject go in droneGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
						go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = !ifAction.ifNot;
				break;
			case 3: // allies
				foreach (GameObject go in playerGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
						go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = !ifAction.ifNot;
				break;
			case 4: // consoles
				foreach (GameObject go in activableConsoleGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
						go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = !ifAction.ifNot;
				break;
			case 5: // detectors
				foreach (GameObject go in redDetectorGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = !ifAction.ifNot;
				break;
			case 6: // coins
				foreach (GameObject go in coinGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = !ifAction.ifNot;
				break;
		}
		return ifok;
	}

	//0 Forward, 1 Backward, 2 Left, 3 Right
	public static Direction.Dir getDirection(Direction.Dir dirEntity, int relativeDir)
	{
		if (relativeDir == 0)
			return dirEntity;
		switch (dirEntity)
		{
			case Direction.Dir.North:
				switch (relativeDir)
				{
					case 1:
						return Direction.Dir.South;
					case 2:
						return Direction.Dir.West;
					case 3:
						return Direction.Dir.East;
				}
				break;
			case Direction.Dir.West:
				switch (relativeDir)
				{
					case 1:
						return Direction.Dir.East;
					case 2:
						return Direction.Dir.South;
					case 3:
						return Direction.Dir.North;
				}
				break;
			case Direction.Dir.East:
				switch (relativeDir)
				{
					case 1:
						return Direction.Dir.West;
					case 2:
						return Direction.Dir.North;
					case 3:
						return Direction.Dir.South;
				}
				break;
			case Direction.Dir.South:
				switch (relativeDir)
				{
					case 1:
						return Direction.Dir.North;
					case 2:
						return Direction.Dir.East;
					case 3:
						return Direction.Dir.West;
				}
				break;
		}
		return dirEntity;
	}

	private void onNewStep(){
		GameObject nextAction;
		foreach(GameObject currentActionGO in currentActions){
			CurrentAction currentAction = currentActionGO.GetComponent<CurrentAction>();
			nextAction = getNextAction(currentActionGO, currentAction.agent);
			// check if we reach last action of a drone
			if(nextAction == null && currentActionGO.GetComponent<CurrentAction>().agent.CompareTag("Drone"))
				currentActionGO.GetComponent<CurrentAction>().agent.GetComponent<ScriptRef>().scriptFinished = true;
			else if(nextAction != null){
				//ask to add CurrentAction on next frame => this frame we will remove current CurrentActions
				MainLoop.instance.StartCoroutine(delayAddCurrentAction(nextAction, currentAction.agent));
			}
			GameObjectManager.removeComponent<CurrentAction>(currentActionGO);
		}
	}

	public GameObject getNextAction(GameObject currentAction, GameObject agent){
		BasicAction current_ba = currentAction.GetComponent<BasicAction>();
		if (current_ba != null)
		{
			// if next is not defined or is a BasicAction we return it
			if(current_ba.next == null || current_ba.next.GetComponent<BasicAction>())
				return current_ba.next;
			else
				return getNextAction(current_ba.next, agent);
		}
		// currentAction is not a BasicAction
		// check if it is a ForAction
		else if(currentAction.GetComponent<ForAction>()){
			ForAction forAct = currentAction.GetComponent<ForAction>();
			// ForAction reach the number of iterations
			if(forAct.currentFor >= forAct.nbFor){
				// reset nb iteration to 0
				forAct.currentFor = 0;
				forAct.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
				// return next action
				if(forAct.next == null || forAct.next.GetComponent<BasicAction>())
					return forAct.next;
				else
					return getNextAction(forAct.next , agent);
			}
			// iteration are available
			else{
				// in case ForAction has no child
				if (forAct.firstChild == null)
				{
					// reset nb iteration to 0
					forAct.currentFor = 0;
					forAct.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
					// return next action
					if (forAct.next == null || forAct.next.GetComponent<BasicAction>())
						return forAct.next;
					else
						return getNextAction(forAct.next, agent);
				}
				else
				// return first child
				{
					// add one iteration
					forAct.currentFor++;
					forAct.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
					// return first child
					if (forAct.firstChild.GetComponent<BasicAction>())
						return forAct.firstChild;
					else
						return getNextAction(forAct.firstChild, agent);
				}
			}
		}
		// check if it is a IfAction
		else if(currentAction.GetComponent<IfAction>()){
			// check if IfAction has a first child and condition is true
			IfAction ifAction = currentAction.GetComponent<IfAction>();
			if (ifAction.firstChild != null && ifValid(ifAction, agent)){ 
				// return first action
				if(ifAction.firstChild.GetComponent<BasicAction>())
					return ifAction.firstChild;
				else
					return getNextAction(ifAction.firstChild, agent);				
			}
			else{
				// return next action
				if(ifAction.next == null || ifAction.next.GetComponent<BasicAction>()){
					return ifAction.next;
				}
				else{
					return getNextAction(ifAction.next , agent);
				}				
			}
		}
		// check if it is a ForeverAction
		else if(currentAction.GetComponent<ForeverAction>()){
			ForeverAction foreverAction = currentAction.GetComponent<ForeverAction>();
			if (foreverAction.firstChild == null || foreverAction.firstChild.GetComponent<BasicAction>())
				return foreverAction.firstChild;
			else
				return getNextAction(foreverAction.firstChild, agent);
		}

		return null;
	}

	private IEnumerator delayAddCurrentAction(GameObject nextAction, GameObject agent)
	{
		yield return null; // we add new CurrentAction next frame otherwise families are not notified to this adding because at the begining of this frame GameObject already contains CurrentAction
		GameObjectManager.addComponent<CurrentAction>(nextAction, new { agent = agent });
	}

	private void removePlayersCurrentActions(int unused)
	{
		foreach (GameObject currentAction in currentActions)
		{
			if (currentAction.GetComponent<CurrentAction>().agent.CompareTag("Player"))
				GameObjectManager.removeComponent<CurrentAction>(currentAction);
		}
	}
}