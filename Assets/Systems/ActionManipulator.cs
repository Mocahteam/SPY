using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.PointerManager;
using TMPro;
public abstract class ActionManipulator
{
   public static void addAction(Script script, Action actionToAdd){
		if(script.actions == null)
			script.actions = new List<Action>();

		script.actions.Add(actionToAdd);
	}

	public static void addAction(Action action, Action actionToAdd){
		if(action.actions == null)
			action.actions = new List<Action>();

		action.actions.Add(actionToAdd);
	}

	public static Action createAction(Action.ActionType type, int nbFor = 0){
		Action action = new Action();
		action.actionType = type;
		action.currentAction = 0;
		action.currentFor = 0;
		action.nbFor = nbFor;
		action.ifValid = false;
		action.ifDirection = 0;
		action.ifEntityType = 0;
		action.range = 0;

		if(type == Action.ActionType.For || type == Action.ActionType.If || type == Action.ActionType.IfElse || type == Action.ActionType.While)
			action.actions = new List<Action>();

		return action;
	}

	//Empty the script
	public static void resetScript(Script script){
		script.actions = new List<Action>();
		script.currentAction = 0;
	}

	//restart the script to 0
    public static void restartScript(Script script){
		script.currentAction = 0;
		foreach(Action act in script.actions){
			restartScript(act);
		}
	}

	public static void restartScript(Action action){
		action.currentAction = 0;
		action.currentFor = 0;
		if(action.actions != null){
			foreach(Action act in action.actions){
				restartScript(act);
			}
		}
	}

	//Return true if the script is at the end
    public static bool endOfScript(GameObject go){
		return go.GetComponent<Script>().currentAction >= go.GetComponent<Script>().actions.Count;
	}

	//Return the current action
    public static Action getCurrentAction(GameObject go) {
		Action action = go.GetComponent<Script>().actions[go.GetComponent<Script>().currentAction]; 
		//end when a pure action is found
		while(!(action.actionType == Action.ActionType.Forward || action.actionType == Action.ActionType.TurnLeft || action.actionType == Action.ActionType.TurnRight
				|| action.actionType == Action.ActionType.Wait || action.actionType == Action.ActionType.Activate || action.actionType == Action.ActionType.TurnBack)){
			//Case For / If
			if(action.actionType == Action.ActionType.For || action.actionType == Action.ActionType.If){
				action = action.actions[action.currentAction];
			}
		}
		return action;
	}

	public static Action getCurrentIf(GameObject go){
		if(go.GetComponent<Script>().actions == null || go.GetComponent<Script>().currentAction >= go.GetComponent<Script>().actions.Count){
			return null;
		}
		Action action = go.GetComponent<Script>().actions[go.GetComponent<Script>().currentAction]; 
		//end when a pure action is found
		while(!(action.actionType == Action.ActionType.Forward || action.actionType == Action.ActionType.TurnLeft || action.actionType == Action.ActionType.TurnRight
				|| action.actionType == Action.ActionType.Wait || action.actionType == Action.ActionType.Activate || action.actionType == Action.ActionType.TurnBack)){
			//Case For / If
			if(action.actionType == Action.ActionType.For){
				if(action.currentAction >= action.actions.Count){
					return null;
				}
				action = action.actions[action.currentAction];
			}
			if(action.actionType == Action.ActionType.If){
				if(action.currentAction == 0 && !action.ifValid){
					return action;
				}
				else{
					if(action.currentAction >= action.actions.Count){
						return null;
					}
					action = action.actions[action.currentAction];
				}
			}
		}

		return null;
	}

	//increment the iterator of the action script
	public static void incrementActionScript(Script script){
		if(incrementAction(script.actions[script.currentAction]))
			script.currentAction++;
		if(script.currentAction >= script.actions.Count && script.repeat)
			script.currentAction = 0;
	}

    public static bool incrementAction(Action act){
		if(act.actionType == Action.ActionType.Forward || act.actionType == Action.ActionType.TurnLeft || act.actionType == Action.ActionType.TurnRight
			|| act.actionType == Action.ActionType.Wait || act.actionType == Action.ActionType.Activate || act.actionType == Action.ActionType.TurnBack)
			return true;
		//Case For
		else if(act.actionType == Action.ActionType.For){
			if(incrementAction(act.actions[act.currentAction]))
				act.currentAction++;

			if(act.currentAction >= act.actions.Count){
				act.currentAction = 0;
				act.currentFor++;
				//End of for
				if(act.currentFor >= act.nbFor){
					act.currentAction = 0;
					act.currentFor = 0;
					return true;
				}
			}
		}
		else if(act.actionType == Action.ActionType.If){
			if(incrementAction(act.actions[act.currentAction]))
				act.currentAction++;
			
			if(act.currentAction >= act.actions.Count){
				act.currentAction = 0;
				return true;
			}
		}
		
		return false;
	}

	//Return the lenght of the script
	public static int getNbStep(Script script){
		int nb = 0;
		foreach(Action act in script.actions){
			nb += getNbStep(act);
		}

		return nb;
	}

	public static int getNbStep(Action action, bool ignoreIf = false){
		if(action.actionType == Action.ActionType.For){
			int nb = 0;
			foreach(Action act in action.actions){
				nb += getNbStep(act) * action.nbFor;
			}
			return nb;
		}
		else if(action.actionType == Action.ActionType.If && !ignoreIf){
			return 0;
		}
		else if(action.actionType == Action.ActionType.If && ignoreIf){
			int nb = 0;
			foreach(Action act in action.actions){
				nb += getNbStep(act);
			}
			return nb;
		}
		else
			return 1;
	}


	//Convert the UI script in a usable script
    public static List<Action> ScriptContainerToActionList(GameObject scriptComposer){
		List<Action> l = new List<Action>();

		for(int i = 0; i< scriptComposer.transform.childCount; i++){
			GameObject child = scriptComposer.transform.GetChild(i).gameObject;
			if(child.GetComponent<UIActionType>().type == Action.ActionType.For){
				Action forAct = ActionManipulator.createAction(child.GetComponent<UIActionType>().type);
                forAct.nbFor = int.Parse(child.transform.GetChild(0).transform.GetChild(1).GetComponent<TMP_InputField>().text);
				if(forAct.nbFor > 0 && child.transform.childCount > 1 && ContainerToActionList(forAct, child))
					l.Add(forAct);

			}
			else if(child.GetComponent<UIActionType>().type == Action.ActionType.If){
				Action IfAct = ActionManipulator.createAction(child.GetComponent<UIActionType>().type);
				IfAct.ifEntityType = child.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<TMP_Dropdown>().value;
				IfAct.ifDirection = child.transform.GetChild(0).GetChild(1).GetChild(2).GetComponent<TMP_Dropdown>().value;
				IfAct.range = int.Parse(child.transform.GetChild(0).GetChild(1).GetChild(3).GetComponent<TMP_InputField>().text);
				IfAct.ifValid = false;
				IfAct.ifNot = (child.transform.GetChild(0).GetChild(2).GetComponent<TMP_Dropdown>().value == 1);
				if(child.transform.childCount > 1 && ContainerToActionList(IfAct, child))
					l.Add(IfAct);

			}
			else{
				l.Add(ActionManipulator.createAction(child.GetComponent<UIActionType>().type));
			}
		}
		return l;
	}

	public static bool ContainerToActionList(Action act, GameObject obj){

		bool nonEmpty = false;
		for(int i = 1; i < obj.transform.childCount; i++){
			GameObject child = obj.transform.GetChild(i).gameObject;
			if(child.GetComponent<UIActionType>().type == Action.ActionType.For){
				Action forAct = ActionManipulator.createAction(child.GetComponent<UIActionType>().type);
				forAct.nbFor = int.Parse(child.transform.GetChild(0).transform.GetChild(1).GetComponent<TMP_InputField>().text);
				if(forAct.nbFor > 0 && child.transform.childCount > 1 && ContainerToActionList(forAct, child)){
					ActionManipulator.addAction(act, forAct);
					nonEmpty = true;
				}
			}
			else if(child.GetComponent<UIActionType>().type == Action.ActionType.If){
				Action IfAct = ActionManipulator.createAction(child.GetComponent<UIActionType>().type);
				IfAct.ifEntityType = child.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<TMP_Dropdown>().value;
				IfAct.ifDirection = child.transform.GetChild(0).GetChild(1).GetChild(2).GetComponent<TMP_Dropdown>().value;
				IfAct.range = int.Parse(child.transform.GetChild(0).GetChild(1).GetChild(3).GetComponent<TMP_InputField>().text);
				IfAct.ifNot = (child.transform.GetChild(0).GetChild(2).GetComponent<TMP_Dropdown>().value == 1);
				IfAct.ifValid = false;
				if(child.transform.childCount > 1 && ContainerToActionList(IfAct, child)){
					ActionManipulator.addAction(act, IfAct);
					nonEmpty = true;
				}
			}
			else{
				ActionManipulator.addAction(act, ActionManipulator.createAction(child.GetComponent<UIActionType>().type));
				nonEmpty = true;
			}
		}
		return nonEmpty;
	}

	//Show the script in the container
	public static void ScriptToContainer(Script script, GameObject container, bool sensitive = false){
		int i = 0;
		foreach(Action action in script.actions){
			if(i == script.currentAction)
				ActionToContainer(action, true).transform.SetParent(container.transform, sensitive);
			else
				ActionToContainer(action, false).transform.SetParent(container.transform, sensitive);
			i++;
		}

		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)container.transform );

	}

	private static GameObject ActionToContainer(Action action, bool nextAction, bool sensitive = false){
		GameObject obj =  null;
		int i = 0;
		switch(action.actionType){
			case Action.ActionType.Forward:
				obj = Object.Instantiate (Resources.Load ("Prefabs/ForwardActionBloc")) as GameObject;
				if(nextAction){
					obj.GetComponent<Image>().color = Color.yellow;
				}
				break;
			case Action.ActionType.TurnLeft:
				obj = Object.Instantiate (Resources.Load ("Prefabs/TurnLeftActionBloc Variant")) as GameObject;
				if(nextAction){
					obj.GetComponent<Image>().color = Color.yellow;
				}
				break;
			case Action.ActionType.TurnRight:
				obj = Object.Instantiate (Resources.Load ("Prefabs/TurnRightActionBloc Variant")) as GameObject;
				if(nextAction){
					obj.GetComponent<Image>().color = Color.yellow;
				}
				break;
			case Action.ActionType.TurnBack:
				obj = Object.Instantiate (Resources.Load ("Prefabs/TurnBackActionBloc Variant")) as GameObject;
				if(nextAction){
					obj.GetComponent<Image>().color = Color.yellow;
				}
				break;
			case Action.ActionType.Wait:
				obj = Object.Instantiate (Resources.Load ("Prefabs/WaitActionBloc Variant")) as GameObject;
				if(nextAction){
					obj.GetComponent<Image>().color = Color.yellow;
				}
				break;
			case Action.ActionType.Activate:
				obj = Object.Instantiate (Resources.Load ("Prefabs/ActivateActionBloc Variant")) as GameObject;
				if(nextAction){
					obj.GetComponent<Image>().color = Color.yellow;
				}
				break;
			case Action.ActionType.For:
				obj = Object.Instantiate (Resources.Load ("Prefabs/ForBloc")) as GameObject;
				obj.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (action.currentFor +1).ToString() + " / " + action.nbFor.ToString();
				obj.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().interactable = false;
				Object.Destroy(obj.GetComponent<UITypeContainer>());
				i = 0;
				foreach(Action act in action.actions){
					if(i == action.currentAction && nextAction)
						ActionToContainer(act, true).transform.SetParent(obj.transform);
					else
						ActionToContainer(act, false).transform.SetParent(obj.transform);
					i++;
				}
				break;
			case Action.ActionType.If:
				obj = Object.Instantiate (Resources.Load ("Prefabs/IfDetectBloc")) as GameObject;
				obj.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<TMP_Dropdown>().value = action.ifEntityType;
				obj.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<TMP_Dropdown>().interactable = false;
				obj.transform.GetChild(0).GetChild(1).GetChild(2).GetComponent<TMP_Dropdown>().value = action.ifDirection;
				obj.transform.GetChild(0).GetChild(1).GetChild(2).GetComponent<TMP_Dropdown>().interactable = false;
				obj.transform.GetChild(0).GetChild(1).GetChild(3).GetComponent<TMP_InputField>().text = action.range.ToString();
				obj.transform.GetChild(0).GetChild(1).GetChild(3).GetComponent<TMP_InputField>().interactable = false;
				if(!action.ifNot)
					obj.transform.GetChild(0).GetChild(2).GetComponent<TMP_Dropdown>().value = 0;
				else
					obj.transform.GetChild(0).GetChild(2).GetComponent<TMP_Dropdown>().value = 1;
				obj.transform.GetChild(0).GetChild(2).GetComponent<TMP_Dropdown>().interactable = false;
				Object.Destroy(obj.GetComponent<UITypeContainer>());
				i = 0;
				foreach(Action act in action.actions){
					if(i == action.currentAction && nextAction)
						ActionToContainer(act, true).transform.SetParent(obj.transform);
					else
						ActionToContainer(act, false).transform.SetParent(obj.transform);
					i++;
				}
				break;
		}
		Object.Destroy(obj.GetComponent<PointerSensitive>());
		return obj;
	}


	//0 Forward, 1 Backward, 2 Left, 3 Right
	public static Direction.Dir getDirection(Direction.Dir dirEntity, int relativeDir){
		if(relativeDir == 0)
			return dirEntity;
		switch(dirEntity){
			case Direction.Dir.North:
				switch(relativeDir){
					case 1:
						return Direction.Dir.South;
					case 2:
						return Direction.Dir.West;
					case 3:
						return Direction.Dir.East;
				}
				break;
			case Direction.Dir.West:
				switch(relativeDir){
					case 1:
						return Direction.Dir.East;
					case 2:
						return Direction.Dir.South;
					case 3:
						return Direction.Dir.North;
				}
				break;
			case Direction.Dir.East:
				switch(relativeDir){
					case 1:
						return Direction.Dir.West;
					case 2:
						return Direction.Dir.North;
					case 3:
						return Direction.Dir.South;
				}
				break;
			case Direction.Dir.South:
				switch(relativeDir){
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

	public static void invalidAllIf(Script script){
		foreach(Action act in script.actions){
			if(act.actionType == Action.ActionType.If)
				act.ifValid = false;
			invalidAllIf(act);
		}
	}
	public static void invalidAllIf(Action action){
		if(action.actions != null){
			foreach(Action act in action.actions){
				if(act.actionType == Action.ActionType.If)
					act.ifValid = false;
				invalidAllIf(act);
			}
		}
	}


	public static void updateActionBlocLimit(GameData gameData, Action.ActionType type, int nb){
		switch(type){
			case Action.ActionType.Forward:
				gameData.actionBlocLimit[0] += nb;
				break;
			case Action.ActionType.TurnLeft:
				gameData.actionBlocLimit[1] += nb;
				break;
			case Action.ActionType.TurnRight:
				gameData.actionBlocLimit[2] += nb;
				break;
			case Action.ActionType.Wait:
				gameData.actionBlocLimit[3] += nb;
				break;
			case Action.ActionType.Activate:
				gameData.actionBlocLimit[4] += nb;
				break;
			case Action.ActionType.For:
				gameData.actionBlocLimit[5] += nb;
				break;
			case Action.ActionType.If:
				gameData.actionBlocLimit[6] += nb;
				break;
			case Action.ActionType.TurnBack:
				gameData.actionBlocLimit[7] += nb;
				break;
			default:
				break;
		}
	}
}
