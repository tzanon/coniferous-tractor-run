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

	/* fields */

	[SerializeField] protected bool _isAnimated = false;

	protected Vector2 _movement = Vector2.zero, _lastMovement = Vector2.zero;

	// animation sprites and names
	[SerializeField] private Sprite _idleFwd, _idleBack, _idleSide;
	protected string _idleAnimFwd, _idleAnimBack, _idleAnimRight, _idleAnimLeft;
	protected string _moveAnimFwd, _moveAnimBack, _moveAnimRight, _moveAnimLeft;

	private Vector2 _verticalCollSize;
	private Vector2 _horizontalCollSize;

	private readonly Dictionary<MovementVector, DirectionCharacteristic> _directionCharacteristics = new Dictionary<MovementVector, DirectionCharacteristic>();

	protected SpriteRenderer _sr;
	protected BoxCollider2D _bc;
	protected Rigidbody2D _rb;
	private Animator _animator;

	/* properties */

	public float CurrentSpeed { get; protected set; }

	public MovementVector CurrentDirection { get; protected set; }

	public bool IsIdle { get => _movement == Vector2.zero; }

	public Vector3 Position { get => transform.position; set => transform.position = value; }

	/* abstract methods */

	protected abstract void AssignAnimationStateNames();

	protected abstract void SetUpStateMachine();

	/* virtual and normal methods */

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

	protected virtual void FixedUpdate()
	{
		if (_movement != Vector2.zero)
		{
			_rb.MovePosition(_rb.position + CurrentSpeed * _movement * Time.fixedDeltaTime);
		}
	}

	/// <summary>
	/// Externally-accessible way of setting movement
	/// </summary>
	/// <param name="movementDirection">Direction to move in</param>
	public void SetMovementDirection(MovementVector movementDirection)
	{
		_movement = movementDirection.Value;
		SetMoveAnimInDirection(movementDirection);
	}

	/// <summary>
	/// Externally-accessible way of stopping movement
	/// </summary>
	/// <param name="stopDirection">Movement direction being stopped in</param>
	public void StopMovement(MovementVector stopDirection)
	{
		_movement = MovementVector.Center.Value;
		SetIdleAnimInDirection(stopDirection);
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
			MessageLogger.LogActorMessage("Trying to start animation in null direction on ", LogLevel.Error, this.name);
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
			MessageLogger.LogActorMessage("Moving in direction {0}", LogLevel.Verbose, direction.Value);
			CurrentDirection = direction;
			_animator.Play(dc.MoveAnimState);
		}
		else
		{
			MessageLogger.LogActorMessage("Stopping in direction {0}", LogLevel.Verbose, direction.Value);
			CurrentDirection = MovementVector.Center;
			_animator.Play(dc.IdleAnimState);
		}

		// set collider to directional size
		_bc.size = dc.ColliderSize;
	}


}
