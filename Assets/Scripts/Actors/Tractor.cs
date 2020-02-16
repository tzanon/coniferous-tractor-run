using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Directions;
using Pathfinding;

public class Tractor : Actor
{
	[SerializeField] private bool debugInfo = false;
	[SerializeField] private RectTransform _debugDisplay; // TODO: display destination/status above tractor
	private LineRenderer _lr; // debug: for pointing to dest?

	private bool _calculatingPath; // if calculating, idle
	private Vector3Int[] _currentPath;
	private Vector3 _destPoint; // world space!

	private const float _defaultSpeed = 4f;
	private const float _chaseSpeed = 8f;

	public float CurrentSpeed { get; private set; }

	protected override void Awake()
	{
		base.Awake();

		// TODO: set animation state names
	}

	private void FixedUpdate()
	{
		// TODO: path following and handling
	}

	

}
