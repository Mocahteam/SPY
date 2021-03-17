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

	public void incrementActionScript(Script script)
	{
		MainLoop.callAppropriateSystemMethod ("ApplyScriptSystem", "incrementActionScript", script);
	}

	public void invalidAllIf(Script script)
	{
		MainLoop.callAppropriateSystemMethod ("ApplyScriptSystem", "invalidAllIf", script);
	}

	public void invalidAllIf(Action action)
	{
		MainLoop.callAppropriateSystemMethod ("ApplyScriptSystem", "invalidAllIf", action);
	}

}
