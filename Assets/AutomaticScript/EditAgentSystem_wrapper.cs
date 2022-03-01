using UnityEngine;
using FYFY;

public class EditAgentSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject agentCanvas;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "agentCanvas", agentCanvas);
	}

}
