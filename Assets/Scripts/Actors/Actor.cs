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

	// animation sprites and names
	[SerializeField] private Sprite _idleFwd, _idleBack, _idleSide;
	protected string _idleAnimFwd, _idleAnimBack, _idleAnimRight, _idleAnimLeft;
	protected string _moveAnimFwd, _moveAnimBack, _moveAnimRight, _moveAnimLeft;

	private Vector2 _verticalCollSize;
	private Vector2 _horizontalCollSize;

	private readonly Dictionary<MovementVector, DirectionCharacteristic> _directionCharacteristics = new Dictionary<MovementVector, DirectionCharacteristic>();

	// components
	protected SpriteRenderer _sr;
	protected BoxCollider2D _bc;
	protected Rigidbody2D _rb;
	private Animator _animator;

	[SerializeField] protected TilemapManager _tilemapManager;
	protected TilemapHighlighter _highlighter;
	protected NavigationMap _map;


	/* properties */

	public float CurrentSpeed { get; protected set; }

	public MovementVector CurrentDirection { get; protected set; }

	public abstract bool IsIdle { get; }

	public Vector3 Position { get => transform.position; set => transform.position = value; }

	/* abstract methods */

	protected abstract void AssignAnimationStateNames();

	protected abstract void SetUpStateMachine();

	/* virtual and normal methods */

	protected virtual void Awake()
	{
		_verticalCollSize = _idleFwd.bounds.size;
		_horizontalCollSize = _idleSide.bounds.size;

		CacheComponents();
		AssignAnimationStateNames();
		SetUpDirectionCharacteristics();
		SetUpStateMachine();
	}

	/// <summary>
	/// Get references to all components attached to the gameobject
	/// </summary>
	private void CacheComponents()
	{
		_sr = GetComponent<SpriteRenderer>();
		_bc = GetComponent<BoxCollider2D>();
		_rb = GetComponent<Rigidbody2D>();
		_animator = GetComponent<Animator>();

		_highlighter = _tilemapManager.GetComponent<TilemapHighlighter>();
		_map = _tilemapManager.GetComponent<NavigationMap>();
	}

	private void SetUpDirectionCharacteristics()
	{
		_directionCharacteristics[MovementVector.Down] = new DirectionCharacteristic(_idleAnimFwd, _moveAnimFwd, _verticalCollSize);
		_directionCharacteristics[MovementVector.Up] = new DirectionCharacteristic(_idleAnimBack, _moveAnimBack, _verticalCollSize);
		_directionCharacteristics[MovementVector.Right] = new DirectionCharacteristic(_idleAnimRight, _moveAnimRight, _horizontalCollSize);
		_directionCharacteristics[MovementVector.Left] = new DirectionCharacteristic(_idleAnimLeft, _moveAnimLeft, _horizontalCollSize);
		_directionCharacteristics[MovementVector.Center] = _directionCharacteristics[MovementVector.Down];
	}

	protected virtual void Start()
	{
		SetIdleAnimInDirection(MovementVector.Down);
	}

	public void SetMoveAnimInDirection(MovementVector direction)
	{
		SetDirectionalAnimation(direction, true);
	}

	public void SetIdleAnimInDirection(MovementVector direction)
	{
		SetDirectionalAnimation(direction, false);
	}

	/// <summary>
	/// Sets a moving or idle animation in the given direction
	/// </summary>
	/// <param name="direction"> Direction of the animation to be played </param>
	/// <param name="isMoving"> Whether a moving or idle animation should be played </param>
	protected void SetDirectionalAnimation(MovementVector direction, bool isMoving)
	{
		// return if desired direction is the same as the current one
		//if (isMoving && (CurrentDirection == direction || direction == MovementVector.Center)) return;

		if (direction == MovementVector.Null)
		{
			MessageLogger.LogActorMessage("Trying to start animation in null direction on ", LogLevel.Error, this.name);
			return;
		}

		// TODO: if direction is Center should dc just be set to that of CurrentDirection?

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
			CurrentDirection = direction;
			_animator.Play(dc.IdleAnimState);
		}

		// set collider to directional size
		_bc.size = dc.ColliderSize;
	}

}
