using UnityEngine;

public class BaseCaptor : BaseCondition {
    public enum CaptorType { Wall, Enemy, RedArea, FieldGate, Terminal, Exit }; 
    public CaptorType captorType; // Identifie quel est le block
}