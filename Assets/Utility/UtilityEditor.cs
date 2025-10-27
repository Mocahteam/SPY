using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Tilemaps;

public static class UtilityEditor
{
	public static int gridMaxSize = 64;

	public static Vector2Int mousePosToGridPos(Tilemap tilemap)
	{
		Vector2Control pointerPos = Pointer.current.position;
		var pos = Camera.main.ScreenToWorldPoint(new Vector2(pointerPos.x.value, pointerPos.y.value));
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

	/// <summary>
	/// Called when trying to save
	/// </summary>
	public static bool CheckSaveNameValidity(string nameCandidate)
	{
		bool isValid = nameCandidate != "";

		if (isValid)
		{
			char[] chars = Path.GetInvalidFileNameChars();

			foreach (char c in chars)
				if (nameCandidate.IndexOf(c) != -1)
				{
					isValid = false;
					break;
				}
		}
		return isValid;
	}
}