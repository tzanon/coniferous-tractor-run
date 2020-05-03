using System.Collections.Generic;

public abstract class FSMState
{
	protected List<FSMTransition> _transitions;

	public FSMTransition[] Transitions { get => _transitions.ToArray(); }

	/// <summary>
	/// An empty state has no transitions
	/// </summary>
	public bool Empty { get => _transitions.Count <= 0; }

	/// <summary>
	/// Create state with transitions
	/// </summary>
	/// <param name="transitions">Starting transitions</param>
	public FSMState(params FSMTransition[] transitions)
	{
		if (transitions.Length <= 0)
			_transitions = new List<FSMTransition>();
		else
			_transitions = new List<FSMTransition>(transitions);
	}

	/// <summary>
	/// Check if a condition for a transition has been met
	/// </summary>
	/// <returns>Transition whose condition is met</returns>
	public FSMTransition GetTransitionIfReady()
	{
		foreach (FSMTransition transition in _transitions)
		{
			if (transition.Condition())
				return transition;
		}

		return null;
	}

	/// <summary>
	/// Adds a transition to this state
	/// </summary>
	/// <param name="transition">Transition to add</param>
	public void AddTransition(FSMTransition transition)
	{
		_transitions.Add(transition);
	}

	public abstract void PerformAction();
	public abstract void OnEnter();
	public abstract void OnExit();
}
