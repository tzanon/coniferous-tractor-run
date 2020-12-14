using System;
using UnityEngine;
using Pathfinding;

public class LevelPathManager : MonoBehaviour
{
	// TODO: get rid of and replace with reference to Gameplay Manager
	[SerializeField] private Collectible[] _collectibles;

	private TilemapManager _tilemapManager;
	private NavigationMap _navMap;

	private void Awake()
	{
		_tilemapManager = GetComponent<TilemapManager>();
		_navMap = GetComponent<NavigationMap>();
	}

	public CyclicRoute GetLevelPatrolRoute()
	{
		// TODO: for each apple, get waypoints
		var totalWaypoints = 0;
		Vector3Int[][] waypointGroups = new Vector3Int[_collectibles.Length][];

		for (var i = 0; i < waypointGroups.Length; i++)
		{
			Vector3[] patrolPositions = _collectibles[i].PatrolPositions;
			Vector3Int[] waypointGroup = new Vector3Int[patrolPositions.Length];
			totalWaypoints += patrolPositions.Length;

			// convert Vector3 positions to Vector3Int cells
			for (var j = 0; j < patrolPositions.Length; j++)
			{
				waypointGroup[j] = _tilemapManager.CellOfPosition(patrolPositions[j]);
				
				if (!ValidatePoints(waypointGroup))
				{
					MessageLogger.LogErrorMessage(LogType.Path, "ERROR: Some patrol waypoints are not pathfinding nodes!");
					return CyclicRoute.EmptyCycle;
				}
			}
		}

		Vector3Int[] patrolWaypoints = new Vector3Int[totalWaypoints];
		// TODO: make complete waypoint array here or refactor above nested loop

		var cycle = _navMap.FindCycle();

		//return cycle;
		return CyclicRoute.EmptyCycle;
	}

	private bool ValidatePoints(Vector3Int[] points)
	{
		foreach (var point in points)
			if (!_navMap.IsPathfindingNode(point))
				return false;

		return true;
	}

}
