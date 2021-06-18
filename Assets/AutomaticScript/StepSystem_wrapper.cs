using UnityEngine;
using FYFY;

[ExecuteInEditMode]
public class StepSystem_wrapper : MonoBehaviour
{
	private void Start()
	{
		this.hideFlags = HideFlags.HideInInspector; // Hide this component in Inspector
	}

	public void autoExecuteStep(System.Boolean on)
	{
		MainLoop.callAppropriateSystemMethod ("StepSystem", "autoExecuteStep", on);
	}

	public void goToNextStep()
	{
		MainLoop.callAppropriateSystemMethod ("StepSystem", "goToNextStep", null);
	}

	public void updateTotalStep()
	{
		MainLoop.callAppropriateSystemMethod ("StepSystem", "updateTotalStep", null);
	}

	public void speedTimeStep()
	{
		MainLoop.callAppropriateSystemMethod ("StepSystem", "speedTimeStep", null);
	}

	public void setToDefaultTimeStep()
	{
		MainLoop.callAppropriateSystemMethod ("StepSystem", "setToDefaultTimeStep", null);
	}

}
