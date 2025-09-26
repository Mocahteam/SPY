using UnityEngine;
using FYFY;

public class EditorGridSystem_wrapper : BaseWrapper
{
	public UnityEngine.Tilemaps.Tile voidTile;
	public UnityEngine.Tilemaps.Tile floorTile;
	public UnityEngine.Tilemaps.Tile wallTile;
	public UnityEngine.Tilemaps.Tile spawnTile;
	public UnityEngine.Tilemaps.Tile teleportTile;
	public UnityEngine.Tilemaps.Tile kyleTile;
	public UnityEngine.Tilemaps.Tile r102Tile;
	public UnityEngine.Tilemaps.Tile destinyTile;
	public UnityEngine.Tilemaps.Tile enemyTile;
	public UnityEngine.Tilemaps.Tile decoTile;
	public UnityEngine.Tilemaps.Tile doorTile;
	public UnityEngine.Tilemaps.Tile consoleTile;
	public UnityEngine.Tilemaps.Tile coinTile;
	public UnityEngine.Texture2D placingCursor;
	public System.String defaultDecoration;
	public PaintableGrid paintableGrid;
	public Tooltip tooltip;
	public UnityEngine.GameObject brushSelect;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "voidTile", voidTile);
		MainLoop.initAppropriateSystemField (system, "floorTile", floorTile);
		MainLoop.initAppropriateSystemField (system, "wallTile", wallTile);
		MainLoop.initAppropriateSystemField (system, "spawnTile", spawnTile);
		MainLoop.initAppropriateSystemField (system, "teleportTile", teleportTile);
		MainLoop.initAppropriateSystemField (system, "kyleTile", kyleTile);
		MainLoop.initAppropriateSystemField (system, "r102Tile", r102Tile);
		MainLoop.initAppropriateSystemField (system, "destinyTile", destinyTile);
		MainLoop.initAppropriateSystemField (system, "enemyTile", enemyTile);
		MainLoop.initAppropriateSystemField (system, "decoTile", decoTile);
		MainLoop.initAppropriateSystemField (system, "doorTile", doorTile);
		MainLoop.initAppropriateSystemField (system, "consoleTile", consoleTile);
		MainLoop.initAppropriateSystemField (system, "coinTile", coinTile);
		MainLoop.initAppropriateSystemField (system, "placingCursor", placingCursor);
		MainLoop.initAppropriateSystemField (system, "defaultDecoration", defaultDecoration);
		MainLoop.initAppropriateSystemField (system, "paintableGrid", paintableGrid);
		MainLoop.initAppropriateSystemField (system, "tooltip", tooltip);
		MainLoop.initAppropriateSystemField (system, "brushSelect", brushSelect);
	}

	public void resetGrid()
	{
		MainLoop.callAppropriateSystemMethod (system, "resetGrid", null);
	}

	public void setBrush(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "setBrush", go);
	}

}
