using System;
using System.Collections.Generic;
using UnityEngine;

public class PaintableGrid : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public Cell[,] grid;
	public Dictionary<Tuple<int, int>, FloorObject[]> floorObjects; // Pour chaque position sur la carte, contient les objets pouvant y être positionnés (3 layers)
}