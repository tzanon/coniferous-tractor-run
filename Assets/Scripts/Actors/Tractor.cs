﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Directions;
using Pathfinding;

public class Tractor : Actor
{
	[SerializeField] private RectTransform _debugDisplay; // TODO: display destination/status above tractor
	private LineRenderer _lr; // debug: for pointing to dest?

	private bool _calculatingPath; // if calculating, idle

	private const float _defaultSpeed = 4.0f;
	private const float _chaseSpeed = 8.0f;

	protected override void Awake()
	{
		base.Awake();

		CurrentSpeed = _defaultSpeed;
		// TODO: set animation state names
	}

	protected override void AssignAnimationStateNames()
	{
		_idleAnimFwd = "Tractor_idleForward";
		_idleAnimBack = "Tractor_idleBackward";
		_idleAnimRight = "Tractor_idleRight";
		_idleAnimLeft = "Tractor_idleLeft";

		_moveAnimFwd = "Tractor_moveForward";
		_moveAnimBack = "Tractor_moveBackward";
		_moveAnimRight = "Tractor_moveRight";
		_moveAnimLeft = "Tractor_moveLeft";
	}

	protected override void SetUpStateMachine()
	{
		base.SetUpStateMachine();

		// create states
		var errorState = new Error(this);
		var idleState = new Idle(this);
		var findPatrolState = new AutoFindPatrol(this, _tilemapManager, _highlighter, _navMap, _pathManager);
		var patrolState = new AutoPatrol(this, _tilemapManager, _highlighter, _navMap, _pathManager);

		// create trigger functions
		bool tractorStuck() => Stuck;
		bool reachedPatrol() => findPatrolState.ActorReachedDestination;

		// create transitions
		var anyToError = new FSMTransition(errorState, tractorStuck);
		var searchToPatrol = new FSMTransition(patrolState, reachedPatrol);

		// add everything and set starting state

		_stateMachine.AddState(idleState);
		_stateMachine.AddState(errorState);
		_stateMachine.AddUniversalTransition(anyToError);

		_stateMachine.AddState(patrolState);
		_stateMachine.AddState(findPatrolState, searchToPatrol);

		_stateMachine.CurrentState = findPatrolState;
	}

	private void FixedUpdate()
	{
		// TODO: path following and handling (or not...?)
		_stateMachine?.Run();
	}
}
