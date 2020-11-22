using UnityEngine;
using Pathfinding;

public abstract class AutoControl : FSMState
{
	/* fields */

	private Path _currentPath;
	private int _pathIdx;

	private const float _distanceThreshold = 0.1f;

	// components
	protected readonly Actor _actor;
	protected readonly TilemapManager _tilemapManager;
	protected readonly NavigationMap _navMap;

	/* properties */

	private bool PathDefined { get => _currentPath != null && !_currentPath.Empty; }

	private bool PathIndexInRange { get => _pathIdx >= 0 && _pathIdx < _currentPath.Length; }

	/* methods */

	public AutoControl(Actor actor, TilemapManager tm, NavigationMap nm)
	{
		_actor = actor;
		_tilemapManager = tm;
		_navMap = nm;
	}

	protected void FindPath(Vector3Int start, Vector3Int end)
	{
		_currentPath = _navMap.FindPathBetweenNodes(start, end);
	}

	protected abstract void CalculatePath();

	protected virtual void InitializeData()
	{
		CalculatePath();
		if (!_currentPath.Empty)
		{
			_pathIdx = 0;
		}
	}

	protected virtual void ClearData()
	{
		_currentPath = Path.EmptyPath;
		_pathIdx = -1;
	}

	protected abstract void NoPathAction();

	protected abstract void PathEndAction();

	private bool IncrementPathIndex() => ++_pathIdx < _currentPath.Length;

	protected bool ActorAtPoint(Vector3 point)
	{
		var distance = point - _actor.Position;
		return distance.sqrMagnitude < Mathf.Pow(_distanceThreshold, 2f);
	}

	public override void PerformAction()
	{
		// check if path is defined and that index is in range
		if (!(PathDefined && PathIndexInRange))
		{
			NoPathAction();
			return;
		}

		// check if actor is at the current path point
		var nextPoint = _tilemapManager.CenterPositionOfCell(_currentPath[_pathIdx]);
		if (ActorAtPoint(nextPoint))
		{
			_actor.Position = nextPoint; // correct inaccuracy
			if (!IncrementPathIndex()) // increment path index and check for end of path
				PathEndAction();
		}
		else
		{
			// move along path
			var pointDifference = nextPoint - _actor.Position;
			_actor.MoveActor(pointDifference);
		}
	}

	public override void OnEnter()
	{
		InitializeData();
	}

	public override void OnExit()
	{
		ClearData();
	}
}
