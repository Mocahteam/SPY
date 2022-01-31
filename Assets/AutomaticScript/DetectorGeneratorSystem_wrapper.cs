using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class DetectorGeneratorSystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void detectCollision(System.Boolean on)
	{
		MainLoop.callAppropriateSystemMethod (null, "detectCollision", on);
	}

	public void updateDetector()
	{
		MainLoop.callAppropriateSystemMethod (null, "updateDetector", null);
	}

}
