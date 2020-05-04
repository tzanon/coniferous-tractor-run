using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using Directions;
using Pathfinding;

public class NavigationMap : MonoBehaviour
{
	/* fields */
	[SerializeField] private Sprite _nodeSprite;

	private Graph _graph;
	private Pathfinder _pathfinder;

	// finding node from cell
	[SerializeField] [Range(4, 100)] private int _bfsLimit = 20;
	private bool _isFindingNode = false;
	private Dictionary<Vector3Int, Vector3Int> _closestNodes = new Dictionary<Vector3Int, Vector3Int>();

	// components
	private Tilemap _map;
	private TilemapManager _tileManager;

	/* properties */
	public int PathfindingNodeCount { get => _graph.NumNodes; }
	public Vector3Int[] PathfindingNodes { get => _graph.Nodes; }

	/* methods */

	private void Awake()
	{
		_map = GetComponent<Tilemap>();
		_tileManager = GetComponent<TilemapManager>();

		_graph = new Graph();
		_pathfinder = new Pathfinder(_graph);
	}

	private void Start()
	{
		CalculateGraph();
	}

	public bool IsPathfindingNode(Vector3Int cell)
	{
		return _map.HasTile(cell) && _map.GetSprite(cell) == _nodeSprite;
	}

	/* former node direction method
	/// <summary>
	/// Calculates the direction vector between a node and its neighbour
	/// </summary>
	/// <param name="from">Node starting from</param>
	/// <param name="to">Neighbour to get direction to</param>
	/// <returns>MovementVector corresponding to the direction, Null MV if to is not a neighbour</returns>
	public MovementVector DirectionToNode(Vector3Int from, Vector3Int to)
	{
		Vector3Int difference = to - from;
		MovementVector mv = MovementVector.Vec3IntToMV(difference);

		if (mv == MovementVector.Null)
		{
			MessageLogger.LogGraphMessage("Null direction to node", LogLevel.Error);
		}

		return mv;
	}
	/**/

	public Vector3Int[] GetNeighboursOfNode(Vector3Int node)
	{
		return _graph.Neighbours(node);
	}

	public Vector3Int[] FindPathBetweenNodes(Vector3Int start, Vector3Int end)
	{
		return _pathfinder.CalculatePath(start, end);
	}

	/// <summary>
	/// Use BFS to find closest node to given cell
	/// </summary>
	/// <param name="startCell">Cell to start search from</param>
	/// <param name="searchedCells">Nodes that were searched</param>
	/// <returns>The first node found around cell</returns>
	public Vector3Int ClosestNodeToCell(Vector3Int startCell, out Queue<Vector3Int> evaluatedCells)
	{
		// return if already searching
		if (_isFindingNode)
		{
			MessageLogger.LogGraphMessage("Already searching for a node, must wait", LogLevel.Debug);
			evaluatedCells = null;
			return new Vector3Int(0, 0, -1);
		}

		_isFindingNode = true;

		// return if graph not initialized
		if (_graph.Nodes.Length <= 0)
		{
			MessageLogger.LogGraphMessage("Error: either no node tiles in map or graph is uninitialized", LogLevel.Error);
			evaluatedCells = null;
			_isFindingNode = false;
			return new Vector3Int(0, 0, -1);
		}

		HashSet<Vector3Int> searchedCells = new HashSet<Vector3Int>();
		searchedCells.Add(startCell);

		Queue<Vector3Int> cellsToEval = new Queue<Vector3Int>();
		cellsToEval.Enqueue(startCell);

		evaluatedCells = new Queue<Vector3Int>();
		int numCellsSoFar = 0;

		while (cellsToEval.Count > 0)
		{
			Vector3Int cell = cellsToEval.Dequeue();
			evaluatedCells.Enqueue(cell);

			// break and return first cell that is found to be a node
			if (IsPathfindingNode(cell))
			{
				_isFindingNode = false;
				_closestNodes[startCell] = cell;
				return cell;
			}

			if (++numCellsSoFar >= _bfsLimit)
			{
				MessageLogger.LogGraphMessage("Error: have exceeded search limit of {0} cells", LogLevel.Error, numCellsSoFar);
				break;
			}

			Vector3Int[] surroundingCells = GetSurroundingCells(cell);

			foreach (Vector3Int neighbour in surroundingCells)
			{
				if (_map.HasTile(neighbour) && !searchedCells.Contains(neighbour))
				{
					cellsToEval.Enqueue(neighbour);
					searchedCells.Add(neighbour);
				}
			}
		}

		MessageLogger.LogGraphMessage("Error: could not find any nodes", LogLevel.Error);

		_isFindingNode = false;
		return new Vector3Int(0, 0, -1);
	}

	/// <summary>
	/// Find closest node without returning the searched cells
	/// </summary>
	/// <param name="startCell">Cell to start search from</param>
	/// <returns>The first node found around cell</returns>
	public Vector3Int ClosestNodeToCell(Vector3Int startCell)
	{
		Queue<Vector3Int> unusedCells;
		return ClosestNodeToCell(startCell, out unusedCells);
	}

	/// <summary>
	/// Calculates the adjacent nodes of the given node
	/// </summary>
	/// <param name="node">Node to calculate neighbours of</param>
	/// <returns>List of the nieghbours</returns>
	private List<Vector3Int> CalculateNodeNeighbours(Vector3Int node)
	{
		if (!IsPathfindingNode(node))
		{
			//Debug.LogError("Given cell is either not in the map or not a node");
			MessageLogger.LogGraphMessage("Given cell is either not in the map or not a node", LogLevel.Error);
			return null;
		}

		List<Vector3Int> neighbours = new List<Vector3Int>();

		Vector3Int[] possibleNeighbours = GetSurroundingCells(node);

		foreach (Vector3Int cell in possibleNeighbours)
		{
			if (IsPathfindingNode(cell))
			{
				neighbours.Add(cell);
			}
		}

		return neighbours;
	}

	/// <summary>
	/// Returns the cardinal cells surrounding the given one
	/// </summary>
	/// <param name="cell">Cell to get surrounding cells of</param>
	/// <returns>Array of coordinates of the surrounding cells</returns>
	private Vector3Int[] GetSurroundingCells(Vector3Int cell)
	{
		Vector3Int[] surroundingCells =
		{
			new Vector3Int(cell.x+1, cell.y, 0),
			new Vector3Int(cell.x-1, cell.y, 0),
			new Vector3Int(cell.x, cell.y+1, 0),
			new Vector3Int(cell.x, cell.y-1, 0),
		};

		return surroundingCells;
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

		// print graph info (if debug enabled)
		_tileManager.PrintGraphInfo();
	}

}
