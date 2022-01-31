using UnityEngine;
using FYFY;

public class MoveSystem_wrapper : BaseWrapper
{
	public System.Single turnSpeed;
	public System.Single moveSpeed;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "turnSpeed", turnSpeed);
		MainLoop.initAppropriateSystemField (system, "moveSpeed", moveSpeed);
	}

}
