using System.Collections.Generic;
public class Action {
	public int currentAction;
	public List<Action> actions;
	public enum ActionType {Forward, TurnLeft, TurnRight, Wait, Activate, If, IfElse, For, While, Detect, TurnBack};
	public ActionType actionType;  

	public int currentFor;
	public int nbFor;

	//If Attributs
	public int ifDirection;
	public int ifEntityType;
	public int range;
	public bool ifValid;
	public bool ifNot;
}