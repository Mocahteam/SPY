using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class UITrigger
{
    public MonoBehaviour interactableUI;
    public bool requireActiveInHierarchy;
    public bool requireInteractable;
}

[Serializable]
public class HotKeySpec
{
    public string inputActionName;
    public List<UITrigger> UItriggers;

    [HideInInspector]
    public InputAction inputAction;
    [HideInInspector]
    public bool hasModifier;
}
public class HotKeysSpec : MonoBehaviour
{
    public List<HotKeySpec> spec;
}
