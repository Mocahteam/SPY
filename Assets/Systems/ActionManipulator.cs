using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FYFY;
using FYFY_plugins.PointerManager;
using TMPro;

public abstract class ActionManipulator
{
	//private static Color baseColor;
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

	//Convert the UI script in a usable script //used in applyscriptsys & uisys
    public static List<Action> ScriptContainerToActionList(GameObject scriptComposer){
		List<Action> l = new List<Action>();

		for(int i = 0; i< scriptComposer.transform.childCount; i++){
			GameObject child = scriptComposer.transform.GetChild(i).gameObject;
			if(child.GetComponent<UIActionType>().type == Action.ActionType.For){
				Action forAct = ActionManipulator.createAction(child.GetComponent<UIActionType>().type);
                forAct.nbFor = int.Parse(child.transform.GetChild(0).transform.GetChild(1).GetComponent<TMP_InputField>().text);
				if(forAct.nbFor >= 0 && ContainerToActionList(forAct, child)){
					Debug.Log("add for");
					l.Add(forAct);
				}
			}
			else if(child.GetComponent<UIActionType>().type == Action.ActionType.If){
				Action IfAct = ActionManipulator.createAction(child.GetComponent<UIActionType>().type);
				IfAct.ifEntityType = child.transform.GetChild(0).Find("DropdownEntityType").GetComponent<TMP_Dropdown>().value;
				IfAct.ifDirection = child.transform.GetChild(0).Find("DropdownDirection").GetComponent<TMP_Dropdown>().value;
				IfAct.range = int.Parse(child.transform.GetChild(0).Find("InputFieldRange").GetComponent<TMP_InputField>().text);
				IfAct.ifValid = false;
				IfAct.ifNot = (child.transform.GetChild(0).Find("DropdownIsOrIsNot").GetComponent<TMP_Dropdown>().value == 1);
				if(ContainerToActionList(IfAct, child))
					l.Add(IfAct);

			}
			else{
				l.Add(ActionManipulator.createAction(child.GetComponent<UIActionType>().type));
			}
		}
		return l;
	}

	public static bool ContainerToActionList(Action act, GameObject obj){
		//Debug.Log("ContainerToActionList");
		bool nonEmpty = false;
		for(int i = 0; i < obj.transform.childCount; i++){
			//Debug.Log("for");
			GameObject child = obj.transform.GetChild(i).gameObject;
			if (child.GetComponent<UIActionType>() != null){
				if(child.GetComponent<UIActionType>().type == Action.ActionType.For){
					//Debug.Log("if for");
					Action forAct = ActionManipulator.createAction(child.GetComponent<UIActionType>().type);
					forAct.nbFor = int.Parse(child.transform.GetChild(0).transform.GetChild(1).GetComponent<TMP_InputField>().text);
					if(forAct.nbFor >= 0 && ContainerToActionList(forAct, child)){
						//Debug.Log("add action");
						ActionManipulator.addAction(act, forAct);
						nonEmpty = true;
					}
				}
				else if(child.GetComponent<UIActionType>().type == Action.ActionType.If){
					Action IfAct = ActionManipulator.createAction(child.GetComponent<UIActionType>().type);
					IfAct.ifEntityType = child.transform.GetChild(0).Find("DropdownEntityType").GetComponent<TMP_Dropdown>().value;
					IfAct.ifDirection = child.transform.GetChild(0).Find("DropdownDirection").GetComponent<TMP_Dropdown>().value;
					IfAct.range = int.Parse(child.transform.GetChild(0).Find("InputFieldRange").GetComponent<TMP_InputField>().text);
					IfAct.ifValid = false;
					IfAct.ifNot = (child.transform.GetChild(0).Find("DropdownIsOrIsNot").GetComponent<TMP_Dropdown>().value == 1);
					if(ContainerToActionList(IfAct, child)){
						ActionManipulator.addAction(act, IfAct);
						nonEmpty = true;
					}
				}
				else{
					//Debug.Log("else");
					ActionManipulator.addAction(act, ActionManipulator.createAction(child.GetComponent<UIActionType>().type));
					nonEmpty = true;
				}		
			}
			else{
				nonEmpty = true;
			}

		}
		return nonEmpty;
	}
	/*
	public static Color getBaseColor(){
		return baseColor;
	}*/
	//used in highlightsys & levelgeneratorsys
	public static GameObject ActionToContainer(Action action, bool nextAction, bool sensitive = false, bool isExecutableScriptDisplay = false){
		GameObject obj =  null;
		int i = 0;
		GameObject prefab = null;
		switch(action.actionType){
			case Action.ActionType.Forward:
				prefab = Resources.Load("Prefabs/ForwardActionBloc") as GameObject;
				obj = Object.Instantiate (prefab);
				obj.GetComponent<UIActionType>().linkedTo = GameObject.Find("Forward");
				break;
			case Action.ActionType.TurnLeft:
				prefab = Resources.Load ("Prefabs/TurnLeftActionBloc Variant") as GameObject;
				obj = Object.Instantiate (prefab);
				obj.GetComponent<UIActionType>().linkedTo = GameObject.Find("TurnLeft");
				break;
			case Action.ActionType.TurnRight:
				prefab = Resources.Load ("Prefabs/TurnRightActionBloc Variant") as GameObject;
				obj = Object.Instantiate (prefab);
				obj.GetComponent<UIActionType>().linkedTo = GameObject.Find("TurnRight");
				break;
			case Action.ActionType.TurnBack:
				prefab = Resources.Load ("Prefabs/TurnBackActionBloc Variant") as GameObject;
				obj = Object.Instantiate (prefab);
				obj.GetComponent<UIActionType>().linkedTo = GameObject.Find("TurnBack");
				break;
			case Action.ActionType.Wait:
				prefab = Resources.Load ("Prefabs/WaitActionBloc Variant")  as GameObject;
				obj = Object.Instantiate (prefab);
				obj.GetComponent<UIActionType>().linkedTo = GameObject.Find("Wait");
				break;
			case Action.ActionType.Activate:
				prefab = Resources.Load ("Prefabs/ActivateActionBloc Variant") as GameObject;
				obj = Object.Instantiate (prefab);
				obj.GetComponent<UIActionType>().linkedTo = GameObject.Find("Activate");
				break;
			case Action.ActionType.For:
				prefab = Resources.Load ("Prefabs/ForBloc") as GameObject;
				obj = Object.Instantiate (prefab);
				obj.GetComponent<UIActionType>().linkedTo = GameObject.Find("For");
				//TMP_InputField input = obj.GetComponentInChildren<TMP_InputField>();
				//Debug.Log("aaaaaaaaaaaaaa");
				//Debug.Log(input.name);
				if(isExecutableScriptDisplay){ //executable for loop display
					obj.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = action.nbFor.ToString();
					obj.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().interactable = true;
				}
				else{ //execution script display
					if (action.nbFor > 0)
						obj.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (action.currentFor +1).ToString() + " / " + action.nbFor.ToString();
					else
						obj.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().text = (action.currentFor).ToString() + " / " + action.nbFor.ToString();
					obj.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().interactable = false;
					Object.Destroy(obj.GetComponent<UITypeContainer>());
				}
				
				i = 0;
				foreach(Action act in action.actions){
					if(i == action.currentAction && nextAction)
						ActionToContainer(act, true, sensitive, isExecutableScriptDisplay).transform.SetParent(obj.transform);
					else
						ActionToContainer(act, false, sensitive, isExecutableScriptDisplay).transform.SetParent(obj.transform);
					i++;
				}
				break;
			case Action.ActionType.If:
				prefab = Resources.Load ("Prefabs/IfDetectBloc") as GameObject;
				obj = Object.Instantiate (prefab);
				obj.GetComponent<UIActionType>().linkedTo = GameObject.Find("If");

				obj.transform.GetChild(0).Find("DropdownEntityType").GetComponent<TMP_Dropdown>().value = action.ifEntityType;
				obj.transform.GetChild(0).Find("DropdownDirection").GetComponent<TMP_Dropdown>().value = action.ifDirection;
				obj.transform.GetChild(0).Find("InputFieldRange").GetComponent<TMP_InputField>().text = action.range.ToString();
				
				if(!action.ifNot)
					obj.transform.GetChild(0).Find("DropdownIsOrIsNot").GetComponent<TMP_Dropdown>().value = 0;
				else
					obj.transform.GetChild(0).Find("DropdownIsOrIsNot").GetComponent<TMP_Dropdown>().value = 1;
				
				if(isExecutableScriptDisplay){ //executable if display
					
					obj.transform.GetChild(0).Find("DropdownEntityType").GetComponent<TMP_Dropdown>().interactable = true;
					obj.transform.GetChild(0).Find("DropdownDirection").GetComponent<TMP_Dropdown>().interactable = true;
					obj.transform.GetChild(0).Find("InputFieldRange").GetComponent<TMP_InputField>().interactable = true;
					obj.transform.GetChild(0).Find("DropdownIsOrIsNot").GetComponent<TMP_Dropdown>().interactable = true;
				}
				else{ //execution script display

					obj.transform.GetChild(0).Find("DropdownEntityType").GetComponent<TMP_Dropdown>().interactable = false;
					obj.transform.GetChild(0).Find("DropdownDirection").GetComponent<TMP_Dropdown>().interactable = false;
					obj.transform.GetChild(0).Find("InputFieldRange").GetComponent<TMP_InputField>().interactable = false;
					obj.transform.GetChild(0).Find("DropdownIsOrIsNot").GetComponent<TMP_Dropdown>().interactable = false;
					
					Object.Destroy(obj.GetComponent<UITypeContainer>());
				}
				
				i = 0;
				foreach(Action act in action.actions){
					if(i == action.currentAction && nextAction)
						ActionToContainer(act, true, sensitive, isExecutableScriptDisplay).transform.SetParent(obj.transform);
					else
						ActionToContainer(act, false, sensitive, isExecutableScriptDisplay).transform.SetParent(obj.transform);
					i++;
				}
				break;
		}
		obj.GetComponent<UIActionType>().prefab = prefab;
		//Debug.Log("actionmanipulator obj "+obj.name);
		action.target = obj;
		//if(action.actionType != Action.ActionType.For && action.actionType != Action.ActionType.If)
		if(!isExecutableScriptDisplay){ //execution script display
			Object.Destroy(obj.GetComponent<PointerSensitive>());
		}	
		return obj;
	}

/*
	//used in levelgeneratorsys & uisys
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
	*/
}
