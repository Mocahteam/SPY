using UnityEngine;
using FYFY;

public class StepSystem_wrapper : BaseWrapper
{
	public UnityEngine.RectTransform editableContainers;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "editableContainers", editableContainers);
	}

	public void autoExecuteStep(System.Boolean on)
	{
		MainLoop.callAppropriateSystemMethod (system, "autoExecuteStep", on);
	}

	public void goToNextStep()
	{
		MainLoop.callAppropriateSystemMethod (system, "goToNextStep", null);
	}

	public void cancelTotalStep()
	{
		MainLoop.callAppropriateSystemMethod (system, "cancelTotalStep", null);
	}

	public void speedTimeStep()
	{
		MainLoop.callAppropriateSystemMethod (system, "speedTimeStep", null);
	}

	public void setToDefaultTimeStep()
	{
		MainLoop.callAppropriateSystemMethod (system, "setToDefaultTimeStep", null);
	}

	public void startStepImmediate()
	{
		MainLoop.callAppropriateSystemMethod (system, "startStepImmediate", null);
	}

}
