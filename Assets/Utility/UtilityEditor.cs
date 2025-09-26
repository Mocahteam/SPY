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

	public static int SkinToInt (Cell skin)
    {
		return skin switch
					{
						Cell.Kyle => 0,
						Cell.R102 => 1,
						Cell.Destiny => 2,
						_ => 0
					};
	}

	public static Cell IntToSkin(int skin)
	{
		return skin switch
		{
			0 => Cell.Kyle,
			1 => Cell.R102,
			2 => Cell.Destiny,
			_ => Cell.Kyle
		};
	}
}