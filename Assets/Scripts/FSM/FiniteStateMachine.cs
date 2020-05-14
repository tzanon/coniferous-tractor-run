
/// <summary>
/// General FSM
/// </summary>
public class FiniteStateMachine
{
	/* properties */

	public FSMState CurrentState { get; private set; }

	/// <summary>
	/// Construct FSM with an initial state
	/// </summary>
	/// <param name="initialState">Starting state</param>
	public FiniteStateMachine(FSMState initialState)
	{
		CurrentState = initialState;
		CurrentState.OnEnter();
	}

	/* methods */

	/// <summary>
	/// Run a cycle of the FSM
	/// </summary>
	public void Run()
	{
		CheckTransitionConditions();
		CurrentState.PerformAction();
	}

	/// <summary>
	/// Check if a transition is available and execute if it is
	/// </summary>
	private void CheckTransitionConditions()
	{
		FSMTransition transition = CurrentState.GetTransitionIfReady();

		if (transition != null)
		{
			ExecuteTransition(transition);
		}
	}

	/// <summary>
	/// Transitions to another state
	/// </summary>
	/// <param name="transition"></param>
	private void ExecuteTransition(FSMTransition transition)
	{
		// don't transition if going to same state as current
		if (transition.ResultantState == CurrentState)
		{
			MessageLogger.LogVerboseMessage(LogType.FSM, "FSM {0} attempting to transition to same state", this.ToString());
			return;
		}

		MessageLogger.LogDebugMessage(LogType.FSM, "FSM {0} Transitioning to state {1} from state {2}", this.ToString(),
			transition.ResultantState.ToString(), CurrentState.ToString());

		CurrentState.OnExit();
		CurrentState = transition.ResultantState;
		CurrentState.OnEnter();
	}

}
