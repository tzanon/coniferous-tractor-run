using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using Pathfinding;

public class NavigationMap : MonoBehaviour
{
	/* fields */
	[SerializeField] private Sprite _nodeSprite;

	private Graph _graph;
	private Pathfinder _pathfinder;

	// finding node from cell
	[SerializeField] [Range(0, 100)] private int _bfsLimit = 20;
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
	/// <param name="startCell"> cell to start search from </param>
	/// <param name="searched"> nodes that were searched </param>
	/// <returns> first node found around cell </returns>
	public Vector3Int ClosestNodeToCell(Vector3Int startCell /*, out List<Vector3Int> searched */)
	{
		//searched = new List<Vector3Int>();

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

			/* TODO: refactor visualization out
			if (cell == startCell)
				HighlightStandardCell(cell);
			else
				HighlightCell(cell, _searchHighlight, false);
			/**/

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

		// print graph info (if debug enabled)
		_tileManager.PrintGraphInfo();
	}

}
