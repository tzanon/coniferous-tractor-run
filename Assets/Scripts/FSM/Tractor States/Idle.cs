public class Idle : FSMState
{
	private Actor _actor;

	public Idle(Actor actor) => _actor = actor;

	public override void PerformAction() { }

	public override void OnEnter()
	{
		MessageLogger.LogWarningMessage(LogType.Actor, "Warning: {0} has entered an idle state!", _actor.name);
	}

	public override void OnExit() { }
}
