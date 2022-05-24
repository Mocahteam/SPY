using UnityEngine;

public class BaseCondition : Highlightable {
    public enum ConditionType { AndOperator, OrOperator, NotOperator, Wall, Enemie, RedArea, FieldGate, Terminal };
    public ConditionType conditionType;
    public enum blockType { Operator, Element };
    public blockType Type;
}