using System;
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
				//Logger.LogPathError("Cannot pop from empty queue!");
				MessageLogger.LogPathMessage("Cannot pop from empty queue!", MessageLogger.Level.Error);
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
				//Logger.LogPathError("Could not remove item from queue");
				MessageLogger.LogPathMessage("Could not remove item from queue", MessageLogger.Level.Error);
				return Graph.NullPos;
			}

			return minItem.Point;
		}

		public Vector3Int PopMax()
		{
			if (_queue.Count <= 0)
			{
				//Debug.LogError("Cannot pop from empty queue!");
				//Logger.LogPathError("Cannot pop from empty queue!");
				MessageLogger.LogPathMessage("Cannot pop from empty queue!", MessageLogger.Level.Error);
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
				//Logger.LogPathError("Could not remove item from queue");
				MessageLogger.LogPathMessage("Could not remove item from queue", MessageLogger.Level.Error);
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

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return "Coord pair (" + Coord1 + ", " + Coord2 + ")";
		}
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
				return null;
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

		/* pathfinding
		// todo: refactor pathfinding into separate class?

		public int ManhattanDistance(Vector3Int a, Vector3Int b)
		{
			return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
		}

		public int EuclideanDistance(Vector3Int a, Vector3Int b)
		{
			return (int)(Math.Pow(b.x - a.x, 2) + Mathf.Pow(b.y - a.y, 2));
		}

		private int Cost(Vector3Int a, Vector3Int b)
		{
			return ManhattanDistance(a, b);
		}

		public Vector3Int[] GetPathBetweenNodes(Vector3Int start, Vector3Int end)
		{
			switch (_pathfinder)
			{
				case PathfinderType.BFS:
					return BFSEarlyExit(start, end);
				case PathfinderType.ASTAR:
					return AStarSearch(start, end);
				default:
					return AStarSearch(start, end);
			}
		}

		private Vector3Int[] BFSEarlyExit(Vector3Int start, Vector3Int goal)
		{
			Queue<Vector3Int> frontier = new Queue<Vector3Int>();
			frontier.Enqueue(start);
			Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>() { { start, _nullPos } };

			while (frontier.Count > 0)
			{
				Vector3Int cell = frontier.Dequeue();

				if (cell == goal)
					break;

				foreach (Vector3Int next in _nodeNeighbours[cell])
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
			if (start == goal)
			{
				return null;
			}

			if (_nodeNeighbours[start].Contains(goal))
			{
				return new Vector3Int[] { goal };
			}

			NaivePriorityQueue frontier = new NaivePriorityQueue();
			frontier.Push(start, 0);
			Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>() { { start, _nullPos } };
			Dictionary<Vector3Int, int> costSoFar = new Dictionary<Vector3Int, int>() { { start, 0 } };

			Debug.Log("Starting A* Search...");

			while (frontier.Count > 0)
			{
				Vector3Int cell = frontier.PopMin();

				if (cell == goal)
					break;

				foreach (Vector3Int next in _nodeNeighbours[cell])
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

			Debug.Log("Finished search.");

			return ReconstructPath(start, goal, cameFrom);
		}

		private Vector3Int[] ReconstructPath(Vector3Int start, Vector3Int goal, Dictionary<Vector3Int, Vector3Int> cameFrom)
		{
			if (!cameFrom.ContainsKey(goal) || !cameFrom.ContainsValue(start))
			{
				Debug.LogError("Given cameFrom does not contain the goal, start, or both");
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
		/**/
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
			if (start == goal)
			{
				return null;
			}
			
			if (_graph.NodeHasNeighbour(start, goal))
			{
				return new Vector3Int[] { start, goal };
			}

			NaivePriorityQueue frontier = new NaivePriorityQueue();
			frontier.Push(start, 0);
			Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>() { { start, Graph.NullPos } };
			Dictionary<Vector3Int, int> costSoFar = new Dictionary<Vector3Int, int>() { { start, 0 } };

			Debug.Log("Starting A* Search...");

			while (frontier.Count > 0)
			{
				Vector3Int cell = frontier.PopMin();

				if (cell == goal)
					break;

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

			Debug.Log("Finished search.");

			return ReconstructPath(start, goal, cameFrom);
		}

		private Vector3Int[] ReconstructPath(Vector3Int start, Vector3Int goal, Dictionary<Vector3Int, Vector3Int> cameFrom)
		{
			if (!cameFrom.ContainsKey(goal) || !cameFrom.ContainsValue(start))
			{
				Debug.LogError("Given cameFrom does not contain the goal, start, or both");
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
}
