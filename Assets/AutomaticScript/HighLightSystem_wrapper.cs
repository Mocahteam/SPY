using UnityEngine;
using FYFY;

public class HighLightSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject dialogPanel;
	public UnityEngine.AudioClip activationSound;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "dialogPanel", dialogPanel);
		MainLoop.initAppropriateSystemField (system, "activationSound", activationSound);
	}

}
