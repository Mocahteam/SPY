using UnityEngine;
using FYFY;

public class LobbyAnimation_wrapper : BaseWrapper
{
	public UnityEngine.GameObject Door;
	public UnityEngine.GameObject Drone;
	public UnityEngine.GameObject Kyle;
	public UnityEngine.GameObject Destiny;
	public UnityEngine.GameObject R102;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "Door", Door);
		MainLoop.initAppropriateSystemField (system, "Drone", Drone);
		MainLoop.initAppropriateSystemField (system, "Kyle", Kyle);
		MainLoop.initAppropriateSystemField (system, "Destiny", Destiny);
		MainLoop.initAppropriateSystemField (system, "R102", R102);
	}

}
