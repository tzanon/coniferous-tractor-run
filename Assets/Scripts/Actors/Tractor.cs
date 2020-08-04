﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Directions;
using Pathfinding;

public class Tractor : Actor
{
	[SerializeField] private RectTransform _debugDisplay; // TODO: display destination/status above tractor
	private LineRenderer _lr; // debug: for pointing to dest?

	private bool _calculatingPath; // if calculating, idle
	private Vector3Int[] _currentPath;
	private Vector3 _destPoint; // world space!

	private const float _defaultSpeed = 4f;
	private const float _chaseSpeed = 8f;

	protected override void Awake()
	{
		base.Awake();

		CurrentSpeed = _defaultSpeed;
		// TODO: set animation state names
	}

	protected override void AssignAnimationStateNames()
	{
		
	}

	protected override void SetUpStateMachine()
	{

	}

	private void FixedUpdate()
	{
		// TODO: path following and handling (or not...?)
	}

}
