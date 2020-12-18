using System;
using UnityEngine;

public class Collectible : MonoBehaviour
{
	// markers constituting patrol route points
	[SerializeField] private Navpoint[] _patrolNavpoints;

	public Vector3[] NavpointPositions
	{
		get
		{
			var positions = Array.ConvertAll(_patrolNavpoints, navpoint => navpoint.WorldPosition);

			/*
			Vector3[] positions = new Vector3[_patrolNavpoints.Length];

			for (var i = 0; i < positions.Length; i++)
				positions[i] = _patrolNavpoints[i].transform.position;
			/**/

			return positions;
		}
	}

	private void Awake()
	{
		tag = "Collectible";
	}

	private void Start()
	{
		_patrolNavpoints = GetComponentsInChildren<Navpoint>();
	}
}
