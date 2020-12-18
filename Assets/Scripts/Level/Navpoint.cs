using UnityEngine;

public class Navpoint : MonoBehaviour
{
	private SpriteRenderer renderer;

	public Vector3 WorldPosition => transform.position;

	public void Awake()
	{
		renderer = GetComponent<SpriteRenderer>();
		renderer.enabled = false;
	}
}
