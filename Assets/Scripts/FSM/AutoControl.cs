using UnityEngine;
using Pathfinding;

public abstract class AutoControl : FSMState
{
	/* fields */

	protected Path _currentPath;
	protected int _pathIdx;

	private const float _distanceThreshold = 0.1f;

	// components
	protected readonly Actor _actor;
	protected readonly TilemapManager _tilemapManager;
	protected readonly TilemapHighlighter _highlighter;
	protected readonly NavigationMap _navMap;
	protected readonly LevelPathManager _pathManager;

	/* properties */

	private bool PathDefined { get => _currentPath != null && !_currentPath.Empty; }

	private bool PathIndexInRange { get => _pathIdx >= 0 && _pathIdx < _currentPath.Length; }

	/* methods */

	public AutoControl(Actor actor, TilemapManager tm, TilemapHighlighter th, NavigationMap nm, LevelPathManager lpm)
	{
		_actor = actor;
		_tilemapManager = tm;
		_highlighter = th;
		_navMap = nm;
		_pathManager = lpm;

		ClearData();
	}

	/*
	protected virtual void FindPath(Vector3Int start, Vector3Int end)
	{
		_currentPath = _navMap.FindPathBetweenNodes(start, end);
	}
	/**/

	private bool IncrementPathIndex() => ++_pathIdx < _currentPath.Length;

	protected bool ActorAtPoint(Vector3 point)
	{
		var distance = point - _actor.Position;
		return distance.sqrMagnitude < Mathf.Pow(_distanceThreshold, 2f);
	}

	protected void Teleport(Vector3Int cell) => _actor.Position = _tilemapManager.CenterPositionOfCell(cell);

	protected abstract void CalculatePath();

	protected virtual void InitializeData()
	{
		CalculatePath();
	}

	protected virtual void ClearData()
	{
		_currentPath = Path.EmptyPath;
		_pathIdx = -1;
	}

	protected abstract void NoPathAction();

	protected abstract void PathEndAction();

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
