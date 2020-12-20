using UnityEngine;
using Pathfinding;

public class AutoPatrol : AutoControl
{

	private CyclicRoute _patrolRoute;

	public AutoPatrol(Actor actor, TilemapManager tm, TilemapHighlighter th, NavigationMap nm, LevelPathManager lpm) : base(actor, tm, th, nm, lpm) { }

	protected override void CalculatePath()
	{
		// get patrol route
		_patrolRoute = _pathManager.LevelPatrolRoute;
		_currentPath = _patrolRoute.CompletePath;
	}

	protected override void InitializeData()
	{
		base.InitializeData();

		//_highlighter.HighlightPath(_currentPath);

		var actorCell = _tilemapManager.CellOfPosition(_actor.Position);
		_pathIdx = _currentPath.IndexOf(actorCell);
	}

	protected override void ClearData()
	{
		base.ClearData();
		_patrolRoute = null;
	}

	protected override void NoPathAction()
	{
		MessageLogger.LogErrorMessage(LogType.Path, "ERROR: actor {0} is not on patrol route!", _actor.name);
		_actor.Stuck = true;
	}

	protected override void PathEndAction()
	{
		// when at last index, go back to zero
		_pathIdx = 0;
	}

	public override void PerformAction()
	{
		base.PerformAction();
		// nothing while testing (highlight on entry)
		return;
	}
}
