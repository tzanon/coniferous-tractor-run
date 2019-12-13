using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
	class Graph
	{
		private Dictionary<Vector3Int, List<Vector3Int>> nodeNeighbours;
		
		private int _maxNumNeighbours;
		
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
		/// <param name="maxNeighbours">  </param>
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

				return;
			}

			nodeNeighbours.Add(node, neighbours);
		}

		public void AddNeighbour(Vector3Int node, Vector3Int neighbour)
		{
			
		}

		public bool RemoveNode(Vector3Int node)
		{

			return true;
		}

		private bool RemoveNodeNeighbour(Vector3Int node, Vector3Int neighbour)
		{

			return true;
		}

		public void Clear()
		{
			nodeNeighbours.Clear();
		}

	}
}
