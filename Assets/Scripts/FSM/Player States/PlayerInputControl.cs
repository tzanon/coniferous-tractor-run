
public class PlayerInputControl : FSMState
{
	private Player _player;

	public PlayerInputControl(Player player, params FSMTransition[] transitions)
		: base(transitions)
	{
		_player = player;
	}

	/// <summary>
	/// Do nothing; Player (of the game) controls movement
	/// </summary>
	public override void PerformAction() { }

	/// <summary>
	/// Enable input for movement
	/// </summary>
	public override void OnEnter()
	{
		_player.InputBlocked = false;
	}

	public override void OnExit() { }
}
