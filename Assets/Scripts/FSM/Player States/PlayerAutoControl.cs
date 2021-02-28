using UnityEngine;

public sealed class PlayerAutoControl : AutoMoveToPoint
{
	/* fields */

	private Player _player;
	private LevelCompletionChecker _completionChecker;

	/* methods */

	public PlayerAutoControl(Player player, TilemapManager tm, TilePainter tp, NavigationMap nm, LevelPathManager lpm, LevelCompletionChecker lcc) :
		base(player, tm, tp, nm, lpm)
	{
		_completionChecker = lcc;
		_player = (Player)_actor;
	}

	protected override Vector3Int CalculateDestination()
	{
		return _completionChecker.CurrentDestNode;
	}

	protected override void NoPathAction()
	{
		// teleport to location
		Teleport(_destNode);
		MessageLogger.LogWarningMessage(LogType.Path, "Couldn't find path for {0}, teleporting", _player.name);
	}

	public override void OnEnter()
	{
		_player.InputBlocked = true;
		base.OnEnter();
	}
}
