using System;
using UnityEngine;
using Pathfinding;

public class LevelPathManager : MonoBehaviour, IObserver<CollectibleStatus>
{
	// TODO: get rid of and replace with reference to Gameplay Manager
	private Collectible[] _collectibles;

	[SerializeField] private Navpoint[] _testNavMarkers;

	[SerializeField] GameplayManager _gameplayManager;

	private TilemapManager _tilemapManager;
	private NavigationMap _navMap;
	private TilemapHighlighter _highlighter;

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

	public void CalculatePatrolRoute(Collectible[] collectibles)
	{

	}

	public CyclicRoute GetLevelPatrolRoute()
	{
		// TODO: for each apple, get waypoints
		var totalWaypoints = 0;
		var waypointGroups = new Vector3Int[_collectibles.Length][];

		for (var i = 0; i < waypointGroups.Length; i++)
		{
			var patrolPositions = _collectibles[i].NavpointPositions;
			var waypointGroup = new Vector3Int[patrolPositions.Length];
			totalWaypoints += patrolPositions.Length;

			// convert Vector3 positions to Vector3Int cells
			for (var j = 0; j < patrolPositions.Length; j++)
			{
				waypointGroup[j] = _tilemapManager.CellOfPosition(patrolPositions[j]);
			}

			if (!ValidatePoints(waypointGroup))
			{
				MessageLogger.LogErrorMessage(LogType.Path, "ERROR: Some patrol waypoints are not pathfinding nodes!");
				return CyclicRoute.EmptyCycle;
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

	public void OnNext(CollectibleStatus status)
	{
		MessageLogger.LogDebugMessage(LogType.Game, "Path Manager got {0} collectibles", status.RemainingCollectibles.Length);

		// TODO: (re)calculate level patrol route
		//CalculatePatrolRoute(status.RemainingCollectibles);

		HighlightCollectibleRoutes(status.RemainingCollectibles);
	}

	public void OnError(Exception error) => throw error;

	public void OnCompleted()
	{
		// TODO: all apples taken?
	}
}
