using UnityEngine;

public abstract class AutoMoveToPoint : AutoControl
{
	protected Vector3Int _destNode;

	/* properties */

	public bool ActorReachedDestination
	{
		get => ActorAtPoint(_tilemapManager.CenterPositionOfCell(_destNode));
	}

	public AutoMoveToPoint(Actor actor, TilemapManager tm, TilePainter tp, NavigationMap nm, LevelPathManager lpm) :
		base(actor, tm, tp, nm, lpm)
	{
		_destNode = TilemapManager.UndefinedCell;
	}

	protected override void CalculatePath()
	{
		var actorCell = _tilemapManager.CellOfPosition(_actor.Position);
		var closestNode = _navMap.ClosestNodeToCell(actorCell);
		_destNode = CalculateDestination();
		_currentPath = _navMap.FindPathBetweenNodes(closestNode, _destNode);
	}

	protected abstract Vector3Int CalculateDestination();

	protected override void InitializeData()
	{
		base.InitializeData();

		if (!_currentPath.Empty)
		{
			_pathIdx = 0;
		}
	}

	protected override void PathEndAction()
	{
		// Nothing: ActorReachedDestination is used to trigger exiting of state
	}

	protected override void ClearData()
	{
		base.ClearData();
		_destNode = TilemapManager.UndefinedCell;
	}
}
