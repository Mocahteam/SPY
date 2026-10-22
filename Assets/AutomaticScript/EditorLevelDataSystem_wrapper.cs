using UnityEngine;
using FYFY;

public class EditorLevelDataSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject executionLimitContainer;
	public UnityEngine.UI.Toggle dragAndDropToggle;
	public UnityEngine.UI.Toggle fogToggle;
	public UnityEngine.UI.Toggle hideExitsToggle;
	public UnityEngine.UI.Toggle omniscientViewToggle;
	public UnityEngine.UI.Toggle userExecutorToggle;
	public TMPro.TMP_InputField score2Input;
	public TMPro.TMP_InputField score3Input;
	public UnityEngine.Transform editableContainers;
	public DataLevelBehaviour dataLevel;
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
		MainLoop.initAppropriateSystemField (system, "omniscientViewToggle", omniscientViewToggle);
		MainLoop.initAppropriateSystemField (system, "userExecutorToggle", userExecutorToggle);
		MainLoop.initAppropriateSystemField (system, "score2Input", score2Input);
		MainLoop.initAppropriateSystemField (system, "score3Input", score3Input);
		MainLoop.initAppropriateSystemField (system, "editableContainers", editableContainers);
		MainLoop.initAppropriateSystemField (system, "dataLevel", dataLevel);
		MainLoop.initAppropriateSystemField (system, "hideColor", hideColor);
		MainLoop.initAppropriateSystemField (system, "limitedColor", limitedColor);
		MainLoop.initAppropriateSystemField (system, "unlimitedColor", unlimitedColor);
	}

	public void UnlockAll()
	{
		MainLoop.callAppropriateSystemMethod (system, "UnlockAll", null);
	}

	public void HideAll()
	{
		MainLoop.callAppropriateSystemMethod (system, "HideAll", null);
	}

	public void PreventMinusSign(TMPro.TMP_InputField input)
	{
		MainLoop.callAppropriateSystemMethod (system, "PreventMinusSign", input);
	}

	public void ExecutionLimitChanged(System.Boolean newState)
	{
		MainLoop.callAppropriateSystemMethod (system, "ExecutionLimitChanged", newState);
	}

	public void ScoreTwoStarsExit(System.String newData)
	{
		MainLoop.callAppropriateSystemMethod (system, "ScoreTwoStarsExit", newData);
	}

	public void ScoreThreeStarsExit(System.String newData)
	{
		MainLoop.callAppropriateSystemMethod (system, "ScoreThreeStarsExit", newData);
	}

}
