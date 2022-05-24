using UnityEngine;

public class ContainerActionBloc : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public bool blockSpecial = false; // Determine si le container et un container présent au sein d'un bloc If ou for
	public bool containerCondition = false; // Determine si le container doit contenir des conditons et opérateurs uniquement ou bien seulement des actions
}