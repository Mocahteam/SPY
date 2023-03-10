using UnityEngine;
using FYFY;

public class PopupManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject panelInfoUser;
	public TMPro.TMP_Text messageForUser;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "panelInfoUser", panelInfoUser);
		MainLoop.initAppropriateSystemField (system, "messageForUser", messageForUser);
	}

	public void focusLastButton()
	{
		MainLoop.callAppropriateSystemMethod (system, "focusLastButton", null);
	}

}
