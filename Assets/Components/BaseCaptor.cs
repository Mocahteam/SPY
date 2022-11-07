using UnityEngine;

public class BaseCaptor : BaseCondition {
    public enum CaptorType { WallFront, WallLeft, WallRight, Enemy, RedArea, FieldGate, Terminal, Exit }; 
    public CaptorType captorType; // Identifie quel est le block
}