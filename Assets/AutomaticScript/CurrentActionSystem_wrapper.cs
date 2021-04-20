using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class CurrentActionSystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void updateCurrentAction(UnityEngine.GameObject currentAction)
	{
		MainLoop.callAppropriateSystemMethod ("CurrentActionSystem", "updateCurrentAction", currentAction);
	}

	public void firstAction()
	{
		MainLoop.callAppropriateSystemMethod ("CurrentActionSystem", "firstAction", null);
	}

}
