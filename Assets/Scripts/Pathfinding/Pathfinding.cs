using System;
using System.Collections.Generic;
using UnityEngine;

using Directions;

namespace Pathfinding
{
	

	struct PriorityItem
	{
		public readonly int priority;
		public readonly Vector3Int point;

		public PriorityItem(Vector3Int pt, int pri)
		{
			point = pt;
			priority = pri;
		}
	}

	/// <summary>
	/// Inefficient, brute force priority queue
	/// </summary>
	class NaivePriorityQueue
	{
		private List<PriorityItem> queue;

		public int Count { get => queue.Count; }

		public NaivePriorityQueue()
		{
			queue = new List<PriorityItem>();
		}

		public void Push(Vector3Int point, int priority)
		{
			PriorityItem item = new PriorityItem(point, priority);
			queue.Add(item);
		}

		public Vector3Int PopMin()
		{
			if (queue.Count <= 0)
			{
				throw new Exception("Cannot pop from empty queue!");
			}

			PriorityItem minItem = queue[0];

			foreach (PriorityItem item in queue)
			{
				if (item.priority < minItem.priority)
					minItem = item;
			}

			if (!queue.Remove(minItem))
			{
				Debug.LogError("Could not remove item from queue");
			}

			return minItem.point;
		}

		public Vector3Int PopMax()
		{
			if (queue.Count <= 0)
			{
				throw new Exception("Cannot pop from empty queue!");
			}

			PriorityItem maxItem = queue[0];

			foreach (PriorityItem item in queue)
			{
				if (item.priority > maxItem.priority)
					maxItem = item;
			}

			if (!queue.Remove(maxItem))
			{
				Debug.LogError("Could not remove item from queue");
			}

			return maxItem.point;
		}

		public void Clear()
		{
			queue.Clear();
		}
	}

	class Graph
	{
		// for tracking paths
		struct NodePair
		{
			public Vector3Int node1, node2;
		}

		enum PathfinderType { BFS, DIJKSTRA, ASTAR}
		PathfinderType pathfinder = PathfinderType.ASTAR;

		private readonly Vector3Int nullPos = new Vector3Int(0, 0, -1);

		private readonly Dictionary<Vector3Int, List<Vector3Int>> nodeNeighbours;
		
		private int _maxNumNeighbours;

		private delegate void Heuristic();

		public int NumNodes { get => nodeNeighbours.Keys.Count; }
		
		public int MaxNumNeighbours
		{
			get => _maxNumNeighbours;
			set => _maxNumNeighbours = (int)Mathf.Clamp(value, -1, Mathf.Infinity);
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector3Int[] Nodes { get => (new List<Vector3Int>(nodeNeighbours.Keys)).ToArray(); }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="maxNeighbours"> Maximum number of neighbours that each node can have </param>
		public Graph(int maxNeighbours=-1)
		{
			_maxNumNeighbours = maxNeighbours;
			nodeNeighbours = new Dictionary<Vector3Int, List<Vector3Int>>();
		}

		public Vector3Int[] NeighboursOfNode(Vector3Int node)
		{
			if (!nodeNeighbours.ContainsKey(node))
			{
				return null;
			}
			else
			{
				return nodeNeighbours[node].ToArray();
			}
		}

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
			if (MaxNumNeighbours > 0 && neighbours.Count > MaxNumNeighbours)
			{
				Debug.Log("Trying to add " + node + " with too many neighbours! Adding first " + MaxNumNeighbours);
				List<Vector3Int> truncatedNeighbours = neighbours.GetRange(0, MaxNumNeighbours);
				nodeNeighbours.Add(node, truncatedNeighbours);
			}
			else
			{
				nodeNeighbours.Add(node, neighbours);
			}
		}

		public bool AddNeighbour(Vector3Int node, Vector3Int neighbour)
		{
			if (!nodeNeighbours.ContainsKey(node))
			{
				AddNode(node, new List<Vector3Int>() { neighbour });
				return true;
			}

			if (nodeNeighbours[node].Count < MaxNumNeighbours)
			{
				nodeNeighbours[node].Add(neighbour);
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool RemoveNode(Vector3Int node)
		{
			return nodeNeighbours.Remove(node);
		}

		public bool RemoveNeighbour(Vector3Int node, Vector3Int neighbour)
		{
			if (!nodeNeighbours.ContainsKey(node))
			{
				return false;
			}

			return nodeNeighbours[node].Remove(neighbour);
		}

		public void Clear()
		{
			nodeNeighbours.Clear();
		}


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
			switch (pathfinder)
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
			Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>() { { start, nullPos } };

			while (frontier.Count > 0)
			{
				Vector3Int cell = frontier.Dequeue();

				if (cell == goal)
					break;

				foreach (Vector3Int next in nodeNeighbours[cell])
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

			if (nodeNeighbours[start].Contains(goal))
			{
				return new Vector3Int[] { goal };
			}

			NaivePriorityQueue frontier = new NaivePriorityQueue();
			frontier.Push(start, 0);
			Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>() { { start, nullPos } };
			Dictionary<Vector3Int, int> costSoFar = new Dictionary<Vector3Int, int>() { { start, 0 } };

			Debug.Log("Starting A* Search...");

			while (frontier.Count > 0)
			{
				Vector3Int cell = frontier.PopMin();

				if (cell == goal)
					break;

				foreach (Vector3Int next in nodeNeighbours[cell])
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
