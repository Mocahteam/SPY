using UnityEngine;
using FYFY;

public class VirtualKeyboardManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject virtualKeyboard;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "virtualKeyboard", virtualKeyboard);
	}

	public void closeKeyboard()
	{
		MainLoop.callAppropriateSystemMethod (system, "closeKeyboard", null);
	}

	public void virtualKeyPressed(System.String carac)
	{
		MainLoop.callAppropriateSystemMethod (system, "virtualKeyPressed", carac);
	}

	public void supprLastCarac()
	{
		MainLoop.callAppropriateSystemMethod (system, "supprLastCarac", null);
	}

}
