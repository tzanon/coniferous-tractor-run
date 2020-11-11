using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyState : FSMState
{
	private string _name;

	public DummyState(string n)
	{
		_name = n;
	}

	public override void PerformAction()
	{
		Debug.Log("At state " + _name);
	}

	public override void OnEnter() { }

	public override void OnExit() { }
}
