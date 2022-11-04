using UnityEngine;

public class NewEnd : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public static int Detected = 1;
	public static int Win = 2;
	public static int BadCondition = 3;
	public static int NoMoreAttempt = 4;
	public static int NoAction = 5;
	public static int InfiniteLoop = 6;
	public static int Error = 7;

	public int endType;
}