using UnityEngine;
using FYFY;

public class HighLightSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject dialogPanel;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "dialogPanel", dialogPanel);
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
