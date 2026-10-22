using UnityEngine;
using FYFY;

public class StepSystem_wrapper : BaseWrapper
{
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
	}

	public void AutoExecuteStep(System.Boolean on)
	{
		MainLoop.callAppropriateSystemMethod (system, "AutoExecuteStep", on);
	}

	public void GoToNextStep()
	{
		MainLoop.callAppropriateSystemMethod (system, "GoToNextStep", null);
	}

	public void CancelTotalStep()
	{
		MainLoop.callAppropriateSystemMethod (system, "CancelTotalStep", null);
	}

	public void SpeedTimeStep()
	{
		MainLoop.callAppropriateSystemMethod (system, "SpeedTimeStep", null);
	}

	public void SetToDefaultTimeStep()
	{
		MainLoop.callAppropriateSystemMethod (system, "SetToDefaultTimeStep", null);
	}

}
