<<<<<<< HEAD
using UnityEngine;
=======
﻿using UnityEngine;
>>>>>>> d279ca19856d0c78f795c23fcc180dd5d9d63441
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
        public bool isActivated;
        public bool isFullyActivated;
        public List<int> slotID;
        public int side;
        public RawActivable(Activable act)
        {
            isActivated = act.isActivated;
            isFullyActivated = act.isFullyActivated;
            slotID = new List<int>(act.slotID);
            side = act.side;
        }
    }

    [Serializable]
<<<<<<< HEAD
    public class RawCurrentAction {
        public GameObject action;
        public GameObject agent;
        public RawCurrentAction(GameObject go){
            action = go;
            agent = go.GetComponent<CurrentAction>().agent;
        }
    }

    [Serializable]
=======
>>>>>>> d279ca19856d0c78f795c23fcc180dd5d9d63441
    public class RawSave
    {
        public List<bool> coinsState = new List<bool>();
        public List<bool> doorsState = new List<bool>();
        public List<Direction.Dir> directions = new List<Direction.Dir>();
        public List<RawPosition> positions = new List<RawPosition>();
        public List<RawActivable> activables = new List<RawActivable>();
<<<<<<< HEAD
        public List<RawCurrentAction> currentDroneActions = new List<RawCurrentAction>();
=======
>>>>>>> d279ca19856d0c78f795c23fcc180dd5d9d63441
    }

    public RawSave rawSave = new RawSave();
}