using UnityEngine;
using FYFY;

public class CoinManager_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void CancelAllDestroyCoroutines()
	{
		MainLoop.callAppropriateSystemMethod (system, "CancelAllDestroyCoroutines", null);
	}

}
