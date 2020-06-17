using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
	struct PriorityItem
	{
		public int Priority { get; }
		public Vector3Int Point { get; }

		public PriorityItem(Vector3Int pt, int pri)
		{
			Point = pt;
			Priority = pri;
		}
	}

	/// <summary>
	/// Inefficient, brute force priority queue
	/// </summary>
	class NaivePriorityQueue
	{
		private List<PriorityItem> _queue;

		public int Count { get => _queue.Count; }

		public NaivePriorityQueue()
		{
			_queue = new List<PriorityItem>();
		}

		public void Push(Vector3Int point, int priority)
		{
			PriorityItem item = new PriorityItem(point, priority);
			_queue.Add(item);
		}

		public Vector3Int PopMin()
		{
			if (_queue.Count <= 0)
			{
				//Debug.LogError("Cannot pop from empty queue!");
				MessageLogger.LogErrorMessage(LogType.Path, "Cannot pop from empty queue!");
				return Graph.NullPos;
			}

			PriorityItem minItem = _queue[0];

			foreach (PriorityItem item in _queue)
			{
				if (item.Priority < minItem.Priority)
					minItem = item;
			}

			if (!_queue.Remove(minItem))
			{
				//Debug.LogError("Could not remove item from queue");
				MessageLogger.LogErrorMessage(LogType.Path, "Could not remove item from queue");
				return Graph.NullPos;
			}

			return minItem.Point;
		}

		public Vector3Int PopMax()
		{
			if (_queue.Count <= 0)
			{
				//Debug.LogError("Cannot pop from empty queue!");
				MessageLogger.LogErrorMessage(LogType.Path, "Cannot pop from empty queue!");
				return Graph.NullPos;
			}

			PriorityItem maxItem = _queue[0];

			foreach (PriorityItem item in _queue)
			{
				if (item.Priority > maxItem.Priority)
					maxItem = item;
			}

			if (!_queue.Remove(maxItem))
			{
				//Debug.LogError("Could not remove item from queue");
				MessageLogger.LogErrorMessage(LogType.Path, "Could not remove item from queue");
				return Graph.NullPos;
			}

			return maxItem.Point;
		}

		public void Clear() => _queue.Clear();
	}

	/// <summary>
	/// Pair of coordinates
	/// </summary>
	struct CoordinatePair
	{
		public Vector3Int Coord1 { get; private set; }
		public Vector3Int Coord2 { get; private set; }

		public CoordinatePair(Vector3Int n1, Vector3Int n2)
		{
			Coord1 = n1;
			Coord2 = n2;
		}

		public static bool operator ==(CoordinatePair a, CoordinatePair b)
		{
			return (a.Coord1 == b.Coord1 && a.Coord2 == b.Coord2) || (a.Coord1 == b.Coord2 && a.Coord2 == b.Coord1);
		}

		public static bool operator !=(CoordinatePair a, CoordinatePair b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj) => base.Equals(obj);

		public override int GetHashCode() => base.GetHashCode();

		public override string ToString()
		{
			return "Coord pair (" + Coord1 + ", " + Coord2 + ")";
		}
	}

	class Path
	{
		public static Vector3Int[] EmptyPath { get => new Vector3Int[0]; }

		public Path()
		{

		}

		public static bool IsEmpty(Vector3Int[] path) => (path.Length <= 0);

	}

	class Graph
	{
		//enum PathfinderType { BFS, DIJKSTRA, ASTAR} // TODO: implement or get rid of Dijkstra
		
		/* fields */
		
		//private PathfinderType _pathfinder = PathfinderType.ASTAR;

		private readonly Vector3Int _nullPos = new Vector3Int(0, 0, -1);

		private readonly Dictionary<Vector3Int, List<Vector3Int>> _nodeNeighbours;

		//private delegate void Heuristic(); // TODO: look up conventions and find if this is necessary

		/* properties */

		public int MaxNumNeighbours { get; }

		public int NumNodes { get => _nodeNeighbours.Keys.Count; }

		public Vector3Int[] Nodes { get => (new List<Vector3Int>(_nodeNeighbours.Keys)).ToArray(); }

		public static Vector3Int NullPos { get => new Vector3Int(0, 0, -1); }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="maxNeighbours"> Maximum number of neighbours that each node can have </param>
		public Graph(int maxNeighbours = 4)
		{
			MaxNumNeighbours = maxNeighbours;
			_nodeNeighbours = new Dictionary<Vector3Int, List<Vector3Int>>(MaxNumNeighbours);
		}

		public bool ContainsNode(Vector3Int node)
		{
			return _nodeNeighbours.ContainsKey(node);
		}

		/// <summary>
		/// Gets the neighbours of the given node
		/// </summary>
		/// <param name="node">The node to find neighbours of</param>
		/// <returns>an array of each of the neighbours' tile positions</returns>
		public Vector3Int[] Neighbours(Vector3Int node)
		{
			if (!_nodeNeighbours.ContainsKey(node))
			{
				Debug.LogError("Error: node " + node + " is not in the graph");
				return new Vector3Int[0];
			}
			else
			{
				return _nodeNeighbours[node].ToArray();
			}
		}

		public bool NodeHasNeighbour(Vector3Int node, Vector3Int neighbour)
		{
			Vector3Int[] neighbours = Neighbours(node);

			if (neighbours == null)
			{
				Debug.LogError("Error: trying to check neighbours of nonexistant node");
				return false;
			}

			foreach (Vector3Int nbr in neighbours)
			{
				if (neighbour == nbr) return true;
			}

			return false;
		}

		public void AddNode(Vector3Int node, List<Vector3Int> neighbours)
		{
			if (neighbours.Count > MaxNumNeighbours)
			{
				Debug.Log("Trying to add " + node + " with too many neighbours! Adding first " + MaxNumNeighbours);
				List<Vector3Int> truncatedNeighbours = neighbours.GetRange(0, MaxNumNeighbours);
				_nodeNeighbours.Add(node, truncatedNeighbours);
			}
			else
			{
				_nodeNeighbours.Add(node, neighbours);
			}
		}

		public bool AddNeighbour(Vector3Int node, Vector3Int neighbour)
		{
			if (!_nodeNeighbours.ContainsKey(node))
			{
				AddNode(node, new List<Vector3Int>() { neighbour });
				return true;
			}

			if (_nodeNeighbours[node].Count < MaxNumNeighbours)
			{
				_nodeNeighbours[node].Add(neighbour);
				return true;
			}
			else
			{
				Debug.LogError("Node " + node + " already has a maximum number of neigbours");
				return false;
			}
		}

		public bool RemoveNode(Vector3Int node)
		{
			return _nodeNeighbours.Remove(node);
		}

		public bool RemoveNeighbour(Vector3Int node, Vector3Int neighbour)
		{
			if (!_nodeNeighbours.ContainsKey(node))
			{
				return false;
			}

			return _nodeNeighbours[node].Remove(neighbour);
		}

		public void Clear()
		{
			_nodeNeighbours.Clear();
		}
	}

	class Pathfinder
	{
		public enum PathfinderType { Astar, Bfs, Dijkstra } // TODO: implement or get rid of Dijkstra
		private PathfinderType _pathfinder = PathfinderType.Astar;

		private Graph _graph;

		private delegate void Heuristic(); // TODO: look up conventions and find if this is necessary

		public Pathfinder(Graph graph)
		{
			_graph = graph;
		}

		// methods
		public int ManhattanDistance(Vector3Int a, Vector3Int b)
		{
			return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
		}

		public int EuclideanDistanceSquared(Vector3Int a, Vector3Int b)
		{
			return (int)(Math.Pow(b.x - a.x, 2) + Mathf.Pow(b.y - a.y, 2));
		}

		private int Cost(Vector3Int a, Vector3Int b)
		{
			return ManhattanDistance(a, b);
		}

		public Vector3Int[] CalculatePath(Vector3Int start, Vector3Int end)
		{
			switch (_pathfinder)
			{
				case PathfinderType.Bfs:
					return BFSEarlyExit(start, end);
				case PathfinderType.Astar:
					return AStarSearch(start, end);
				default:
					return AStarSearch(start, end);
			}
		}

		private Vector3Int[] BFSEarlyExit(Vector3Int start, Vector3Int goal)
		{
			Queue<Vector3Int> frontier = new Queue<Vector3Int>();
			frontier.Enqueue(start);
			Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>() { { start, Graph.NullPos } };

			while (frontier.Count > 0)
			{
				Vector3Int cell = frontier.Dequeue();

				if (cell == goal)
					break;

				Vector3Int[] neighbours = _graph.Neighbours(cell);

				foreach (Vector3Int next in neighbours)
				{
					if (!cameFrom.ContainsKey(next))
					{
						frontier.Enqueue(next);
						cameFrom[next] = cell;
					}
				}
			}

			return ReconstructPath(start, goal, cameFrom);
		}

		private Vector3Int[] Dijkstra(Vector3Int start, Vector3Int goal)
		{
			return null;
		}

		private Vector3Int[] AStarSearch(Vector3Int start, Vector3Int goal)
		{
			if (!_graph.ContainsNode(start) || !_graph.ContainsNode(goal))
			{
				MessageLogger.LogErrorMessage(LogType.Path, "Cannot find path between undefined nodes");
				return Path.EmptyPath;
			}

			if (start == goal)
			{
				return new Vector3Int[0];
			}
			
			if (_graph.NodeHasNeighbour(start, goal))
			{
				return new Vector3Int[] { start, goal };
			}

			NaivePriorityQueue frontier = new NaivePriorityQueue();
			frontier.Push(start, 0);
			Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>() { { start, Graph.NullPos } };
			Dictionary<Vector3Int, int> costSoFar = new Dictionary<Vector3Int, int>() { { start, 0 } };

			MessageLogger.LogDebugMessage(LogType.Path, "Starting A* Search...");

			while (frontier.Count > 0)
			{
				Vector3Int cell = frontier.PopMin();

				// found goal
				if (cell == goal)
				{
					MessageLogger.LogDebugMessage(LogType.Path, "Finished search.");
					return ReconstructPath(start, goal, cameFrom);
				}

				Vector3Int[] neighbours = _graph.Neighbours(cell);

				foreach (Vector3Int next in neighbours)
				{
					int newCost = costSoFar[cell] + Cost(cell, next);
					if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
					{
						costSoFar[next] = newCost;
						int priority = newCost + ManhattanDistance(next, goal);
						frontier.Push(next, priority);
						cameFrom[next] = cell;
					}
				}
			}

			MessageLogger.LogErrorMessage(LogType.Path, "No path found between {0} and {1}", start, goal);
			return Path.EmptyPath;
		}

		private Vector3Int[] ReconstructPath(Vector3Int start, Vector3Int goal, Dictionary<Vector3Int, Vector3Int> cameFrom)
		{
			if (!cameFrom.ContainsKey(goal) || !cameFrom.ContainsValue(start))
			{
				MessageLogger.LogErrorMessage(LogType.Path, "Given cameFrom does not contain the goal, start, or both");
				return null;
			}

			Debug.Log("Reconstructing path...");

			List<Vector3Int> path = new List<Vector3Int>(cameFrom.Count);
			Vector3Int cell = goal;

			while (cell != start)
			{
				path.Add(cell);
				cell = cameFrom[cell];
			}

			path.Add(start);
			path.Reverse();

			return path.ToArray();
		}
	}

	/// <summary>
	/// Abstract class for finding a path between two points
	/// </summary>
	abstract class PathfindingAlgorithm
	{
		protected Graph _graph;
		protected Vector3Int _currentStart, _currentEnd;
		protected readonly Dictionary<Vector3Int, Vector3Int> _pathTracker;
		protected readonly Dictionary<CoordinatePair, Vector3Int[]> _paths;

		protected readonly Queue<ColoredTile> _tileColors;
		protected readonly HashSet<Vector3Int> _totalVisitedTiles;
		protected Color _pathColor = new Color(0.7f, 0.1f, 0.0f, 1.0f); // same as standard tile highlight
		protected Color _clearColor = Color.white;

		/// <summary>
		/// Array of tiles in the order they had their highlights defined
		/// </summary>
		public ColoredTile[] TileHighlightOrder { get => _tileColors.ToArray(); }

		/// <summary>
		/// Total tiles visited by the algorithm
		/// </summary>
		public Vector3Int[] TotalVisitedTiles
		{
			get
			{
				Vector3Int[] visitedArr = new Vector3Int[_totalVisitedTiles.Count];
				_totalVisitedTiles.CopyTo(visitedArr);
				return visitedArr;
			}
		}

		public PathfindingAlgorithm(Graph graph)
		{
			_graph = graph;
			_currentStart = _currentEnd = Graph.NullPos;

			_pathTracker = new Dictionary<Vector3Int, Vector3Int>();
			_paths = new Dictionary<CoordinatePair, Vector3Int[]>();

			_tileColors = new Queue<ColoredTile>();
			_totalVisitedTiles = new HashSet<Vector3Int>();
		}

		public int ManhattanDistance(Vector3Int a, Vector3Int b)
		{
			return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
		}

		public int EuclideanDistanceSquared(Vector3Int a, Vector3Int b)
		{
			return (int)(Math.Pow(b.x - a.x, 2) + Mathf.Pow(b.y - a.y, 2));
		}

		protected int Cost(Vector3Int a, Vector3Int b)
		{
			return ManhattanDistance(a, b);
		}

		/// <summary>
		/// Set up data for a new search
		/// </summary>
		/// <param name="start">start node to search from</param>
		/// <param name="end">end node to find path to</param>
		protected virtual void PrepareForSearch(Vector3Int start, Vector3Int end)
		{
			_currentStart = start;
			_currentEnd = end;
			_pathTracker[start] = Graph.NullPos;
		}

		/// <summary>
		/// Reset data after a search
		/// </summary>
		protected virtual void ResetData()
		{
			_currentStart = _currentEnd = Graph.NullPos;
			_pathTracker.Clear();
			_tileColors.Clear();
			_totalVisitedTiles.Clear();
		}

		/// <summary>
		/// Calculates the path using the defined algorithm
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		protected abstract Vector3Int[] CalculatePath();

		/// <summary>
		/// Checks if the start and end nodes are in the tracker
		/// </summary>
		private bool IsTrackerValid()
		{
			if (_pathTracker.ContainsKey(_currentEnd) && _pathTracker.ContainsValue(_currentStart))
			{
				return true;
			}
			else
			{
				MessageLogger.LogErrorMessage(LogType.Path, "Tracker does not contain the goal, start, or both");
				return false;
			}
		}

		private bool AreEndpointsValid()
		{
			// return empty path if start or end node doesn't exist
			if (!_graph.ContainsNode(_currentStart) || !_graph.ContainsNode(_currentEnd))
			{
				MessageLogger.LogErrorMessage(LogType.Path, "Cannot find path between undefined nodes");
				return false;
			}
			else if (_currentStart == _currentEnd) // check if start and end are the same
			{
				MessageLogger.LogVerboseMessage(LogType.Path, "Returning empty path between two nodes that are the same");
				return false;
			}
			else
			{
				return true;
			}
			
		}

		/// <summary>
		/// Gets path between given points
		/// </summary>
		/// <param name="start">start node</param>
		/// <param name="end">end node</param>
		/// <returns>Path between start and end</returns>
		public Vector3Int[] GetPathBetweenPoints(Vector3Int start, Vector3Int end)
		{
			Vector3Int[] path;

			// TODO: get stored path if already calculated

			ResetData(); // clear data from last search
			PrepareForSearch(start, end); // set up necessary data structures
			path = AreEndpointsValid() ? CalculatePath() : Path.EmptyPath;

			return path;
		}
		
		/// <summary>
		/// Reconstructs path from nodes stored in search tracker
		/// </summary>
		/// <returns>An array of Vector3Ints representing the path</returns>
		protected Vector3Int[] ReconstructPath()
		{
			if (!IsTrackerValid())
				return null;

			MessageLogger.LogVerboseMessage(LogType.Path, "Reconstructing path...");

			List<Vector3Int> nodeList = new List<Vector3Int>(_pathTracker.Count);
			Vector3Int cell = _currentEnd;
			
			while (cell != _currentStart)
			{
				_tileColors.Enqueue(new ColoredTile(cell, _pathColor));
				nodeList.Add(cell);
				cell = _pathTracker[cell];
			}

			_tileColors.Enqueue(new ColoredTile(_currentStart, _pathColor));
			nodeList.Add(_currentStart);
			nodeList.Reverse();

			/* TODO: do this in highlighter class
			foreach (Vector3Int node in nodeList)
				_totalVisitedTiles.Remove(node);

			foreach (Vector3Int node in _totalVisitedTiles)
				_tileColors.Enqueue(new ColoredTile(node, _clearColor));
			/**/
			return nodeList.ToArray();
		}
	}

	/// <summary>
	/// A* Search algorithm
	/// </summary>
	class AStarSearch : PathfindingAlgorithm
	{
		private readonly NaivePriorityQueue _frontier;
		private readonly Dictionary<Vector3Int, int> _costSoFar;

		private Color _exploredColor = new Color(1.0f, 0.5f, 0.0f, 1.0f);
		private Color _frontierColor = new Color(0.1f, 0.5f, 1.0f, 1.0f); // medium blue

		public AStarSearch(Graph graph) : base(graph)
		{
			_frontier = new NaivePriorityQueue();
			_costSoFar = new Dictionary<Vector3Int, int>();
		}

		protected override void PrepareForSearch(Vector3Int start, Vector3Int end)
		{
			base.PrepareForSearch(start, end);
			_frontier.Push(_currentStart, 0);
			_costSoFar.Add(_currentStart, 0);
			_tileColors.Enqueue(new ColoredTile(start, _pathColor));
		}

		protected override void ResetData()
		{
			base.ResetData();
			_frontier.Clear();
			_costSoFar.Clear();
		}

		/// <summary>
		/// Calculates the shortest path between _start and _end
		/// </summary>
		/// <returns>A Vector3Int[] representing the shortest path</returns>
		protected override Vector3Int[] CalculatePath()
		{
			MessageLogger.LogVerboseMessage(LogType.Path, "Starting A* Search...");

			while (_frontier.Count > 0)
			{
				Vector3Int cell = _frontier.PopMin();
				_tileColors.Enqueue(new ColoredTile(cell, _frontierColor));
				_totalVisitedTiles.Add(cell);

				// check if current goal found
				if (cell == _currentEnd)
				{
					MessageLogger.LogVerboseMessage(LogType.Path, "Finished A* Search.");
					_tileColors.Enqueue(new ColoredTile(cell, _pathColor));
					return ReconstructPath();
				}

				Vector3Int[] neighbours = _graph.Neighbours(cell);
				
				foreach (Vector3Int next in neighbours)
				{
					int newCost = _costSoFar[cell] + Cost(cell, next);
					if (!_costSoFar.ContainsKey(next) || newCost < _costSoFar[next])
					{
						_tileColors.Enqueue(new ColoredTile(next, _exploredColor));
						_totalVisitedTiles.Add(next);
						_costSoFar[next] = newCost;
						int priority = newCost + ManhattanDistance(next, _currentEnd);
						_frontier.Push(next, priority);
						_pathTracker[next] = cell;
						//_tileColors.Enqueue(new ColoredTile(cell, _pathColor));
					}
				}
			}

			MessageLogger.LogErrorMessage(LogType.Path, "No path found between {0} and {1}", _currentStart, _currentEnd);
			return Path.EmptyPath;
		}
	}

	class BFSEarlyExit
	{

	}

	class Dijkstra
	{

	}
}
