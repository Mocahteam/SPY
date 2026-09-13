using FYFY;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

	private Family f_walls = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Wall"));
	private Family f_furnitures = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Furniture"));
	private Family f_drone = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef), typeof(Position)), new AnyOfTags("Drone"));
	private Family f_door = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"));
	private Family f_redDetector = FamilyManager.getFamily(new AllOfComponents(typeof(Rigidbody), typeof(Detector), typeof(Position)));
	private Family f_activableConsole = FamilyManager.getFamily(new AllOfComponents(typeof(Activable), typeof(Position), typeof(AudioSource)));
	private Family f_exit = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Exit"));
    private Family f_inventory = FamilyManager.getFamily(new AllOfComponents(typeof(ElementToDrag)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF)); // les éléments disponibles dans l'inventaire

    private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));

	private HashSet<int> exploredScripItem;
	private bool infiniteLoopDetected;
    private GameData gameData;
	private Coroutine delayCheckEnd_cor;

    public Transform editableContainers;

	public static CurrentActionManager instance;

	public CurrentActionManager()
	{
		instance = this;
	}

	protected override void onStart()
    {
        GameObject go = GameObject.Find("GameData");
        if (go != null)
            gameData = go.GetComponent<GameData>();

        f_executionReady.addEntryCallback(delegate (GameObject go){
			// Ici on est dans le cas où le panneau d'execution est initialisé et pret. Si on n'est pas en mode traçage de code et qu'il n'y a pas de fin (possible pour les cript avec une mauvaise condition, alors on initialise le currentAction sur la première action à exécuter
			if (f_ends.Count <= 0 && !gameData.userExecutor)
				initFirstsActions(go);
            GameObjectManager.removeComponent<ExecutablePanelReady>(go);
		});
		f_newStep.addEntryCallback(delegate {
            // Sur un newStep si on a déjà une currentAction s'est qu'on est en train d'exécuter le script, on passe donc à l'action suivante. Si on a pas de currentAction et qu'on est en mode traçage de code c'est que jusqu'à maintenant on été en attente de la sélections des actions à exécuter par le joueur, c'est chose faite et on demande un nouveau Step donc il faut initialiser le currentAction sur la première action à exécuter (uniquement si une fin n'est pas aussi demandée bien sûr)
            if (f_currentActions.Count > 0)
				onNewStep();
			else
                if (f_ends.Count <= 0 && gameData.userExecutor)
					initFirstsActions(null);
		});
		f_playingMode.addEntryCallback(delegate {
			// reset inaction counters
			foreach (GameObject robot in f_player)
				robot.GetComponent<ScriptRef>().nbOfInactions = 0;
		});

        // each time a current action is added, we check if the level is over after the end of animation
        f_currentActions.addEntryCallback(delegate {
			// on s'assure qu'une coroutine n'est pas déjà lancée avant d'en lancer une nouvelle
			if (delayCheckEnd_cor != null)
				MainLoop.instance.StopCoroutine(delayCheckEnd_cor);
			delayCheckEnd_cor = MainLoop.instance.StartCoroutine(delayCheckEnd());
        });

        Pause = true;
    }

    private IEnumerator delayCheckEnd()
    {
        // Attendre que l'on ait atteint 90% d'un pas de simulation
        yield return new WaitUntil(() => Time.time - gameData.startStepTime >= 0.9f / gameData.gameSpeed_current);

		if (f_ends.Count <= 0)
		{
			bool atLeastOneNextAction = false;
			GameObject nextAction;
			foreach (GameObject currentActionGO in f_currentActions)
			{
				CurrentAction currentAction = currentActionGO.GetComponent<CurrentAction>();
				nextAction = getNextAction(currentActionGO, currentAction.agent, true);
				// check if a new action is available for this currentAction
				if (nextAction != null && currentAction.agent.CompareTag("Player"))
				{
					atLeastOneNextAction = true;
					break;
				}
			}

			if (!atLeastOneNextAction)
			{
				// Aucun robot contrôlé par le joueur n'aure de prochaine action à exécuter

				// On vérifie si on ne serait pas dans une situation de victoire
				int nbEnd = 0;
				bool endDetected = false;
				// parse all exits
				for (int e = 0; e < f_exit.Count && !endDetected; e++)
				{
					GameObject exit = f_exit.getAt(e);
					// parse all players
					for (int p = 0; p < f_player.Count && !endDetected; p++)
					{
						GameObject player = f_player.getAt(p);
						// check if positions are equals
						if (player.GetComponent<Position>().x == exit.GetComponent<Position>().x && player.GetComponent<Position>().y == exit.GetComponent<Position>().y)
							nbEnd++;
					}
				}
				// if all players reached end position or all exits are filled
				if (nbEnd >= f_exit.Count || nbEnd >= f_player.Count)
					// trigger end
					GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.Win });
				else
				{
					// on vérifie s'il reste des blocks dans l'inventaire des joueurs, si oui on redonne la main au joueur pour qu'il continue à programmer, si non on déclenche une fin de type "NoMoreActionAvailableInInventory"
					if (f_inventory.Count > 0)
					{
						// Redonner la main au joueur pour continuer à programmer
						GameObjectManager.addComponent<EditMode>(MainLoop.instance.gameObject);
						GameObjectManager.addComponent<AskToSaveHistory>(MainLoop.instance.gameObject);
					}
					else
					{
						// Déclencher une fin de type "NoMoreActionAvailableInInventory"
						GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.NoMoreActionAvailableInInventory });
					}
				}
			}
		}
		delayCheckEnd_cor = null;
    }

    private void initFirstsActions(GameObject go)
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
			if (infiniteLoopDetected)
				GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.InfiniteLoop });
			else
			{
				if (atLeastOnePlayerAndScriptIsAssociated())
					GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.NoActionAvailableForExecution });
				else
					GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.NamingError });
            }
        }
		else
		{
			// init currentAction on the first action of ennemies
			bool forceNewStep = false;
			foreach (GameObject drone in f_drone)
			{
				ScriptRef scriptRef = drone.GetComponent<ScriptRef>();
				if (!scriptRef.executableScript.GetComponentInChildren<CurrentAction>(true) && !scriptRef.scriptFinished && !scriptRef.isBroken)
					addCurrentActionOnFirstAction(drone);
				else
					forceNewStep = true; // will move currentAction on next action
			}

			if (forceNewStep)
				onNewStep();
		}
	}

	private bool atLeastOnePlayerAndScriptIsAssociated()
    {
		foreach (Transform container in editableContainers)
			foreach (GameObject player in f_player)
				if (container.GetComponentInChildren<UIRootContainer>(true).scriptName.ToLower() == player.GetComponent<AgentEdit>().associatedScriptName.ToLower())
					return true;
		return false;
	}

	private GameObject addCurrentActionOnFirstAction(GameObject agent)
    {
		GameObject firstAction = null;
		// try to get the first action
		Transform container = agent.GetComponent<ScriptRef>().executableScript.transform;
		if (container.childCount > 1) // > 1 to jump the first child "Header"
			firstAction = getFirstActionOf(container.GetChild(1).gameObject, agent, false);

		if (firstAction != null)
		{
			// Set this action as CurrentAction
			GameObjectManager.addComponent<CurrentAction>(firstAction, new { agent = agent });
		}

		return firstAction;
	}

	// get first action inside "action"
	private GameObject getFirstActionOf(GameObject action, GameObject agent, bool simulation)
    {
		exploredScripItem = new HashSet<int>();
		infiniteLoopDetected = false;
		return rec_getFirstActionOf(action, agent, simulation);
	}

	// look for first action recursively, it could be control structure (if, for...)
	private GameObject rec_getFirstActionOf(GameObject action, GameObject agent, bool simulation)
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
				// check if condition is evaluated to true
				if (ifValid(ifCont.condition, agent))
				{
					// check if this IfControl includes a child
					if (ifCont.firstChild != null)
						// get first action of its first child (could be if, for...)
						return rec_getFirstActionOf(ifCont.firstChild, agent, simulation);
				}
				else
					// check if this If is an IfElseControl and includes a child
					if (ifCont is IfElseControl && (ifCont as IfElseControl).elseFirstChild != null)
                        // get first action of its else first child (could be if, for...)
                        return rec_getFirstActionOf((ifCont as IfElseControl).elseFirstChild, agent, simulation);
                
				// this if doesn't contain action on the selected branch => get first action of next action (could be if, for...)
				return rec_getFirstActionOf(ifCont.next, agent, simulation);
			}
			// check if action is a WhileControl
			else if (action.GetComponent<WhileControl>())
			{
				WhileControl whileCont = action.GetComponent<WhileControl>();
				// check if condition is evaluated to true
				if (ifValid(whileCont.condition, agent))
					// get first action of its first child (could be if, for...)
					return rec_getFirstActionOf(whileCont.firstChild, agent, simulation);
				else
					// this condition is false => get first action of next action (could be if, for...)
					return rec_getFirstActionOf(whileCont.next, agent, simulation);
			}
			// check if action is a ForControl
			else if (action.GetComponent<ForControl>())
			{
				ForControl forCont = action.GetComponent<ForControl>();
				TMP_InputField counter = forCont.GetComponentInChildren<TMP_InputField>(true);
				// pulse counter
				if (forCont.gameObject.activeInHierarchy)
					forCont.StartCoroutine(UtilityGame.pulseItem(counter.gameObject));
				// check if this ForControl include a child and nb iteration != 0 and end loop not reached
				if (forCont.firstChild != null && forCont.nbFor != 0 && forCont.currentFor < forCont.nbFor)
                {
					if (!simulation)
					{
						forCont.currentFor++;
						counter.text = (forCont.currentFor).ToString() + " / " + forCont.nbFor.ToString();
					}
					// get first action of its first child (could be if, for...)
					return rec_getFirstActionOf(forCont.firstChild, agent, simulation);
				}
				else
				{
					// this for doesn't contain action or nb iteration == 0 or end loop reached => get first action of next action (could be if, for...)
					if (forCont.currentFor >= forCont.nbFor && !simulation)
                    {
                        // reset nb iteration to 0
                        forCont.currentFor = 0;
						counter.text = (forCont.currentFor).ToString() + " / " + forCont.nbFor.ToString();
					}
					return rec_getFirstActionOf(forCont.next, agent, simulation);
				}
			}
			// check if action is a ForeverControl
			else if (action.GetComponent<ForeverControl>())
			{
				// always return firstchild of this ForeverControl
				return rec_getFirstActionOf(action.GetComponent<ForeverControl>().firstChild, agent, simulation);
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
		bool result = false;
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
				foreach (GameObject wall in f_walls)
					if (wall.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
					 wall.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y && wall.GetComponent<Renderer>() != null && wall.GetComponent<Renderer>().enabled)
					{
						result = true;
						break;
					}
				break;
			case "PathFront":
			case "PathLeft":
			case "PathRight":
				result = true;
				// check visible and invisible obstacles
				foreach (GameObject wall in f_walls)
					if (wall.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
						wall.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
					{
						result = false;
						break;
					}
                if (result)
                {
					// check doors closed
					foreach (GameObject door in f_door)
						if (door.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
							door.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y && !door.GetComponent<ActivationSlot>().state)
						{
							result = false;
							break;
						}
				}
				if (result)
				{
					// check furnitures
					foreach (GameObject furniture in f_furnitures)
						if (furniture.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
							furniture.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
						{
							result = false;
							break;
						}
				}
				if (result)
				{
					// check furnitures
					foreach (GameObject player in f_player)
						if (player.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
							player.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
						{
							result = false;
							break;
						}
				}
				break;
			case "FieldGate": // doors
				foreach (GameObject door in f_door)
					if (door.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
					 door.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y && !door.GetComponent<ActivationSlot>().state)
					{
						result = true;
						break;
					}
				break;
			case "Enemy": // enemies
				foreach (GameObject drone in f_drone)
					if (drone.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
						drone.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y && !drone.GetComponent<ScriptRef>().isBroken)
					{
						result = true;
						break;
					}
				break;
			case "Player":
				foreach (GameObject player in f_player)
					if (player.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
						player.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
					{
						result = true;
						break;
					}
				break;
			case "Furniture":
				foreach (GameObject furniture in f_furnitures)
					if (furniture.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
						furniture.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
					{
						result = true;
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
						result = true;
						break;
					}
				}
				break;
			case "RedArea": // detectors
				foreach (GameObject detector in f_redDetector)
					if (detector.GetComponent<Position>().x == agent.GetComponent<Position>().x + vec.x &&
					 detector.GetComponent<Position>().y == agent.GetComponent<Position>().y + vec.y)
					{
						result = true;
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
						result = true;
						break;
					}
				}
				break;
		}
		// notification de l'évaluation 
		GameObject notif = ele.target.transform.Find(result ? "true" : "false").gameObject;
		GameObjectManager.setGameObjectState(notif, true);
		MainLoop.instance.StartCoroutine(UtilityGame.pulseItem(notif));
		return result;

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
			nextAction = getNextAction(currentActionGO, currentAction.agent, false);
			// check if we reach last action of a drone
			if (nextAction == null && currentAction.agent.CompareTag("Drone"))
				currentAction.agent.GetComponent<ScriptRef>().scriptFinished = true;
			else if (nextAction != null && !currentAction.agent.GetComponent<ScriptRef>().isBroken)
			{
				//ask to add CurrentAction on next frame => this frame we remove current CurrentActions
				MainLoop.instance.StartCoroutine(delayAddCurrentAction(nextAction, currentAction.agent));
			}
			else if (infiniteLoopDetected)
				GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.InfiniteLoop });
			GameObjectManager.removeComponent(currentAction);
		}
	}

	// return the next action to execute, return null if no next action available
	private GameObject getNextAction(GameObject currentAction, GameObject agent, bool simulation){
		BasicAction current_ba = currentAction.GetComponent<BasicAction>();
		if (current_ba != null)
		{
			// if next is not defined or is a BasicAction we return it
			if(current_ba.next == null || current_ba.next.GetComponent<BasicAction>())
				return current_ba.next;
			else
				return getFirstActionOf(current_ba.next, agent, simulation);
        }
        // currentAction is not a BasicAction
        // check if it is a WhileControl
        else if (currentAction.GetComponent<WhileControl>())
        {
			if(ifValid(currentAction.GetComponent<WhileControl>().condition, agent))
            {
				if (currentAction.GetComponent<WhileControl>().firstChild == null || currentAction.GetComponent<WhileControl>().firstChild.GetComponent<BasicAction>())
					return currentAction.GetComponent<WhileControl>().firstChild;
				else
					return getFirstActionOf(currentAction.GetComponent<WhileControl>().firstChild, agent, simulation);
			}
            else
            {
				if (currentAction.GetComponent<WhileControl>().next == null || currentAction.GetComponent<WhileControl>().next.GetComponent<BasicAction>())
					return currentAction.GetComponent<WhileControl>().next;
				else
					return getFirstActionOf(currentAction.GetComponent<WhileControl>().next, agent, simulation);
			}
		}
		// check if it is a ForAction
		else if(currentAction.GetComponent<ForControl>()){
			ForControl forAct = currentAction.GetComponent<ForControl>();
			TMP_InputField counter = forAct.GetComponentInChildren<TMP_InputField>(true);
			// pulse counter
			forAct.StartCoroutine(UtilityGame.pulseItem(counter.gameObject));
			// ForAction reach the number of iterations
			if (forAct.currentFor >= forAct.nbFor){
				if (!simulation)
				{
					// reset nb iteration to 0
					forAct.currentFor = 0;
					counter.text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
				}
				// return next action
				if(forAct.next == null || forAct.next.GetComponent<BasicAction>())
					return forAct.next;
				else
					return getFirstActionOf(forAct.next , agent, simulation);
			}
			// iteration are available
			else{
				// in case ForAction has no child
				if (forAct.firstChild == null)
				{
					if (!simulation)
					{
						// reset nb iteration to 0
						forAct.currentFor = 0;
						counter.text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
					}
					// return next action
					if (forAct.next == null || forAct.next.GetComponent<BasicAction>())
						return forAct.next;
					else
						return getFirstActionOf(forAct.next, agent, simulation);
				}
				else
				// return first child
				{
					if (!simulation)
					{
						// add one iteration
						forAct.currentFor++;
						counter.text = (forAct.currentFor).ToString() + " / " + forAct.nbFor.ToString();
					}
					// return first child
					if (forAct.firstChild == null || forAct.firstChild.GetComponent<BasicAction>())
						return forAct.firstChild;
					else
						return getFirstActionOf(forAct.firstChild, agent, simulation);
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
					return getFirstActionOf(ifAction.firstChild, agent, simulation);
				else
					return getFirstActionOf(ifAction.next, agent, simulation);
			}
			else if (currentAction.GetComponent<IfElseControl>()) {
				IfElseControl ifElse = currentAction.GetComponent<IfElseControl>();
				// return first child
				if (ifElse.elseFirstChild != null && ifElse.elseFirstChild.GetComponent<BasicAction>())
					return ifElse.elseFirstChild;
				else if (ifElse.elseFirstChild != null)
					return getFirstActionOf(ifElse.elseFirstChild, agent, simulation);
				else
					return getFirstActionOf(ifAction.next, agent, simulation);
			}
			else
			{
				// return next action
				getFirstActionOf(ifAction.next, agent, simulation);
			}
		}
		// check if it is a ForeverAction
		else if(currentAction.GetComponent<ForeverControl>()){
			ForeverControl foreverAction = currentAction.GetComponent<ForeverControl>();
			if (foreverAction.firstChild == null || foreverAction.firstChild.GetComponent<BasicAction>())
				return foreverAction.firstChild;
			else
				return getFirstActionOf(foreverAction.firstChild, agent, simulation);
		}

		return null;
	}

	private IEnumerator delayAddCurrentAction(GameObject nextAction, GameObject agent)
	{
		yield return null; // we add new CurrentAction next frame otherwise families are not notified to this adding because at the begining of this frame GameObject already contains CurrentAction
		GameObjectManager.addComponent<CurrentAction>(nextAction, new { agent = agent });
    }
}