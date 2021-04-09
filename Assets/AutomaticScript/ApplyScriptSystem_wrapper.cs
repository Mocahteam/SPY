using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class ApplyScriptSystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void applyScriptToPlayer()
	{
		MainLoop.callAppropriateSystemMethod ("ApplyScriptSystem", "applyScriptToPlayer", null);
	}

	public void applyIfEntityType()
	{
		MainLoop.callAppropriateSystemMethod ("ApplyScriptSystem", "applyIfEntityType", null);
	}

}
