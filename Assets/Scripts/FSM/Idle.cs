public class Idle : FSMState
{
	protected Actor _actor;

	public Idle(Actor actor) => _actor = actor;

	public override void PerformAction() { }

	public override void OnEnter() { }

	public override void OnExit() { }
}
