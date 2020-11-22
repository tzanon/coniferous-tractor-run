using UnityEngine;
using Pathfinding;

public class PlayerAutoControl : FSMState
{
	/* fields */

	private Player _player;
	private LevelCompletionChecker _completionChecker;
	private Vector3Int _destNode;
	private Path _path;
	private int _nextPathPointIndex;

	private const float _distanceThreshold = 0.1f;

	private bool _pathFound;

	// components
	private TilemapManager _tilemapManager;
	private TilemapHighlighter _highlighter;
	private NavigationMap _navMap;

	/* properties */

	/// <summary>
	/// Location the player is to be moved to
	/// </summary>
	public Vector3Int DestinationNode { get => _destNode; }

	/// <summary>
	/// Whether the player is at its destined coordinates.
	/// Condition used by the FSM to transition back to the input-based movement state.
	/// </summary>
	public bool PlayerReachedDest
	{
		get => PlayerReachedPoint(_tilemapManager.CenterPositionOfCell(_destNode));
	}

	/// <summary>
	/// If path or destination exists
	/// </summary>
	private bool PathDefined
	{
		get => ( _path.Length > 0 && _nextPathPointIndex >= 0 && _destNode != TilemapManager.UndefinedCell);
	}

	/// <summary>
	/// Whether index is within the path's boundaries
	/// </summary>
	private bool PathIndexInRange { get => _nextPathPointIndex >= 0 && _nextPathPointIndex < _path.Length; }

	/* methods */

	/// <summary>
	/// Constructs an auto control state
	/// </summary>
	/// <param name="player">Player to control</param>
	/// <param name="mc">LevelCompletionChecker containing destination nodes to move the player to</param>
	/// <param name="tm">Tilemap Manager</param>
	/// <param name="th">Highlighting tiles for debugging</param>
	/// <param name="nm">Nav map for pathfinding</param>
	public PlayerAutoControl(Player player, LevelCompletionChecker lcc, TilemapManager tm, TilemapHighlighter th, NavigationMap nm)
	{
		_player = player;
		_completionChecker = lcc;

		ResetMovementData();

		_tilemapManager = tm;
		_highlighter = th;
		_navMap = nm;
	}

	// data setting

	/// <summary>
	/// Sets new destination and calculates path to it
	/// </summary>
	/// <param name="dest">Location for player to move to</param>
	public void SetMovementData(Vector3Int dest)
	{
		_destNode = dest;
		_pathFound = FindPathToDest();
	}

	/// <summary>
	/// Resets destination and path to undefined state
	/// </summary>
	private void ResetMovementData()
	{
		_destNode = TilemapManager.UndefinedCell;
		_path = Path.EmptyPath;
		_nextPathPointIndex = -1;
	}

	// player moving

	/// <summary>
	/// Move player along path
	/// </summary>
	public override void PerformAction()
	{
		if (PlayerReachedDest)
			return;

		// if a path to the dest couldn't be found, just teleport the player to it
		if (!_pathFound)
		{
			MessageLogger.LogWarningMessage(LogType.Path, "Couldn't find path to dest, teleporting");
			TeleportMove();
			return;
		}

		// don't move if path is undefined or at its end
		if (!PathDefined || _nextPathPointIndex >= _path.Length)
			return;

		var nextPosition = _tilemapManager.CenterPositionOfCell(_path[_nextPathPointIndex]);

		// if player has reached the point, increment index to point to next point
		if (PlayerReachedPoint(nextPosition))
		{
			_player.Position = nextPosition; // correct threshold innaccuracy
			if (!IncrementPathIndex()) // stop movement if at path's end
				return;
		}

		// move player in direction of next point if it hasn't been reached
		var pointDifference = nextPosition - _player.Position;
		if (pointDifference != Vector3.zero)
			_player.MoveActor(pointDifference);
	}

	// helpers

	/// <summary>
	/// "Teleports" player to destination (use if no path can be found)
	/// </summary>
	private void TeleportMove() => _player.Position = _tilemapManager.CenterPositionOfCell(_destNode);

	/// <summary>
	/// Checks if player's position is the same as the given point
	/// </summary>
	/// <returns>True if player at point, false if not</returns>
	private bool PlayerReachedPoint(Vector3 point)
	{
		Vector3 distance = point - _player.Position;

		if (distance.sqrMagnitude < Mathf.Pow(_distanceThreshold, 2f))
		{
			MessageLogger.LogVerboseMessage(LogType.Actor, "Player reached node with {0} units remaining", distance.magnitude);
			return true;
		}
		else
			return false;
	}

	/// <summary>
	/// Increments index to point to the next node in the path
	/// </summary>
	/// <returns>True if index incremented, false if at end of path</returns>
	private bool IncrementPathIndex()
	{
		if (_nextPathPointIndex >= _path.Length)
		{
			MessageLogger.LogDebugMessage(LogType.FSM, "Player has reached destination");
			return false;
		}

		_nextPathPointIndex++;
		return true;
	}

	/// <summary>
	/// Calculate path from player's location to destination
	/// </summary>
	/// <returns>True if path found, false if not</returns>
	private bool FindPathToDest()
	{
		// must have a valid destination
		if (_destNode == TilemapManager.UndefinedCell)
		{
			MessageLogger.LogErrorMessage(LogType.FSM, "Error: cannot calculate path to nonexistant node");
			return false;
		}

		Vector3Int playerCell = _tilemapManager.CellOfPosition(_player.Position);
		Vector3Int closestNodeToPlayer = _navMap.ClosestNodeToCell(playerCell);

		_path = _navMap.FindPathBetweenNodes(closestNodeToPlayer, _destNode);

		if (_path.Empty)
		{
			MessageLogger.LogErrorMessage(LogType.FSM, "Error: could not find path to Player's destination");
			return false;
		}
		else
		{
			_nextPathPointIndex = 0;
			return true;
		}
	}

	// enter/exit

	/// <summary>
	/// Disable manual input
	/// </summary>
	public override void OnEnter()
	{
		MessageLogger.LogVerboseMessage(LogType.FSM, "Entered player auto control state");

		_player.InputBlocked = true;
		var movementDestination = _completionChecker.CurrentDestNode;
		SetMovementData(movementDestination);
	}

	/// <summary>
	/// Revert movement data to undefined state
	/// </summary>
	public override void OnExit()
	{
		MessageLogger.LogVerboseMessage(LogType.FSM, "Exiting player auto control state");
		ResetMovementData();
	}
}
