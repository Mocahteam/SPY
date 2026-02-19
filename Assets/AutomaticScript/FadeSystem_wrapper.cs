using UnityEngine;
using FYFY;

public class FadeSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject fade;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "fade", fade);
	}

}
