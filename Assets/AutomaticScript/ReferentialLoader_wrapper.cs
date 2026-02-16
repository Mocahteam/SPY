using UnityEngine;
using FYFY;

public class ReferentialLoader_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void saveReferetialSelected(System.Int32 referentialId)
	{
		MainLoop.callAppropriateSystemMethod (system, "saveReferetialSelected", referentialId);
	}

}
