using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using Pathfinding;

public class TilemapManager : MonoBehaviour
{
	public bool debugMode = false;
	public bool visualDebugMode = false;
	public Color nodeHighlight;
	public Color neighbourHighlight;
	public Color pathHighlight;
	public float highlightRefreshRate = 0.15f;
	public Vector3Int testCell;

	public RectTransform visualDebugMenu;

	private enum VisualDebugType { None, Cell, Neighbours }
	private VisualDebugType _visualDebugtype = VisualDebugType.None;

	private ChaserControls controls;
	private InputAction leftClick;
	private InputAction mousePosAction;

	private Vector2 _mousePosition;

	private Tilemap map;
	private Graph pathfindingGraph;

	private List<Vector3Int> highlightedCells;

	[SerializeField] private Sprite nodeSprite;

	public Player player;

	public Vector3 PlayerCell { get => CellOfPosition(player.transform.position); }

	#region Unity functions

	private void Awake()
	{
		map = GetComponent<Tilemap>();
		foreach (Vector3Int pos in map.cellBounds.allPositionsWithin)
		{
			map.SetTileFlags(pos, (TileFlags.LockTransform));
		}

		highlightedCells = new List<Vector3Int>();
		pathfindingGraph = new Graph(maxNeighbours: 4);

		controls = new ChaserControls();
		leftClick = controls.Debug.LeftClick;
		mousePosAction = controls.Debug.Position;

		//leftClick.started += HandleMouseClick;
		leftClick.performed += HandleMouseClick;
		mousePosAction.started += ctx => _mousePosition = ctx.ReadValue<Vector2>();
		mousePosAction.performed += ctx => _mousePosition = ctx.ReadValue<Vector2>();
	}

	private void Start()
	{
		PrintMapInfo();
		PrintPlayerPosition();
		//PrintCellInfo(testCell);

		visualDebugMenu.gameObject.SetActive(visualDebugMode);

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

	public bool IsCellInBounds(Vector3Int cell)
	{
		return map.cellBounds.Contains(cell);
	}

	public bool IsPathfindingNode(Vector3Int cell)
	{
		return map.HasTile(cell) && map.GetSprite(cell) == nodeSprite;
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
			HighlightAllNodes();
		}
	}

	#endregion

	#region debugging

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

	/// <summary>
	/// Handle left mouse click for visual debugging
	/// </summary>
	/// <param name="ctx"> unused callback context </param>
	private void HandleMouseClick(InputAction.CallbackContext ctx)
	{
		if (_visualDebugtype == VisualDebugType.None)
			return;

		Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(_mousePosition);
		Vector3Int mouseCell = CellOfPosition(worldMousePos);

		if (debugMode)
			Debug.Log("mouse position is " + worldMousePos);

		switch (_visualDebugtype)
		{
			case VisualDebugType.Cell:
				HighlightCell(mouseCell);
				break;
			case VisualDebugType.Neighbours:
				HighlightNodeNeighbours(mouseCell);
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// Highlight a tile on the tilemap
	/// </summary>
	/// <param name="cell"> position of tile to highlight </param>
	public void HighlightCell(Vector3Int cell)
	{
		HighlightCells(new Vector3Int[] { cell }, nodeHighlight);
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
			HighlightCell(node);
			return;
		}

		Vector3Int[] neighbours = pathfindingGraph.NeighboursOfNode(node);

		HighlightCell(node);
		HighlightCells(neighbours, neighbourHighlight, false);
	}

	/// <summary>
	/// Highlights all pathfinding node tiles
	/// </summary>
	public void HighlightAllNodes()
	{
		HighlightCells(pathfindingGraph.Nodes.ToArray(), nodeHighlight);
	}

	/// <summary>
	/// Highlights the path between the given nodes
	/// </summary>
	/// <param name="start"> start node of path </param>
	/// <param name="end"> end node of path </param>
	public void HighlightPath(Vector3Int start, Vector3Int end)
	{

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
	/// Highlight a set of tiles with the given colour
	/// </summary>
	/// <param name="cells"> Cells to highlight </param>
	/// <param name="col"> Colour to highlight tiles with </param>
	/// <param name="removeExistingHighlight"> whether to reset currently highlighted cell </param>
	private void HighlightCells(Vector3Int[] cells, Color col, bool removeExistingHighlight=true)
	{
		if (removeExistingHighlight)
			RemoveHighlight();

		foreach (Vector3Int cell in cells)
		{
			if (!map.HasTile(cell))
			{
				if (debugMode)
					Debug.Log("No tile here");
				continue;
			}

			map.SetColor(cell, col);
			highlightedCells.Add(cell);
		}
	}

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
		Debug.Log("Player at position " + playerPos + " is in cell " + PlayerCell);
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
