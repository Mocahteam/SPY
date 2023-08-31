using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using FYFY_plugins.PointerManager;

public class EditorGridSystem : FSystem
{
	// Contains current UI focused
	private Family f_UIfocused = FamilyManager.getFamily(new AllOfComponents(typeof(RectTransform), typeof(PointerOver)));
	private Family f_brushes = FamilyManager.getFamily(new AllOfComponents(typeof(CellBrush)));
	private Family f_newLoading = FamilyManager.getFamily(new AllOfComponents(typeof(NewLevelToLoad)));

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
	public GameObject mainCanvas;
	public GameObject dialogViewPortContent;
	public GameObject listEntryPrefab;
	public PaintableGrid paintableGrid;
	
	private Vector2Int _gridSize;
	public LevelData levelData;

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
		if (!paintableGrid.gridActive)
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			return;
		}

		Vector2Int pos = UtilityEditor.mousePosToGridPos(paintableGrid.GetComponent<Tilemap>());
		Tuple<int, int> posTuple = new Tuple<int, int>(pos.y, pos.x);

		if (pos.x < 0 || pos.x >= _gridSize.x || pos.y < 0 || pos.y >= _gridSize.y || !canBePlaced(paintableGrid.activeBrush, pos.y, pos.x))
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			return;
		}

		if (Input.GetMouseButtonDown(1) && paintableGrid.floorObjects.ContainsKey(posTuple))
		{
			resetTile(pos.y, pos.x);
			return;
		}
		
		if (paintableGrid.activeBrush == Cell.Select)
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			return;
		}

		if (placingCursor != null)
			Cursor.SetCursor(placingCursor, new Vector2(placingCursor.width / 2.0f, placingCursor.height / 2.0f), CursorMode.Auto);

		if (f_UIfocused.Count == 0 && Input.GetMouseButton(0) && paintableGrid.activeBrush != Cell.Select)
			setTile(pos.y, pos.x, paintableGrid.activeBrush);
	}

	public void resetGrid()
	{
		paintableGrid.floorObjects = new Dictionary<Tuple<int, int>, FloorObject>();
		paintableGrid.gridActive = true;
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
		paintableGrid.activeBrush = go.GetComponent<CellBrush>().brush;
		foreach (var fBrush in f_brushes)
			fBrush.GetComponent<Button>().interactable = true;

		go.GetComponent<Button>().interactable = false;
	}

	private void loadLevel(GameObject go)
	{
		resetGrid();

		string levelKey = go.GetComponent<NewLevelToLoad>().levelKey;
		XmlDocument doc = gameData.levels[levelKey].OwnerDocument;
		// remove comments
		Utility.removeComments(doc);
		XmlNode root = doc.ChildNodes[1];

		Tuple<int, int> position;
		ObjectDirection orientation;
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
						orientation = (ObjectDirection)int.Parse(child.Attributes.GetNamedItem("direction").Value);

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
						orientation = (ObjectDirection)int.Parse(child.Attributes.GetNamedItem("direction").Value);
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
						orientation = (ObjectDirection)int.Parse(child.Attributes.GetNamedItem("direction").Value);
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
						orientation = (ObjectDirection)int.Parse(child.Attributes.GetNamedItem("direction").Value);
						setTile(position.Item1, position.Item2, Cell.Enemy, orientation);
						inputLine = child.Attributes.GetNamedItem("inputLine").Value;
						int enemyRange = int.Parse(child.Attributes.GetNamedItem("range").Value);
						bool selfRange = child.Attributes.GetNamedItem("selfRange").Value == "True";
						int typeRange = int.Parse(child.Attributes.GetNamedItem("typeRange").Value);
						((EnemyRobot)paintableGrid.floorObjects[position]).inputLine = inputLine;
						((EnemyRobot)paintableGrid.floorObjects[position]).range = enemyRange;
						((EnemyRobot)paintableGrid.floorObjects[position]).selfRange = selfRange;
						((EnemyRobot)paintableGrid.floorObjects[position]).typeRange = (EnemyTypeRange)typeRange;
					}
					catch
					{
						Debug.Log("Warning: Skipped enemy from file " + levelKey + ". Wrong data!");
					}
					break;
			}
		}

		/*
		if (File.Exists(levelData.filePath))
		{
			var xmlScriptGenerator = new XmlScriptGenerator(mainCanvas);
			var xmlFile = XDocument.Load(levelData.filePath);
			var scripts = new List<XElement>();
			var robots = new Dictionary<string, Tuple<int, int>>();
			foreach (var element in xmlFile.Descendants("level").Elements())
			{
				Tuple<int, int> position;
				ObjectDirection orientation;
				int slotId;
				string associatedScriptName;
				switch (element.Name.LocalName)
				{
					case "player":
						// TODO: player script type
						// Factorise
						position = getPositionFromXElement(element);
						orientation = (ObjectDirection) int.Parse(element.Attribute("direction").Value);
						setTile(position.Item1, position.Item2, Cell.Player, orientation);
						
						if (element.Attribute("associatedScriptName") != null)
						{
							associatedScriptName = element.Attribute("associatedScriptName").Value;
							((PlayerRobot)paintableGrid.floorObjects[position]).associatedScriptName =
								associatedScriptName;
							robots[associatedScriptName] = position;
						}
						
						break;
					case "enemy":
						position = getPositionFromXElement(element);
						orientation = (ObjectDirection) int.Parse(element.Attribute("direction").Value);
						setTile(position.Item1, position.Item2, Cell.Enemy, orientation);
						
						if (element.Attribute("associatedScriptName") != null)
						{
							associatedScriptName = element.Attribute("associatedScriptName").Value;
							((EnemyRobot)paintableGrid.floorObjects[position]).associatedScriptName =
								associatedScriptName;
							robots[associatedScriptName] = position;
						}
						
						var enemyRange = int.Parse(element.Attribute("range").Value);
						var selfRange = element.Attribute("selfRange").Value == "True";
						var typeRange = (EnemyTypeRange)int.Parse(element.Attribute("typeRange").Value);
						((EnemyRobot)paintableGrid.floorObjects[position]).range = enemyRange;
						((EnemyRobot)paintableGrid.floorObjects[position]).selfRange = selfRange;
						((EnemyRobot)paintableGrid.floorObjects[position]).typeRange = typeRange;
						
						break;
					case "decoration":
						position = getPositionFromXElement(element);
						orientation = (ObjectDirection) int.Parse(element.Attribute("direction").Value);
						var decoPath = element.Attribute("name").Value;
						setTile(position.Item1, position.Item2, Cell.Decoration, orientation);
						((DecorationObject)paintableGrid.floorObjects[position]).path = decoPath;
						
						break;
				}
			}

			foreach (var scriptElement in scripts)
			{
				var node = new XmlDocument().ReadNode(scriptElement.CreateReader());
				xmlScriptGenerator.readXMLScript(node, node.Attributes.GetNamedItem("outputLine").Value, UIRootContainer.EditMode.Editable, UIRootContainer.SolutionType.Undefined);
			}

			levelData.requireRefresh = true;
		}
		else
		{
			initGrid();
		}

		levelData.isReady = true;
		GameObjectManager.setGameObjectState(paintableGrid.gameObject, true);*/
	}

	private Tuple<int, int> getPositionFromXElement(XmlNode element)
	{
		if (element.Attributes.GetNamedItem("posX") == null || element.Attributes.GetNamedItem("posY") == null)
			return null;

		return new Tuple<int, int>(
			int.Parse(element.Attributes.GetNamedItem("posY")?.Value ?? throw new InvalidOperationException()),
			int.Parse(element.Attributes.GetNamedItem("posX")?.Value ?? throw new InvalidOperationException()));
	}

	private Vector2Int vector3ToGridPos(Vector3Int vec)
	{
		return new Vector2Int(vec.x + _gridSize.x / 2, _gridSize.y / 2 + vec.y * -1);
	}

	public void setTile(int line, int col, Cell cell, ObjectDirection rotation = ObjectDirection.Up)
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
						Cell.Coin => new FloorObject(Cell.Coin, ObjectDirection.Up, line, col, false, false),
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
	
	private void rotateObject(ObjectDirection newOrientation, int line, int col)
	{
		var newpos = new Vector3Int(col - _gridSize.x / 2,
			_gridSize.y / 2 - line, -1);
		var quat = Quaternion.Euler(0, 0, orientationToInt(newOrientation));

		paintableGrid.GetComponent<Tilemap>().SetTransformMatrix(newpos, Matrix4x4.Rotate(quat));
	}

	private int orientationToInt(ObjectDirection orientation)
	{
		return orientation switch
		{
			ObjectDirection.Up => 0,
			ObjectDirection.Right => 270,
			ObjectDirection.Down => 180,
			ObjectDirection.Left => 90,
			_ => orientationToInt((ObjectDirection) ((int) orientation % 4))
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
	
	private class XmlScriptGenerator
	{ 
		private Family f_draggableElement = FamilyManager.getFamily(new AnyOfComponents(typeof(ElementToDrag)));
		private GameObject canvas;
		
		public XmlScriptGenerator(GameObject canvas)
		{
			this.canvas = canvas;
		}
		
		// Lit le XML d'un script est génère les game objects des instructions
		public void readXMLScript(XmlNode scriptNode, string name, UIRootContainer.EditMode editMode, UIRootContainer.SolutionType type)
		{
			if(scriptNode != null){
				List<GameObject> script = new List<GameObject>();
				foreach(XmlNode actionNode in scriptNode.ChildNodes){
					script.Add(readXMLInstruction(actionNode));
				}
				GameObjectManager.addComponent<AddSpecificContainer>(MainLoop.instance.gameObject, new { title = name, editState = editMode, typeState = type, script = script });
			}
		}

		private GameObject getLibraryItemByName(string itemName)
		{
			foreach (GameObject item in f_draggableElement)
				if (item.name == itemName)
					return item;
			return null;
		}

		// Transforme le noeud d'action XML en gameObject
		private GameObject readXMLInstruction(XmlNode actionNode){
			GameObject obj = null;
			Transform conditionContainer = null;
			Transform firstContainerBloc = null;
			Transform secondContainerBloc = null;
			switch (actionNode.Name)
			{
				case "if":
					obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName("IfThen"), canvas);

					conditionContainer = obj.transform.Find("ConditionContainer");
					firstContainerBloc = obj.transform.Find("Container");

					// On ajoute les éléments enfants dans les bons containers
					foreach (XmlNode containerNode in actionNode.ChildNodes)
					{
						// Ajout des conditions
						if (containerNode.Name == "condition")
						{
							if (containerNode.HasChildNodes)
							{
								// The first child of the conditional container of a If action contains the ReplacementSlot
								GameObject emptyZone = conditionContainer.GetChild(0).gameObject;
								// Parse xml condition
								GameObject child = readXMLCondition(containerNode.FirstChild);
								// Add child to empty zone
								Utility.addItemOnDropArea(child, emptyZone);
							}
						}
						else if (containerNode.Name == "container")
						{
							if (containerNode.HasChildNodes)
								processXMLInstruction(firstContainerBloc, containerNode);
						}
					}
					break;

				case "ifElse":
					obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName("IfElse"), canvas);
					conditionContainer = obj.transform.Find("ConditionContainer");
					firstContainerBloc = obj.transform.Find("Container");
					secondContainerBloc = obj.transform.Find("ElseContainer");

					// On ajoute les éléments enfants dans les bons containers
					foreach (XmlNode containerNode in actionNode.ChildNodes)
					{
						// Ajout des conditions
						if (containerNode.Name == "condition")
						{
							if (containerNode.HasChildNodes)
							{
								// The first child of the conditional container of a IfElse action contains the ReplacementSlot
								GameObject emptyZone = conditionContainer.GetChild(0).gameObject;
								// Parse xml condition
								GameObject child = readXMLCondition(containerNode.FirstChild);
								// Add child to empty zone
								Utility.addItemOnDropArea(child, emptyZone);
							}
						}
						else if (containerNode.Name == "thenContainer")
						{
							if (containerNode.HasChildNodes)
								processXMLInstruction(firstContainerBloc, containerNode);
						}
						else if (containerNode.Name == "elseContainer")
						{
							if (containerNode.HasChildNodes)
								processXMLInstruction(secondContainerBloc, containerNode);
						}
					}
					break;

				case "for":
					obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName("ForLoop"), canvas);
					firstContainerBloc = obj.transform.Find("Container");
					BaseElement action = obj.GetComponent<ForControl>();

					((ForControl)action).nbFor = int.Parse(actionNode.Attributes.GetNamedItem("nbFor").Value);
					obj.transform.GetComponentInChildren<TMP_InputField>().text = ((ForControl)action).nbFor.ToString();

					if (actionNode.HasChildNodes)
						processXMLInstruction(firstContainerBloc, actionNode);
					break;

				case "while":
					obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName("While"), canvas);
					firstContainerBloc = obj.transform.Find("Container");
					conditionContainer = obj.transform.Find("ConditionContainer");

					// On ajoute les éléments enfants dans les bons containers
					foreach (XmlNode containerNode in actionNode.ChildNodes)
					{
						// Ajout des conditions
						if (containerNode.Name == "condition")
						{
							if (containerNode.HasChildNodes)
							{
								// The first child of the conditional container of a While action contains the ReplacementSlot
								GameObject emptyZone = conditionContainer.GetChild(0).gameObject;
								// Parse xml condition
								GameObject child = readXMLCondition(containerNode.FirstChild);
								// Add child to empty zone
								Utility.addItemOnDropArea(child, emptyZone);
							}
						}
						else if (containerNode.Name == "container")
						{
							if (containerNode.HasChildNodes)
								processXMLInstruction(firstContainerBloc, containerNode);
						}
					}
					break;

				case "forever":
					obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName("Forever"), canvas);
					firstContainerBloc = obj.transform.Find("Container");

					if (actionNode.HasChildNodes)
						processXMLInstruction(firstContainerBloc, actionNode);
					break;
				case "action":
					obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName(actionNode.Attributes.GetNamedItem("type").Value), canvas);
					break;
			}

			return obj;
		}

		private void processXMLInstruction(Transform gameContainer, XmlNode xmlContainer)
		{
			// The first child of a control container is an emptySolt
			GameObject emptySlot = gameContainer.GetChild(0).gameObject;
			foreach (XmlNode eleNode in xmlContainer.ChildNodes)
				Utility.addItemOnDropArea(readXMLInstruction(eleNode), emptySlot);
		}

		// Transforme le noeud d'action XML en gameObject élément/opérator
		private GameObject readXMLCondition(XmlNode conditionNode) {
			GameObject obj = null;
			ReplacementSlot[] slots = null;
			switch (conditionNode.Name)
			{
				case "and":
					obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName("AndOperator"), canvas);
					slots = obj.GetComponentsInChildren<ReplacementSlot>(true);
					if (conditionNode.HasChildNodes)
					{
						GameObject emptyZone = null;
						foreach (XmlNode andNode in conditionNode.ChildNodes)
						{
							if (andNode.Name == "conditionLeft")
								// The Left slot is the second ReplacementSlot (first is the And operator)
								emptyZone = slots[1].gameObject;
							if (andNode.Name == "conditionRight")
								// The Right slot is the third ReplacementSlot
								emptyZone = slots[2].gameObject;
							if (emptyZone != null && andNode.HasChildNodes)
							{
								// Parse xml condition
								GameObject child = readXMLCondition(andNode.FirstChild);
								// Add child to empty zone
								Utility.addItemOnDropArea(child, emptyZone);
							}
							emptyZone = null;
						}
					}
					break;

				case "or":
					obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName("OrOperator"), canvas);
					slots = obj.GetComponentsInChildren<ReplacementSlot>(true);
					if (conditionNode.HasChildNodes)
					{
						GameObject emptyZone = null;
						foreach (XmlNode orNode in conditionNode.ChildNodes)
						{
							if (orNode.Name == "conditionLeft")
								// The Left slot is the second ReplacementSlot (first is the And operator)
								emptyZone = slots[1].gameObject;
							if (orNode.Name == "conditionRight")
								// The Right slot is the third ReplacementSlot
								emptyZone = slots[2].gameObject;
							if (emptyZone != null && orNode.HasChildNodes)
							{
								// Parse xml condition
								GameObject child = readXMLCondition(orNode.FirstChild);
								// Add child to empty zone
								Utility.addItemOnDropArea(child, emptyZone);
							}
							emptyZone = null;
						}
					}
					break;

				case "not":
					obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName("NotOperator"), canvas);
					if (conditionNode.HasChildNodes)
					{
						GameObject emptyZone = obj.transform.Find("Container").GetChild(1).gameObject;
						GameObject child = readXMLCondition(conditionNode.FirstChild);
						// Add child to empty zone
						Utility.addItemOnDropArea(child, emptyZone);
					}
					break;
				case "captor":
					obj = Utility.createEditableBlockFromLibrary(getLibraryItemByName(conditionNode.Attributes.GetNamedItem("type").Value), canvas);
					break;
			}

			return obj;
		}
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

public enum ObjectDirection
{
	Up = 0,
	Down = 1,
	Right = 2,
	Left = 3
}

public enum EnemyTypeRange
{
	LineView = 0,
	//The following two are unimplemented
	ConeView = 1,
	AroundView = 2
}

public enum ScriptType
{
	Optimal = 0,
	NonOptimal = 1,
	Bugged = 2,
	Undefined = 3
}

public enum ScriptEditMode
{
	Locked = 0,
	Sync = 1,
	Editable = 2
}

public class FloorObject
{
	public Cell type;
	public ObjectDirection orientation;
	public bool orientable;
	public bool selectable;
	public int line;
	public int col;

	public FloorObject(Cell type, ObjectDirection orientation, int line, int col, bool orientable = true, bool selectable = true)
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

	public DecorationObject(string path, ObjectDirection orientation, int line, int col) : base(Cell.Decoration, orientation, line, col)
	{
		this.path = path;
	}
}

public class Console : FloorObject
{
	public string[] slots;
	public bool state;

	public Console(ObjectDirection orientation, int line, int col) : base(Cell.Console, orientation, line, col)
	{
		this.slots = new string[0];
		this.state = true;
	}
}

public class Door : FloorObject
{
	public string slot;

	public Door(ObjectDirection orientation, int line, int col) : base(Cell.Door, orientation, line, col)
	{
		this.slot = "0";
	}
}

public class Robot : FloorObject
{
	public string inputLine;
	private static readonly Dictionary<string, Tuple<ScriptType, ScriptEditMode>> ScriptParams = 
		new Dictionary<string, Tuple<ScriptType, ScriptEditMode>>();

	protected Robot(Cell cellType, string associatedScriptName, ObjectDirection orientation, int line, int col
		, bool orientable = true, ScriptType scriptType = ScriptType.Undefined, ScriptEditMode editMode = ScriptEditMode.Editable) : base(cellType, orientation, line, col, orientable)
	{
		this.inputLine = associatedScriptName;
		if (!hasScriptParams())
		{
			setScriptParams(scriptType, editMode);
		}
	}

	public void editInputLine(string newName)
	{
		inputLine = newName;
	}

	public Tuple<ScriptType, ScriptEditMode> getScriptParams()
	{
		if (ScriptParams.ContainsKey(inputLine))
			return ScriptParams[inputLine];
		ScriptParams[inputLine] =
			new Tuple<ScriptType, ScriptEditMode>(ScriptType.Undefined, ScriptEditMode.Editable);
		return ScriptParams[inputLine];
	}

	public void setScriptParams(ScriptType robotScriptType, ScriptEditMode editMode)
	{
		ScriptParams[inputLine] = new Tuple<ScriptType, ScriptEditMode>(robotScriptType, editMode);
	}

	public bool hasScriptParams()
	{
		return ScriptParams.ContainsKey(inputLine);
	}

	public static void setParams(string name, ScriptType type, ScriptEditMode editMode)
	{
		ScriptParams[name] = new Tuple<ScriptType, ScriptEditMode>(type, editMode);
	}

	public static Tuple<ScriptType, ScriptEditMode> getScriptParamsFromName(string name)
	{
		return ScriptParams.ContainsKey(name) ? ScriptParams[name] : null;
	}

	public static bool nameHasScriptParams(string name)
	{
		return ScriptParams.ContainsKey(name);
	}
}

public class PlayerRobot : Robot
{
	public PlayerRobot(string associatedScriptName, ObjectDirection orientation, int line, int col,
		bool orientable = true, ScriptType scriptType = ScriptType.Undefined, ScriptEditMode editMode = ScriptEditMode.Editable) : 
		base(Cell.Player, associatedScriptName, orientation, line, col, orientable, scriptType, editMode)
	{
	}
}

public class EnemyRobot : Robot
{
	public EnemyTypeRange typeRange;
	public bool selfRange;
	public int range;

	public EnemyRobot(string associatedScriptName, ObjectDirection orientation, int line, int col, 
		ScriptType scriptType = ScriptType.Undefined, ScriptEditMode editMode = ScriptEditMode.Editable,
		bool selfRange = false, EnemyTypeRange typeRange = EnemyTypeRange.LineView, bool orientable = true, bool selectable = true, int range = 3)
		: base(Cell.Enemy, associatedScriptName,orientation, line, col, orientable, scriptType, editMode)
	{
		this.typeRange = typeRange;
		this.selfRange = selfRange;
		this.range = range;
		this.inputLine = associatedScriptName;
	}
}