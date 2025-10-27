using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FYFY;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using FYFY_plugins.PointerManager;
using TMPro;
using UnityEngine.InputSystem;

public class TilePopupSystem : FSystem
{
	private Family f_popups = FamilyManager.getFamily(new AllOfComponents(typeof(Popup)));
	private Family f_activePopups = FamilyManager.getFamily(new AllOfComponents(typeof(Popup)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family f_focusedPopups = FamilyManager.getFamily(new AllOfComponents(typeof(Popup), typeof(PointerOver)));

	public static TilePopupSystem instance;
	public GameObject orientationPopup;
	public GameObject inputLinePopup;
	public GameObject rangePopup;
	public GameObject consoleSlotsPopup;
	public GameObject doorSlotPopup;
	public GameObject furniturePopup;
	public GameObject skinPopup;
	public GameObject virtualKeyboard;

	public PaintableGrid paintableGrid;

	public GameObject selection;

	private const string FurniturePrefix = "Prefabs/Modern Furniture/Prefabs/";
	private const string PathXmlPrefix = "Modern Furniture/Prefabs/";
	private FloorObject selectedObject;

	private InputAction click;
	private InputAction rightClick;
	private InputAction cancel;
	private InputAction exitWebGL;

	private List<string> furnitureNameToPath = new List<string>();

	public TilePopupSystem()
	{
		instance = this;
	}

	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		hideAllPopups();
		initFurniturePopup();
		selectedObject = null;

		click = InputSystem.actions.FindAction("Click");
		rightClick = InputSystem.actions.FindAction("RightClick");
		cancel = InputSystem.actions.FindAction("Cancel");
		exitWebGL = InputSystem.actions.FindAction("ExitWebGL");
	}

	private void initFurniturePopup()
	{
		// Change the path provided here to load more options
		var prefabs = Resources.LoadAll<GameObject>(FurniturePrefix).ToList();
		var prefabNames = prefabs.GroupBy(p => p.name).Select(g => g.First().name).ToList();
		TMP_Dropdown dropdown = furniturePopup.GetComponentInChildren<TMP_Dropdown>();

		List<string> options = new List<string>();
		foreach (string name in prefabNames)
		{
			furnitureNameToPath.Add(PathXmlPrefix + name);
			options.Add(name);
		}
		furniturePopup.GetComponentInChildren<TMP_Dropdown>().AddOptions(options);
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
		Vector2Int pos = UtilityEditor.mousePosToGridPos(paintableGrid.GetComponent<Tilemap>());
		Tuple<int, int> posTuple = new Tuple<int, int>(pos.y, pos.x);

		if (rightClick.WasPressedThisFrame() && selectedObject != null && selectedObject.line == pos.y && selectedObject.col == pos.x)
		{
			selectedObject = null;
		}

		if (click.WasPressedThisFrame() && paintableGrid.floorObjects.ContainsKey(posTuple) && paintableGrid.floorObjects[posTuple].selectable)
		{
			selectedObject = paintableGrid.floorObjects[posTuple];
		}

		// Shift + Echap est réservé pour sortir du contexte WebGL et revenir sur la page web (voir html)
		if (f_activePopups.Count > 0 && (virtualKeyboard == null || !virtualKeyboard.activeInHierarchy) && ((cancel.WasPressedThisFrame() && !exitWebGL.WasPressedThisFrame()) || selectedObject == null || (!paintableGrid.floorObjects.ContainsKey(posTuple) && click.WasPressedThisFrame() && f_focusedPopups.Count == 0)))
		{
			hideAllPopups();
			selectedObject = null; // be sure
		}

		if (click.WasPressedThisFrame() && selectedObject != null && f_focusedPopups.Count == 0)
		{
			hideAllPopups();
			GameObjectManager.setGameObjectState(orientationPopup.transform.parent.parent.parent.gameObject, true);
			switch (selectedObject)
			{
				case Door d:
					// enable popups
					GameObjectManager.setGameObjectState(orientationPopup, true);
					GameObjectManager.setGameObjectState(doorSlotPopup, true);
					// load data
					doorSlotPopup.GetComponentInChildren<TMP_InputField>().text = d.slot;
					doorSlotPopup.GetComponentInChildren<Toggle>().isOn = d.state;
					break;
				case Console c:
					// enable popups
					GameObjectManager.setGameObjectState(orientationPopup, true);
					GameObjectManager.setGameObjectState(consoleSlotsPopup, true);
					// load data
					consoleSlotsPopup.GetComponentInChildren<TMP_InputField>().text = string.Join(", ", c.slots);
					break;
				case PlayerRobot pr:
					// enable popups
					GameObjectManager.setGameObjectState(orientationPopup, true);
					GameObjectManager.setGameObjectState(inputLinePopup, true);
					GameObjectManager.setGameObjectState(skinPopup, true);
					// load data
					inputLinePopup.GetComponentInChildren<TMP_InputField>().text = pr.inputLine;
					skinPopup.GetComponentInChildren<TMP_Dropdown>().value = UtilityEditor.SkinToInt(pr.type);
					break;
				case EnemyRobot er:
					// enable popups
					GameObjectManager.setGameObjectState(orientationPopup, true);
					GameObjectManager.setGameObjectState(inputLinePopup, true);
					GameObjectManager.setGameObjectState(rangePopup, true);
					// load data
					inputLinePopup.GetComponentInChildren<TMP_InputField>().text = er.inputLine;
					rangePopup.GetComponentInChildren<TMP_InputField>().text = er.range.ToString();
					rangePopup.GetComponentInChildren<Toggle>().isOn = !er.selfRange;
					rangePopup.GetComponentInChildren<TMP_Dropdown>().value = (int)er.typeRange;
					break;
				case DecorationObject deco:
					// enable popups
					GameObjectManager.setGameObjectState(orientationPopup, true);
					GameObjectManager.setGameObjectState(furniturePopup, true);
					// load data
					int i = 0;
					foreach (string value in furnitureNameToPath)
					{
						if (value == ((DecorationObject)selectedObject).path)
							break;
						i++;
					}
					furniturePopup.GetComponentInChildren<TMP_Dropdown>().value = i;
					break;
			}
		}

		if (selectedObject != null)
		{
			GameObjectManager.setGameObjectState(selection, true);
			selection.transform.localPosition = new Vector3(-UtilityEditor.gridMaxSize/2 + selectedObject.col + 0.5f, UtilityEditor.gridMaxSize / 2 - selectedObject.line + 0.5f);
		}
		else
			GameObjectManager.setGameObjectState(selection, false);
	}

	private void hideAllPopups()
	{
		foreach (GameObject popup in f_popups)
			GameObjectManager.setGameObjectState(popup, false);
		if (f_popups.Count > 0)
			GameObjectManager.setGameObjectState(f_popups.First().transform.parent.parent.parent.gameObject, false);
	}

	// See UP, Right, Down and Left GameObjects
	public void rotateObject(int newOrientation)
	{
		if (selectedObject != null)
		{
			selectedObject.orientation = (Direction.Dir)newOrientation;
			Tuple<int, int> posTuple = new Tuple<int, int>(selectedObject.line, selectedObject.col);
			EditorGridSystem.instance.rotateObject((Direction.Dir)newOrientation, selectedObject.line, selectedObject.col);
		}
	}

	// see InputLinePopup GameObject childs
	public void popUpInputLine(string newData)
	{
		if (selectedObject != null)
			((Robot)selectedObject).inputLine = newData;
	}

	// see rangePopup GameObject childs
	public void popupRangeInputField(string newData)
	{
		if (selectedObject != null)
			((EnemyRobot)selectedObject).range = int.TryParse(newData, out int x) ? x : 0;
	}

	// see rangePopup GameObject childs
	public void popupRangeToggle(bool newData)
	{
		if (selectedObject != null)
			((EnemyRobot)selectedObject).selfRange = !newData;
	}

	// see rangePopup GameObject childs
	public void popupRangeDropDown(int newData)
	{
		if (selectedObject != null)
			((EnemyRobot)selectedObject).typeRange = (DetectRange.Type)newData;
	}

	// see consoleSlotsPopup GameObject childs
	public void popupConsoleSlots(string newData)
	{
		if (selectedObject != null)
		{
			string trimmed = String.Concat(newData.Where(c => !Char.IsWhiteSpace(c)));
			int[] ints = Array.ConvertAll(trimmed.Split(','), s => int.TryParse(s, out int x) ? x : -1);
			((Console)selectedObject).slots = trimmed.Split(',');
		}
	}

	// see doorSlotPopup GameObject childs
	public void popupDoorSlot(string newData)
	{
		if (selectedObject != null)
			((Door)selectedObject).slot = newData;
	}

	// see doorSlotsPopup GameObject childs
	public void popupDoorToggle(bool newData)
	{
		if (selectedObject != null)
			((Door)selectedObject).state = newData;
	}

	// see furniturePopup GameObject childs
	public void popupFurnitureDropDown(int newData)
	{
		if (selectedObject != null)
			((DecorationObject)selectedObject).path = furnitureNameToPath[newData];
	}

	// see skinPopup GameObject childs
	public void popupSkinDropDown(int newData)
	{
		if (selectedObject != null)
		{
			PlayerRobot player = ((PlayerRobot)selectedObject);
			// sauvegarde de l'input line
			string inputLine = player.inputLine;
			// on réinitialise le type à Void
			player.type = Cell.Void;
			EditorGridSystem.instance.setTile(player.line, player.col, UtilityEditor.IntToSkin(newData), player.orientation);
			// association de la nouvelle tile créée à l'objet sélectionné
			Tuple<int, int> posTuple = new Tuple<int, int>(player.line, player.col);
			selectedObject = paintableGrid.floorObjects[posTuple];
			// restauration de l'inputLine
			((PlayerRobot)selectedObject).inputLine = inputLine;
		}
	}
}