
using UnityEngine;

/// <summary>
/// Visualizes boundaries of box collider of gameobject
/// </summary>
[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class ColliderVisualizer : ScriptBase
{
	[SerializeField] private float _refreshRate = 0.1f;
	private float _nextRefresh = 0f;

	[SerializeField] private GameObject _marker;
	private GameObject[] _boundMarkers;

	private BoxCollider2D _collider;
	private Rigidbody2D _rb;

	void Awake()
	{
		_rb = GetComponent<Rigidbody2D>();
		_collider = GetComponent<BoxCollider2D>();

		_boundMarkers = new GameObject[4];
		for (int i = 0; i < _boundMarkers.Length; i++)
		{
			_boundMarkers[i] = Instantiate(_marker);
		}

		UpdateCollider();
	}

	private void LateUpdate()
	{
		if (_rb.bodyType == RigidbodyType2D.Static)
		{
			return;
		}

		_nextRefresh -= Time.deltaTime;

		if (_nextRefresh <= 0f)
		{
			UpdateCollider();
			_nextRefresh = _refreshRate;
		}
	}

	private void UpdateCollider()
	{
		Bounds bounds = _collider.bounds;
		_boundMarkers[0].transform.position = new Vector3(bounds.max.x, bounds.max.y, 0f);
		_boundMarkers[1].transform.position = new Vector3(bounds.max.x, bounds.min.y, 0f);
		_boundMarkers[2].transform.position = new Vector3(bounds.min.x, bounds.max.y, 0f);
		_boundMarkers[3].transform.position = new Vector3(bounds.min.x, bounds.min.y, 0f);
	}

}
