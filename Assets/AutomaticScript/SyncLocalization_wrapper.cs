using UnityEngine;
using FYFY;

public class SyncLocalization_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void syncLocale()
	{
		MainLoop.callAppropriateSystemMethod (system, "syncLocale", null);
	}

}
