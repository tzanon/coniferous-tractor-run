using System;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
	// markers constituting patrol route points
	//[SerializeField]
	private Navpoint[] _patrolNavpoints;

	public Vector3[] NavpointPositions => Array.ConvertAll(_patrolNavpoints, navpoint => navpoint.WorldPosition);

	private void Awake()
	{
		tag = "Collectible";
		_patrolNavpoints = GetComponentsInChildren<Navpoint>();
		Array.Sort(_patrolNavpoints, new NavpointNameComparer());
	}

	private void Start()
	{
		
		var pointStr = NamesOfNavpoints();
		//MessageLogger.LogDebugMessage(LogType.Game, "Navpoints after sorting for {0} are {1}", this.name, pointStr);
	}

	private string NamesOfNavpoints()
	{
		var navList = new List<Navpoint>(_patrolNavpoints);
		var pointStr = "";
		navList.ForEach(point => pointStr += (point.name + ", "));
		return pointStr;
	}

}
