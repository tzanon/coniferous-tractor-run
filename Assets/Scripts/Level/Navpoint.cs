using UnityEngine;

public class Navpoint : MonoBehaviour
{
	public Vector3 WorldPosition => transform.position;

	public void Awake()
	{
		var renderer = GetComponent<SpriteRenderer>();
		renderer.enabled = false;
	}
}
