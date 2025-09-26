using UnityEngine;
using FYFY;

public class LevelGenerator_wrapper : BaseWrapper
{
	public UnityEngine.GameObject LevelGO;
	public UnityEngine.GameObject editableCanvas;
	public UnityEngine.GameObject scriptContainer;
	public UnityEngine.GameObject library;
	public TMPro.TMP_Text levelName;
	public UnityEngine.GameObject buttonExecute;
	public UnityEngine.Material[] groundMaterials;
	public UnityEngine.Material[] wallMaterials;
	public UnityEngine.GameObject[] skinPrefabs;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "LevelGO", LevelGO);
		MainLoop.initAppropriateSystemField (system, "editableCanvas", editableCanvas);
		MainLoop.initAppropriateSystemField (system, "scriptContainer", scriptContainer);
		MainLoop.initAppropriateSystemField (system, "library", library);
		MainLoop.initAppropriateSystemField (system, "levelName", levelName);
		MainLoop.initAppropriateSystemField (system, "buttonExecute", buttonExecute);
		MainLoop.initAppropriateSystemField (system, "groundMaterials", groundMaterials);
		MainLoop.initAppropriateSystemField (system, "wallMaterials", wallMaterials);
		MainLoop.initAppropriateSystemField (system, "skinPrefabs", skinPrefabs);
	}

}
