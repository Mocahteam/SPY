using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class CurrentActionSystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void firstAction(UnityEngine.GameObject buttonStop)
	{
		MainLoop.callAppropriateSystemMethod (null, "firstAction", buttonStop);
	}

}
