
public class FiniteStateMachine
{
	private FSMState _currentState;

	public FiniteStateMachine(FSMState initialState)
	{
		_currentState = initialState;
	}

	/// <summary>
	/// Run a cycle of the FSM
	/// </summary>
	public void Run()
	{
		CheckTransitionConditions();
		_currentState.PerformAction();
	}

	/// <summary>
	/// Check if a transition is available and execute if it is
	/// </summary>
	private void CheckTransitionConditions()
	{
		FSMTransition transition = _currentState.GetTransitionIfReady();

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
		if (transition.ResultantState == _currentState)
		{
			MessageLogger.LogFSMMessage("FSM {0} attempting to transition to same state", LogLevel.Verbose, this.ToString());
			return;
		}

		MessageLogger.LogFSMMessage("FSM {0} Transitioning to state {1} from state {2}", LogLevel.Debug,
			this.ToString(), transition.ResultantState.ToString(), _currentState.ToString());

		_currentState.OnExit();
		_currentState = transition.ResultantState;
		_currentState.OnEnter();
	}

}
