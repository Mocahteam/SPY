using UnityEngine;
using FYFY;

public class EditorLevelDataSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject executionLimitContainer;
	public UnityEngine.UI.Toggle dragAndDropToggle;
	public UnityEngine.UI.Toggle fogToggle;
	public UnityEngine.UI.Toggle hideExitsToggle;
	public TMPro.TMP_InputField score2Input;
	public TMPro.TMP_InputField score3Input;
	public UnityEngine.Transform editableContainers;
	public UnityEngine.Color hideColor;
	public UnityEngine.Color limitedColor;
	public UnityEngine.Color unlimitedColor;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "executionLimitContainer", executionLimitContainer);
		MainLoop.initAppropriateSystemField (system, "dragAndDropToggle", dragAndDropToggle);
		MainLoop.initAppropriateSystemField (system, "fogToggle", fogToggle);
		MainLoop.initAppropriateSystemField (system, "hideExitsToggle", hideExitsToggle);
		MainLoop.initAppropriateSystemField (system, "score2Input", score2Input);
		MainLoop.initAppropriateSystemField (system, "score3Input", score3Input);
		MainLoop.initAppropriateSystemField (system, "editableContainers", editableContainers);
		MainLoop.initAppropriateSystemField (system, "hideColor", hideColor);
		MainLoop.initAppropriateSystemField (system, "limitedColor", limitedColor);
		MainLoop.initAppropriateSystemField (system, "unlimitedColor", unlimitedColor);
	}

	public void resetMetaData()
	{
		MainLoop.callAppropriateSystemMethod (system, "resetMetaData", null);
	}

	public void preventMinusSign(TMPro.TMP_InputField input)
	{
		MainLoop.callAppropriateSystemMethod (system, "preventMinusSign", input);
	}

	public void executionLimitChanged(System.Boolean newState)
	{
		MainLoop.callAppropriateSystemMethod (system, "executionLimitChanged", newState);
	}

	public void scoreTwoStarsExit(System.String newData)
	{
		MainLoop.callAppropriateSystemMethod (system, "scoreTwoStarsExit", newData);
	}

	public void scoreThreeStarsExit(System.String newData)
	{
		MainLoop.callAppropriateSystemMethod (system, "scoreThreeStarsExit", newData);
	}

}
