using UnityEngine;
using FYFY;

public class EditorLevelDataSystem_wrapper : BaseWrapper
{
	public UnityEngine.Sprite backgroundAction;
	public UnityEngine.Sprite backgroundControl;
	public UnityEngine.Sprite backgroundOperator;
	public UnityEngine.Sprite backgroundSensor;
	public UnityEngine.Color actionColor;
	public UnityEngine.Color controlColor;
	public UnityEngine.Color operatorColor;
	public UnityEngine.Color sensorColor;
	public LevelData levelData;
	public UnityEngine.GameObject scrollViewContent;
	public UnityEngine.GameObject executionLimitContainer;
	public UnityEngine.UI.Toggle dragAndDropToggle;
	public UnityEngine.UI.Toggle fogToggle;
	public UnityEngine.UI.Toggle hideExitsToggle;
	public UnityEngine.GameObject scoreContainer;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "backgroundAction", backgroundAction);
		MainLoop.initAppropriateSystemField (system, "backgroundControl", backgroundControl);
		MainLoop.initAppropriateSystemField (system, "backgroundOperator", backgroundOperator);
		MainLoop.initAppropriateSystemField (system, "backgroundSensor", backgroundSensor);
		MainLoop.initAppropriateSystemField (system, "actionColor", actionColor);
		MainLoop.initAppropriateSystemField (system, "controlColor", controlColor);
		MainLoop.initAppropriateSystemField (system, "operatorColor", operatorColor);
		MainLoop.initAppropriateSystemField (system, "sensorColor", sensorColor);
		MainLoop.initAppropriateSystemField (system, "levelData", levelData);
		MainLoop.initAppropriateSystemField (system, "scrollViewContent", scrollViewContent);
		MainLoop.initAppropriateSystemField (system, "executionLimitContainer", executionLimitContainer);
		MainLoop.initAppropriateSystemField (system, "dragAndDropToggle", dragAndDropToggle);
		MainLoop.initAppropriateSystemField (system, "fogToggle", fogToggle);
		MainLoop.initAppropriateSystemField (system, "hideExitsToggle", hideExitsToggle);
		MainLoop.initAppropriateSystemField (system, "scoreContainer", scoreContainer);
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
