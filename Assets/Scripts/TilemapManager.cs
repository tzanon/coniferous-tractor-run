//using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
//using UnityEngine.InputSystem;

//using Pathfinding;

public class TilemapManager : MonoBehaviour
{
	/* fields */

	[SerializeField] private bool _debugMode = false;

	/* visual vars
	// visual debugging
	[SerializeField] private bool _UIDebugMode = false;
	[SerializeField] private bool _clearHighlight = true;
	[SerializeField] private Color _nodeHighlight;
	[SerializeField] private Color _neighbourHighlight;
	[SerializeField] private Color _searchHighlight;
	[SerializeField] private float _hoverHighlightRefreshRate = 0.15f; // TODO: highlight tile borders on mouse hover
	[SerializeField] private Vector3Int _testCell = new Vector3Int(4, 6, 0);
	[SerializeField] private RectTransform _UIDebugMenu;

	private ChaserControls _controls;
	private InputAction _leftClick;
	private InputAction _mousePosAction;
	private Vector2 _mousePosition;
	private List<Vector3Int> _highlightedCells;

	private enum VisualDebugType { None, Cell, Neighbours, Path, Closest }
	private VisualDebugType _visualDebugType = VisualDebugType.None;
	private Vector3Int[] _visualPathPoints;
	private int _visualPathIdx;
	/**/

	/* finding node
	[SerializeField] [Range(0, 100)] private int _bfsLimit = 20;
	private bool _isFindingNode = false;
	private Dictionary<Vector3Int, Vector3Int> _closestNodes = new Dictionary<Vector3Int, Vector3Int>();
	/**/

	/* pathfinding-related
	private Graph _graph;
	private Pathfinder _pathfinder;
	[SerializeField] private Sprite _nodeSprite;
	/**/

	[SerializeField] private Player _player;

	// components
	private Tilemap _map;
	private NavigationMap _navMap;
	private TilemapVisualDebugger visualDebugger;

	/* properties */

	public Vector3 PlayerPosition { get => _player.transform.position; }

	public Vector3Int PlayerCell { get => CellOfPosition(_player.transform.position); }

	#region Unity functions

	private void Awake()
	{
		// get ref to tilemap and lock tiles in place (?)
		_map = GetComponent<Tilemap>();
		_navMap = GetComponent<NavigationMap>();

		foreach (Vector3Int pos in _map.cellBounds.allPositionsWithin)
		{
			_map.SetTileFlags(pos, (TileFlags.LockTransform));
		}
		
		/** init DS's for pathfinding
		//_highlightedCells = new List<Vector3Int>();
		//_graph = new Graph();
		//_pathfinder = new Pathfinder(_graph);
		/**/

		/** mouse init
		// init visual path debugging
		_visualPathPoints = new Vector3Int[2];
		_visualPathIdx = 0;

		// init mouse interaction
		_controls = new ChaserControls();
		_leftClick = _controls.Debug.LeftClick;
		_mousePosAction = _controls.Debug.Position;

		_leftClick.performed += HandleMouseClick;
		_mousePosAction.started += ctx => _mousePosition = ctx.ReadValue<Vector2>();
		_mousePosAction.performed += ctx => _mousePosition = ctx.ReadValue<Vector2>();
		/**/
	}

	private void Start()
	{
		//PrintMapInfo();
		//PrintPlayerPosition();

		//_UIDebugMenu.gameObject.SetActive(_UIDebugMode);

		//CalculateGraph();
	}

	/* enable/disable
	private void OnEnable()
	{
		_controls.Debug.Enable();
	}

	private void OnDisable()
	{
		_controls.Debug.Disable();
	}
	*/

	#endregion

	#region Tilemap functions

	public Vector3Int CellOfPosition(Vector3 pos)
	{
		return _map.WorldToCell(pos);
	}

	public Vector3 CenterPositionOfCell(Vector3Int cell)
	{
		return _map.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0f);
	}

	public bool IsCellInBounds(Vector3Int cell)
	{
		return _map.cellBounds.Contains(cell);
	}

	#endregion

	/* pathfinding
	public bool IsPathfindingNode(Vector3Int cell)
	{
		return _map.HasTile(cell) && _map.GetSprite(cell) == _nodeSprite;
	}

	/// <summary>
	/// Use BFS to find closest node to given cell
	/// </summary>
	/// <param name="startCell"> cell to start search from </param>
	/// <param name="searched"> nodes that were searched </param>
	/// <returns> first node found around cell </returns>
	public Vector3Int ClosestNodeToCell(Vector3Int startCell, out List<Vector3Int> searched)
	{
		searched = new List<Vector3Int>();

		if (_isFindingNode)
		{
			Debug.Log("Already searching for a node, must wait");
			return new Vector3Int(0, 0, -1);
		}

		if (_graph.Nodes.Length <= 0)
		{
			Debug.LogError("Error: either no node tiles in map or graph is uninitialized");
			return new Vector3Int(0, 0, -1);
		}

		Queue<Vector3Int> cellsToEval = new Queue<Vector3Int>();
		HashSet<Vector3Int> visitedCells = new HashSet<Vector3Int>();
		cellsToEval.Enqueue(startCell);
		visitedCells.Add(startCell);

		int numCellsSoFar = 0;
		_isFindingNode = true;

		while (cellsToEval.Count > 0)
		{
			Vector3Int cell = cellsToEval.Dequeue();

			if (cell == startCell)
				HighlightStandardCell(cell);
			else
				HighlightCell(cell, _searchHighlight, false);
			

			numCellsSoFar++;

			if (IsPathfindingNode(cell))
			{
				_isFindingNode = false;
				_closestNodes[startCell] = cell;
				return cell;
			}

			if (numCellsSoFar >= _bfsLimit)
			{
				Debug.LogError("Error: have searched " + numCellsSoFar + " cells");
				break;
			}

			Vector3Int[] surroundingCells = {
				new Vector3Int(cell.x+1, cell.y, 0),
				new Vector3Int(cell.x-1, cell.y, 0),
				new Vector3Int(cell.x, cell.y+1, 0),
				new Vector3Int(cell.x, cell.y-1, 0),
			};

			foreach (Vector3Int neighbour in surroundingCells)
			{
				if (_map.HasTile(neighbour) && !visitedCells.Contains(neighbour))
					cellsToEval.Enqueue(neighbour);
			}
		}

		Debug.LogError("Error: could not find any nodes");
		_isFindingNode = false;
		return new Vector3Int(0, 0, -1);
	}

	private List<Vector3Int> CalculateNodeNeighbours(Vector3Int node)
	{
		if (!IsPathfindingNode(node))
		{
			Debug.LogError("Given cell is either not in the map or not a node");
			return null;
		}

		List<Vector3Int> neighbours = new List<Vector3Int>();

		Vector3Int[] possibleNeighbours = {
			new Vector3Int(node.x + 1, node.y, node.z),
			new Vector3Int(node.x - 1, node.y, node.z),
			new Vector3Int(node.x, node.y + 1, node.z),
			new Vector3Int(node.x, node.y - 1, node.z),
		};

		foreach (Vector3Int cell in possibleNeighbours)
		{
			if (IsPathfindingNode(cell))
			{
				neighbours.Add(cell);
			}
		}

		return neighbours;
	}

	public void CalculateGraph()
	{
		foreach (Vector3Int pos in _map.cellBounds.allPositionsWithin)
		{
			if (IsPathfindingNode(pos))
			{
				List<Vector3Int> neighbours = CalculateNodeNeighbours(pos);
				_graph.AddNode(pos, neighbours);
			}
		}

		if (_debugMode)
		{
			PrintGraphInfo();
		}
	}
	/**/

	/* visual debugging
	#region Visual debug selecting

	public void ToggleHighlightRefresh()
	{
		_clearHighlight = !_clearHighlight;
	}

	public void NoVisualDebug()
	{
		RemoveHighlight();
		_visualDebugType = VisualDebugType.None;
	}

	public void CellVisualDebug()
	{
		_visualDebugType = VisualDebugType.Cell;
	}

	public void NeighbourVisualDebug()
	{
		_visualDebugType = VisualDebugType.Neighbours;
	}

	public void ClosestNodeVisualDebug()
	{
		_visualDebugType = VisualDebugType.Closest;
	}

	public void PathVisualDebug()
	{
		_visualDebugType = VisualDebugType.Path;
		_visualPathIdx = 0;
		_visualPathPoints[0] = _visualPathPoints[1] = Vector3Int.zero;
	}

	#endregion

	#region Visual debugging

	/// <summary>
	/// Handle left mouse click for visual debugging
	/// </summary>
	/// <param name="ctx"> unused callback context </param>
	private void HandleMouseClick(InputAction.CallbackContext ctx)
	{
		if (EventSystem.current.IsPointerOverGameObject() || _visualDebugType == VisualDebugType.None)
			return;

		Vector3 screenMousePos = _mousePosition;
		Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(_mousePosition);

		Vector3Int mouseCell = CellOfPosition(worldMousePos);

		if (_debugMode)
		{
			Debug.Log("screen mouse position is " + screenMousePos);
			//Debug.Log("world mouse position is " + worldMousePos);
		}

		switch (_visualDebugType)
		{
			case VisualDebugType.Cell:
				HighlightStandardCell(mouseCell);
				break;
			case VisualDebugType.Neighbours:
				HighlightNodeNeighbours(mouseCell);
				break;
			case VisualDebugType.Path:
				AddVisualPathPoint(mouseCell);
				break;
			case VisualDebugType.Closest:
				HighlightClosestNode(mouseCell);
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// Highlight a tile with default tint
	/// </summary>
	/// <param name="cell"> position of tile to highlight </param>
	public void HighlightStandardCell(Vector3Int cell)
	{
		HighlightCell(cell, _nodeHighlight, _clearHighlight);
	}

	/// <summary>
	/// Highlights the given node and its neighbours
	/// </summary>
	/// <param name="node"> node to highlight neighbours of </param>
	public void HighlightNodeNeighbours(Vector3Int node)
	{
		if (!IsPathfindingNode(node))
		{
			if (_debugMode)
				Debug.Log("Cell " + node + " is not a node");
			HighlightStandardCell(node);
			return;
		}

		Vector3Int[] neighbours = _graph.NeighboursOfNode(node);

		HighlightStandardCell(node);
		HighlightCells(neighbours, _neighbourHighlight, false);
	}

	/// <summary>
	/// Highlights closest node of given cell
	/// </summary>
	/// <param name="cell"> cell to search from </param>
	public void HighlightClosestNode(Vector3Int cell)
	{
		HighlightStandardCell(ClosestNodeToCell(cell));
	}

    /// <summary>
    /// adds node to use for highlighting a path
    /// </summary>
    /// <param name="node"> node to add </param>
	public void AddVisualPathPoint(Vector3Int node)
	{
		if (!IsPathfindingNode(node))
		{
			Debug.Log("Cell is not a node");
			return;
		}

		if (_debugMode)
			Debug.Log("Adding node " + node + " to path");

		_visualPathPoints[_visualPathIdx++] = node;
		HighlightCells(new Vector3Int[] { node }, _nodeHighlight, false);

		if (_visualPathIdx > 1)
		{
			HighlightPath();
		}
	}

	/// <summary>
	/// Highlights the path between currently chosen nodes
	/// </summary>
	/// <param name="start"> start node of path </param>
	/// <param name="end"> end node of path </param>
	public void HighlightPath()
	{
		Vector3Int start = _visualPathPoints[0];
		Vector3Int end = _visualPathPoints[1];

		if (_debugMode)
			Debug.Log(string.Format("Highlighting path between {0} and {1}...", start, end));

		if (!(IsPathfindingNode(start) && IsPathfindingNode(end)))
		{
			Debug.LogError(string.Format("one or both of cells {0} and {1} are not nodes", start, end));
			return;
		}

		Vector3Int[] path = _pathfinder.GetPathBetweenNodes(start, end);

		if (_debugMode)
			Debug.Log("path calculated successfully!");

		HighlightCells(path, _nodeHighlight, _clearHighlight);

		_visualPathIdx = 0;
		_visualPathPoints[0] = _visualPathPoints[1] = Vector3Int.zero;
	}

	/// <summary>
	/// Highlights all pathfinding node tiles
	/// </summary>
	public void HighlightAllNodes()
	{
		HighlightCells(_graph.Nodes, _nodeHighlight, _clearHighlight);
	}

	/// <summary>
	/// Remove any highlighting
	/// </summary>
	public void RemoveHighlight()
	{
		foreach (Vector3Int cell in _highlightedCells)
		{
			_map.SetColor(cell, Color.white);
		}

		_highlightedCells.Clear();
	}

	/// <summary>
	/// Highlight a single tile with the given colour
	/// </summary>
	/// <param name="cell"> Coordinates of tile to highlight </param>
	/// <param name="col"> Colour to highlight tile with </param>
	/// <param name="removeExistingHighlight"> Whether to reset currently highlighted cells </param>
	public void HighlightCell(Vector3Int cell, Color col, bool removeExistingHighlight = true)
	{
		if (removeExistingHighlight)
			RemoveHighlight();

		if (!_map.HasTile(cell))
		{
			if (_debugMode)
				Debug.Log("No tile here");
			return;
		}

		_map.SetColor(cell, col);
		_highlightedCells.Add(cell);
	}

	/// <summary>
	/// Highlight a set of tiles with the given colour
	/// </summary>
	/// <param name="cells"> Cells to highlight </param>
	/// <param name="col"> Colour to highlight tiles with </param>
	/// <param name="removeExistingHighlight"> Whether to reset currently highlighted cells </param>
	private void HighlightCells(Vector3Int[] cells, Color col, bool removeExistingHighlight = true)
	{
		if (removeExistingHighlight)
			RemoveHighlight();

		foreach (Vector3Int cell in cells)
		{
			HighlightCell(cell, col, false);
		}
	}

	#endregion
	/**/

	#region Text debugging

	public void PrintCellInfo(Vector3Int cell)
	{
		if (!IsCellInBounds(cell))
		{
			Debug.Log("Cell " + cell + " is not within bounds of this tilemap");
			return;
		}

		if (_navMap.IsPathfindingNode(cell))
		{
			Debug.Log("Cell " + cell + " is in bounds and is a node");
			Debug.Log("Neighbours: " +  _navMap.GetNeighboursOfNode(cell).ToString());
		}
		else
		{
			Debug.Log("Cell " + cell + " is in bounds");
		}
	}

	public void PrintMapInfo()
	{
		BoundsInt mapBounds = _map.cellBounds;
		Debug.Log("bounds and size of tilemap are " + mapBounds);
		Debug.Log("bound min is " + mapBounds.min + ", bound max is " + mapBounds.max);
	}

	public void PrintGraphInfo()
	{
		Debug.Log("Total pathfinding nodes: " + _navMap.PathfindingNodeCount);
	}

	public void PrintPlayerPosition()
	{
		Vector3 playerPos = _player.transform.position;
		Debug.Log("Player at position " + playerPos + " is in cell " + PlayerCell + ", cell position is " + CenterPositionOfCell(PlayerCell));
	}

	public void PrintSpriteAtCell(Vector3Int cell)
	{
		Sprite spr = _map.GetSprite(cell);
		if (spr == null)
			Debug.Log("No sprite at " + cell);
		else
			Debug.Log("sprite at " + cell + " is: " + spr.ToString());
	}

	#endregion
	/**/
}
