using UnityEngine;
using Pathfinding;

public abstract class AutoPatrol : AutoControl
{

	private CyclicRoute _patrolRoute;

	public AutoPatrol(Actor actor, TilemapManager tm, TilemapHighlighter th, NavigationMap nm, LevelPathManager lpm) : base(actor, tm, th, nm, lpm)
	{

	}

	protected abstract void GetPatrolRoute();

	protected override void InitializeData()
	{
		// get patrol route
		_patrolRoute = _pathManager.LevelPatrolRoute;
		_currentPath = _patrolRoute.CompletePath;

		_highlighter.HighlightPath(_currentPath);

		// check if actor is currently on a point in the route
		var actorCell = _tilemapManager.CellOfPosition(_actor.Position);
		if (!_currentPath.Contains(actorCell))
		{
			MessageLogger.LogErrorMessage(LogType.Path, "ERROR: actor {0} is not currently on patrol route!", _actor.name);
			_pathIdx = -1;
			return;
		}

		_pathIdx = _patrolRoute.PathIndexOfClosestPoint(actorCell);
	}

	protected override void ClearData()
	{
		base.ClearData();
		_patrolRoute = null;
	}

	protected override void NoPathAction()
	{
		// TODO
	}

	protected override void PathEndAction()
	{
		// when at last index, go back to zero
		_pathIdx = 0;
	}

	public override void PerformAction()
	{
		//base.PerformAction();
		// nothing while testing (highlight on entry)
		return;
	}

	public override void OnEnter()
	{
		base.OnEnter();
	}

}
