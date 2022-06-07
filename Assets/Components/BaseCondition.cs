using UnityEngine;

public class BaseCondition : Highlightable {
    public enum ConditionType { AndOperator, OrOperator, NotOperator, Wall, Enemie, RedArea, FieldGate, Terminal }; 
    public ConditionType conditionType; // Identifie quel est le block
    public enum blockType { Operator, Element };
    public blockType Type; // Identifie si c'est un block operator ou un élément
}