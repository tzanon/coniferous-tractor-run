using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

using Pathfinding;

public class TilemapManager : MonoBehaviour
{
	// fields
	[SerializeField] private bool debugMode = false;
	[SerializeField] private bool UIDebugMode = false;
	[SerializeField] private bool clearHighlight = true;
	[SerializeField] private Color nodeHighlight;
	[SerializeField] private Color neighbourHighlight;
	[SerializeField] private Color searchHighlight;
	[SerializeField] private float hoverHighlightRefreshRate = 0.15f;
	[SerializeField] private Vector3Int testCell;
	[SerializeField] private RectTransform UIDebugMenu;

	private enum VisualDebugType { None, Cell, Neighbours, Path, Closest }
	private VisualDebugType _visualDebugtype = VisualDebugType.None;
	private Vector3Int[] visualPathPoints;
	private int visualPathIdx;


	[SerializeField] private int bfsLimit = 20;
	private bool isFindingNode = false;
	private Dictionary<Vector3Int, Vector3Int> closestNodes = new Dictionary<Vector3Int, Vector3Int>();

	private ChaserControls controls;
	private InputAction leftClick;
	private InputAction mousePosAction;

	private Vector2 _mousePosition;

	private Tilemap map;
	private Graph pathfindingGraph;

	private List<Vector3Int> highlightedCells;

	[SerializeField] private Sprite nodeSprite;

	[SerializeField] private Player player;

	// properties

	public Vector3Int PlayerCell { get => CellOfPosition(player.transform.position); }

	#region Unity functions

	private void Awake()
	{
		map = GetComponent<Tilemap>();
		foreach (Vector3Int pos in map.cellBounds.allPositionsWithin)
		{
			map.SetTileFlags(pos, (TileFlags.LockTransform));
		}

		highlightedCells = new List<Vector3Int>();
		pathfindingGraph = new Graph();

		visualPathPoints = new Vector3Int[2];
		visualPathIdx = 0;

		controls = new ChaserControls();
		leftClick = controls.Debug.LeftClick;
		mousePosAction = controls.Debug.Position;

		leftClick.performed += HandleMouseClick;
		mousePosAction.started += ctx => _mousePosition = ctx.ReadValue<Vector2>();
		mousePosAction.performed += ctx => _mousePosition = ctx.ReadValue<Vector2>();
	}

	private void Start()
	{
		PrintMapInfo();
		PrintPlayerPosition();

		UIDebugMenu.gameObject.SetActive(UIDebugMode);

		CalculateGraph();
	}

	private void OnEnable()
	{
		controls.Debug.Enable();
	}

	private void OnDisable()
	{
		controls.Debug.Disable();
	}

	#endregion

	#region Tilemap functions

	public Vector3Int CellOfPosition(Vector3 pos)
	{
		return map.WorldToCell(pos);
	}

	public Vector3 CenterPositionOfCell(Vector3Int cell)
	{
		return map.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0f);
	}

	public bool IsCellInBounds(Vector3Int cell)
	{
		return map.cellBounds.Contains(cell);
	}

	public bool IsPathfindingNode(Vector3Int cell)
	{
		return map.HasTile(cell) && map.GetSprite(cell) == nodeSprite;
	}

	/// <summary>
	/// Use BFS to find closest node to given cell
	/// </summary>
	/// <param name="startCell"> cell to start search from </param>
	/// <returns> first node found around cell </returns>
	public Vector3Int ClosestNodeToCell(Vector3Int startCell)
	{
		if (isFindingNode)
		{
			Debug.Log("Already searching for a node, must wait");
			return new Vector3Int(0, 0, -1);
		}

		if (pathfindingGraph.Nodes.Length <= 0)
		{
			Debug.LogError("Error: either no node tiles in map or graph is uninitialized");
			return new Vector3Int(0, 0, -1);
		}

		Queue<Vector3Int> cellsToEval = new Queue<Vector3Int>();
		HashSet<Vector3Int> visitedCells = new HashSet<Vector3Int>();
		cellsToEval.Enqueue(startCell);
		visitedCells.Add(startCell);

		int numCellsSoFar = 0;
		isFindingNode = true;

		while (cellsToEval.Count > 0)
		{
			Vector3Int cell = cellsToEval.Dequeue();
			if (cell == startCell)
				HighlightStandardCell(cell);
			else
				HighlightCell(cell, searchHighlight, false);

			numCellsSoFar++;

			if (IsPathfindingNode(cell))
			{
				isFindingNode = false;
				closestNodes[startCell] = cell;
				return cell;
			}

			if (numCellsSoFar >= bfsLimit)
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
				if (map.HasTile(neighbour) && !visitedCells.Contains(neighbour))
					cellsToEval.Enqueue(neighbour);
			}
		}

		Debug.LogError("Error: could not find any nodes");
		isFindingNode = false;
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
		foreach (Vector3Int pos in map.cellBounds.allPositionsWithin)
		{
			if (IsPathfindingNode(pos))
			{
				List<Vector3Int> neighbours = CalculateNodeNeighbours(pos);
				pathfindingGraph.AddNode(pos, neighbours);
			}
		}

		if (debugMode)
		{
			PrintGraphInfo();
		}
	}

	#endregion

	#region Visual debug selecting

	public void ToggleHighlightRefresh()
	{
		clearHighlight = !clearHighlight;
	}

	public void NoVisualDebug()
	{
		RemoveHighlight();
		_visualDebugtype = VisualDebugType.None;
	}

	public void CellVisualDebug()
	{
		_visualDebugtype = VisualDebugType.Cell;
	}

	public void NeighbourVisualDebug()
	{
		_visualDebugtype = VisualDebugType.Neighbours;
	}

	public void ClosestNodeVisualDebug()
	{
		_visualDebugtype = VisualDebugType.Closest;
	}

	public void PathVisualDebug()
	{
		_visualDebugtype = VisualDebugType.Path;
		visualPathIdx = 0;
		visualPathPoints[0] = visualPathPoints[1] = Vector3Int.zero;
	}

	#endregion

	#region Visual debugging

	/// <summary>
	/// Handle left mouse click for visual debugging
	/// </summary>
	/// <param name="ctx"> unused callback context </param>
	private void HandleMouseClick(InputAction.CallbackContext ctx)
	{
		if (EventSystem.current.IsPointerOverGameObject() || _visualDebugtype == VisualDebugType.None)
			return;

		Vector3 screenMousePos = _mousePosition;
		Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(_mousePosition);

		Vector3Int mouseCell = CellOfPosition(worldMousePos);

		if (debugMode)
		{
			Debug.Log("screen mouse position is " + screenMousePos);
			//Debug.Log("world mouse position is " + worldMousePos);
		}

		switch (_visualDebugtype)
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
		HighlightCell(cell, nodeHighlight, clearHighlight);
	}

	/// <summary>
	/// Highlights the given node and its neighbours
	/// </summary>
	/// <param name="node"> node to highlight neighbours of </param>
	public void HighlightNodeNeighbours(Vector3Int node)
	{
		if (!IsPathfindingNode(node))
		{
			if (debugMode)
				Debug.Log("Cell " + node + " is not a node");
			HighlightStandardCell(node);
			return;
		}

		Vector3Int[] neighbours = pathfindingGraph.NeighboursOfNode(node);

		HighlightStandardCell(node);
		HighlightCells(neighbours, neighbourHighlight, false);
	}

	/// <summary>
	/// Highlights closest node of given cell
	/// </summary>
	/// <param name="cell"> cell to search from </param>
	public void HighlightClosestNode(Vector3Int cell)
	{
		HighlightStandardCell(ClosestNodeToCell(cell));
	}

	public void AddVisualPathPoint(Vector3Int node)
	{
		if (!IsPathfindingNode(node))
		{
			Debug.Log("Cell is not a node");
			return;
		}

		if (debugMode)
			Debug.Log("Adding node " + node + " to path");

		visualPathPoints[visualPathIdx++] = node;
		HighlightCells(new Vector3Int[] { node }, nodeHighlight, false);

		if (visualPathIdx > 1)
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
		Vector3Int start = visualPathPoints[0];
		Vector3Int end = visualPathPoints[1];

		if (debugMode)
			Debug.Log(string.Format("Highlighting path between {0} and {1}...", start, end));

		if (!(IsPathfindingNode(start) && IsPathfindingNode(end)))
		{
			Debug.LogError(string.Format("one or both of cells {0} and {1} are not nodes", start, end));
			return;
		}

		//Vector3Int[] path = pathfindingGraph.BFSEarlyExit(start, end);
		Vector3Int[] path = pathfindingGraph.GetPathBetweenNodes(start, end);

		if (debugMode)
			Debug.Log("path calculated successfully!");

		HighlightCells(path, nodeHighlight, clearHighlight);

		visualPathIdx = 0;
		visualPathPoints[0] = visualPathPoints[1] = Vector3Int.zero;
	}

	/// <summary>
	/// Highlights all pathfinding node tiles
	/// </summary>
	public void HighlightAllNodes()
	{
		HighlightCells(pathfindingGraph.Nodes, nodeHighlight, clearHighlight);
	}

	/// <summary>
	/// Remove any highlighting
	/// </summary>
	public void RemoveHighlight()
	{
		foreach (Vector3Int cell in highlightedCells)
		{
			map.SetColor(cell, Color.white);
		}

		highlightedCells.Clear();
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

		if (!map.HasTile(cell))
		{
			if (debugMode)
				Debug.Log("No tile here");
			return;
		}

		map.SetColor(cell, col);
		highlightedCells.Add(cell);
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

	#region Text debugging

	public void PrintCellInfo(Vector3Int cell)
	{
		if (!IsCellInBounds(cell))
		{
			Debug.Log("Cell " + cell + " is not within bounds of this tilemap");
			return;
		}

		if (IsPathfindingNode(cell))
		{
			Debug.Log("Cell " + cell + " is in bounds and is a node");
			Debug.Log("Neighbours: " + pathfindingGraph.NeighboursOfNode(cell).ToString());
		}
		else
		{
			Debug.Log("Cell " + cell + " is in bounds");
		}
	}

	public void PrintMapInfo()
	{
		BoundsInt mapBounds = map.cellBounds;
		Debug.Log("bounds and size of tilemap are " + mapBounds);
		Debug.Log("bound min is " + mapBounds.min + ", bound max is " + mapBounds.max);
	}

	public void PrintGraphInfo()
	{
		Debug.Log("Total pathfinding nodes: " + pathfindingGraph.NumNodes);
	}

	public void PrintPlayerPosition()
	{
		Vector3 playerPos = player.transform.position;
		Debug.Log("Player at position " + playerPos + " is in cell " + PlayerCell + ", cell position is " + CenterPositionOfCell(PlayerCell));
	}

	public void PrintSpriteAtCell(Vector3Int cell)
	{
		Sprite spr = map.GetSprite(cell);
		if (spr == null)
			Debug.Log("No sprite at " + cell);
		else
			Debug.Log("sprite at " + cell + " is: " + spr.ToString());
	}

	#endregion
}
