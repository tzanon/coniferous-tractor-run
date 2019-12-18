
using UnityEngine;

/// <summary>
/// Visualizes boundaries of box collider of gameobject
/// </summary>
[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class ColliderVisualizer : MonoBehaviour
{
	[SerializeField] private float refreshRate = 0.1f;
	private float nextRefresh = 0f;

	public GameObject marker;
	private GameObject[] boundMarkers;

	private BoxCollider2D bc;
	private Rigidbody2D rb;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		bc = GetComponent<BoxCollider2D>();

		boundMarkers = new GameObject[4];
		for (int i = 0; i < boundMarkers.Length; i++)
		{
			boundMarkers[i] = Instantiate(marker);
		}

		UpdateCollider();
	}

	private void LateUpdate()
	{
		if (rb.bodyType == RigidbodyType2D.Static)
		{
			return;
		}

		nextRefresh -= Time.deltaTime;

		if (nextRefresh <= 0f)
		{
			UpdateCollider();
			nextRefresh = refreshRate;
		}
	}

	private void UpdateCollider()
	{
		Bounds bounds = bc.bounds;
		boundMarkers[0].transform.position = new Vector3(bounds.max.x, bounds.max.y, 0f);
		boundMarkers[1].transform.position = new Vector3(bounds.max.x, bounds.min.y, 0f);
		boundMarkers[2].transform.position = new Vector3(bounds.min.x, bounds.max.y, 0f);
		boundMarkers[3].transform.position = new Vector3(bounds.min.x, bounds.min.y, 0f);
	}

}
