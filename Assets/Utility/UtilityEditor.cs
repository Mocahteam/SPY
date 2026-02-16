using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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

	public static void buildLoadingPanelNavigation(Transform loadingPanel)
    {
		// Build navigation
		Transform content = loadingPanel.Find("Scroll View/Viewport/Content");
		Selectable up;
		Selectable down;
		Selectable filterSelect = loadingPanel.Find("Filter").GetComponent<TMP_InputField>();
		Navigation filterNavigation = filterSelect.navigation;
		Selectable loadButtonSelect = loadingPanel.Find("Buttons/LoadButton").GetComponent<Button>();
		Navigation loadButtonNavigation = loadButtonSelect.navigation;
		// Définir par défaut une navigation entre le filtre et le bouton de chargement au cas où la liste serait vide
		filterNavigation.selectOnDown = loadButtonSelect;
		filterNavigation.selectOnRight = loadButtonSelect;
		loadButtonNavigation.selectOnUp = filterSelect;
		loadButtonNavigation.selectOnLeft = filterSelect;
		// Parcourir tous les enfants pour définir la bonne navigation
		for (int i = 0; i < content.childCount; i++)
		{
			Selectable currentScenar = content.GetChild(i).GetComponentInChildren<Button>();
			if (i == 0)
			{
				up = filterSelect;
				filterNavigation.selectOnDown = currentScenar;
				filterNavigation.selectOnRight = currentScenar;
			}
			else
				up = content.GetChild(i - 1).GetComponentInChildren<Button>();
			if (i == content.childCount - 1)
			{
				down = loadButtonSelect;
				loadButtonNavigation.selectOnUp = currentScenar;
				loadButtonNavigation.selectOnLeft = currentScenar;
			}
			else
				down = content.GetChild(i + 1).GetComponentInChildren<Button>();
			Navigation nav = currentScenar.navigation;
			nav.selectOnUp = up;
			nav.selectOnLeft = up;
			nav.selectOnDown = down;
			nav.selectOnRight = down;
			currentScenar.navigation = nav;
		}
		// Valider pour le filtre et le bouton de chargement la navigation finale
		filterSelect.navigation = filterNavigation;
		loadButtonSelect.navigation = loadButtonNavigation;
	}
}