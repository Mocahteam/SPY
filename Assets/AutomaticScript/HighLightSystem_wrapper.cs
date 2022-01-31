using UnityEngine;
using FYFY;

public class HighLightSystem_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void highLightItem(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "highLightItem", go);
	}

	public void unHighLightItem(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "unHighLightItem", go);
	}

}
