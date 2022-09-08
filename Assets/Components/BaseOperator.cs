using UnityEngine;

public class BaseOperator : BaseCondition {
    public enum OperatorType { AndOperator, OrOperator, NotOperator}; 
    public OperatorType operatorType; // Identifie quel est le type de bloc
}