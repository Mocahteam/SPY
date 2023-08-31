using UnityEngine;
using UnityEngine.Tilemaps;

public static class UtilityEditor
{
	public static int gridMaxSize = 64;

	public static Vector2Int mousePosToGridPos(Tilemap tilemap)
	{
		var pos = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
		var tilePos = tilemap.WorldToCell(pos);
		return new Vector2Int(tilePos.x + gridMaxSize / 2, gridMaxSize / 2 + tilePos.y * -1);
	}
}