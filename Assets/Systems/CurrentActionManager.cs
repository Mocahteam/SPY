using UnityEngine;
using FYFY;
using TMPro;
using System.Collections;
using System.Globalization;


/// <summary>
/// Manage CurrentAction components, parse scripts and define next CurrentActions
/// </summary>
public class CurrentActionManager : FSystem
{
	private Family firstStep = FamilyManager.getFamily(new AllOfComponents(typeof(FirstStep)));
	private Family newStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private Family currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(BasicAction),typeof(UIActionType), typeof(CurrentAction)));
	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));
	private Family droneGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Drone"));
	private Family scriptIsRunning = FamilyManager.getFamily(new AllOfComponents(typeof(PlayerIsMoving)));

	public static CurrentActionManager instance;

	public CurrentActionManager()
	{
		instance = this;
	}

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
			// Set this action as CurrentAction
			GameObjectManager.addComponent<CurrentAction>(firstAction, new { agent = agent });
		else
			GameObjectManager.addComponent<EmptyExecution>(MainLoop.instance.gameObject);
	}

	// get first action inside "action", it could be control structure (if, for...) => recursive call
	// Si mon script commence par un if
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
				ForAction forAct = action.GetComponent<ForAction>();
                if (action.GetComponent<WhileAction>() && action.GetComponent<WhileAction>().firstChild != null)
                {
					return getFirstActionOf(action.GetComponent<WhileAction>().firstChild, agent);
				}
				// check if this ForAction include a child and nb iteration != 0 and end loop not reached
				else if (action.GetComponent<ForAction>().firstChild != null && forAct.nbFor != 0 && forAct.currentFor < forAct.nbFor)
				{
					forAct.currentFor++;
					forAct.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
					// get first action of its first child (could be if, for...)
					return getFirstActionOf(action.GetComponent<ForAction>().firstChild, agent);
				}
				else
					// this for doesn't contain action or nb iteration == 0 or end loop reached => get first action of next action (could be if, for...)
					return getFirstActionOf(action.GetComponent<ForAction>().next, agent);
			}
			// check if action is a IfAction
			else if (action.GetComponent<IfAction>())
			{
				// check if this IfAction include a child and if condition is evaluated to true
				if (action.GetComponent<IfAction>().firstChild != null && ConditionManagement.instance.ifValid(action.GetComponent<IfAction>().condition, agent))
					// get first action of its first child (could be if, for...)
					return getFirstActionOf(action.GetComponent<IfAction>().firstChild, agent);
				else if (action.GetComponent<ElseAction>() && action.GetComponent<ElseAction>().firstChild != null)
					return getFirstActionOf(action.GetComponent<ElseAction>().elseFirstChild, agent);
				else
					// this if doesn't contain action or its condition is false => get first action of next action (could be if, for...)
					return getFirstActionOf(action.GetComponent<IfAction>().next, agent);
			}
			// check if action is a ForeverAction
			else if (action.GetComponent<ForeverAction>())
			{
				// always return firstchild of this ForeverAction
				return getFirstActionOf(action.GetComponent<ForeverAction>().firstChild, agent);
			}
		}
		return null;
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
		else if (currentAction.GetComponent<WhileAction>())
        {
			if(ConditionManagement.instance.ifValid(currentAction.GetComponent<WhileAction>().condition, agent))
            {
				if (currentAction.GetComponent<WhileAction>().firstChild.GetComponent<BasicAction>())
					return currentAction.GetComponent<WhileAction>().firstChild;
				else
					return getNextAction(currentAction.GetComponent<WhileAction>().firstChild, agent);
			}
            else
            {
				if (currentAction.GetComponent<WhileAction>().next == null || currentAction.GetComponent<WhileAction>().next.GetComponent<BasicAction>())
					return currentAction.GetComponent<WhileAction>().next;
				else
					return getNextAction(currentAction.GetComponent<WhileAction>().next, agent);
			}
		}
		// currentAction is not a BasicAction
		// check if it is a ForAction
		else if(currentAction.GetComponent<ForAction>()){
			ForAction forAct = currentAction.GetComponent<ForAction>();
			// ForAction reach the number of iterations
			if (!forAct.gameObject.GetComponent<WhileAction>() && forAct.currentFor >= forAct.nbFor){
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
					if (!forAct.gameObject.GetComponent<WhileAction>()) {
					// reset nb iteration to 0
					forAct.currentFor = 0;
					forAct.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
					}
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
			if (ifAction.firstChild != null && ConditionManagement.instance.ifValid(ifAction.condition, agent)){ 
				// return first action
				if(ifAction.firstChild.GetComponent<BasicAction>())
					return ifAction.firstChild;
				else
					return getNextAction(ifAction.firstChild, agent);				
			}
			else if (currentAction.GetComponent<ElseAction>() && currentAction.GetComponent<ElseAction>().firstChild != null)
				return currentAction.GetComponent<ElseAction>().elseFirstChild;
			else
			{
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