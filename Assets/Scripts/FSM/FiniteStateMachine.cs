﻿using System;
using System.Collections.Generic;

/// <summary>
/// General FSM
/// </summary>
public class FiniteStateMachine
{
	private readonly Dictionary<FSMState, List<FSMTransition>> _transitions = new Dictionary<FSMState, List<FSMTransition>>();
	private List<FSMTransition> _currentTransitions = new List<FSMTransition>();
	private readonly List<FSMTransition> _anyTransitions = new List<FSMTransition>();
	private readonly static List<FSMTransition> EmptyTransitions = new List<FSMTransition>(0);

	private FSMState _currentState;

	/* properties */

	public FSMState CurrentState
	{
		get { return _currentState; }
		set
		{
			if (!_transitions.ContainsKey(value))
			{
				MessageLogger.LogErrorMessage(LogType.FSM, "ERROR: Attempting to set unavailable state {0} on FSM {1}", value.ToString(), this.ToString());
				return;
			}

			if (_currentState != null)
			{
				_currentState.OnExit();
				MessageLogger.LogDebugMessage(LogType.FSM, "FSM {0} Transitioning to state {1} from state {2}",
					this.ToString(), value.ToString(), CurrentState.ToString());
			}
			
			_currentState = value;
			_currentTransitions = _transitions[_currentState];

			_currentState.OnEnter();
		}
	}

	/* methods */

	/// <summary>
	/// Adds a state and its transitions
	/// </summary>
	/// <param name="state">State to add</param>
	/// <param name="transitions">State's transitions</param>
	/// <returns>True if state added, false if already in</returns>
	public bool AddState(FSMState state, params FSMTransition[] transitions)
	{
		if (_transitions.ContainsKey(state))
		{
			return false;
		}

		List<FSMTransition> transitionList = new List<FSMTransition>(transitions);
		_transitions.Add(state, transitionList);
		return true;
	}

	/// <summary>
	/// Add a transition that can be triggered from any state
	/// </summary>
	/// <param name="transition">Transition to another state</param>
	public void AddUniversalTransition(FSMTransition transition)
	{
		if (!_anyTransitions.Contains(transition))
		{
			_anyTransitions.Add(transition);
		}
	}

	/// <summary>
	/// Add a transition to a given state
	/// </summary>
	/// <param name="from">State being given the transition</param>
	/// <param name="transition">Transition to the other state</param>
	public void AddTransition(FSMState from, FSMTransition transition)
	{
		if (_transitions.ContainsKey(from))
		{
			_transitions[from].Add(transition);
		}
		else
		{
			List<FSMTransition> transitionList = new List<FSMTransition>() { transition };
			_transitions.Add(from, transitionList);
		}
	}

	/// <summary>
	/// Add a transition to a given state
	/// </summary>
	/// <param name="from">State being given the transition</param>
	/// <param name="condition">Condition for the transition to be activated</param>
	/// <param name="to">State that results from the transition</param>
	public void AddTransition(FSMState from, Func<bool> condition, FSMState to)
	{
		FSMTransition transition = new FSMTransition(to, condition);
		this.AddTransition(from, transition);
	}

	/// <summary>
	/// Run a cycle of the FSM
	/// </summary>
	public void Run()
	{
		FSMTransition transition = TryGetTransition();
		if (transition != null)
		{
			if (CurrentState != transition.ResultantState)
				CurrentState = transition.ResultantState;
			MessageLogger.LogVerboseMessage(LogType.FSM, "FSM {0} transitioning to state {1}", this.ToString(), CurrentState.ToString());
		}

		CurrentState?.PerformAction();
	}

	/// <summary>
	/// Checks if a transition is ready to be executed and returns it
	/// </summary>
	/// <returns>Transition if ready, null if none ready</returns>
	private FSMTransition TryGetTransition()
	{
		foreach (FSMTransition transition in _anyTransitions)
		{
			if (transition.Condition())
				return transition;
		}

		foreach (FSMTransition transition in _currentTransitions)
		{
			if (transition.Condition())
				return transition;
		}

		return null;
	}

}
