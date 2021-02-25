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
	public class NaivePriorityQueue
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
	public struct CoordinatePair
	{
		public Vector3Int Coord1 { get; private set; }
		public Vector3Int Coord2 { get; private set; }

		public CoordinatePair(Vector3Int n1, Vector3Int n2)
		{
			Coord1 = n1;
			Coord2 = n2;
		}

		public override string ToString()
		{
			return "Coord pair (" + Coord1 + ", " + Coord2 + ")";
		}
	}

	/// <summary>
	/// List of adjacent points for computer-controlled movement to use
	/// </summary>
	public class Path : IEnumerable<Vector3Int>
	{
		/* properties */

		public Vector3Int[] Points { get; }

		public int Length { get => Points.Length; }

		public bool Empty { get => Length <= 0; }

		public static Path EmptyPath { get => new Path(new Vector3Int[0]); }

		public Path Reverse
		{
			get
			{
				Vector3Int[] revPoints = new Vector3Int[Length];
				Array.Copy(Points, revPoints, Length);
				Array.Reverse(revPoints);

				Path reversed = new Path(revPoints);
				return reversed;
			}
		}

		public Vector3Int this[int idx] { get => Points[idx]; }

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="pathPoints">Vector3Int points to create path from</param>
		public Path(Vector3Int[] pathPoints)
		{
			Points = pathPoints;

			if (!Validate())
			{
				throw new Exception("ERROR: Trying to create path from non-adjacent points!");
			}
		}

		/* methods */

		private bool Validate() => ArrayIsValidPath(Points);

		/// <summary>
		/// Checks if path contains the given point
		/// </summary>
		/// <param name="point">Point to check</param>
		/// <returns>True if point in path, false if not</returns>
		public bool Contains(Vector3Int point)
		{
			foreach (var p in Points)
				if (p == point)
					return true;
			return false;
		}

		/// <summary>
		/// Gets index of given point
		/// </summary>
		/// <param name="point">Point to search index of</param>
		/// <returns>Point's index or -1 if not found</returns>
		public int IndexOf(Vector3Int point)
		{
			for (int i = 0; i < Points.Length; i++)
				if (Points[i] == point)
					return i;

			return -1;
		}

		/// <summary>
		/// Concatenate two adjacent paths together, NOT commutative!
		/// </summary>
		/// <param name="path1">First path</param>
		/// <param name="path2">Path whose starting point is the same as the first path's end point</param>
		/// <returns>New path consisting of the operands' points</returns>
		public static Path operator+(Path path1, Path path2)
		{
			if (path1.Empty)
				return path2;
			if (path2.Empty)
				return path1;

			if (path1[path1.Length-1] != path2[0])
			{
				MessageLogger.LogErrorMessage(LogType.Path, "ERROR: attempting to concatenate two non-adjacent paths!");
				return EmptyPath;
			}

			Vector3Int[] concatPoints = new Vector3Int[path1.Length + path2.Length - 1];

			for (int i = 0; i < path1.Length; i++)
			{
				concatPoints[i] = path1[i];
			}
			for (int i = 1; i < path2.Length; i++)
			{
				concatPoints[path1.Length + i - 1] = path2[i];
			}

			return new Path(concatPoints);
		}

		/// <summary>
		/// Checks if an array of points is a valid path
		/// </summary>
		/// <param name="points">Vector3Int points to validate</param>
		/// <returns>True if valid, false if not</returns>
		public static bool ArrayIsValidPath(Vector3Int[] points)
		{
			if (points.Length == 0 || points.Length == 1) return true;

			for (int i = 0; i < points.Length - 1; i++)
			{
				if ((points[i] - points[i+1]).sqrMagnitude != 1 )
				{
					return false;
				}
			}

			return true;
		}

		public IEnumerator<Vector3Int> GetEnumerator()
		{
			return ((IEnumerable<Vector3Int>)Points).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)Points).GetEnumerator();
		}

		public override bool Equals(object obj)
		{
			Path otherPath = (Path)obj;

			if (this.Length != otherPath.Length)
				return false;

			for (int i = 0; i < this.Length; i++)
			{
				if (otherPath[i] != this[i])
				{
					return false;
				}
			}

			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	/// <summary>
	/// A sequence of waypoints with paths between them
	/// </summary>
	public class Route
	{
		protected Vector3Int[] _waypoints;

		/* properties */

		public Path CompletePath { get; private set; }

		public bool Empty { get => CompletePath.Empty || _waypoints.Length <= 0; }

		public static Route EmptyRoute { get => new Route(Path.EmptyPath); }

		/* methods */

		public Route(Path completePath, params Vector3Int[] waypoints)
		{
			SetRoute(completePath, waypoints);
		}

		public bool Contains(Vector3Int point) => CompletePath.Contains(point);

		public void SetRoute(Path completePath, params Vector3Int[] waypoints)
		{
			if (!Valid(completePath, waypoints))
			{
				throw new Exception("ERROR: cannot create route from given path and waypoints!");
			}

			_waypoints = waypoints;
			CompletePath = completePath;
		}

		protected virtual bool Valid(Path completePath, params Vector3Int[] waypoints)
		{
			if (waypoints.Length == 0 && completePath.Length == 0)
				return true;

			// make sure every waypoint is in the path and in the same order as its own list
			var waypointIdx = 0;
			foreach (var point in completePath)
			{
				//Debug.Log("comparing point " + point + " with waypoint at index " + waypointIdx + " out of " + (waypoints.Length - 1));
				if (point == waypoints[waypointIdx])
				{
					if (waypointIdx == waypoints.Length - 1)
						return point == completePath[completePath.Length - 1];
					else
						waypointIdx++;
				}
			}

			return false;
		}

		public Vector3Int ClosestWaypoint(Vector3Int cell)
		{
			if (_waypoints.Length <= 0)
				MessageLogger.LogWarningMessage(LogType.Path, "Route is empty. Returning null position.");

			var minDistance = float.MaxValue;
			var closestWaypoint = Graph.NullPos;

			// find closest waypoint
			foreach (var waypoint in _waypoints)
			{
				if (waypoint == cell)
					return waypoint;

				var distance = Vector3.SqrMagnitude(cell - waypoint);
				if (distance < minDistance)
				{
					minDistance = distance;
					closestWaypoint = waypoint;
				}
			}

			return closestWaypoint;
		}

		public int PathIndexOfClosestWaypoint(Vector3Int cell)
		{
			var closestWaypoint = ClosestWaypoint(cell);
			return Array.IndexOf(CompletePath.Points, closestWaypoint);
		}

		public Vector3Int ClosestPathPoint(Vector3Int cell)
		{
			var closestIdx = PathIndexOfClosestPoint(cell);
			return CompletePath[closestIdx];
		}

		public int PathIndexOfClosestPoint(Vector3Int cell)
		{
			if (CompletePath.Length <= 0)
				MessageLogger.LogWarningMessage(LogType.Path, "Route is empty. Returning null index.");

			var minDistance = float.MaxValue;
			var minIdx = -1;

			// find index of closest path point
			for (var i = 0; i < CompletePath.Length; i++)
			{
				if (CompletePath[i] == cell)
					return i;

				var distance = Vector3.SqrMagnitude(cell - CompletePath[i]);
				if (distance < minDistance)
				{
					minDistance = distance;
					minIdx = i;
				}
			}

			return minIdx;
		}

		public override bool Equals(object obj)
		{
			var otherRoute = (Route)obj;
			return (this.CompletePath.Equals(otherRoute.CompletePath));
		}

		public override int GetHashCode() => base.GetHashCode();
	}

	/// <summary>
	/// Route with a path connecting the last and first waypoints
	/// </summary>
	public class CyclicRoute : Route
	{
		public static CyclicRoute EmptyCycle => new CyclicRoute(Path.EmptyPath);

		public CyclicRoute(Path completePath, params Vector3Int[] waypoints) : base(completePath, waypoints) { }

		protected override bool Valid(Path completePath, params Vector3Int[] waypoints)
		{
			if (waypoints.Length == 0 && completePath.Length == 0)
				return true;

			var cycleWaypoints = new Vector3Int[waypoints.Length + 1];
			Array.Copy(waypoints, cycleWaypoints, waypoints.Length);
			cycleWaypoints[waypoints.Length] = waypoints[0];

			return base.Valid(completePath, cycleWaypoints);
		}
	}

	/// <summary>
	/// Calculates and stores paths for a graph
	/// </summary>
	public class PathCalculator
	{
		private readonly PathfindingAlgorithm _pathfinder;
		private readonly Dictionary<CoordinatePair, Path> _paths;

		public PathfindingAlgorithm Algorithm { get => _pathfinder; }

		public PathCalculator(PathfindingAlgorithm pathfinder)
		{
			_pathfinder = pathfinder;
			_paths = new Dictionary<CoordinatePair, Path>();
		}

		public bool HasPath(Vector3Int start, Vector3Int end)
		{
			return _paths.ContainsKey(new CoordinatePair(start, end));
		}

		public Path GetPath(Vector3Int start, Vector3Int end)
		{
			var pair = new CoordinatePair(start, end);

			if (_paths.ContainsKey(pair))
			{
				return _paths[pair];
			}
			
			Path path = _pathfinder.GetPathBetweenPoints(start, end);
			_paths.Add(pair, path);
			return path;
		}
	}

	/// <summary>
	/// Calculates and stores routes for a graph
	/// </summary>
	public class RouteCalculator
	{
		private readonly PathCalculator _pathCalculator;
		private readonly Dictionary<Vector3Int[], Route> _routes;
		private readonly Dictionary<Vector3Int[], CyclicRoute> _cyclicRoutes;

		public RouteCalculator(PathCalculator pathCalculator)
		{
			_pathCalculator = pathCalculator;
			_routes = new Dictionary<Vector3Int[], Route>();
			_cyclicRoutes = new Dictionary<Vector3Int[], CyclicRoute>();
		}

		private Path CreatePathFromWaypoints(Vector3Int[] waypoints)
		{
			var path = Path.EmptyPath;

			// calculate paths between each pair of waypoints
			for (var i = 0; i < waypoints.Length - 1; i++)
			{
				// add intermediate path into central large path
				var intermediatePath = _pathCalculator.GetPath(waypoints[i], waypoints[i + 1]);
				path += intermediatePath;
			}

			return path;
		}

		public Route GetRoute(Vector3Int[] waypoints)
		{
			if (_routes.ContainsKey(waypoints))
				return _routes[waypoints];

			var completePath = CreatePathFromWaypoints(waypoints);

			// create route DS with waypoints and path
			var route = new Route(completePath, waypoints);
			_routes.Add(waypoints, route);
			return route;
		}

		public CyclicRoute GetCyclicRoute(Vector3Int[] waypoints)
		{
			// if already calculated cycle, return it
			if (_cyclicRoutes.ContainsKey(waypoints))
				return _cyclicRoutes[waypoints];

			// get normal route to use its path as a starting point
			var normalRoute = GetRoute(waypoints);
			var completePath = normalRoute.CompletePath;

			// add path from end to start
			var end = waypoints[waypoints.Length - 1];
			var start = waypoints[0];
			var endToStartPath = _pathCalculator.GetPath(end, start);
			completePath += endToStartPath;

			var cycle = new CyclicRoute(completePath, waypoints);
			_cyclicRoutes.Add(waypoints, cycle);
			return cycle;
		}
	}

	/// <summary>
	/// Finds and stores closest nodes for a given non-node cell
	/// </summary>
	public class NodeFinder
	{
		// TODO
	}

	/// <summary>
	/// Graph data structure
	/// </summary>
	public class Graph
	{
		/* fields */

		private readonly Vector3Int _nullPos = new Vector3Int(0, 0, -1);

		private readonly Dictionary<Vector3Int, List<Vector3Int>> _nodeNeighbours;

		/* properties */

		public int MaxNumNeighbours { get; }

		public int NumNodes { get => _nodeNeighbours.Keys.Count; }

		public Vector3Int[] Nodes { get => (new List<Vector3Int>(_nodeNeighbours.Keys)).ToArray(); }

		public static Vector3Int NullPos { get => new Vector3Int(0, 0, -1); }

		/// <summary>
		/// Constructs graph with given max neighbours for each node
		/// </summary>
		/// <param name="maxNeighbours">Maximum number of neighbours that each node can have</param>
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

	/// <summary>
	/// Abstract class for finding a path between two points
	/// </summary>
	public abstract class PathfindingAlgorithm
	{
		protected Graph _graph;
		protected Vector3Int _currentStart, _currentEnd;
		protected readonly Dictionary<Vector3Int, Vector3Int> _pathTracker;

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
		protected abstract Path CalculatePath();

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
				MessageLogger.LogWarningMessage(LogType.Path, "Returning empty path between two identical nodes");
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
		public Path GetPathBetweenPoints(Vector3Int start, Vector3Int end)
		{
			Path path;

			ResetData(); // clear data from last search
			PrepareForSearch(start, end); // set up necessary data structures
			path = AreEndpointsValid() ? CalculatePath() : Path.EmptyPath;

			return path;
		}
		
		/// <summary>
		/// Reconstructs path from nodes stored in search tracker
		/// </summary>
		/// <returns>An array of Vector3Ints representing the path</returns>
		protected Path ReconstructPath()
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

			return new Path(nodeList.ToArray());
		}
	}

	/// <summary>
	/// A* Search algorithm
	/// </summary>
	public class AStarSearch : PathfindingAlgorithm
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
		protected override Path CalculatePath()
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

	public class BFSEarlyExit
	{

	}

	public class Dijkstra
	{

	}
}
