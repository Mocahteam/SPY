using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicVariable : BaseElement
{
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    //Ajouter ici une nouvelle action type init variable
    public enum VariableType { Int };
    public VariableType variableType;
}
