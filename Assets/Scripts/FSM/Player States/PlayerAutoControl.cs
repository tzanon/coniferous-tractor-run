using System.Collections;

using UnityEngine;
using Pathfinding;
using Directions;

public class PlayerAutoControl : FSMState
{
	/* fields */

	private Player _player;
	private LevelCompletionChecker _movementController;
	private Vector3Int _destNode;
	private Vector3Int[] _path;
	private int _nextPathPointIndex;

	private float _distanceThreshold = 0.1f;

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

	// TODO: this will be the condition for transitioning out
	/// <summary>
	/// Whether the player is at its destined coordinates
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

		if (_pathFound)
		{
			// TODO: set direction to first path point
			Vector3Int playerCell = _tilemapManager.CellOfPosition(_player.Position);
			Vector3Int pathStart = _path[0];
			MovementVector directionToNextPoint = MovementVector.DirectionBetweenPoints(playerCell, pathStart);

			_player.SetMoveAnimInDirection(directionToNextPoint);
		}
	}

	/// <summary>
	/// Resets destination and path to undefined state
	/// </summary>
	private void ResetMovementData()
	{
		_destNode = TilemapManager.UndefinedCell;
		_path = new Vector3Int[0];
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
			MessageLogger.LogPathMessage("Couldn't find path to dest, teleporing", LogLevel.Warning);
			TeleportMove();
			return;
		}

		// don't move if path is undefined or at its end
		if (!PathDefined || _nextPathPointIndex >= _path.Length)
			return;

		
		Vector3 nextPosition = _tilemapManager.CenterPositionOfCell(_path[_nextPathPointIndex]);

		if (PlayerReachedPoint(nextPosition))
		{
			_player.Position = nextPosition; // correcting threshold innaccuracy
			if (!IncrementPathIndex()) // stop movement if at path's end
				return;
		}
		/**/

		Vector3 movedPosition =	Vector3.MoveTowards(_player.Position, nextPosition, _player.CurrentSpeed * 0.75f * Time.deltaTime);
		_player.Position = movedPosition;
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
		return (Vector3.SqrMagnitude(distance) < Mathf.Pow(_distanceThreshold, 2f));
	}

	/// <summary>
	/// Increments index to point to the next node in the path
	/// </summary>
	/// <returns>True if index incremented, false if at end of path</returns>
	private bool IncrementPathIndex()
	{
		if (_nextPathPointIndex >= _path.Length)
		{
			MessageLogger.LogFSMMessage("Player has reached destination", LogLevel.Debug);
			return false;
		}

		_nextPathPointIndex++;

		// TODO: put in own method
		int lastIndex = _nextPathPointIndex - 1;

		Vector3Int lastNode = _path[lastIndex];
		Vector3Int nextNode = _path[_nextPathPointIndex];
		MovementVector directionToNextPoint = MovementVector.DirectionBetweenPoints(lastNode, nextNode);

		_player.SetMoveAnimInDirection(directionToNextPoint);

		return true;
	}

	/// <summary>
	/// Calculate path from player's location to destination
	/// </summary>
	/// <returns>True if path found, false, if not</returns>
	private bool FindPathToDest()
	{
		// must have a valid destination
		if (_destNode == TilemapManager.UndefinedCell)
		{
			MessageLogger.LogFSMMessage("Error: cannot calculate path to nonexistant node", LogLevel.Error);
			return false;
		}

		Vector3Int playerCell = _tilemapManager.CellOfPosition(_player.Position);
		Vector3Int closestNodeToPlayer = _navMap.ClosestNodeToCell(playerCell);

		_path = _navMap.FindPathBetweenNodes(closestNodeToPlayer, _destNode);

		if (Path.IsEmpty(_path))
		{
			MessageLogger.LogFSMMessage("Error: could not find path to Player's destination", LogLevel.Error);
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
		MessageLogger.LogFSMMessage("Entered player auto control state", LogLevel.Verbose);
		//_player.InputBlocked = true;
		_player.DisableInput();

		Vector3Int movementDestination = _movementController.CurrentDestNode;
		SetMovementData(movementDestination);
	}

	/// <summary>
	/// Revert movement data to undefined state
	/// </summary>
	public override void OnExit()
	{
		MessageLogger.LogFSMMessage("Exiting player auto control state", LogLevel.Verbose);
		_player.SetIdleAnimInDirection(_player.CurrentDirection);
		ResetMovementData();
	}
}
