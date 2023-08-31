using UnityEngine;
using FYFY;

public class EditorLevelDataSystem_wrapper : BaseWrapper
{
	public LevelData levelData;
	public UnityEngine.GameObject scrollViewContent;
	public UnityEngine.GameObject executionLimitContainer;
	public UnityEngine.UI.Toggle dragAndDropToggle;
	public UnityEngine.UI.Toggle fogToggle;
	public UnityEngine.UI.Toggle hideExitsToggle;
	public TMPro.TMP_InputField score2Input;
	public TMPro.TMP_InputField score3Input;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "levelData", levelData);
		MainLoop.initAppropriateSystemField (system, "scrollViewContent", scrollViewContent);
		MainLoop.initAppropriateSystemField (system, "executionLimitContainer", executionLimitContainer);
		MainLoop.initAppropriateSystemField (system, "dragAndDropToggle", dragAndDropToggle);
		MainLoop.initAppropriateSystemField (system, "fogToggle", fogToggle);
		MainLoop.initAppropriateSystemField (system, "hideExitsToggle", hideExitsToggle);
		MainLoop.initAppropriateSystemField (system, "score2Input", score2Input);
		MainLoop.initAppropriateSystemField (system, "score3Input", score3Input);
	}

	public void resetMetaData()
	{
		MainLoop.callAppropriateSystemMethod (system, "resetMetaData", null);
	}

	public void hideToggleChanged()
	{
		MainLoop.callAppropriateSystemMethod (system, "hideToggleChanged", null);
	}

	public void limitToggleChanged()
	{
		MainLoop.callAppropriateSystemMethod (system, "limitToggleChanged", null);
	}

	public void onDragDropToggled(System.Boolean newState)
	{
		MainLoop.callAppropriateSystemMethod (system, "onDragDropToggled", newState);
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
