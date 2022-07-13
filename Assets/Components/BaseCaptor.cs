using UnityEngine;

public class BaseCaptor : BaseCondition {
    public enum CaptorType { Wall, Enemie, RedArea, FieldGate, Terminal, Exit }; 
    public CaptorType captorType; // Identifie quel est le block
}