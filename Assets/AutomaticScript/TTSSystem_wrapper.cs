using UnityEngine;
using FYFY;

public class TTSSystem_wrapper : BaseWrapper
{
	public UnityEngine.EventSystems.EventSystem eventSystem;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "eventSystem", eventSystem);
	}

}
