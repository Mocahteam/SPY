using UnityEngine;

[DisallowMultipleComponent]
public class BaseElement : Highlightable {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    
    // the next item to execute
    public GameObject next;
}