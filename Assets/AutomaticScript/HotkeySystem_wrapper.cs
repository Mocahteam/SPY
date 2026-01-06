using UnityEngine;
using FYFY;

public class HotkeySystem_wrapper : BaseWrapper
{
	public UnityEngine.UI.Button mainMenu;
	public UnityEngine.UI.Button buttonExecute;
	public UnityEngine.UI.Button buttonPause;
	public UnityEngine.UI.Button buttonNextStep;
	public UnityEngine.UI.Button buttonContinue;
	public UnityEngine.UI.Button buttonSpeed;
	public UnityEngine.UI.Button buttonStop;
	public UnityEngine.UI.Button cameraSwitchView;
	public UnityEngine.EventSystems.EventTrigger cameraRotateLeft;
	public UnityEngine.EventSystems.EventTrigger cameraRotateRight;
	public UnityEngine.EventSystems.EventTrigger cameraTop;
	public UnityEngine.EventSystems.EventTrigger cameraDown;
	public UnityEngine.EventSystems.EventTrigger cameraLeft;
	public UnityEngine.EventSystems.EventTrigger cameraRight;
	public UnityEngine.EventSystems.EventTrigger cameraFocusOn;
	public UnityEngine.EventSystems.EventTrigger cameraZoomIn;
	public UnityEngine.EventSystems.EventTrigger cameraZoomOut;
	public UnityEngine.UI.Button showBriefing;
	public UnityEngine.UI.Button showMapDesc;
	public UnityEngine.UI.Button buttonCopyCode;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "mainMenu", mainMenu);
		MainLoop.initAppropriateSystemField (system, "buttonExecute", buttonExecute);
		MainLoop.initAppropriateSystemField (system, "buttonPause", buttonPause);
		MainLoop.initAppropriateSystemField (system, "buttonNextStep", buttonNextStep);
		MainLoop.initAppropriateSystemField (system, "buttonContinue", buttonContinue);
		MainLoop.initAppropriateSystemField (system, "buttonSpeed", buttonSpeed);
		MainLoop.initAppropriateSystemField (system, "buttonStop", buttonStop);
		MainLoop.initAppropriateSystemField (system, "cameraSwitchView", cameraSwitchView);
		MainLoop.initAppropriateSystemField (system, "cameraRotateLeft", cameraRotateLeft);
		MainLoop.initAppropriateSystemField (system, "cameraRotateRight", cameraRotateRight);
		MainLoop.initAppropriateSystemField (system, "cameraTop", cameraTop);
		MainLoop.initAppropriateSystemField (system, "cameraDown", cameraDown);
		MainLoop.initAppropriateSystemField (system, "cameraLeft", cameraLeft);
		MainLoop.initAppropriateSystemField (system, "cameraRight", cameraRight);
		MainLoop.initAppropriateSystemField (system, "cameraFocusOn", cameraFocusOn);
		MainLoop.initAppropriateSystemField (system, "cameraZoomIn", cameraZoomIn);
		MainLoop.initAppropriateSystemField (system, "cameraZoomOut", cameraZoomOut);
		MainLoop.initAppropriateSystemField (system, "showBriefing", showBriefing);
		MainLoop.initAppropriateSystemField (system, "showMapDesc", showMapDesc);
		MainLoop.initAppropriateSystemField (system, "buttonCopyCode", buttonCopyCode);
	}

	public void paste(System.String content)
	{
		MainLoop.callAppropriateSystemMethod (system, "paste", content);
	}

}
