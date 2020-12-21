public class PlayerIdle : Idle
{
	protected Player _player;

	public PlayerIdle(Actor actor) : base(actor)
	{
		_player = (Player)actor;
	}

	public override void OnEnter()
	{
		_player.InputBlocked = true;
	}

	public override void OnExit()
	{
		_player.InputBlocked = false;
	}
}
