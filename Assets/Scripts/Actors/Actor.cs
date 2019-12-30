using System.Collections.Generic;
using UnityEngine;

using Directions;

public abstract class Actor : MonoBehaviour
{
	struct DirectionCharacteristic
	{
		public readonly string idleAnimState;
		public readonly string moveAnimState;
		public readonly Vector2 colliderSize;

		public DirectionCharacteristic(string idle, string move, Vector2 collSize)
		{
			idleAnimState = idle;
			moveAnimState = move;
			colliderSize = collSize;
		}
	}

	public bool isAnimated = false;
	public bool isColliderConstant = false;

	public Sprite idleFwd, idleBack, idleSide;
	protected string idleAnimFwd, idleAnimBack, idleAnimRight, idleAnimLeft;
	protected string moveAnimFwd, moveAnimBack, moveAnimRight, moveAnimLeft;

	[Header("Collider Sizes")]
	public Vector2 verticalCollSize;
	public Vector2 horizontalCollSize;

	protected MovementVector _currentDirection = MovementVector.Null;

	private readonly Dictionary<MovementVector, DirectionCharacteristic> _directionCharacteristics = new Dictionary<MovementVector, DirectionCharacteristic>();

	protected SpriteRenderer sr;
	protected BoxCollider2D bc;
	protected Rigidbody2D rb;
	private Animator animator;

	protected virtual void Awake()
	{
		sr = GetComponent<SpriteRenderer>();
		bc = GetComponent<BoxCollider2D>();
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();

		if (isColliderConstant)
		{
			verticalCollSize = horizontalCollSize = idleFwd.bounds.size;
		}
		else
		{
			verticalCollSize = idleFwd.bounds.size;
			horizontalCollSize = idleSide.bounds.size;
		}
	}

	protected virtual void Start()
	{
		_directionCharacteristics[MovementVector.Down] = new DirectionCharacteristic(idleAnimFwd, moveAnimFwd, verticalCollSize);
		_directionCharacteristics[MovementVector.Up] = new DirectionCharacteristic(idleAnimBack, moveAnimBack, verticalCollSize);
		_directionCharacteristics[MovementVector.Right] = new DirectionCharacteristic(idleAnimRight, moveAnimRight, horizontalCollSize);
		_directionCharacteristics[MovementVector.Left] = new DirectionCharacteristic(idleAnimLeft, moveAnimLeft, horizontalCollSize);

		SetIdleAnimInDirection(MovementVector.Down);
	}

	protected void SetMoveAnimInDirection(MovementVector direction)
	{
		SetAnimation(direction, true);
	}

	protected void SetIdleAnimInDirection(MovementVector direction)
	{
		SetAnimation(direction, false);
	}

	private void SetAnimation(MovementVector direction, bool isMoving)
	{
		if (isMoving && (_currentDirection == direction || direction == MovementVector.Center))
			return;

		if (direction == MovementVector.Null)
		{
			Debug.LogError("Trying to start animation in null direction on " + this.name);
			return;
		}

		DirectionCharacteristic dc = _directionCharacteristics[direction];

		// flip side animation for left movement
		if (direction == MovementVector.Left)
			sr.flipX = true;
		else
			sr.flipX = false;

		// play walking animation if moving, idle if not
		if (isMoving)
		{
			Debug.Log("Moving in direction " + direction.Value);
			_currentDirection = direction;
			animator.Play(dc.moveAnimState);
		}
		else
		{
			Debug.Log("Stopping in direction " + direction.Value);
			_currentDirection = MovementVector.Center;
			animator.Play(dc.idleAnimState);
		}

		if (!isColliderConstant)
		{
			bc.size = dc.colliderSize;
		}
	}
}
