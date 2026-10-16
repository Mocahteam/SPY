using UnityEngine;
using FYFY;

public class AutoClickSystem_wrapper : BaseWrapper
{
	public CurrentSettingsValues cs;
	public UnityEngine.GameObject tempoUI;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "cs", cs);
		MainLoop.initAppropriateSystemField (system, "tempoUI", tempoUI);
	}

}
