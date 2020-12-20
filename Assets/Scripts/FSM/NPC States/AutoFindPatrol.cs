using UnityEngine;
using Pathfinding;

public class AutoFindPatrol : AutoMoveToPoint
{
	CyclicRoute _patrolRoute;

	public AutoFindPatrol(Actor actor, TilemapManager tm, TilemapHighlighter th, NavigationMap nm, LevelPathManager lpm) : base(actor, tm, th, nm, lpm) { }

	protected override void CalculatePath()
	{
		base.CalculatePath();

		//_highlighter.HighlightPath(_currentPath);
	}

	protected override Vector3Int CalculateDestination()
	{
		// calculate closest point on patrol route
		_patrolRoute = _pathManager.LevelPatrolRoute;

		var actorCell = _tilemapManager.CellOfPosition(_actor.Position);
		var closestPatrolCell = _patrolRoute.ClosestPathPoint(actorCell);

		return closestPatrolCell;
	}

	protected override void NoPathAction()
	{
		MessageLogger.LogWarningMessage(LogType.Path, "Warning: actor {0} cannot find patrol route!", _actor.name);
		_actor.Stuck = true;
	}

	public override void PerformAction()
	{
		base.PerformAction();
	}

}
