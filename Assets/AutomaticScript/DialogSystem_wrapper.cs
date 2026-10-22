using UnityEngine;
using FYFY;

public class DialogSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject LevelGO;
	public UnityEngine.GameObject dialogPanel;
	public UnityEngine.GameObject showDialogsMenu;
	public UnityEngine.GameObject showDialogsBottom;
	public UnityEngine.GameObject highlightGOPrefab;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "LevelGO", LevelGO);
		MainLoop.initAppropriateSystemField (system, "dialogPanel", dialogPanel);
		MainLoop.initAppropriateSystemField (system, "showDialogsMenu", showDialogsMenu);
		MainLoop.initAppropriateSystemField (system, "showDialogsBottom", showDialogsBottom);
		MainLoop.initAppropriateSystemField (system, "highlightGOPrefab", highlightGOPrefab);
	}

	public void ShowDialogPanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "ShowDialogPanel", null);
	}

	public void NextDialog()
	{
		MainLoop.callAppropriateSystemMethod (system, "NextDialog", null);
	}

	public void PrevDialog()
	{
		MainLoop.callAppropriateSystemMethod (system, "PrevDialog", null);
	}

	public void CloseDialogPanel()
	{
		MainLoop.callAppropriateSystemMethod (system, "CloseDialogPanel", null);
	}

	public void PlayVideo()
	{
		MainLoop.callAppropriateSystemMethod (system, "PlayVideo", null);
	}

	public void PauseVideo()
	{
		MainLoop.callAppropriateSystemMethod (system, "PauseVideo", null);
	}

}
