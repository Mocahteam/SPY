using UnityEngine;
using FYFY;

public class FollowMouseSystem_wrapper : BaseWrapper
{
	public GameData prefabGameData;
	public UnityEngine.Camera camera;
	public UnityEngine.GameObject leftMenu;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "prefabGameData", prefabGameData);
		MainLoop.initAppropriateSystemField (system, "camera", camera);
		MainLoop.initAppropriateSystemField (system, "leftMenu", leftMenu);
	}

}
