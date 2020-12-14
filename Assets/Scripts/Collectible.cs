using UnityEngine;

public class Collectible : MonoBehaviour
{
	// markers constituting patrol route points
	[SerializeField] private GameObject[] _patrolNavpoints;

	public Vector3[] PatrolPositions
	{
		get
		{
			Vector3[] positions = new Vector3[_patrolNavpoints.Length];

			for (var i = 0; i < positions.Length; i++)
				positions[i] = _patrolNavpoints[i].transform.position;

			return positions;
		}
	}

	private void Awake()
	{
		this.tag = "Collectible";
	}

}
