using UnityEngine;
using FYFY;

public class LevelGenerator_wrapper : BaseWrapper
{
	public UnityEngine.GameObject LevelGO;
	public UnityEngine.GameObject scriptContainer;
	public TMPro.TMP_Text scenarioName;
	public TMPro.TMP_Text levelName;
	public UnityEngine.GameObject buttonExecute;
	public UnityEngine.Material[] groundMaterials;
	public UnityEngine.Material[] wallMaterials;
	public UnityEngine.Material[] wallMaterialsFade;
	public UnityEngine.GameObject[] skinPrefabs;
	public UnityEngine.GameObject dronePrefab;
	public UnityEngine.GameObject executablePanelPrefab;
	public UnityEngine.GameObject doorPrefab;
	public UnityEngine.GameObject activableConsolePrefab;
	public UnityEngine.GameObject teleporterSpawnPrefab;
	public UnityEngine.GameObject teleporterExitPrefab;
	public UnityEngine.GameObject coinPrefab;
	public UnityEngine.GameObject cubeGroundPrefab;
	public UnityEngine.GameObject cubeWallPrefab;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "LevelGO", LevelGO);
		MainLoop.initAppropriateSystemField (system, "scriptContainer", scriptContainer);
		MainLoop.initAppropriateSystemField (system, "scenarioName", scenarioName);
		MainLoop.initAppropriateSystemField (system, "levelName", levelName);
		MainLoop.initAppropriateSystemField (system, "buttonExecute", buttonExecute);
		MainLoop.initAppropriateSystemField (system, "groundMaterials", groundMaterials);
		MainLoop.initAppropriateSystemField (system, "wallMaterials", wallMaterials);
		MainLoop.initAppropriateSystemField (system, "wallMaterialsFade", wallMaterialsFade);
		MainLoop.initAppropriateSystemField (system, "skinPrefabs", skinPrefabs);
		MainLoop.initAppropriateSystemField (system, "dronePrefab", dronePrefab);
		MainLoop.initAppropriateSystemField (system, "executablePanelPrefab", executablePanelPrefab);
		MainLoop.initAppropriateSystemField (system, "doorPrefab", doorPrefab);
		MainLoop.initAppropriateSystemField (system, "activableConsolePrefab", activableConsolePrefab);
		MainLoop.initAppropriateSystemField (system, "teleporterSpawnPrefab", teleporterSpawnPrefab);
		MainLoop.initAppropriateSystemField (system, "teleporterExitPrefab", teleporterExitPrefab);
		MainLoop.initAppropriateSystemField (system, "coinPrefab", coinPrefab);
		MainLoop.initAppropriateSystemField (system, "cubeGroundPrefab", cubeGroundPrefab);
		MainLoop.initAppropriateSystemField (system, "cubeWallPrefab", cubeWallPrefab);
	}

}
