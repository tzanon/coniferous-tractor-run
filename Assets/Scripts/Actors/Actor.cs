using System.Collections.Generic;
using UnityEngine;

using Directions;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEditor.VersionControl;

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

	protected Vector2 _lastMovement = Vector3.zero;

	// animation sprites and names
	[SerializeField] private Sprite _idleFwd, _idleBack, _idleSide;
	protected string _idleAnimFwd, _idleAnimBack, _idleAnimRight, _idleAnimLeft;
	protected string _moveAnimFwd, _moveAnimBack, _moveAnimRight, _moveAnimLeft;

	private Vector2 _verticalCollSize;
	private Vector2 _horizontalCollSize;

	private readonly Dictionary<CardinalDirection, DirectionCharacteristic> _directionCharacteristics = new Dictionary<CardinalDirection, DirectionCharacteristic>();

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

	//public CardinalDirection CurrentDirection { get; protected set; }

	public bool IsIdle
	{
		get;
		protected set;
	}

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

		_highlighter = _tilemapManager?.GetComponent<TilemapHighlighter>();
		_map = _tilemapManager?.GetComponent<NavigationMap>();
	}

	private void SetUpDirectionCharacteristics()
	{
		_directionCharacteristics[CardinalDirection.South] = new DirectionCharacteristic(_idleAnimFwd, _moveAnimFwd, _verticalCollSize);
		_directionCharacteristics[CardinalDirection.North] = new DirectionCharacteristic(_idleAnimBack, _moveAnimBack, _verticalCollSize);
		_directionCharacteristics[CardinalDirection.East] = new DirectionCharacteristic(_idleAnimRight, _moveAnimRight, _horizontalCollSize);
		_directionCharacteristics[CardinalDirection.West] = new DirectionCharacteristic(_idleAnimLeft, _moveAnimLeft, _horizontalCollSize);
		_directionCharacteristics[CardinalDirection.Center] = _directionCharacteristics[CardinalDirection.South];
	}

	protected virtual void Start()
	{
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

		var movementDirection = CardinalDirection.Vec3ToCardinal(normalizedMovement);

		// move actor if nonzero movement
		if (normalizedMovement != Vector2.zero)
		{
			_rb.MovePosition(_rb.position + CurrentSpeed * normalizedMovement * Time.fixedDeltaTime);

			if (normalizedMovement != _lastMovement)
			{
				MessageLogger.LogDebugMessage(LogType.Actor, "Setting animation in direction {0} from last direction {1}", normalizedMovement, _lastMovement);
				SetMoveAnimInDirection(movementDirection);
				IsIdle = false;
			}
		}
		else // zero movement, if actor was moving last time set idle anim in movement's direction
		{
			if (_lastMovement != Vector2.zero)
			{
				var lastMovementDirection = CardinalDirection.Vec3ToCardinal(_lastMovement);
				SetIdleAnimInDirection(lastMovementDirection);
				IsIdle = true;
			}
		}

		// update last movement
		_lastMovement = normalizedMovement;
	}

	protected void SetMoveAnimInDirection(CardinalDirection direction)
	{
		SetDirectionalAnimation(direction, true);
	}

	protected void SetIdleAnimInDirection(CardinalDirection direction)
	{
		SetDirectionalAnimation(direction, false);
	}

	/// <summary>
	/// Sets a moving or idle animation in the given direction
	/// </summary>
	/// <param name="direction"> Direction of the animation to be played </param>
	/// <param name="isMoving"> Whether a moving or idle animation should be played </param>
	protected void SetDirectionalAnimation(CardinalDirection direction, bool isMoving)
	{
		// check if trying to set animation in nonexistant direction
		if (direction == CardinalDirection.Null)
		{
			MessageLogger.LogErrorMessage(LogType.Actor, "Trying to start animation in null direction on ", this.name);
			return;
		}

		DirectionCharacteristic dc = _directionCharacteristics[direction];

		// flip side animation for left movement
		if (direction == CardinalDirection.West)
			_sr.flipX = true;
		else
			_sr.flipX = false;

		// play walking animation if moving, idle if not
		if (isMoving)
		{
			MessageLogger.LogVerboseMessage(LogType.Actor, "Moving in direction {0}", direction.Value);
			_animator.Play(dc.MoveAnimState);
		}
		else
		{
			MessageLogger.LogVerboseMessage(LogType.Actor, "Stopping in direction {0}", direction.Value);
			_animator.Play(dc.IdleAnimState);
		}

		// set collider to directional size
		_bc.size = dc.ColliderSize;
	}

}
