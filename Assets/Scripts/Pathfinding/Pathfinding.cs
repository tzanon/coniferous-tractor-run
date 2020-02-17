using System;
using System.Collections.Generic;
using UnityEngine;

//using System.Math;

using Directions;

namespace Pathfinding
{
	struct PriorityItem
	{
		private readonly int _priority;
		private readonly Vector3Int _point;
		
		public int Priority { get { return _priority; } }
		public Vector3Int Point { get { return _point; } }

		public PriorityItem(Vector3Int pt, int pri)
		{
			_point = pt;
			_priority = pri;
		}
	}

	/// <summary>
	/// Inefficient, brute force priority _queue
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
				throw new Exception("Cannot pop from empty _queue!");
			}

			PriorityItem minItem = _queue[0];

			foreach (PriorityItem item in _queue)
			{
				if (item.Priority < minItem.Priority)
					minItem = item;
			}

			if (!_queue.Remove(minItem))
			{
				Debug.LogError("Could not remove item from _queue");
			}

			return minItem.Point;
		}

		public Vector3Int PopMax()
		{
			if (_queue.Count <= 0)
			{
				throw new Exception("Cannot pop from empty _queue!");
			}

			PriorityItem maxItem = _queue[0];

			foreach (PriorityItem item in _queue)
			{
				if (item.Priority > maxItem.Priority)
					maxItem = item;
			}

			if (!_queue.Remove(maxItem))
			{
				Debug.LogError("Could not remove item from _queue");
			}

			return maxItem.Point;
		}

		public void Clear()
		{
			_queue.Clear();
		}
	}

	class Graph
	{
		// for tracking paths
		struct NodePair
		{
			public Vector3Int Node1 { get; private set; }
			public Vector3Int Node2 { get; private set; }
			
			public NodePair(Vector3Int n1, Vector3Int n2)
			{
				Node1 = n1;
				Node2 = n2;
			}
			
			// TODO: override == and !=
		}
		
		// TODO: make separate pathfinder class (separate functionality)
		enum PathfinderType { BFS, DIJKSTRA, ASTAR} // TODO: implement or get rid of Dijkstra
		
		// fields
		
		private PathfinderType _pathfinder = PathfinderType.ASTAR;

		private readonly Vector3Int _nullPos = new Vector3Int(0, 0, -1);

		private readonly Dictionary<Vector3Int, List<Vector3Int>> _nodeNeighbours;
		
		private readonly int _maxNumNeighbours; // TODO: look up naming conventions for constants

		private delegate void Heuristic(); // TODO: look up conventions and find if this is necessary

		public int NumNodes { get => _nodeNeighbours.Keys.Count; }
		
		// properties
		public int MaxNumNeighbours { get => _maxNumNeighbours; }

		public Vector3Int[] Nodes { get => (new List<Vector3Int>(_nodeNeighbours.Keys)).ToArray(); }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="maxNeighbours"> Maximum number of neighbours that each node can have </param>
		public Graph(int maxNeighbours = 4)
		{
			_maxNumNeighbours = maxNeighbours;
			_nodeNeighbours = new Dictionary<Vector3Int, List<Vector3Int>>(_maxNumNeighbours);
		}

		public Vector3Int[] NeighboursOfNode(Vector3Int node)
		{
			if (!_nodeNeighbours.ContainsKey(node))
			{
				return null;
			}
			else
			{
				return _nodeNeighbours[node].ToArray();
			}
		}

		// TODO: decide whether to keep/get rid of this
		public MovementVector DirectionBetweenNeighbours(Vector3Int node, Vector3Int neighbour)
		{
			// z must always be 0
			if (node.z != 0 || neighbour.z != 0)
				return MovementVector.Null;

			// direction to self is (0,0)
			if (node == neighbour)
				return MovementVector.Center;

			if (node.x == neighbour.x)
			{
				if (neighbour.y > node.y)
					return MovementVector.Up;
				if (neighbour.y < node.y)
					return MovementVector.Down;
			}

			if (node.y == neighbour.y)
			{
				if (neighbour.x > node.x)
					return MovementVector.Right;
				if (neighbour.x < node.x)
					return MovementVector.Left;
			}

			// nodes are not neighbours
			return MovementVector.Null;
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

		// todo: refactor pathfinding into separate class?
		#region pathfinding

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

		#endregion
	}
}
