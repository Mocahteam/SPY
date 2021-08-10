using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Content of the progression saved
/// </summary>
[Serializable]
public class SaveContent {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

    [Serializable]
    public class RawPosition {
        public int x;
        public int z;
        public RawPosition (Position pos)
        {
            x = pos.x;
            z = pos.z;
        }
    }

    [Serializable]
    public class RawActivable
    {
        public List<int> slotID;
        public RawActivable(Activable act)
        {
            slotID = new List<int>(act.slotID);
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
    public class RawSave
    {
        public List<bool> coinsState = new List<bool>();
        public List<bool> doorsState = new List<bool>();
        public List<Direction.Dir> directions = new List<Direction.Dir>();
        public List<RawPosition> positions = new List<RawPosition>();
        public List<RawActivable> activables = new List<RawActivable>();
        public List<RawCurrentAction> currentDroneActions = new List<RawCurrentAction>();
    }

    public RawSave rawSave = new RawSave();
}
