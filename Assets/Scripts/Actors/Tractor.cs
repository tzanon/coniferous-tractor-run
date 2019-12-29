using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Pathfinding;

public class Tractor : MonoBehaviour
{
	public RectTransform debugDisplay; // TODO: 

	public Sprite fwd, back, right, left;

	private bool _calculatingPath; // if calculating, idle
	private Vector3Int[] currentPath;

	private Vector3 destPoint; // world space!

	private MovementDirection _currentDirection = MovementDirection.Null;
	private float _currentSpeed;
	[SerializeField] private const float _defaultSpeed = 4f;
	[SerializeField] private const float _chaseSpeed = 8f;

	private SpriteRenderer sr;
	private BoxCollider2D bc;
	private Rigidbody2D rb;

	private static readonly Vector2 verticalColliderSize = new Vector2(1.0f, 2.5f);
	private static readonly Vector2 horizontalColliderSize = new Vector2(2.5f, 1.0f);

	private void Awake()
	{
		sr = GetComponent<SpriteRenderer>();
		bc = GetComponent<BoxCollider2D>();
		rb = GetComponent<Rigidbody2D>();

		SetDirection(MovementDirection.Down);
	}

	private void FixedUpdate()
	{
		// TODO: path following and handling
	}

	private void SetDirection(MovementDirection direction)
	{
		if (_currentDirection == direction || direction == MovementDirection.Center)
			return;

		_currentDirection = direction;

		if (_currentDirection == MovementDirection.Up)
		{
			SetSpriteDirection(back, verticalColliderSize);
			return;
		}

		if (_currentDirection == MovementDirection.Down)
		{
			SetSpriteDirection(fwd, verticalColliderSize);
			return;
		}

		if (_currentDirection == MovementDirection.Right)
		{
			SetSpriteDirection(right, horizontalColliderSize);
			return;
		}

		if (_currentDirection == MovementDirection.Left)
		{
			SetSpriteDirection(left, horizontalColliderSize);
			return;
		}

		Debug.LogError("Tractor has null direction!");

	}

	private void SetSpriteDirection(Sprite sprite, Vector2 colliderSize)
	{
		sr.sprite = sprite;
		bc.size = colliderSize;
	}

}
