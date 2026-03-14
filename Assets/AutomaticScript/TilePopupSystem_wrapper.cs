using UnityEngine;
using FYFY;

public class TilePopupSystem_wrapper : BaseWrapper
{
	public UnityEngine.Transform toolboxPanelContent;
	public UnityEngine.GameObject tileSettingsPrefab;
	public UnityEngine.Transform tileSettingsParent;
	public PaintableGrid paintableGrid;
	public UnityEngine.GameObject selection;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "toolboxPanelContent", toolboxPanelContent);
		MainLoop.initAppropriateSystemField (system, "tileSettingsPrefab", tileSettingsPrefab);
		MainLoop.initAppropriateSystemField (system, "tileSettingsParent", tileSettingsParent);
		MainLoop.initAppropriateSystemField (system, "paintableGrid", paintableGrid);
		MainLoop.initAppropriateSystemField (system, "selection", selection);
	}

	public void removeTileSettings(UnityEngine.GameObject settings)
	{
		MainLoop.callAppropriateSystemMethod (system, "removeTileSettings", settings);
	}

}
