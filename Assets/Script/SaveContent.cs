using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Content of the progression saved
/// </summary>
[Serializable]
public class SaveContent {

    [Serializable]
    public class RawPosition {
        public int x;
        public int y;
        public RawPosition (Position pos)
        {
            x = Mathf.RoundToInt(pos.x);
            y = Mathf.RoundToInt(pos.y);
        }
    }

    [Serializable]
    public class RawActivationSlot
    {
        public bool state;
        public RawActivationSlot(ActivationSlot act)
        {
            state = act.state;
        }
    }

    [Serializable]
    public class RawScriptRef
    {
        public bool scriptFinished;
        public bool isBroken;
        public RawScriptRef(ScriptRef scriprRef)
        {
            scriptFinished = scriprRef.scriptFinished;
            isBroken = scriprRef.isBroken;
        }
    }

    [Serializable]
    public class RawCurrentAction {
        public GameObject action;
        public GameObject agent;
        public RawCurrentAction(GameObject go){
            action = go;
            agent = go.GetComponent<CurrentAction>().agent;
        }
    }

    [Serializable]
    public class RawLoop
    {
        public int currentFor;
        public int nbFor;
        public RawLoop(ForControl fc)
        {
            currentFor = fc.currentFor;
            nbFor = fc.nbFor;
        }
    }

    [Serializable]
    public class RawSave
    {
        public List<bool> coinsState = new List<bool>();
        public List<Direction.Dir> directions = new List<Direction.Dir>();
        public List<RawPosition> positions = new List<RawPosition>();
        public List<RawActivationSlot> doors = new List<RawActivationSlot>();
        public List<RawScriptRef> scriptRefs = new List<RawScriptRef>();
        public List<RawCurrentAction> currentDroneActions = new List<RawCurrentAction>();
        public List<RawLoop> currentLoopParams = new List<RawLoop>();
        public int totalCoin = 0;
    }

    public RawSave rawSave = new RawSave();
}
