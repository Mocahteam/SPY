using UnityEngine;
using FYFY;

public class FollowMouseSystem_wrapper : BaseWrapper
{
	public UnityEngine.Camera camera;
	public UnityEngine.GameObject leftMenu;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "camera", camera);
		MainLoop.initAppropriateSystemField (system, "leftMenu", leftMenu);
	}

}
