using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Directions;
using Pathfinding;

public class Tractor : Actor
{
	public bool debugInfo = false;
	public RectTransform debugDisplay; // TODO: display destination/status above tractor
	private LineRenderer lr; // debug: for pointing to dest?

	private bool _calculatingPath; // if calculating, idle
	private Vector3Int[] currentPath;
	private Vector3 destPoint; // world space!

	private float _currentSpeed;
	[SerializeField] private const float _defaultSpeed = 4f;
	[SerializeField] private const float _chaseSpeed = 8f;

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
