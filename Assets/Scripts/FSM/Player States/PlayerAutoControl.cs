using System.Collections;

using UnityEngine;
using Pathfinding;
using Directions;
using UnityEditor.VersionControl;

public class PlayerAutoControl : FSMState
{
	/* fields */

	private Player _player;
	private LevelCompletionChecker _movementController;
	private Vector3Int _destNode;
	private Vector3Int[] _path;
	private int _nextPathPointIndex;

	private const float _distanceThreshold = 0.1f;
	private float _moveFactor = 1.0f;

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

	/// <summary>
	/// Constructs an auto control state
	/// </summary>
	/// <param name="player">Player to control</param>
	/// <param name="mc">LevelCompletionChecker containing destination nodes to move the player to</param>
	/// <param name="tm">Tilemap Manager</param>
	/// <param name="th">Highlighting tiles for debugging</param>
	/// <param name="nm">Nav map for pathfinding</param>
	/// <param name="transitions">Transitions to other states</param>
	public PlayerAutoControl(Player player, LevelCompletionChecker mc, TilemapManager tm, TilemapHighlighter th, NavigationMap nm,
		params FSMTransition[] transitions) : base(transitions)
	{
		_player = player;
		_movementController = mc;

		ResetMovementData();

		_tilemapManager = tm;
		_highlighter = th;
		_navMap = nm;
	}

	/* methods */
	
	// data setting

	/// <summary>
	/// Sets new destination and calculates path to it
	/// </summary>
	/// <param name="dest">Location for player to move to</param>
	public void SetMovementData(Vector3Int dest)
	{
		_destNode = dest;
		_pathFound = FindPathToDest();

		// set player in direction of first path point
		if (_pathFound)
		{
			Vector3Int playerCell = _tilemapManager.CellOfPosition(_player.Position);
			Vector3Int pathStart = _path[0];
			//SetPlayerDirection(playerCell, pathStart);
		}
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
			_player.Position = nextPosition; // correcting threshold innaccuracy
			if (!IncrementPathIndex()) // stop movement if at path's end
				return;
		}

		// move player
		//Vector3 movedPosition =	Vector3.MoveTowards(_player.Position, nextPosition, _player.CurrentSpeed * _moveFactor * Time.deltaTime);
		//_player.Position = movedPosition;

		var pointDifference = nextPosition - _player.Position;
		_player.MoveActor(pointDifference);
	}

	// helpers

	/// <summary>
	/// "Teleports" player to destination (use if no path can be found)
	/// </summary>
	private void TeleportMove()
	{
		_player.Position = _tilemapManager.CenterPositionOfCell(_destNode);
	}

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

	// TODO: put direction change in another method (before refactoring?)
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
		//SetDirectionToNextNode();
		return true;
	}

	/// <summary>
	/// Orient player towards the next point in the path
	/// </summary>
	private void SetDirectionToNextNode()
	{
		if (!PathIndexInRange) return;

		int lastIndex = _nextPathPointIndex - 1;
		Vector3Int lastNode = _path[lastIndex];
		Vector3Int nextNode = _path[_nextPathPointIndex];
		//SetPlayerDirection(lastNode, nextNode);
	}

	/// <summary>
	/// Set player's moving animation to the direction between two points
	/// </summary>
	/// <param name="node1"></param>
	/// <param name="node2"></param>
	private void SetPlayerDirection(Vector3Int node1, Vector3Int node2)
	{
		CardinalDirection directionToNextPoint = CardinalDirection.DirectionBetweenPoints(node1, node2);
		//_player.SetMoveAnimInDirection(directionToNextPoint);
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

		if (Path.IsEmpty(_path))
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
		Vector3Int movementDestination = _movementController.CurrentDestNode;
		SetMovementData(movementDestination);
	}

	/// <summary>
	/// Revert movement data to undefined state
	/// </summary>
	public override void OnExit()
	{
		MessageLogger.LogVerboseMessage(LogType.FSM, "Exiting player auto control state");

		//_player.SetIdleAnimInDirection(_player.CurrentDirection);
		ResetMovementData();
	}
}
