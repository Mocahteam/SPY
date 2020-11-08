using System.Collections.Generic;
public class Action {
	public int currentAction;
	public List<Action> actions;
	public enum ActionType {Forward, TurnLeft, TurnRight, Wait, If, IfElse, For, While};
	public ActionType actionType;  

	public int currentFor;
	public int nbFor;
}