using System;

public class FSMTransition
{
	/// <summary>
	/// State being transitioned into
	/// </summary>
	public FSMState ResultantState { get; private set; }

	/// <summary>
	/// Condition that causes the transition to be initiated
	/// </summary>
	public Func<bool> Condition { get; private set; }

	/// <summary>
	/// Construct transition with given resultant state and condition
	/// </summary>
	/// <param name="state">Resultant state</param>
	/// <param name="conditionFunction">Transition condition</param>
	public FSMTransition(FSMState state, Func<bool> conditionFunction)
	{
		ResultantState = state;
		Condition = conditionFunction;
	}
}
