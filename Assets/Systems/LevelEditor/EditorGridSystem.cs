using FYFY;
using FYFY_plugins.PointerManager;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class EditorGridSystem : FSystem
{
	// Contains current UI focused
	private Family f_UIfocused = FamilyManager.getFamily(new AllOfComponents(typeof(RectTransform), typeof(PointerOver)));
	private Family f_newLoading = FamilyManager.getFamily(new AllOfComponents(typeof(NewLevelToLoad)));
	private Family f_mapCanvasEnabled = FamilyManager.getFamily(new AnyOfTags("EditorMapCanvas"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	public static EditorGridSystem instance;
	public Tile voidTile;
	public Tile floorTile;
	public Tile wallTile;
	public Tile spawnTile;
	public Tile teleportTile;
	public Tile kyleTile;
	public Tile r102Tile;
	public Tile destinyTile;
	public Tile enemyTile;
	public Tile decoTile;
	public Tile doorTile;
	public Tile consoleTile;
	public Tile coinTile;
	public Texture2D placingCursor;
	public string defaultDecoration;
	public PaintableGrid paintableGrid;
	public Tooltip tooltip;
	public GameObject brushSelect;
	
	private Vector2Int _gridSize;

	private CellBrush activeBrush;

	private GameData gameData;

	private InputAction click;
	private InputAction rightClick;
	private InputAction clickHold;

	public EditorGridSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart()
	{
		GameObject go = GameObject.Find("GameData");
		if (go != null)
			gameData = go.GetComponent<GameData>();

		click = InputSystem.actions.FindAction("Click");
		rightClick = InputSystem.actions.FindAction("RightClick");
		clickHold = InputSystem.actions.FindAction("ClickHold");

		resetGrid();
		// Sélectionne par défaut la brush Select
		setBrush(brushSelect);

		f_newLoading.addEntryCallback(loadLevel);
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
		if (f_mapCanvasEnabled.Count == 0 || f_UIfocused.Count > 0)
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			tooltip.HideTooltip();
			return;
		}

		Vector2Int pos = UtilityEditor.mousePosToGridPos(paintableGrid.GetComponent<Tilemap>());

		if (pos.x < 0 || pos.x >= _gridSize.x || pos.y < 0 || pos.y >= _gridSize.y)
			tooltip.HideTooltip();
		else
			tooltip.ShowTooltip(UtilityEditor.IntToLetters(pos.x)+" "+(pos.y+1));

		if ((click.WasPressedThisFrame() && !canBePlaced(activeBrush.brush, pos.y, pos.x)) || rightClick.WasPressedThisFrame())
		{
			setBrush(brushSelect);
			EventSystem.current.SetSelectedGameObject(brushSelect.gameObject);
			return;
		}

		if (pos.x < 0 || pos.x >= _gridSize.x || pos.y < 0 || pos.y >= _gridSize.y || !canBePlaced(activeBrush.brush, pos.y, pos.x))
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			return;
		}
		
		if (activeBrush.brush == Cell.Select)
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			return;
		}

		if (placingCursor != null)
			Cursor.SetCursor(placingCursor, new Vector2(placingCursor.width / 2.0f, placingCursor.height / 2.0f), CursorMode.Auto);

		if (f_UIfocused.Count == 0 && (click.WasPressedThisFrame() || clickHold.IsPressed()) && activeBrush.brush != Cell.Select)
		{
			setTile(pos.y, pos.x, activeBrush.brush);
			EventSystem.current.SetSelectedGameObject(activeBrush.gameObject);
		}
	}

	public void resetGrid()
	{
		paintableGrid.floorObjects = new Dictionary<Tuple<int, int>, FloorObject[]>();
		_gridSize = new Vector2Int(UtilityEditor.gridMaxSize, UtilityEditor.gridMaxSize);
		paintableGrid.grid = new Cell[UtilityEditor.gridMaxSize, UtilityEditor.gridMaxSize];
		for (var l = 0; l < UtilityEditor.gridMaxSize; ++l)
		{
			for (var c = 0; c < UtilityEditor.gridMaxSize; ++c)
			{
				setTile(l, c, Cell.Void);
			}
		}
	}

	public void setBrush(GameObject go)
	{
		activeBrush = go.GetComponent<CellBrush>();
	}

	private void loadLevel(GameObject go)
	{
		resetGrid();

		string levelKey = go.GetComponent<NewLevelToLoad>().levelKey;
		XmlDocument doc = gameData.levels[levelKey].OwnerDocument;
		Utility.removeComments(doc);
		// remove comments
		Utility.removeComments(doc);
		XmlNode root = doc.ChildNodes[1];

		Tuple<int, int> position;
		Direction.Dir orientation;
		string inputLine;
		foreach (XmlNode child in root.ChildNodes)
		{
			switch (child.Name)
			{
				case "map":
					int l = 0;
					foreach (XmlNode lineNode in child.ChildNodes)
					{
						List<int> line = new List<int>();
						int c = 0;
						foreach (XmlNode cellNode in lineNode.ChildNodes)
						{
							try
							{
								int cellValue = int.Parse(cellNode.Attributes.GetNamedItem("value").Value);
								setTile(l, c, (Cell)cellValue);
                            }
                            catch
                            {
								Debug.Log("Warning: Skipped cell (" + l + "," + c + ") from file " + levelKey);
							}
							c++;
						}
						l++;
					}
					break;
				case "coin":
					try
					{
						int posCol = int.Parse(child.Attributes.GetNamedItem("posX").Value);
						int posLig = int.Parse(child.Attributes.GetNamedItem("posY").Value);
						setTile(posLig, posCol, Cell.Coin);
					}
					catch
					{
						Debug.Log("Warning: Skipped coin from file " + levelKey + ". Wrong position!");
					}
					break;
				case "console":
					try
					{
						position = getPositionFromXElement(child);
						orientation = (Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value);

						FloorObject newObj = setTile(position.Item1, position.Item2, Cell.Console, orientation);
						if (newObj != null)
						{
							List<string> slotsID = new List<string>();
							foreach (XmlNode slot in child.ChildNodes)
							{
								slotsID.Add(slot.Attributes.GetNamedItem("slotId").Value);
							}
							((Console)newObj).slots = slotsID.ToArray();
						}
					}
					catch
					{
						Debug.Log("Warning: Skipped console from file " + levelKey + ". Wrong data!");
					}
					break;
				case "door":
					try
					{
						position = getPositionFromXElement(child);
						orientation = (Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value);
						FloorObject newObj = setTile(position.Item1, position.Item2, Cell.Door, orientation);
						if (newObj != null)
						{
							string slotId = child.Attributes.GetNamedItem("slotId").Value;
							int state = int.Parse(child.Attributes.GetNamedItem("state").Value);
							Door door = ((Door)newObj);
							door.slot = slotId;
							door.state = state == 1;
						}
					}
					catch
					{
						Debug.Log("Warning: Skipped door from file " + levelKey + ". Wrong data!");
					}
					break;
				case "robot":
				case "player": // backward compatibility
					try
					{
						position = getPositionFromXElement(child);
						orientation = (Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value);
						int skin = 0;
						XmlNode xmlSkin = child.Attributes.GetNamedItem("skin");
						if (xmlSkin != null)
							skin = int.Parse(xmlSkin.Value);
						FloorObject newObj = setTile(position.Item1, position.Item2, UtilityEditor.IntToSkin(skin), orientation);
						if (newObj != null)
						{
							inputLine = child.Attributes.GetNamedItem("inputLine").Value;
							((PlayerRobot)newObj).inputLine = inputLine;
						}
					}
					catch
					{
						Debug.Log("Warning: Skipped player from file " + levelKey + ". Wrong data!");
					}
					break;
				case "guard":
				case "enemy": // backward compatibility
					try
					{
						position = getPositionFromXElement(child);
						orientation = (Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value);
						FloorObject newObj = setTile(position.Item1, position.Item2, Cell.Enemy, orientation);
						if (newObj != null)
						{
							inputLine = child.Attributes.GetNamedItem("inputLine").Value;
							int enemyRange = int.Parse(child.Attributes.GetNamedItem("range").Value);
							bool selfRange = child.Attributes.GetNamedItem("selfRange").Value == "True";
							int typeRange = int.Parse(child.Attributes.GetNamedItem("typeRange").Value);
							((EnemyRobot)newObj).inputLine = inputLine;
							((EnemyRobot)newObj).range = enemyRange;
							((EnemyRobot)newObj).selfRange = selfRange;
							((EnemyRobot)newObj).typeRange = (DetectRange.Type)typeRange;
						}
					}
					catch
					{
						Debug.Log("Warning: Skipped enemy from file " + levelKey + ". Wrong data!");
					}
					break;
				case "decoration":
					try
					{
						position = getPositionFromXElement(child);
						orientation = (Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value);
						FloorObject newObj = setTile(position.Item1, position.Item2, Cell.Decoration, orientation);
						if (newObj != null)
						{
							string decoPath = child.Attributes.GetNamedItem("name").Value;
							((DecorationObject)newObj).path = decoPath;
						}
					}
					catch
					{
						Debug.Log("Warning: Skipped decoration from file " + levelKey + ". Wrong data!");
					}
					break;
			}
		}
	}

	private Tuple<int, int> getPositionFromXElement(XmlNode element)
	{
		if (element.Attributes.GetNamedItem("posX") == null || element.Attributes.GetNamedItem("posY") == null)
			return null;

		return new Tuple<int, int>(
			int.Parse(element.Attributes.GetNamedItem("posY")?.Value ?? throw new InvalidOperationException()),
			int.Parse(element.Attributes.GetNamedItem("posX")?.Value ?? throw new InvalidOperationException()));
	}

	public FloorObject setTile(int line, int col, Cell cell, Direction.Dir rotation = Direction.Dir.North)
	{
		var tuplePos = new Tuple<int, int>(line, col);
		if (!paintableGrid.floorObjects.ContainsKey(tuplePos))
			paintableGrid.floorObjects[tuplePos] = new FloorObject[3];
		if ((int)cell < 10000) // non-configurable cell
		{
			paintableGrid.grid[line, col] = cell;
			if (cell == Cell.Wall || cell == Cell.Void)
			{
				// reset all layers
				paintableGrid.floorObjects[tuplePos] = new FloorObject[3];
				paintableGrid.GetComponent<Tilemap>().SetTile(new Vector3Int(col - _gridSize.x / 2, _gridSize.y / 2 - line, -1), null);
				paintableGrid.GetComponent<Tilemap>().SetTile(new Vector3Int(col - _gridSize.x / 2, _gridSize.y / 2 - line, -2), null);
				paintableGrid.GetComponent<Tilemap>().SetTile(new Vector3Int(col - _gridSize.x / 2, _gridSize.y / 2 - line, -3), null);
			}
			paintableGrid.GetComponent<Tilemap>().SetTile(new Vector3Int(col - _gridSize.x / 2, _gridSize.y / 2 - line, 0), cellToTile(cell));
		}
		else
		{
			// this cell is a configurable cell
			FloorObject newFloorObj = cell switch
			{
				Cell.Kyle => new PlayerRobot(Cell.Kyle, "Plok", rotation, line, col, -2),
				Cell.R102 => new PlayerRobot(Cell.R102, "R102", rotation, line, col, -2),
				Cell.Destiny => new PlayerRobot(Cell.Destiny, "Destiny", rotation, line, col, -2),
				Cell.Enemy => new EnemyRobot("Guard", rotation, line, col, -3),
				Cell.Decoration => new DecorationObject(defaultDecoration, rotation, line, col, -2),
				Cell.Door => new Door(rotation, line, col, -1),
				Cell.Console => new Console(rotation, line, col, -1),
				Cell.Coin => new FloorObject(Cell.Coin, Direction.Dir.North, line, col, -2, false),
				_ => null
			};

			FloorObject currentFloorObject = paintableGrid.floorObjects[tuplePos][-(newFloorObj.layer + 1)];
			if (currentFloorObject == null || newFloorObj.type != currentFloorObject.type)
			{
				paintableGrid.floorObjects[tuplePos][-(newFloorObj.layer + 1)] = newFloorObj;

				paintableGrid.GetComponent<Tilemap>().SetTile(new Vector3Int(col - _gridSize.x / 2, _gridSize.y / 2 - line, newFloorObj.layer), cellToTile(cell));
				rotateObject(rotation, line, col, newFloorObj.layer);
				return newFloorObj;
			}
			else
				return currentFloorObject;
		}
		return null;
	}

	public void removeTile(FloorObject floorObject)
    {
		Tuple<int, int> tuplePos = new Tuple<int, int>(floorObject.line, floorObject.col);
		paintableGrid.floorObjects[tuplePos][-(floorObject.layer+1)] = null;
		paintableGrid.GetComponent<Tilemap>().SetTile(new Vector3Int(floorObject.col - _gridSize.x / 2, _gridSize.y / 2 - floorObject.line, floorObject.layer), null);
	}

	public bool moveTile(FloorObject floorObject, int newX, int newY)
	{
		if (canBePlaced(floorObject.type, newY, newX))
		{
			Tuple<int, int> tuplePos = new Tuple<int, int>(newX, newY);
			// si la position de destination est déjà occupée
			if (paintableGrid.floorObjects.ContainsKey(tuplePos))
				// on vérifie si on n'a pas un conflit de layer
				foreach (FloorObject f in paintableGrid.floorObjects[tuplePos])
					if (f != null && f.layer == floorObject.layer)
						// si tel est le cas, on supprime l'ancien objet
						EditorGridSystem.instance.removeTile(f);
			// Maintenant on peut supprimer l'objet de sa position
			removeTile(floorObject);
			// et le déplacer à la nouvelle
			floorObject.col = newX;
			floorObject.line = newY;
			if (!paintableGrid.floorObjects.ContainsKey(tuplePos))
				paintableGrid.floorObjects[tuplePos] = new FloorObject[3];
			paintableGrid.floorObjects[tuplePos][-(floorObject.layer + 1)] = floorObject;
			paintableGrid.GetComponent<Tilemap>().SetTile(new Vector3Int(floorObject.col - _gridSize.x / 2, _gridSize.y / 2 - floorObject.line, floorObject.layer), cellToTile(floorObject.type));
			rotateObject(floorObject.orientation, floorObject.line, floorObject.col, floorObject.layer);
			return true;
		}
		return false;
	}
	
	public void rotateObject(Direction.Dir newOrientation, int line, int col, int layer)
	{
		Vector3Int newpos = new Vector3Int(col - _gridSize.x / 2,
			_gridSize.y / 2 - line, layer);
		Quaternion quat = Quaternion.Euler(0, 0, orientationToInt(newOrientation));

		paintableGrid.GetComponent<Tilemap>().SetTransformMatrix(newpos, Matrix4x4.Rotate(quat));
	}

	private int orientationToInt(Direction.Dir orientation)
	{
		return orientation switch
		{
			Direction.Dir.North => 0,
			Direction.Dir.East => 270,
			Direction.Dir.South => 180,
			Direction.Dir.West => 90,
			_ => orientationToInt((Direction.Dir) ((int) orientation % 4))
		};
	}
	
	private Tile cellToTile(Cell cell)
	{
		return cell switch
		{
			Cell.Void => voidTile,
			Cell.Ground => floorTile,
			Cell.Wall => wallTile,
			Cell.Spawn => spawnTile,
			Cell.Teleport => teleportTile,
			Cell.Kyle => kyleTile,
			Cell.R102 => r102Tile,
			Cell.Destiny => destinyTile,
			Cell.Enemy => enemyTile,
			Cell.Decoration => decoTile,
			Cell.Door => doorTile,
			Cell.Console => consoleTile,
			Cell.Coin => coinTile,
			_ => null
		};
	}

	private bool canBePlaced(Cell newCell, int l, int c)
	{
		if (l >= 0 && l < _gridSize.x && c >= 0 && c < _gridSize.y)
		{
			Cell pointedCell = paintableGrid.grid[l, c];
			return (int)newCell < 10000 || pointedCell == Cell.Ground || pointedCell == Cell.Spawn || pointedCell == Cell.Teleport;
		}
		else
			return false;
	}
}

public enum Cell
{
	Select = -10000,
	Void = -1,
	Ground = 0, 
	Wall = 1, 
	Spawn = 2, 
	Teleport = 3,
	Enemy = 10001,
	Decoration = 10002,
	Door = 10003,
	Console = 10004,
	Coin = 10005,
	Kyle = 10006,
	R102 = 10007,
	Destiny = 10008
}

public class FloorObject
{
	public Cell type;
	public Direction.Dir orientation;
	public bool orientable;
	public int line;
	public int col;
	public int layer; // 0 = groud/wall/start/end; -1 = Doot/Console; -2 = robot/furniture/coin; -3 = Enemy

	public FloorObject(Cell type, Direction.Dir orientation, int line, int col, int layer, bool orientable = true)
	{
		this.type = type;
		this.orientation = orientation;
		this.line = line;
		this.col = col;
		this.orientable = orientable;
		this.layer = layer;
	}
}

public class DecorationObject : FloorObject
{
	public string path;

	public DecorationObject(string path, Direction.Dir orientation, int line, int col, int layer) : base(Cell.Decoration, orientation, line, col, layer)
	{
		this.path = path;
	}
}

public class Console : FloorObject
{
	public string[] slots;

	public Console(Direction.Dir orientation, int line, int col, int layer) : base(Cell.Console, orientation, line, col, layer)
	{
		this.slots = new string[0];
	}
}

public class Door : FloorObject
{
	public string slot;
	public bool state;

	public Door(Direction.Dir orientation, int line, int col, int layer) : base(Cell.Door, orientation, line, col, layer)
	{
		this.slot = "0";
		this.state = false;
	}
}

public class Robot : FloorObject
{
	public string inputLine;

	protected Robot(Cell cellType, string associatedScriptName, Direction.Dir orientation, int line, int col, int layer,
		bool orientable = true) : base(cellType, orientation, line, col, layer, orientable)
	{
		this.inputLine = associatedScriptName;
	}
}

public class PlayerRobot : Robot
{
	public PlayerRobot(Cell cellType, string associatedScriptName, Direction.Dir orientation, int line, int col, int layer,
		bool orientable = true) : 
		base(cellType, associatedScriptName, orientation, line, col, layer, orientable)
	{
	}
}

public class EnemyRobot : Robot
{
	public DetectRange.Type typeRange;
	public bool selfRange;
	public int range;

	public EnemyRobot(string associatedScriptName, Direction.Dir orientation, int line, int col, int layer,
		bool selfRange = false, DetectRange.Type typeRange = DetectRange.Type.Line, bool orientable = true, int range = 3)
		: base(Cell.Enemy, associatedScriptName,orientation, line, col, layer, orientable)
	{
		this.typeRange = typeRange;
		this.selfRange = selfRange;
		this.range = range;
	}
}