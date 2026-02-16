using UnityEngine;
using FYFY;

public class InitLevelEditor_wrapper : BaseWrapper
{
	public UnityEngine.UI.Button mapTab;
	public UnityEngine.UI.Button scriptTab;
	public UnityEngine.UI.Button paramTab;
	public UnityEngine.GameObject mapContent;
	public UnityEngine.GameObject scriptContent;
	public UnityEngine.GameObject paramContent;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "mapTab", mapTab);
		MainLoop.initAppropriateSystemField (system, "scriptTab", scriptTab);
		MainLoop.initAppropriateSystemField (system, "paramTab", paramTab);
		MainLoop.initAppropriateSystemField (system, "mapContent", mapContent);
		MainLoop.initAppropriateSystemField (system, "scriptContent", scriptContent);
		MainLoop.initAppropriateSystemField (system, "paramContent", paramContent);
	}

	public void reloadEditor()
	{
		MainLoop.callAppropriateSystemMethod (system, "reloadEditor", null);
	}

	public void returnToLobby()
	{
		MainLoop.callAppropriateSystemMethod (system, "returnToLobby", null);
	}

}
