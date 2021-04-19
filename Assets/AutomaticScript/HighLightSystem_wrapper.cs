using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class HighLightSystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void highLightItem(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod ("HighLightSystem", "highLightItem", go);
	}

	public void unHighLightItem(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod ("HighLightSystem", "unHighLightItem", go);
	}

}
