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
		MainLoop.callAppropriateSystemMethod ("DetectorGeneratorSystem", "detectCollision", on);
	}

	public void updateDetector()
	{
		MainLoop.callAppropriateSystemMethod ("DetectorGeneratorSystem", "updateDetector", null);
	}

}
