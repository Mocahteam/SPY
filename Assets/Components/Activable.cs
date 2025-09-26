﻿using UnityEngine;
using System.Collections.Generic;

public class Activable : MonoBehaviour {
	public List<int> slotID; // target slot this component control
	public List<ActivationSlot> targets; // list of targets this component control
}