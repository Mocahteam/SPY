using UnityEngine;
using FYFY;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Data;

/// <summary>
/// Manage CurrentAction components, parse scripts and define first action, next actions, evaluate boolean expressions (if and while)...
/// </summary>
public class CurrentActionManager : FSystem
{
	private Family f_executionReady = FamilyManager.getFamily(new AllOfComponents(typeof(ExecutablePanelReady)));
	private Family f_ends = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
	private Family f_newStep = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private Family f_currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(BasicAction),typeof(LibraryItemRef), typeof(CurrentAction)));
	private Family f_player = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));
	private Family f_conditionNotifs = FamilyManager.getFamily(new AnyOfTags("ConditionNotif"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	private Family f_wall = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"));
	private Family f_drone = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Drone"));
	private Family f_door = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family f_redDetector = FamilyManager.getFamily(new AllOfComponents(typeof(Rigidbody), typeof(Detector), typeof(Position)));
	private Family f_activableConsole = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(AudioSource)));
	private Family f_exit = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Exit"));

	private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));

	private HashSet<int> exploredScripItem;
	private bool infiniteLoopDetected;

	public static CurrentActionManager instance;

	public CurrentActionManager()
	{
		instance = this;
	}

	protected override void onStart()
	{
		f_executionReady.addEntryCallback(initFirstsActions);
		f_newStep.addEntryCallback(delegate { onNewStep(); });
		f_playingMode.addEntryCallback(delegate {
			// reset inaction counters
			foreach (GameObject robot in f_player)
				robot.GetComponent<ScriptRef>().nbOfInactions = 0;
		});
	}

	private void initFirstsActions(GameObject go)
	{
		// init first action if no ends occur (possible for scripts with bad condition)
		if (f_ends.Count <= 0)
		{
			// init currentAction on the first action of players
			bool atLeastOneFirstAction = false;
			foreach (GameObject player in f_player)
			{
				if (addCurrentActionOnFirstAction(player) != null)
					atLeastOneFirstAction = true;
				if (infiniteLoopDetected)
					break;
			}
			if (!atLeastOneFirstAction || infiniteLoopDetected)
			{
				GameObjectManager.addComponent<EditMode>(MainLoop.instance.gameObject);
				if (infiniteLoopDetected)
					GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.InfiniteLoop });
				else
					GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.NoAction });
			}
			else
			{
				// init currentAction on the first action of ennemies
				bool forceNewStep = false;
				foreach (GameObject drone in f_drone)
					if (!drone.GetComponent<ScriptRef>().executableScript.GetComponentInChildren<CurrentAction>(true) && !drone.GetComponent<ScriptRef>().scriptFinished)
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

	// get first action inside "action"
	private GameObject getFirstActionOf(GameObject action, GameObject agent)
    {
		exploredScripItem = new HashSet<int>();
		infiniteLoopDetected = false;
		return rec_getFirstActionOf(action, agent);
	}

	// look for first action recursively, it could be control structure (if, for...)
	private GameObject rec_getFirstActionOf(GameObject action, GameObject agent)
	{
		infiniteLoopDetected = exploredScripItem.Contains(action.GetInstanceID());
		if (action == null || infiniteLoopDetected)
			return null;
		exploredScripItem.Add(action.GetInstanceID());
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
					return rec_getFirstActionOf(ifCont.firstChild, agent);
				else if (action.GetComponent<IfElseControl>() && action.GetComponent<IfElseControl>().firstChild != null)
					return rec_getFirstActionOf(action.GetComponent<IfElseControl>().elseFirstChild, agent);
				else
					// this if doesn't contain action or its condition is false => get first action of next action (could be if, for...)
					return rec_getFirstActionOf(ifCont.next, agent);
			}
			// check if action is a WhileControl
			else if (action.GetComponent<WhileControl>())
			{
				WhileControl whileCont = action.GetComponent<WhileControl>();
				// check if condition is evaluated to true
				if (ifValid(whileCont.condition, agent))
					// get first action of its first child (could be if, for...)
					return rec_getFirstActionOf(whileCont.firstChild, agent);
				else
					// this condition is false => get first action of next action (could be if, for...)
					return rec_getFirstActionOf(whileCont.next, agent);
			}
			// check if action is a ForControl
			else if (action.GetComponent<ForControl>())
			{
				ForControl forCont = action.GetComponent<ForControl>();
				// pulse counter
				forCont.StartCoroutine(Utility.pulseItem(forCont.transform.GetChild(1).GetChild(1).gameObject));
				// check if this ForControl include a child and nb iteration != 0 and end loop not reached
				if (forCont.firstChild != null && forCont.nbFor != 0 && forCont.currentFor < forCont.nbFor)
				{
					forCont.currentFor++;
					forCont.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = (forCont.currentFor).ToString() + " / " + forCont.nbFor.ToString();
					// get first action of its first child (could be if, for...)
					return rec_getFirstActionOf(forCont.firstChild, agent);
				}
				else
				{
					// this for doesn't contain action or nb iteration == 0 or end loop reached => get first action of next action (could be if, for...)
					if (forCont.currentFor >= forCont.nbFor)
                    {
						// reset nb iteration to 0
						forCont.currentFor = 0;
						forCont.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = (forCont.currentFor).ToString() + " / " + forCont.nbFor.ToString();
					}
					return rec_getFirstActionOf(forCont.next, agent);
				}
			}
			// check if action is a ForeverControl
			else if (action.GetComponent<ForeverControl>())
			{
				// always return firstchild of this ForeverControl
				return rec_getFirstActionOf(action.GetComponent<ForeverControl>().firstChild, agent);
			}
		}
		return null;
	}

	// Return true if "condition" is valid and false otherwise
	private bool ifValid(List<ConditionItem> condition, GameObject agent)
	{
		string cond = "";
		for (int i = 0; i < condition.Count; i++)
		{
			if (condition[i].key == "(" || condition[i].key == ")" || condition[i].key == "OR" || condition[i].key == "AND" || condition[i].key == "NOT")
			{
				cond = cond + condition[i].key + " ";
			}
			else
			{
				cond = cond + checkCaptor(condition[i], agent) + " ";
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

	// return true if the captor is true, and false otherwise
	private bool checkCaptor(ConditionItem ele, GameObject agent)
	{
		string key = ele.key;
		bool ifok = false;
		// get absolute target position depending on player orientation and relative direction to observe
		// On commence par identifier quelle case doit être regardée pour voir si la condition est respectée
		Vector2 vec = new Vector2();
		switch (agent.GetComponent<Direction>().direction)
		{
			case Direction.Dir.North:
				vec = key == "WallLeft" || key == "PathLeft" ? new Vector2(-1, 0) : (key == "WallRight" || key == "PathRight" ? new Vector2(1, 0) : new Vector2(0, -1));
				break;
			case Direction.Dir.South:
				vec = key == "WallLeft" || key == "PathLeft" ? new Vector2(1, 0) : (key == "WallRight" || key == "PathRight" ? new Vector2(-1, 0) : new Vector2(0, 1));
				break;
			case Direction.Dir.East:
				vec = key == "WallLeft" || key == "PathLeft" ? new Vector2(0, -1) : (key == "WallRight" || key == "PathRight" ? new Vector2(0, 1) : new Vector2(1, 0));
				break;
			case Direction.Dir.West:
				vec = key == "WallLeft" || key == "PathLeft" ? new Vector2(0, 1) : (key == "WallRight" || key == "PathRight" ? new Vector2(0, -1) : new Vector2(-1, 0));
				break;
		}

		// check target position
		switch (key)
		{
			case "WallFront":
			case "WallLeft":
			case "WallRight":
				// check only visible walls
				foreach (GameObject wall in f_wall)
					if (wall.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
					 wall.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y && wall.GetComponent<Renderer>() != null && wall.GetComponent<Renderer>().enabled)
					{
						ifok = true;
						break;
					}
				break;
			case "PathFront":
			case "PathLeft":
			case "PathRight":
				ifok = true;
				// check visible and invisible walls
				foreach (GameObject wall in f_wall)
					if (wall.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
						wall.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
					{
						ifok = false;
						break;
					}
                if (ifok)
                {
					// check doors
					foreach (GameObject door in f_door)
						if (door.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
							door.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
						{
							ifok = false;
							break;
						}
				}
				break;
			case "FieldGate": // doors
				foreach (GameObject door in f_door)
					if (door.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
					 door.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
					{
						ifok = true;
						break;
					}
				break;
			case "Enemy": // enemies
				foreach (GameObject drone in f_drone)
					if (drone.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
						drone.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
					{
						ifok = true;
						break;
					}
				break;
			case "Terminal": // consoles
				vec = new Vector2(0, 0);
				foreach (GameObject console in f_activableConsole)
				{
					if (console.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
						console.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
					{
						ifok = true;
						break;
					}
				}
				break;
			case "RedArea": // detectors
				foreach (GameObject detector in f_redDetector)
					if (detector.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
					 detector.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
					{
						ifok = true;
						break;
					}
				break;
			case "Exit": // exits
				vec = new Vector2(0, 0);
				foreach (GameObject exit in f_exit)
				{
					if (exit.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
					 exit.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
					{
						ifok = true;
						break;
					}
				}
				break;
		}
		// notification de l'évaluation 
		GameObject notif = ele.target.transform.Find(ifok ? "true" : "false").gameObject;
		GameObjectManager.setGameObjectState(notif, true);
		MainLoop.instance.StartCoroutine(Utility.pulseItem(notif));
		return ifok;

	}

	// one step consists in removing the current actions this frame and adding new CurrentAction components next frame
	private void onNewStep()
	{
		// hide all conditions notifications
		foreach (GameObject notif in f_conditionNotifs)
			GameObjectManager.setGameObjectState(notif, false);

		GameObject nextAction;
		foreach(GameObject currentActionGO in f_currentActions){
			CurrentAction currentAction = currentActionGO.GetComponent<CurrentAction>();
			nextAction = getNextAction(currentActionGO, currentAction.agent);
			// check if we reach last action of a drone
			if (nextAction == null && currentActionGO.GetComponent<CurrentAction>().agent.CompareTag("Drone"))
				currentActionGO.GetComponent<CurrentAction>().agent.GetComponent<ScriptRef>().scriptFinished = true;
			else if (nextAction != null)
			{
				//ask to add CurrentAction on next frame => this frame we will remove current CurrentActions
				MainLoop.instance.StartCoroutine(delayAddCurrentAction(nextAction, currentAction.agent));
			}
			else if (infiniteLoopDetected)
				GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.InfiniteLoop });
			GameObjectManager.removeComponent<CurrentAction>(currentActionGO);
		}
	}

	// return the next action to execute, return null if no next action available
	private GameObject getNextAction(GameObject currentAction, GameObject agent){
		BasicAction current_ba = currentAction.GetComponent<BasicAction>();
		if (current_ba != null)
		{
			// if next is not defined or is a BasicAction we return it
			if(current_ba.next == null || current_ba.next.GetComponent<BasicAction>())
				return current_ba.next;
			else
				return getFirstActionOf(current_ba.next, agent);
		}
		else if (currentAction.GetComponent<WhileControl>())
        {
			if(ifValid(currentAction.GetComponent<WhileControl>().condition, agent))
            {
				if (currentAction.GetComponent<WhileControl>().firstChild == null || currentAction.GetComponent<WhileControl>().firstChild.GetComponent<BasicAction>())
					return currentAction.GetComponent<WhileControl>().firstChild;
				else
					return getFirstActionOf(currentAction.GetComponent<WhileControl>().firstChild, agent);
			}
            else
            {
				if (currentAction.GetComponent<WhileControl>().next == null || currentAction.GetComponent<WhileControl>().next.GetComponent<BasicAction>())
					return currentAction.GetComponent<WhileControl>().next;
				else
					return getFirstActionOf(currentAction.GetComponent<WhileControl>().next, agent);
			}
		}
		// currentAction is not a BasicAction
		// check if it is a ForAction
		else if(currentAction.GetComponent<ForControl>()){
			ForControl forAct = currentAction.GetComponent<ForControl>();
			// pulse counter
			forAct.StartCoroutine(Utility.pulseItem(forAct.transform.GetChild(1).GetChild(1).gameObject));
			// ForAction reach the number of iterations
			if (forAct.currentFor >= forAct.nbFor){
				// reset nb iteration to 0
				forAct.currentFor = 0;
				forAct.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
				// return next action
				if(forAct.next == null || forAct.next.GetComponent<BasicAction>())
					return forAct.next;
				else
					return getFirstActionOf(forAct.next , agent);
			}
			// iteration are available
			else{
				// in case ForAction has no child
				if (forAct.firstChild == null)
				{
					// reset nb iteration to 0
					forAct.currentFor = 0;
					forAct.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
					// return next action
					if (forAct.next == null || forAct.next.GetComponent<BasicAction>())
						return forAct.next;
					else
						return getFirstActionOf(forAct.next, agent);
				}
				else
				// return first child
				{
					// add one iteration
					forAct.currentFor++;
					forAct.transform.GetChild(1).GetChild(1).GetComponent<TMP_InputField>().text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
					// return first child
					if (forAct.firstChild == null || forAct.firstChild.GetComponent<BasicAction>())
						return forAct.firstChild;
					else
						return getFirstActionOf(forAct.firstChild, agent);
				}
			}
		}
		// check if it is a IfAction
		else if(currentAction.GetComponent<IfControl>()){
			// check if IfAction has a first child and condition is true
			IfControl ifAction = currentAction.GetComponent<IfControl>();
			if (ifValid(ifAction.condition, agent)) {
				// return first action
				if (ifAction.firstChild != null && ifAction.firstChild.GetComponent<BasicAction>())
					return ifAction.firstChild;
				else if (ifAction.firstChild != null)
					return getFirstActionOf(ifAction.firstChild, agent);
				else
					return getFirstActionOf(ifAction.next, agent);
			}
			else if (currentAction.GetComponent<IfElseControl>()) {
				IfElseControl ifElse = currentAction.GetComponent<IfElseControl>();
				// return first child
				if (ifElse.elseFirstChild != null && ifElse.elseFirstChild.GetComponent<BasicAction>())
					return ifElse.elseFirstChild;
				else if (ifElse.elseFirstChild != null)
					return getFirstActionOf(ifElse.elseFirstChild, agent);
				else
					return getFirstActionOf(ifAction.next, agent);
			}
			else
			{
				// return next action
				getFirstActionOf(ifAction.next, agent);
			}
		}
		// check if it is a ForeverAction
		else if(currentAction.GetComponent<ForeverControl>()){
			ForeverControl foreverAction = currentAction.GetComponent<ForeverControl>();
			if (foreverAction.firstChild == null || foreverAction.firstChild.GetComponent<BasicAction>())
				return foreverAction.firstChild;
			else
				return getFirstActionOf(foreverAction.firstChild, agent);
		}

		return null;
	}

	private IEnumerator delayAddCurrentAction(GameObject nextAction, GameObject agent)
	{
		yield return null; // we add new CurrentAction next frame otherwise families are not notified to this adding because at the begining of this frame GameObject already contains CurrentAction
		GameObjectManager.addComponent<CurrentAction>(nextAction, new { agent = agent });
	}
}