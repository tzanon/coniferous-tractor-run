using UnityEngine;

public sealed class PlayerAutoControl : AutoControl
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

	public PlayerAutoControl(Player player, TilemapManager tm, TilemapHighlighter th, NavigationMap nm, LevelPathManager lpm, LevelCompletionChecker lcc) :
		base(player, tm, th, nm, lpm)
	{
		_completionChecker = lcc;
		_player = (Player)_actor;
		_destNode = TilemapManager.UndefinedCell;
	}

	private void Teleport() => _player.Position = _tilemapManager.CenterPositionOfCell(_destNode);

	protected override void InitializeData()
	{
		var playerCell = _tilemapManager.CellOfPosition(_player.Position);
		var closestNodeToPlayer = _navMap.ClosestNodeToCell(playerCell);

		_destNode = _completionChecker.CurrentDestNode;
		FindPath(closestNodeToPlayer, _destNode);

		if (!_currentPath.Empty)
		{
			_pathIdx = 0;
		}
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
