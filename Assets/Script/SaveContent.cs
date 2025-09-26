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
            x = pos.x;
            y = pos.y;
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
        public List<RawCurrentAction> currentDroneActions = new List<RawCurrentAction>();
        public List<RawLoop> currentLoopParams = new List<RawLoop>();
        public int totalCoin = 0;
    }

    public RawSave rawSave = new RawSave();
}
