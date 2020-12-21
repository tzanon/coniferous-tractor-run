using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class LevelPathManager : MonoBehaviour, IObserver<CollectibleStatus>
{
	/* fields */
	[SerializeField] private Navpoint[] _testNavMarkers;

	// components
	[SerializeField] GameplayManager _gameplayManager;
	private TilemapManager _tilemapManager;
	private NavigationMap _navMap;
	private TilemapHighlighter _highlighter;

	/* properties */

	public CyclicRoute LevelPatrolRoute { get; private set; }

	/* methods */

	private void Awake()
	{
		_tilemapManager = GetComponent<TilemapManager>();
		_navMap = GetComponent<NavigationMap>();
		_highlighter = GetComponent<TilemapHighlighter>();
	}

	private void Start()
	{
		_gameplayManager.Subscribe(this);
		//HighlightTestCycle();
	}

	//debug
	private void HighlightTestCycle()
	{
		var navMarkerCells = _tilemapManager.CellsofPositions(_testNavMarkers);

		var msg = "cycle waypoints: ";
		foreach(var cell in navMarkerCells)
			msg += cell + ", ";

		Debug.Log(msg);

		var cycle = _navMap.FindCycle(navMarkerCells);
		_highlighter.HighlightPath(cycle.CompletePath);
	}

	//debug
	private void HighlightCollectibleRoutes(Collectible[] collectibles, bool cyclic = true)
	{
		if (_highlighter.RefreshEnabled)
			_highlighter.ToggleHighlightRefresh();

		foreach (var collectible in collectibles)
		{
			var waypoints = _tilemapManager.CellsofPositions(collectible.NavpointPositions);
			var route = cyclic ? _navMap.FindCycle(waypoints) : _navMap.FindRoute(waypoints);

			_highlighter.HighlightPath(route.CompletePath);
		}

		if (!_highlighter.RefreshEnabled)
			_highlighter.ToggleHighlightRefresh();
	}

	/// <summary>
	/// Highlight current patrol route
	/// </summary>
	private void HighlightPatrolRoute() => _highlighter.HighlightPath(LevelPatrolRoute.CompletePath);

	// TODO: have dummy object go through route at high speed?
	private void RunThroughPatrol()
	{

	}

	public void CalculatePatrolRoute(Collectible[] collectibles)
	{
		if (collectibles.Length <= 0)
		{
			// TODO: calculate exit patrol route
			MessageLogger.LogDebugMessage(LogType.Game, "All collectibles taken, defend the exit!");
			return;
		}

		// create array for patrol waypoints
		var totalNumWaypoints = 0;
		Array.ForEach(collectibles, coll => totalNumWaypoints += coll.NavpointPositions.Length);
		var patrolWaypoints = new Vector3Int[totalNumWaypoints];

		// populate array with cells of navpoints
		var copyIdx = 0;
		for (var i = 0; i < collectibles.Length; i++)
		{
			var navpointCells = _tilemapManager.CellsofPositions(collectibles[i].NavpointPositions);
			navpointCells.CopyTo(patrolWaypoints, copyIdx);
			copyIdx += navpointCells.Length;
		}

		// ensure that all waypoints are graph nodes
		var invalidWaypoints = CheckInvalidNodes(patrolWaypoints);
		if (invalidWaypoints.Length > 0)
		{
			var invalidStr = "";
			Array.ForEach(invalidWaypoints, point => invalidStr += point + ", ");
			MessageLogger.LogErrorMessage(LogType.Path, "ERROR: trying to create patrol route from non-node points: " + invalidStr);
			LevelPatrolRoute = CyclicRoute.EmptyCycle;
			return;
		}

		var waypointStr = "";
		Array.ForEach(patrolWaypoints, waypoint => waypointStr += waypoint.ToString() + ", ");
		Debug.Log("Level has " + patrolWaypoints.Length + " waypoints: " + waypointStr);

		LevelPatrolRoute = _navMap.FindCycle(patrolWaypoints); // calculate cycle and assign as level's patrol route
	}

	private Vector3Int[] CheckInvalidNodes(Vector3Int[] points)
	{
		var invalidNodes = new List<Vector3Int>();
		foreach (var point in points)
			if (!_navMap.IsPathfindingNode(point))
				invalidNodes.Add(point);

		return invalidNodes.ToArray();
	}

	/// <summary>
	/// Recieve update on collectibles from Gameplay Manager
	/// </summary>
	/// <param name="status">Status class containing remaining collectibles and </param>
	public void OnNext(CollectibleStatus status)
	{
		MessageLogger.LogDebugMessage(LogType.Game, "Path Manager got {0} collectibles", status.RemainingCollectibles.Length);

		// (re)calculate level patrol route
		CalculatePatrolRoute(status.RemainingCollectibles);
		HighlightPatrolRoute();
	}

	public void OnError(Exception error) => throw error;

	public void OnCompleted()
	{
		// TODO: all apples taken?
	}
}
