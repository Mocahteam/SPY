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
using UnityEngine.Events;

public class TilePopupSystem : FSystem
{
	private Family f_popups = FamilyManager.getFamily(new AllOfComponents(typeof(Popup)));
	private Family f_activePopups = FamilyManager.getFamily(new AllOfComponents(typeof(Popup)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family f_focusedPopups = FamilyManager.getFamily(new AllOfComponents(typeof(Popup), typeof(PointerOver)));

	public static TilePopupSystem instance;
	public Transform toolboxPanelContent;
	public GameObject tileSettingsPrefab;
	public Transform tileSettingsParent;

	public PaintableGrid paintableGrid;

	public GameObject selection;

	private const string FurniturePrefix = "Prefabs/Modern Furniture/Prefabs/";
	private const string PathXmlPrefix = "Modern Furniture/Prefabs/";
	private FloorObject[] selectedObjects = null;

	private InputAction click;
	private InputAction rightClick;

	private List<string> furnitureNameToPath = new List<string>();
	private List<string> furnitureOptions = new List<string>();

	private UnityAction localCallback;
	private GameData gameData;

	public TilePopupSystem()
	{
		instance = this;
	}

	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();

		destroyAllPopups();
		initFurnitureDropDownData();
		selectedObjects = new FloorObject[3];

		click = InputSystem.actions.FindAction("Click");
		rightClick = InputSystem.actions.FindAction("RightClick");
	}

	private void initFurnitureDropDownData()
	{
		// Change the path provided here to load more options
		List<GameObject> prefabs = Resources.LoadAll<GameObject>(FurniturePrefix).ToList();
		furnitureOptions = prefabs.GroupBy(p => p.name).Select(g => g.First().name).ToList();

		foreach (string name in furnitureOptions)
			furnitureNameToPath.Add(PathXmlPrefix + name);
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
		Vector2Int pos = UtilityEditor.mousePosToGridPos(paintableGrid.GetComponent<Tilemap>());
		Tuple<int, int> posTuple = new Tuple<int, int>(pos.y, pos.x);

		// sur un clic droit ou un Echap, on déselectionne la tile
		if (isContentOnLayer(selectedObjects) && (
			rightClick.WasPressedThisFrame() || // on déselectionne sur un clic droit
			click.WasPressedThisFrame() && (!paintableGrid.floorObjects.ContainsKey(posTuple) || !isContentOnLayer(paintableGrid.floorObjects[posTuple])) && f_focusedPopups.Count == 0)) // on déselectionne sur un clic gauche dans le vide qui n'est pas sur une popup sinon quand on intéragit avec la popup, ça déselectionne automatiquement
		{
			selectedObjects = new FloorObject[3];
		}
		// sur un clic gauche qui pointe des objets, on les sélectionne
		if (click.WasPressedThisFrame() && paintableGrid.floorObjects.ContainsKey(posTuple) && f_focusedPopups.Count == 0)
			selectedObjects = paintableGrid.floorObjects[posTuple];

		// si des popup sont affichées et qu'aucun objet n'est sélectionné, on les supprime
		if (f_activePopups.Count > 0 && !isContentOnLayer(selectedObjects))
			destroyAllPopups();

		// sur la frame ou le clic a eu lieu, on crée les nouvelles popups
		if (click.WasPressedThisFrame() && isContentOnLayer(selectedObjects) && f_focusedPopups.Count == 0)
			refreshPopups(pos.x, pos.y, false);

		if (isContentOnLayer(selectedObjects))
		{
			FloorObject floorObject = selectedObjects[0] ?? selectedObjects[1] ?? selectedObjects[2];
			GameObjectManager.setGameObjectState(selection, true);
			selection.transform.localPosition = new Vector3(-UtilityEditor.gridMaxSize/2 + floorObject.col + 0.5f, UtilityEditor.gridMaxSize / 2 - floorObject.line + 0.5f);
		}
		else
			GameObjectManager.setGameObjectState(selection, false);
	}

	private bool isContentOnLayer(FloorObject[] floorObjects)
    {
		return floorObjects != null && (floorObjects[0] != null || floorObjects[1] != null || floorObjects[2] != null);
	}

	private void refreshPopups(int x, int y, bool autoFocusLastPosition)
    {
		// on commence par supprimer les anciennes
		destroyAllPopups();
		foreach (FloorObject selectedObject in selectedObjects)
		{
			if (selectedObject == null)
				continue;
			GameObject tileSettings = GameObject.Instantiate<GameObject>(tileSettingsPrefab, tileSettingsParent);
			GameObject positionPopup = tileSettings.transform.Find("PositionPopup").gameObject;
			positionPopup.GetComponentInChildren<TMP_InputField>().text = UtilityEditor.IntToLetters(x) + (y + 1);
			GameObject orientationPopup = tileSettings.transform.Find("OrientationPopup").gameObject;
			GameObject inputLinePopup = tileSettings.transform.Find("InputLinePopup").gameObject;
			GameObject rangePopup = tileSettings.transform.Find("rangePopup").gameObject;
			GameObject consoleSlotsPopup = tileSettings.transform.Find("consoleSlotsPopup").gameObject;
			GameObject doorSlotPopup = tileSettings.transform.Find("doorSlotPopup").gameObject;
			GameObject furniturePopup = tileSettings.transform.Find("furniturePopup").gameObject;
			GameObject skinPopup = tileSettings.transform.Find("skinPopup").gameObject;
			// par défaut onj désactive tout
			orientationPopup.SetActive(false);
			inputLinePopup.SetActive(false);
			rangePopup.SetActive(false);
			consoleSlotsPopup.SetActive(false);
			doorSlotPopup.SetActive(false);
			furniturePopup.SetActive(false);
			skinPopup.SetActive(false);
			tileSettings.GetComponent<Popup>().floorObject = selectedObject;
			string title;
			switch (selectedObject)
			{
				case Door d:
					title = toolboxPanelContent.Find("Door").GetComponentInChildren<TMP_Text>().text;
					// enable popups
					orientationPopup.SetActive(true);
					doorSlotPopup.SetActive(true);
					// load data
					doorSlotPopup.GetComponentInChildren<TMP_InputField>(true).text = d.slot;
					doorSlotPopup.GetComponentInChildren<Toggle>(true).isOn = d.state;
					break;
				case Console c:
					title = toolboxPanelContent.Find("Console").GetComponentInChildren<TMP_Text>().text;
					// enable popups
					orientationPopup.SetActive(true);
					consoleSlotsPopup.SetActive(true);
					// load data
					consoleSlotsPopup.GetComponentInChildren<TMP_InputField>(true).text = string.Join(", ", c.slots);
					break;
				case PlayerRobot pr:
					title = toolboxPanelContent.Find("AllyRobot").GetComponentInChildren<TMP_Text>().text;
					// enable popups
					orientationPopup.SetActive(true);
					inputLinePopup.SetActive(true);
					skinPopup.SetActive(true);
					// load data
					inputLinePopup.GetComponentInChildren<TMP_InputField>(true).text = pr.inputLine;
					skinPopup.GetComponentInChildren<TMP_Dropdown>(true).value = UtilityEditor.SkinToInt(pr.type);
					break;
				case EnemyRobot er:
					title = toolboxPanelContent.Find("EnemyRobot").GetComponentInChildren<TMP_Text>().text;
					// enable popups
					orientationPopup.SetActive(true);
					inputLinePopup.SetActive(true);
					rangePopup.SetActive(true);
					// load data
					inputLinePopup.GetComponentInChildren<TMP_InputField>(true).text = er.inputLine;
					rangePopup.GetComponentInChildren<TMP_InputField>(true).text = er.range.ToString();
					rangePopup.GetComponentInChildren<Toggle>(true).isOn = !er.selfRange;
					rangePopup.GetComponentInChildren<TMP_Dropdown>(true).value = (int)er.typeRange;
					break;
				case DecorationObject _:
					title = toolboxPanelContent.Find("Deco").GetComponentInChildren<TMP_Text>().text;
					// enable popups
					orientationPopup.SetActive(true);
					furniturePopup.SetActive(true);
					// load data
					furniturePopup.GetComponentInChildren<TMP_Dropdown>(true).AddOptions(furnitureOptions);
					// select appropriate entry
					int i = 0;
					foreach (string value in furnitureNameToPath)
					{
						if (value == ((DecorationObject)selectedObject).path)
							break;
						i++;
					}
					furniturePopup.GetComponentInChildren<TMP_Dropdown>(true).value = i;
					break;
				default:
					title = toolboxPanelContent.Find("Coin").GetComponentInChildren<TMP_Text>().text;
					break;
			}
			tileSettings.transform.Find("Title").GetComponentInChildren<TMP_Text>().text = title;
			GameObjectManager.bind(tileSettings);
		}
		if (autoFocusLastPosition)
			MainLoop.instance.StartCoroutine(Utility.delayGOSelection(tileSettingsParent.GetChild(tileSettingsParent.childCount - 1).GetComponentInChildren<TMP_InputField>(true).gameObject, 1));
	}

	private void destroyAllPopups()
	{
		Debug.Log("destroyAllPopups !!!!!!!!!!!!!");
		foreach (GameObject popup in f_popups)
		{
			GameObjectManager.unbind(popup);
			popup.transform.SetParent(null);
			UnityEngine.Object.Destroy(popup);
		}
		GameObjectManager.setGameObjectState(selection, false);
	}

	// See trash gameObject
	public void removeTileSettings(GameObject settings)
    {
		if (settings.transform.parent.childCount == 1)
			GameObjectManager.setGameObjectState(selection, false);
		EditorGridSystem.instance.removeTile(settings.GetComponent<Popup>().floorObject);
		GameObjectManager.unbind(settings);
		settings.transform.SetParent(null);
		UnityEngine.Object.Destroy(settings);
    }

	// See PositionPopup GameoBject childs
	public void moveTile(GameObject settings, string newPosition)
    {
		FloorObject selectedObject = settings.GetComponent<Popup>().floorObject;
		Tuple<int, int> newPos = UtilityEditor.LettersToInts(newPosition);
		bool moveDone = false;
		if (newPos.Item1 != -1 && newPos.Item2 != -1)
		{
			if (newPos.Item1 != selectedObject.col || newPos.Item2 != selectedObject.line)
			{
				if (EditorGridSystem.instance.moveTile(selectedObject, newPos.Item1, newPos.Item2))
				{
					selectedObjects = paintableGrid.floorObjects[newPos];
					refreshPopups(newPos.Item1, newPos.Item2, true);
					moveDone = true;
				}
			}
			else
				moveDone = true; // Si la nouvelle poisition est égale à l'ancienne on considère que le déplacement c'est bien passé
		}
		if (!moveDone)
		{
			localCallback = null;
			Localization loc = gameData.GetComponent<Localization>();
			GameObjectManager.addComponent<MessageForUser>(MainLoop.instance.gameObject, new { message = Utility.getFormatedText(loc.localization[49], newPosition), OkButton = loc.localization[0], CancelButton = loc.localization[1], call = localCallback });
		}
	}

	// See UP, Right, Down and Left GameObjects
	public void rotateObject(GameObject settings, int newOrientation)
	{
		FloorObject selectedObject = settings.GetComponent<Popup>().floorObject;
		selectedObject.orientation = (Direction.Dir)newOrientation;
		EditorGridSystem.instance.rotateObject((Direction.Dir)newOrientation, selectedObject.line, selectedObject.col, selectedObject.layer);
	}

	// see InputLinePopup GameObject childs
	public void popUpInputLine(GameObject settings, string newData)
	{
		FloorObject selectedObject = settings.GetComponent<Popup>().floorObject;
		((Robot)selectedObject).inputLine = newData;
	}

	// see rangePopup GameObject childs
	public void popupRangeInputField(GameObject settings, string newData)
	{
		FloorObject selectedObject = settings.GetComponent<Popup>().floorObject;
		((EnemyRobot)selectedObject).range = int.TryParse(newData, out int x) ? x : 0;
	}

	// see rangePopup GameObject childs
	public void popupRangeToggle(GameObject settings, bool newData)
	{
		FloorObject selectedObject = settings.GetComponent<Popup>().floorObject;
		((EnemyRobot)selectedObject).selfRange = !newData;
	}

	// see rangePopup GameObject childs
	public void popupRangeDropDown(GameObject settings, int newData)
	{
		FloorObject selectedObject = settings.GetComponent<Popup>().floorObject;
		((EnemyRobot)selectedObject).typeRange = (DetectRange.Type)newData;
	}

	// see consoleSlotsPopup GameObject childs
	public void popupConsoleSlots(GameObject settings, string newData)
	{
		FloorObject selectedObject = settings.GetComponent<Popup>().floorObject;
		string trimmed = String.Concat(newData.Where(c => !Char.IsWhiteSpace(c)));
		int[] ints = Array.ConvertAll(trimmed.Split(','), s => int.TryParse(s, out int x) ? x : -1);
		((Console)selectedObject).slots = trimmed.Split(',');
	}

	// see doorSlotPopup GameObject childs
	public void popupDoorSlot(GameObject settings, string newData)
	{
		FloorObject selectedObject = settings.GetComponent<Popup>().floorObject;
		((Door)selectedObject).slot = newData;
	}

	// see doorSlotsPopup GameObject childs
	public void popupDoorToggle(GameObject settings, bool newData)
	{
		FloorObject selectedObject = settings.GetComponent<Popup>().floorObject;
		((Door)selectedObject).state = newData;
	}

	// see furniturePopup GameObject childs
	public void popupFurnitureDropDown(GameObject settings, int newData)
	{
		FloorObject selectedObject = settings.GetComponent<Popup>().floorObject;
		((DecorationObject)selectedObject).path = furnitureNameToPath[newData];
	}

	// see skinPopup GameObject childs
	public void popupSkinDropDown(GameObject settings, int newData)
	{
		Popup setting = settings.GetComponent<Popup>();
		// Parce que dans le setTile de l'EditorGridSystem on va recréer un FloorObject, on sauvegarde son input line pour pouvoir la restaurer
		// sauvegarde de l'input line
		string inputLine = (setting.floorObject as PlayerRobot).inputLine;
		EditorGridSystem.instance.removeTile(setting.floorObject);
		FloorObject newFloorObject = EditorGridSystem.instance.setTile(setting.floorObject.line, setting.floorObject.col, UtilityEditor.IntToSkin(newData), setting.floorObject.orientation);
		// restauration de l'inputLine
		(newFloorObject as PlayerRobot).inputLine = inputLine;
		// et réassociation du nouvel objet à sa fenêtre de config
		setting.floorObject = newFloorObject;
	}
}