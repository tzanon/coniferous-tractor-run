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

		public Vector2 ColliderOffset { get; }

		public DirectionCharacteristic(string idle, string move, Vector2 collSize, Vector2 collOffset)
		{
			IdleAnimState = idle;
			MoveAnimState = move;
			ColliderSize = collSize;
			ColliderOffset = collOffset;
		}
	}

	/* fields */

	[SerializeField] protected bool _isAnimated = false;

	protected Vector2 _lastMovement = Vector3.zero;

	protected FiniteStateMachine _stateMachine;

	// animation sprites and names
	[SerializeField] private Sprite _idleFwd, _idleBack, _idleSide;
	protected string _idleAnimFwd, _idleAnimBack, _idleAnimRight, _idleAnimLeft;
	protected string _moveAnimFwd, _moveAnimBack, _moveAnimRight, _moveAnimLeft;

	// TODO: split vertical into forward/backward with offset
	[SerializeField] private Vector2 _verticalCollOffset;
	[SerializeField] private Vector2 _verticalCollSize;

	[SerializeField] private Vector2 _horizontalCollOffset;
	[SerializeField] private Vector2 _horizontalCollSize;

	private readonly Dictionary<CardinalDirection, DirectionCharacteristic> _directionCharacteristics = new Dictionary<CardinalDirection, DirectionCharacteristic>();

	// components
	protected SpriteRenderer _sr;
	protected BoxCollider2D _bc;
	protected Rigidbody2D _rb;
	private Animator _animator;

	[SerializeField] protected GameplayManager _gameplayManager;
	[SerializeField] protected TilemapManager _tilemapManager;
	protected TilemapHighlighter _highlighter;
	protected NavigationMap _navMap;
	protected LevelPathManager _pathManager;

	/* properties */

	public bool Stuck { get; set; }

	public float CurrentSpeed { get; protected set; }

	public bool IsIdle
	{
		get;
		protected set;
	}

	public Vector3 Position { get => transform.position; set => transform.position = value; }

	/* abstract methods */

	protected abstract void AssignAnimationStateNames();

	protected virtual void SetUpStateMachine()
	{
		_stateMachine = new FiniteStateMachine();
	}

	/* virtual and normal methods */

	protected virtual void Awake()
	{
		if (_verticalCollSize == Vector2.zero)
			_verticalCollSize = _idleFwd.bounds.size;

		if (_horizontalCollSize == Vector2.zero)
			_horizontalCollSize = _idleSide.bounds.size;
		
		Stuck = false;

		CacheComponents();
		AssignAnimationStateNames();
		SetUpDirectionCharacteristics();
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
		_navMap = _tilemapManager.GetComponent<NavigationMap>();
		_pathManager = _tilemapManager.GetComponent<LevelPathManager>();
	}

	/// <summary>
	/// Initialize each direction's associated animations and colliders
	/// </summary>
	private void SetUpDirectionCharacteristics()
	{
		_directionCharacteristics[CardinalDirection.South] = new DirectionCharacteristic(_idleAnimFwd, _moveAnimFwd, _verticalCollSize, _verticalCollOffset);
		_directionCharacteristics[CardinalDirection.North] = new DirectionCharacteristic(_idleAnimBack, _moveAnimBack, _verticalCollSize, _verticalCollOffset);
		_directionCharacteristics[CardinalDirection.East] = new DirectionCharacteristic(_idleAnimRight, _moveAnimRight, _horizontalCollSize, _horizontalCollOffset);
		_directionCharacteristics[CardinalDirection.West] = new DirectionCharacteristic(_idleAnimLeft, _moveAnimLeft, _horizontalCollSize, _horizontalCollOffset);
		_directionCharacteristics[CardinalDirection.Center] = _directionCharacteristics[CardinalDirection.South];
	}

	protected virtual void Start()
	{
		SetUpStateMachine();
		SetIdleAnimInDirection(CardinalDirection.South);
	}

	/// <summary>
	/// Moves the actor in the direction of the given Vector3
	/// </summary>
	/// <param name="movement">Direction to move in</param>
	public void MoveActor(Vector2 movement)
	{
		// convert to directional unit vector
		var normalizedMovement = movement.normalized;

		// move actor if nonzero movement
		if (normalizedMovement != Vector2.zero)
		{
			_rb.MovePosition(_rb.position + CurrentSpeed * normalizedMovement * Time.fixedDeltaTime);

			if (normalizedMovement != _lastMovement)
			{
				MessageLogger.LogVerboseMessage(LogType.Actor, "Setting move animation in direction {0} from last direction {1}", normalizedMovement, _lastMovement);
				var movementDirection = CardinalDirection.Vec3ToCardinal(normalizedMovement);
				SetMoveAnimInDirection(movementDirection);
				IsIdle = false;
			}
		}
		else // zero movement, if actor was moving last time set idle anim in movement's direction
		{
			if (_lastMovement != Vector2.zero)
			{
				MessageLogger.LogVerboseMessage(LogType.Actor, "Setting idle animation from last direction {0}", _lastMovement);
				var lastMovementDirection = CardinalDirection.Vec3ToCardinal(_lastMovement);
				SetIdleAnimInDirection(lastMovementDirection);
				IsIdle = true;
			}
		}

		// update last movement
		_lastMovement = normalizedMovement;
	}

	/// <summary>
	/// Set move animation in given direction
	/// </summary>
	/// <param name="direction">Direction of the animation to be played</param>
	protected void SetMoveAnimInDirection(CardinalDirection direction) => SetDirectionalAnimation(direction, true);

	/// <summary>
	/// Set idle animation in given direction
	/// </summary>
	/// <param name="direction">Direction of the animation to be played</param>
	protected void SetIdleAnimInDirection(CardinalDirection direction) => SetDirectionalAnimation(direction, false);

	/// <summary>
	/// Sets a moving or idle animation in the given direction
	/// </summary>
	/// <param name="direction">Direction of the animation to be played</param>
	/// <param name="isMoving">Whether a moving or idle animation should be played</param>
	private void SetDirectionalAnimation(CardinalDirection direction, bool isMoving)
	{
		if (!_isAnimated)
			return;

		// check if trying to set animation in nonexistant direction
		if (direction == CardinalDirection.Null)
		{
			MessageLogger.LogErrorMessage(LogType.Actor, "Trying to start animation in null direction on ", this.name);
			return;
		}

		// flip side animation for left movement
		_sr.flipX = direction == CardinalDirection.West;

		// play walking animation if moving, idle if not
		var dc = _directionCharacteristics[direction];
		var anim = isMoving ? dc.MoveAnimState : dc.IdleAnimState;
		_animator.Play(anim);

		// set collider to directional size
		_bc.size = dc.ColliderSize;
		_bc.offset = dc.ColliderOffset;
	}
}
