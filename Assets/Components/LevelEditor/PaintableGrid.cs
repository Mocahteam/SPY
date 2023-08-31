using System;
using System.Collections.Generic;
using UnityEngine;

public class PaintableGrid : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public Cell[,] grid;
	public Cell activeBrush;
	public Dictionary<Tuple<int, int>, FloorObject> floorObjects;
	public FloorObject selectedObject;
	public bool gridActive;
}