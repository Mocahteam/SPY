using UnityEngine;
using FYFY;

public class SendStatements_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void initGBLXAPI()
	{
		MainLoop.callAppropriateSystemMethod (system, "initGBLXAPI", null);
	}

}
