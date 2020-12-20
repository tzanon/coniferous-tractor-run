public class Error : Idle
{
	public Error(Actor actor) : base(actor) { }

	public override void OnEnter()
	{
		MessageLogger.LogWarningMessage(LogType.Actor, "Warning: {0} has entered an error state!", _actor.name);
	}
}
