using UnityEngine;
using Pathfinding;

public sealed class PlayerAutoMovement : AutoControl
{
	/* fields */

	private Player _player;
	private LevelCompletionChecker _completionChecker;
	private Vector3Int _destNode;

	/* properties */

	public bool PlayerReachedDest
	{
		get => ActorAtPoint(_tilemapManager.CenterPositionOfCell(_destNode));
	}

	/* methods */

	public PlayerAutoMovement(Player player, TilemapManager tm, NavigationMap nm, LevelCompletionChecker lcc) : base(player, tm, nm)
	{
		_completionChecker = lcc;
		_player = (Player)_actor;

		ClearData();
	}

	private void Teleport() => _player.Position = _tilemapManager.CenterPositionOfCell(_destNode);

	protected override void CalculatePath()
	{
		var playerCell = _tilemapManager.CellOfPosition(_player.Position);
		var closestNodeToPlayer = _navMap.ClosestNodeToCell(playerCell);

		_destNode = _completionChecker.CurrentDestNode;
		FindPath(closestNodeToPlayer, _destNode);
	}

	protected override void ClearData()
	{
		base.ClearData();
		_destNode = TilemapManager.UndefinedCell;
	}

	protected override void NoPathAction()
	{
		// teleport to location
		Teleport();
		MessageLogger.LogWarningMessage(LogType.Path, "Couldn't find path for {0}, teleporting", _player.name);
	}

	protected override void PathEndAction()
	{
		// nothing: FSM detects when player is at destination
	}

	public override void OnEnter()
	{
		_player.InputBlocked = true;
		base.OnEnter();
	}
}
