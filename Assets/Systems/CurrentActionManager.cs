using UnityEngine;
using FYFY;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Data;

/// <summary>
/// Manage CurrentAction components, parse scripts and define next CurrentActions
/// </summary>
public class CurrentActionManager : FSystem
{
	private Family executionReady = FamilyManager.getFamily(new AllOfComponents(typeof(ExecutablePanelReady)));
	private Family ends_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
	private Family newStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private Family currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(BasicAction),typeof(LibraryItemRef), typeof(CurrentAction)));
	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));

	private Family wallGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"));
	private Family droneGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Drone"));
	private Family doorGO = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
	private Family redDetectorGO = FamilyManager.getFamily(new AllOfComponents(typeof(Rigidbody), typeof(Detector), typeof(Position)));
	private Family activableConsoleGO = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(AudioSource)));
	private Family exitGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position), typeof(AudioSource)), new AnyOfTags("Exit"));


	private Family playingMode_f = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
	private Family editingMode_f = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));


	public static CurrentActionManager instance;

	public CurrentActionManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		executionReady.addEntryCallback(initFirstsActions);
		newStep_f.addEntryCallback(delegate { onNewStep(); });
		editingMode_f.addEntryCallback(delegate {
			// remove all player's current actions
			foreach (GameObject currentAction in currentActions)
				if (currentAction.GetComponent<CurrentAction>().agent.CompareTag("Player"))
					GameObjectManager.removeComponent<CurrentAction>(currentAction);
		});
		playingMode_f.addEntryCallback(delegate {
			// reset inaction counters
			foreach (GameObject robot in playerGO)
				robot.GetComponent<ScriptRef>().nbOfInactions = 0;
		});
	}

	private void initFirstsActions(GameObject go)
	{
		// init first action if no ends occur (possible for scripts with bad condition)
		if (ends_f.Count <= 0)
		{
			// init currentAction on the first action of players
			bool atLeastOneFirstAction = false;
			foreach (GameObject player in playerGO)
				if (addCurrentActionOnFirstAction(player) != null)
					atLeastOneFirstAction = true;
			if (!atLeastOneFirstAction)
			{
				ModeManager.instance.setEditMode();
				// TODO : afficher un message pour dire qu'aucune action n'est accessible ?
			}
			else
			{
				// init currentAction on the first action of ennemies
				bool forceNewStep = false;
				foreach (GameObject drone in droneGO)
					if (!drone.GetComponent<ScriptRef>().executableScript.GetComponentInChildren<CurrentAction>() && !drone.GetComponent<ScriptRef>().scriptFinished)
						addCurrentActionOnFirstAction(drone);
					else
						forceNewStep = true; // will move currentAction on next action

				if (forceNewStep)
					onNewStep();
			}
		}

		GameObjectManager.removeComponent<ExecutablePanelReady>(go);
	}

	private GameObject addCurrentActionOnFirstAction(GameObject agent)
    {
		GameObject firstAction = null;
		// try to get the first action
		Transform container = agent.GetComponent<ScriptRef>().executableScript.transform;
		if (container.childCount > 0)
			firstAction = getFirstActionOf(container.GetChild(0).gameObject, agent);

		if (firstAction != null)
		{
			// Set this action as CurrentAction
			GameObjectManager.addComponent<CurrentAction>(firstAction, new { agent = agent });
		}

		return firstAction;
	}

	private bool ifValid(List<string> condition, GameObject scripted)
	{

		string cond = "";
		for (int i = 0; i < condition.Count; i++)
		{
			if (condition[i] == "(" || condition[i] == ")" || condition[i] == "OR" || condition[i] == "AND" || condition[i] == "NOT")
			{
				cond = cond + condition[i] + " ";
			}
			else
			{
				cond = cond + verifCondition(condition[i], scripted) + " ";
			}
		}

		DataTable dt = new DataTable();
		var v = dt.Compute(cond, "");
		bool result;
		try
		{
			result = bool.Parse(v.ToString());
		}
		catch
		{
			result = false;
		}
		return result;
	}

	private bool verifCondition(string ele, GameObject scripted)
	{

		bool ifok = false;
		// get absolute target position depending on player orientation and relative direction to observe
		// On commence par identifier quelle case doit être regardé pour voir si la condition est respecté
		Vector2 vec = new Vector2();
		switch (scripted.GetComponent<Direction>().direction)
		{
			case Direction.Dir.North:
				vec = new Vector2(0, 1);
				break;
			case Direction.Dir.South:
				vec = new Vector2(0, -1);
				break;
			case Direction.Dir.East:
				vec = new Vector2(1, 0);
				break;
			case Direction.Dir.West:
				vec = new Vector2(-1, 0);
				break;
		}

		// check target position
		switch (ele)
		{
			case "Wall": // walls
				foreach (GameObject go in wallGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = true;
				break;
			case "FieldGate": // doors
				foreach (GameObject go in doorGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = true;
				break;
			case "Enemie": // ennemies
				foreach (GameObject go in droneGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
						go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = true;
				break;
			case "Terminal": // consoles
				foreach (GameObject go in activableConsoleGO)
				{
					vec = new Vector2(0, 0);
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
						go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = true;
				}
				break;
			case "RedArea": // detectors
				foreach (GameObject go in redDetectorGO)
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = true;
				break;
			case "Exit": // exits
				foreach (GameObject go in exitGO)
				{
					vec = new Vector2(0, 0);
					if (go.GetComponent<Position>().x == scripted.GetComponent<Position>().x + vec.x &&
					 go.GetComponent<Position>().z == scripted.GetComponent<Position>().z + vec.y)
						ifok = true;
				}
				break;
		}
		return ifok;

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
			// check if action is a IfControl
			if (action.GetComponent<IfControl>())
			{
				IfControl ifCont = action.GetComponent<IfControl>();
				// check if this IfControl include a child and if condition is evaluated to true
				if (ifCont.firstChild != null && ifValid(ifCont.condition, agent))
					// get first action of its first child (could be if, for...)
					return getFirstActionOf(ifCont.firstChild, agent);
				else if (action.GetComponent<IfElseControl>() && action.GetComponent<IfElseControl>().firstChild != null)
					return getFirstActionOf(action.GetComponent<IfElseControl>().elseFirstChild, agent);
				else
					// this if doesn't contain action or its condition is false => get first action of next action (could be if, for...)
					return getFirstActionOf(ifCont.next, agent);
			}
			// check if action is a WhileControl
			else if (action.GetComponent<WhileControl>())
			{
				WhileControl whileCont = action.GetComponent<WhileControl>();
				// check if this WhileControl include a child and if condition is evaluated to true
				if (whileCont.firstChild != null && ifValid(whileCont.condition, agent))
					// get first action of its first child (could be if, for...)
					return getFirstActionOf(whileCont.firstChild, agent);
				else
					// this if doesn't contain action or its condition is false => get first action of next action (could be if, for...)
					return getFirstActionOf(whileCont.next, agent);
			}
			// check if action is a ForControl
			else if (action.GetComponent<ForControl>())
			{
				ForControl forCont = action.GetComponent<ForControl>();
				// check if this ForControl include a child and nb iteration != 0 and end loop not reached
				if (forCont.firstChild != null && forCont.nbFor != 0 && forCont.currentFor < forCont.nbFor)
				{
					forCont.currentFor++;
					forCont.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (forCont.currentFor).ToString() + " / " + forCont.nbFor.ToString();
					// get first action of its first child (could be if, for...)
					return getFirstActionOf(forCont.firstChild, agent);
				}
				else
					// this for doesn't contain action or nb iteration == 0 or end loop reached => get first action of next action (could be if, for...)
					return getFirstActionOf(forCont.next, agent);
			}
			// check if action is a ForeverControl
			else if (action.GetComponent<ForeverControl>())
			{
				// always return firstchild of this ForeverControl
				return getFirstActionOf(action.GetComponent<ForeverControl>().firstChild, agent);
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
		else if (currentAction.GetComponent<WhileControl>())
        {
			if(ifValid(currentAction.GetComponent<WhileControl>().condition, agent))
            {
				if (currentAction.GetComponent<WhileControl>().firstChild.GetComponent<BasicAction>())
					return currentAction.GetComponent<WhileControl>().firstChild;
				else
					return getNextAction(currentAction.GetComponent<WhileControl>().firstChild, agent);
			}
            else
            {
				if (currentAction.GetComponent<WhileControl>().next == null || currentAction.GetComponent<WhileControl>().next.GetComponent<BasicAction>())
					return currentAction.GetComponent<WhileControl>().next;
				else
					return getNextAction(currentAction.GetComponent<WhileControl>().next, agent);
			}
		}
		// currentAction is not a BasicAction
		// check if it is a ForAction
		else if(currentAction.GetComponent<ForControl>()){
			ForControl forAct = currentAction.GetComponent<ForControl>();
			// ForAction reach the number of iterations
			if (!forAct.gameObject.GetComponent<WhileControl>() && forAct.currentFor >= forAct.nbFor){
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
					if (!forAct.gameObject.GetComponent<WhileControl>()) {
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
		else if(currentAction.GetComponent<IfControl>()){
			// check if IfAction has a first child and condition is true
			IfControl ifAction = currentAction.GetComponent<IfControl>();
			if (ifAction.firstChild != null && ifValid(ifAction.condition, agent)){ 
				// return first action
				if(ifAction.firstChild.GetComponent<BasicAction>())
					return ifAction.firstChild;
				else
					return getNextAction(ifAction.firstChild, agent);				
			}
			else if (currentAction.GetComponent<IfElseControl>() && currentAction.GetComponent<IfElseControl>().firstChild != null)
				return currentAction.GetComponent<IfElseControl>().elseFirstChild;
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
		else if(currentAction.GetComponent<ForeverControl>()){
			ForeverControl foreverAction = currentAction.GetComponent<ForeverControl>();
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
}