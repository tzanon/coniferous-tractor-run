using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class LevelPathManager : MonoBehaviour, IObserver<CollectibleStatus>
{
	/* fields */
	[SerializeField] private GameObject _exitPatrolPointGroup;

	// components
	[SerializeField] GameplayManager _gameplayManager;
	private TilemapManager _tilemapManager;
	private NavigationMap _navMap;
	private TilePainter _mapPainter;

	/* properties */

	public CyclicRoute LevelPatrolRoute { get; private set; }

	/* methods */

	private void Awake()
	{
		_tilemapManager = GetComponent<TilemapManager>();
		_navMap = GetComponent<NavigationMap>();
	}

	private void Start()
	{
		_mapPainter = GetComponent<HighlightManager>().CreateTilePainter(Color.cyan);
		_gameplayManager.Subscribe(this);
	}

	//debug
	private void HighlightCollectibleRoutes(Collectible[] collectibles, bool cyclic = true)
	{
		_mapPainter.RefreshEnabled = false;

		foreach (var collectible in collectibles)
		{
			var waypoints = _tilemapManager.CellsofPositions(collectible.NavpointPositions);
			var route = cyclic ? _navMap.FindCycle(waypoints) : _navMap.FindRoute(waypoints);

			//_highlighter.HighlightPath(route.CompletePath);
			_mapPainter.PaintPath(route.CompletePath);
		}

		_mapPainter.RefreshEnabled = true;
	}

	/// <summary>
	/// Highlight current patrol route
	/// </summary>
	private void HighlightPatrolRoute() => _mapPainter.PaintPath(LevelPatrolRoute.CompletePath);

	// TODO: have dummy object go through route at high speed?
	private void RunThroughPatrol()
	{

	}

	private void CalculateExitRoute()
	{
		var exitNavpoints = _exitPatrolPointGroup.GetComponentsInChildren<Navpoint>();
		Array.Sort(exitNavpoints, new NavpointNameComparer());

		var navpointPositions = Array.ConvertAll(exitNavpoints, navpoint => navpoint.WorldPosition);
		Vector3Int[] navpointCells = _tilemapManager.CellsofPositions(navpointPositions);

		LevelPatrolRoute = _navMap.FindCycle(navpointCells);
	}

	public void CalculatePatrolRoute(Collectible[] collectibles)
	{
		if (collectibles.Length <= 0)
		{
			// TODO: calculate exit patrol 
			CalculateExitRoute();

			MessageLogger.LogDebugMessage(LogType.Game, "All collectibles taken, get to the exit!");
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
		var invalidWaypoints = CalculateInvalidNodes(patrolWaypoints);
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

	private Vector3Int[] CalculateInvalidNodes(Vector3Int[] points)
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
	/// <param name="status">Status class containing collectible information</param>
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
