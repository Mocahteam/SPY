using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class CoinSystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void detectCollision(System.Boolean on)
	{
		MainLoop.callAppropriateSystemMethod (null, "detectCollision", on);
	}

}
