using UnityEngine;
using FYFY;

public class ObstableTransparencySystem_wrapper : BaseWrapper
{
	public UnityEngine.Camera playerCamera;
	public UnityEngine.LayerMask wallLayerMask;
	public System.Single transparencyAlpha;
	public System.Single fadeSpeed;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "playerCamera", playerCamera);
		MainLoop.initAppropriateSystemField (system, "wallLayerMask", wallLayerMask);
		MainLoop.initAppropriateSystemField (system, "transparencyAlpha", transparencyAlpha);
		MainLoop.initAppropriateSystemField (system, "fadeSpeed", fadeSpeed);
	}

}
