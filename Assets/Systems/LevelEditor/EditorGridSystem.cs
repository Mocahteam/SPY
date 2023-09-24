using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using FYFY;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using FYFY_plugins.PointerManager;

public class EditorGridSystem : FSystem
{
	// Contains current UI focused
	private Family f_UIfocused = FamilyManager.getFamily(new AllOfComponents(typeof(RectTransform), typeof(PointerOver)));
	private Family f_brushes = FamilyManager.getFamily(new AllOfComponents(typeof(CellBrush)));
	private Family f_newLoading = FamilyManager.getFamily(new AllOfComponents(typeof(NewLevelToLoad)));
	private Family f_mapCanvasEnabled = FamilyManager.getFamily(new AnyOfTags("EditorMapCanvas"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

	public static EditorGridSystem instance;
	public Tile voidTile;
	public Tile floorTile;
	public Tile wallTile;
	public Tile spawnTile;
	public Tile teleportTile;
	public Tile playerTile;
	public Tile enemyTile;
	public Tile decoTile;
	public Tile doorTile;
	public Tile consoleTile;
	public Tile coinTile;
	public Texture2D placingCursor;
	public string defaultDecoration;
	public PaintableGrid paintableGrid;
	
	private Vector2Int _gridSize;

	private Cell activeBrush = Cell.Ground;

	private GameData gameData;

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

		resetGrid();
		// Sélectionne par défaut la brush Select
		foreach (var brush in f_brushes)
		{
			if (brush.GetComponent<CellBrush>().brush == Cell.Select)
			{
				setBrush(brush);
				break;
			}
		}
		f_newLoading.addEntryCallback(loadLevel);
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount)
	{
		if (f_mapCanvasEnabled.Count == 0 || f_UIfocused.Count > 0)
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			return;
		}

		Vector2Int pos = UtilityEditor.mousePosToGridPos(paintableGrid.GetComponent<Tilemap>());
		Tuple<int, int> posTuple = new Tuple<int, int>(pos.y, pos.x);

		if (pos.x < 0 || pos.x >= _gridSize.x || pos.y < 0 || pos.y >= _gridSize.y || !canBePlaced(activeBrush, pos.y, pos.x))
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			return;
		}

		if (Input.GetMouseButtonDown(1) && paintableGrid.floorObjects.ContainsKey(posTuple))
		{
			resetTile(pos.y, pos.x);
			return;
		}
		
		if (activeBrush == Cell.Select)
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			return;
		}

		if (placingCursor != null)
			Cursor.SetCursor(placingCursor, new Vector2(placingCursor.width / 2.0f, placingCursor.height / 2.0f), CursorMode.Auto);

		if (f_UIfocused.Count == 0 && Input.GetMouseButton(0) && activeBrush != Cell.Select)
			setTile(pos.y, pos.x, activeBrush);
	}

	public void resetGrid()
	{
		paintableGrid.floorObjects = new Dictionary<Tuple<int, int>, FloorObject>();
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
		activeBrush = go.GetComponent<CellBrush>().brush;
		foreach (var fBrush in f_brushes)
			fBrush.GetComponent<Button>().interactable = true;

		go.GetComponent<Button>().interactable = false;
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

						setTile(position.Item1, position.Item2, Cell.Console, orientation);
						
						List<string> slotsID = new List<string>();
						foreach (XmlNode slot in child.ChildNodes)
						{
							slotsID.Add(slot.Attributes.GetNamedItem("slotId").Value);
						}
						((Console)paintableGrid.floorObjects[position]).slots = slotsID.ToArray();

						int state = int.Parse(child.Attributes.GetNamedItem("state").Value);
						((Console)paintableGrid.floorObjects[position]).state = state == 1;
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
						setTile(position.Item1, position.Item2, Cell.Door, orientation);
						string slotId = child.Attributes.GetNamedItem("slotId").Value;
						((Door)paintableGrid.floorObjects[position]).slot = slotId;
					}
					catch
					{
						Debug.Log("Warning: Skipped door from file " + levelKey + ". Wrong data!");
					}
					break;
				case "player":
					try
					{
						position = getPositionFromXElement(child);
						orientation = (Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value);
						setTile(position.Item1, position.Item2, Cell.Player, orientation);
						inputLine = child.Attributes.GetNamedItem("inputLine").Value;
						((PlayerRobot)paintableGrid.floorObjects[position]).inputLine = inputLine;
					}
					catch
					{
						Debug.Log("Warning: Skipped player from file " + levelKey + ". Wrong data!");
					}
					break;
				case "enemy":
					try
					{
						position = getPositionFromXElement(child);
						orientation = (Direction.Dir)int.Parse(child.Attributes.GetNamedItem("direction").Value);
						setTile(position.Item1, position.Item2, Cell.Enemy, orientation);
						inputLine = child.Attributes.GetNamedItem("inputLine").Value;
						int enemyRange = int.Parse(child.Attributes.GetNamedItem("range").Value);
						bool selfRange = child.Attributes.GetNamedItem("selfRange").Value == "True";
						int typeRange = int.Parse(child.Attributes.GetNamedItem("typeRange").Value);
						((EnemyRobot)paintableGrid.floorObjects[position]).inputLine = inputLine;
						((EnemyRobot)paintableGrid.floorObjects[position]).range = enemyRange;
						((EnemyRobot)paintableGrid.floorObjects[position]).selfRange = selfRange;
						((EnemyRobot)paintableGrid.floorObjects[position]).typeRange = (DetectRange.Type)typeRange;
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
						string decoPath = child.Attributes.GetNamedItem("name").Value;
						setTile(position.Item1, position.Item2, Cell.Decoration, orientation);
						((DecorationObject)paintableGrid.floorObjects[position]).path = decoPath;
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

	public void setTile(int line, int col, Cell cell, Direction.Dir rotation = Direction.Dir.North)
	{
		var tuplePos = new Tuple<int, int>(line, col);
		if ((int)cell < 10000) // non-configurable cell
		{
			paintableGrid.grid[line, col] = cell;
			if (cell != Cell.Ground)
			{
				resetTile(line, col);
			}
		}
		else
		{
			// this cell is a configurable cell
			// check if this position is free or contains a different cell
			if (!paintableGrid.floorObjects.ContainsKey(tuplePos) || paintableGrid.floorObjects[tuplePos].type != cell) 
			{
				paintableGrid.floorObjects[tuplePos] =
					cell switch
					{
						Cell.Player => new PlayerRobot("Bob", rotation, line, col),
						Cell.Enemy => new EnemyRobot("Eve", rotation, line, col),
						Cell.Decoration => new DecorationObject(defaultDecoration, rotation, line, col),
						Cell.Door => new Door(rotation, line, col),
						Cell.Console => new Console(rotation, line, col),
						Cell.Coin => new FloorObject(Cell.Coin, Direction.Dir.North, line, col, false, false),
						_ => null
					};
			}
			else
			{
				return;
			}
		}

		paintableGrid.GetComponent<Tilemap>().SetTile(new Vector3Int(col - _gridSize.x / 2,
			_gridSize.y / 2 - line, 
			(int) cell < 10000 ? 0 : -1), 
			cellToTile(cell));
		
		if((int) cell >= 10000)	
			rotateObject(rotation, line, col);
	}

	public void resetTile(int l, int c)
	{
		var tuplePos = new Tuple<int, int>(l, c);
		paintableGrid.floorObjects.Remove(tuplePos);
		paintableGrid.GetComponent<Tilemap>().SetTile(
			new Vector3Int(c - _gridSize.x / 2, _gridSize.y / 2 - l, -1), 
			null);
	}
	
	private void rotateObject(Direction.Dir newOrientation, int line, int col)
	{
		var newpos = new Vector3Int(col - _gridSize.x / 2,
			_gridSize.y / 2 - line, -1);
		var quat = Quaternion.Euler(0, 0, orientationToInt(newOrientation));

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
			Cell.Player => playerTile,
			Cell.Enemy => enemyTile,
			Cell.Decoration => decoTile,
			Cell.Door => doorTile,
			Cell.Console => consoleTile,
			Cell.Coin => coinTile,
			_ => null
		};
	}

	private bool canBePlaced(Cell cell, int l, int c)
	{
		var curCell = paintableGrid.grid[l, c];
		return (int) cell < 10000 || curCell == Cell.Ground || curCell == Cell.Spawn;
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
	Player = 10000,
	Enemy = 10001,
	Decoration = 10002,
	Door = 10003,
	Console = 10004,
	Coin = 10005
}

public class FloorObject
{
	public Cell type;
	public Direction.Dir orientation;
	public bool orientable;
	public bool selectable;
	public int line;
	public int col;

	public FloorObject(Cell type, Direction.Dir orientation, int line, int col, bool orientable = true, bool selectable = true)
	{
		this.type = type;
		this.orientation = orientation;
		this.line = line;
		this.col = col;
		this.orientable = orientable;
		this.selectable = selectable;
	}
}

public class DecorationObject : FloorObject
{
	public string path;

	public DecorationObject(string path, Direction.Dir orientation, int line, int col) : base(Cell.Decoration, orientation, line, col)
	{
		this.path = path;
	}
}

public class Console : FloorObject
{
	public string[] slots;
	public bool state;

	public Console(Direction.Dir orientation, int line, int col) : base(Cell.Console, orientation, line, col)
	{
		this.slots = new string[0];
		this.state = true;
	}
}

public class Door : FloorObject
{
	public string slot;

	public Door(Direction.Dir orientation, int line, int col) : base(Cell.Door, orientation, line, col)
	{
		this.slot = "0";
	}
}

public class Robot : FloorObject
{
	public string inputLine;

	protected Robot(Cell cellType, string associatedScriptName, Direction.Dir orientation, int line, int col
		, bool orientable = true, UIRootContainer.SolutionType scriptType = UIRootContainer.SolutionType.Undefined, UIRootContainer.EditMode editMode = UIRootContainer.EditMode.Editable) : base(cellType, orientation, line, col, orientable)
	{
		this.inputLine = associatedScriptName;
	}
}

public class PlayerRobot : Robot
{
	public PlayerRobot(string associatedScriptName, Direction.Dir orientation, int line, int col,
		bool orientable = true, UIRootContainer.SolutionType scriptType = UIRootContainer.SolutionType.Undefined, UIRootContainer.EditMode editMode = UIRootContainer.EditMode.Editable) : 
		base(Cell.Player, associatedScriptName, orientation, line, col, orientable, scriptType, editMode)
	{
	}
}

public class EnemyRobot : Robot
{
	public DetectRange.Type typeRange;
	public bool selfRange;
	public int range;

	public EnemyRobot(string associatedScriptName, Direction.Dir orientation, int line, int col,
		UIRootContainer.SolutionType scriptType = UIRootContainer.SolutionType.Undefined, UIRootContainer.EditMode editMode = UIRootContainer.EditMode.Editable,
		bool selfRange = false, DetectRange.Type typeRange = DetectRange.Type.Line, bool orientable = true, bool selectable = true, int range = 3)
		: base(Cell.Enemy, associatedScriptName,orientation, line, col, orientable, scriptType, editMode)
	{
		this.typeRange = typeRange;
		this.selfRange = selfRange;
		this.range = range;
	}
}