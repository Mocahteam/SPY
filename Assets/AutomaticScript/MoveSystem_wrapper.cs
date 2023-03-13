using UnityEngine;
using FYFY;

public class MoveSystem_wrapper : BaseWrapper
{
	public System.Single turnSpeed;
	public System.Single moveSpeed;
	public UnityEngine.AudioClip footSlow;
	public UnityEngine.AudioClip footSpeed;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "turnSpeed", turnSpeed);
		MainLoop.initAppropriateSystemField (system, "moveSpeed", moveSpeed);
		MainLoop.initAppropriateSystemField (system, "footSlow", footSlow);
		MainLoop.initAppropriateSystemField (system, "footSpeed", footSpeed);
	}

	public void idleAnimations()
	{
		MainLoop.callAppropriateSystemMethod (system, "idleAnimations", null);
	}

}
