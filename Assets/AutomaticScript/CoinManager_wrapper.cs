using UnityEngine;
using FYFY;

public class CoinManager_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void cancelAllDestroyCoroutines()
	{
		MainLoop.callAppropriateSystemMethod (system, "cancelAllDestroyCoroutines", null);
	}

}
