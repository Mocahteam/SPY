
using System;
using UnityEngine;

[Serializable]
public class ConditionItem
{
    public string key;
    public GameObject target;

    public ConditionItem(string key, GameObject target)
    {
        this.key = key;
        this.target = target;
    }
}
