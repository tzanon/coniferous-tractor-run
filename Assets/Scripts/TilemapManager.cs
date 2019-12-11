using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using Pathfinding;

public class TilemapManager : MonoBehaviour
{
	public bool debugMode = false;
	public Color nodeHighlight;
	public Color pathHighlight;

	public Vector3Int testCell;

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

	public void Start()
	{
		PrintMapInfo();
		PrintPlayerPosition();
		//PrintCellInfo(testCell);

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

	private void HandleMouseClick(InputAction.CallbackContext ctx)
	{
		Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(_mousePosition);
		Vector3Int mouseCell = CellOfPosition(worldMousePos);

		Debug.Log("mouse position is " + worldMousePos);

		//HighlightCell(mouseCell);
		HighlightNodeNeighbours(mouseCell);
	}

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

	#region debugging

	public void HighlightCell(Vector3Int cell)
	{
		HighlightCells(new Vector3Int[] { cell }, nodeHighlight);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="node">  </param>
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
		Vector3Int[] nodes = new Vector3Int[neighbours.Length + 1];

		nodes[neighbours.Length] = node;
		for (int i = 0; i < neighbours.Length; i++)
			nodes[i] = neighbours[i];

		HighlightCells(nodes, nodeHighlight);
	}

	public void HighlightAllNodes()
	{
		HighlightCells(pathfindingGraph.Nodes.ToArray(), nodeHighlight);
	}

	public void RemoveHighlight()
	{
		foreach (Vector3Int cell in highlightedCells)
		{
			map.SetColor(cell, Color.white);
		}

		highlightedCells.Clear();
	}

	// not highlighting grass tiles?
	private void HighlightCells(Vector3Int[] cells, Color col)
	{
		RemoveHighlight();

		foreach (Vector3Int cell in cells)
		{
			if (!map.HasTile(cell))
			{
				if (debugMode)
					Debug.Log("No tile here");
				continue;
			}

			if (debugMode)
				Debug.Log("Setting cell " + cell + " to node color");
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
