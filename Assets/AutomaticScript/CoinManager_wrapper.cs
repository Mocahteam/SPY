using UnityEngine;
using FYFY;

public class CoinManager_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void detectCollision(System.Boolean on)
	{
		MainLoop.callAppropriateSystemMethod (system, "detectCollision", on);
	}

}
