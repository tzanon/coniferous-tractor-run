using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoInvestigate : AutoControl
{
	private Tractor _tractor;

	public bool ActorAtEnd
	{
		get => ActorAtPoint(_tilemapManager.CenterPositionOfCell(_currentPath[_currentPath.Length - 1]));
	}

	public AutoInvestigate(Actor actor, TilemapManager tm, TilePainter tp, NavigationMap nm, LevelPathManager lpm) : base(actor, tm, tp, nm, lpm)
	{
		_tractor = (Tractor)actor;
	}

	protected override void CalculatePath()
	{
		// TODO: make route out of actor's position and apple navpoints,
		// set current path to route's path
		var navpointPositions = _tractor.LastCollectibleTakenStatus.LastTakenNavpointPositions;
		var points = new Vector3[navpointPositions.Length + 1];
		points[0] = _tractor.Position;
		navpointPositions.CopyTo(points, 1);

		var cells = _tilemapManager.CellsofPositions(points);
		var investigateRoute = _navMap.FindRoute(cells);
		_currentPath = investigateRoute.CompletePath;
	}

	protected override void InitializeData()
	{
		base.InitializeData();

		if (!_currentPath.Empty)
			_pathIdx = 0;
	}

	protected override void NoPathAction()
	{
		MessageLogger.LogWarningMessage(LogType.Path, "Warning: actor {0} cannot find investigate route!", _actor.name);
	}

	protected override void PathEndAction() { }

	public override void OnExit()
	{
		_tractor.DoneInvestigating();
	}
}
