using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.PointerManager;

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

		if(type == Action.ActionType.For || type == Action.ActionType.If || type == Action.ActionType.IfElse || type == Action.ActionType.While)
			action.actions = new List<Action>();

		return action;
	}

	public static void resetScript(Script script){
		script.actions = new List<Action>();
		script.currentAction = 0;
	}

    public static void restartScript(Script script){
		script.currentAction = 0;
	}

    public static bool endOfScript(GameObject go){
		return go.GetComponent<Script>().currentAction >= go.GetComponent<Script>().actions.Count;
	}

    public static Action getCurrentAction(GameObject go) {
		Action action = go.GetComponent<Script>().actions[go.GetComponent<Script>().currentAction]; 
		//end when a pure action is found
		while(!(action.actionType == Action.ActionType.Forward || action.actionType == Action.ActionType.TurnLeft || action.actionType == Action.ActionType.TurnRight
				|| action.actionType == Action.ActionType.Wait)){
			//Case For
			if(action.actionType == Action.ActionType.For){
				action = action.actions[action.currentAction];
			}
		}

		return action;
	}

	public static void incrementActionScript(Script script){
		if(incrementAction(script.actions[script.currentAction]))
			script.currentAction++;
		if(script.currentAction >= script.actions.Count && script.repeat)
			script.currentAction = 0;
	}

    public static bool incrementAction(Action act){
		if(act.actionType == Action.ActionType.Forward || act.actionType == Action.ActionType.TurnLeft || act.actionType == Action.ActionType.TurnRight
			|| act.actionType == Action.ActionType.Wait)
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
		
		return false;
	}

    public static List<Action> ScriptContainerToActionList(GameObject scriptComposer){
		List<Action> l = new List<Action>();

		for(int i = 0; i< scriptComposer.transform.childCount; i++){
			GameObject child = scriptComposer.transform.GetChild(i).gameObject;
			if(child.GetComponent<UIActionType>().type == Action.ActionType.For){
				Action forAct = ActionManipulator.createAction(child.GetComponent<UIActionType>().type);
				forAct.nbFor = int.Parse(child.transform.GetChild(0).transform.GetChild(1).GetComponent<InputField>().text);
				if(forAct.nbFor > 0 && child.transform.childCount > 1 && ContainerToActionList(forAct, child))
					l.Add(forAct);

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
				forAct.nbFor = int.Parse(child.transform.GetChild(0).transform.GetChild(1).GetComponent<InputField>().text);
				if(forAct.nbFor > 0 && child.transform.childCount > 1 && ContainerToActionList(forAct, child)){
					ActionManipulator.addAction(act, forAct);
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

	public static void ScriptToContainer(Script script, GameObject container){

		foreach(Action action in script.actions){
			(ActionToContainer(action)).transform.SetParent(container.transform);
		}

		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)container.transform );

	}

	private static GameObject ActionToContainer(Action action){
		GameObject obj =  null;;
		switch(action.actionType){
			case Action.ActionType.Forward:
				obj = Object.Instantiate (Resources.Load ("Prefabs/ForwardActionBloc")) as GameObject;
				break;
			case Action.ActionType.TurnLeft:
				obj = Object.Instantiate (Resources.Load ("Prefabs/TurnLeftActionBloc Variant")) as GameObject;
				break;
			case Action.ActionType.TurnRight:
				obj = Object.Instantiate (Resources.Load ("Prefabs/TurnRightActionBloc Variant")) as GameObject;
				break;
			case Action.ActionType.Wait:
				obj = Object.Instantiate (Resources.Load ("Prefabs/WaitActionBloc Variant")) as GameObject;
				break;
			case Action.ActionType.For:
				obj = Object.Instantiate (Resources.Load ("Prefabs/ForBloc")) as GameObject;
				obj.transform.GetChild(0).GetChild(1).GetComponent<InputField>().text = action.nbFor.ToString();
				obj.transform.GetChild(0).GetChild(1).GetComponent<InputField>().interactable = false;
				Object.Destroy(obj.GetComponent<PointerSensitive>());
				Object.Destroy(obj.GetComponent<UITypeContainer>());
				foreach(Action act in action.actions){
					ActionToContainer(act).transform.SetParent(obj.transform);
				}
				break;
		}
		return obj;
	}
}
