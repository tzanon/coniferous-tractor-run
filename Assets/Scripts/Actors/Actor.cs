using System.Collections.Generic;
using UnityEngine;

using Directions;

public abstract class Actor : MonoBehaviour
{
	/// <summary>
	/// Grouping of animation and collider characteristics based on direction
	/// </summary>
	struct DirectionCharacteristic
	{
		public string IdleAnimState { get; }
		public string MoveAnimState { get; }
		public Vector2 ColliderSize { get; }

		public DirectionCharacteristic(string idle, string move, Vector2 collSize)
		{
			IdleAnimState = idle;
			MoveAnimState = move;
			ColliderSize = collSize;
		}
	}

	[SerializeField] protected bool _isAnimated = false;

	[SerializeField] private Sprite _idleFwd, _idleBack, _idleSide;
	protected string _idleAnimFwd, _idleAnimBack, _idleAnimRight, _idleAnimLeft;
	protected string _moveAnimFwd, _moveAnimBack, _moveAnimRight, _moveAnimLeft;

	[Header("Collider Sizes")]
	private Vector2 _verticalCollSize;
	private Vector2 _horizontalCollSize;

	private readonly Dictionary<MovementVector, DirectionCharacteristic> _directionCharacteristics = new Dictionary<MovementVector, DirectionCharacteristic>();

	protected SpriteRenderer _sr;
	protected BoxCollider2D _bc;
	protected Rigidbody2D _rb;
	private Animator _animator;

	public MovementVector CurrentDirection { get; protected set; }

	protected virtual void Awake()
	{
		_sr = GetComponent<SpriteRenderer>();
		_bc = GetComponent<BoxCollider2D>();
		_rb = GetComponent<Rigidbody2D>();
		_animator = GetComponent<Animator>();

		
		_verticalCollSize = _idleFwd.bounds.size;
		_horizontalCollSize = _idleSide.bounds.size;
	}

	protected virtual void Start()
	{
		_directionCharacteristics[MovementVector.Down] = new DirectionCharacteristic(_idleAnimFwd, _moveAnimFwd, _verticalCollSize);
		_directionCharacteristics[MovementVector.Up] = new DirectionCharacteristic(_idleAnimBack, _moveAnimBack, _verticalCollSize);
		_directionCharacteristics[MovementVector.Right] = new DirectionCharacteristic(_idleAnimRight, _moveAnimRight, _horizontalCollSize);
		_directionCharacteristics[MovementVector.Left] = new DirectionCharacteristic(_idleAnimLeft, _moveAnimLeft, _horizontalCollSize);

		SetIdleAnimInDirection(MovementVector.Down);
	}

	protected void SetMoveAnimInDirection(MovementVector direction)
	{
		SetDirectionalAnimation(direction, true);
	}

	protected void SetIdleAnimInDirection(MovementVector direction)
	{
		SetDirectionalAnimation(direction, false);
	}

	/// <summary>
	/// Sets a moving or idle animation in the given direction
	/// </summary>
	/// <param name="direction"> Direction of the animation to be played </param>
	/// <param name="isMoving"> Whether a moving or idle animation should be played </param>
	private void SetDirectionalAnimation(MovementVector direction, bool isMoving)
	{
		// return if desired direction is the same as the current one
		if (isMoving && (CurrentDirection == direction || direction == MovementVector.Center))
			return;

		if (direction == MovementVector.Null)
		{
			Debug.LogError("Trying to start animation in null direction on " + this.name);
			return;
		}

		DirectionCharacteristic dc = _directionCharacteristics[direction];

		// flip side animation for left movement
		if (direction == MovementVector.Left)
			_sr.flipX = true;
		else
			_sr.flipX = false;

		// play walking animation if moving, idle if not
		if (isMoving)
		{
			Debug.Log("Moving in direction " + direction.Value);
			CurrentDirection = direction;
			_animator.Play(dc.MoveAnimState);
		}
		else
		{
			Debug.Log("Stopping in direction " + direction.Value);
			CurrentDirection = MovementVector.Center;
			_animator.Play(dc.IdleAnimState);
		}

		// set collider to directional size
		_bc.size = dc.ColliderSize;
	}


}
