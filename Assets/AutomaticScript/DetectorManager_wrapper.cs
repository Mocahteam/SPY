using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class DetectorManager_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void detectCollision(System.Boolean on)
	{
		MainLoop.callAppropriateSystemMethod ("DetectorManager", "detectCollision", on);
	}

	public void updateDetector()
	{
		MainLoop.callAppropriateSystemMethod ("DetectorManager", "updateDetector", null);
	}

}
