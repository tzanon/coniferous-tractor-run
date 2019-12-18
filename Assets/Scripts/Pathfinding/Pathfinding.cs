using System;
using System.Collections.Generic;
using UnityEngine;

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

		public void Add(Vector3Int point, int priority)
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

			return maxItem.point;
		}

		public void Clear()
		{
			queue.Clear();
		}
	}

	class Graph
	{
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
		public List<Vector3Int> Nodes { get => new List<Vector3Int>(nodeNeighbours.Keys); }

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

		public List<Vector3Int> ReconstructPath(Vector3Int start, Vector3Int goal, Dictionary<Vector3Int, Vector3Int> cameFrom)
		{
			if (!cameFrom.ContainsKey(goal) || !cameFrom.ContainsValue(start))
			{
				Debug.LogError("Given cameFrom does not contain the start");
				return null;
			}

			List<Vector3Int> path = new List<Vector3Int>(cameFrom.Count);
			Vector3Int cell = goal;
			
			while(cell != start)
			{
				path.Add(cell);
				cell = cameFrom[cell];
			}

			path.Add(start);
			path.Reverse();

			return path;
		}

	}
}
