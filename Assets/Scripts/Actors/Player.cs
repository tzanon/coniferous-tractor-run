using System;

using UnityEngine;
using UnityEngine.InputSystem;

using Directions;

// TODO: put user input in separate class?
public class Player : Actor
{
	/* fields */

	[SerializeField] private GameplayManager _level;
	[SerializeField] private LevelCompletionChecker _levelCompletionChecker;

	private ChaserControls _controls;
	private InputAction _moveAction;

	private FiniteStateMachine _stateMachine;

	[SerializeField] private float _playerSpeed = 5.0f;
	protected Vector2 _movement = Vector2.zero, _lastMovement = Vector2.zero;

	// TODO: delete this?
	private MovementVector _currentMovement;

	/* properties */

	public override bool IsIdle { get => _movement == Vector2.zero; }

	/// <summary>
	/// Whether player is controlled by input
	/// </summary>
	public bool InputBlocked { get; set; }

	/// <summary>
	/// 
	/// </summary>
	public int NumCollectibles { get; private set; }

	/* methods */

	protected override void Awake()
	{
		base.Awake();

		CurrentSpeed = _playerSpeed;
		InputBlocked = false;

		AssignAnimationStateNames();
		SetUpInput();

		SetUpStateMachine();
	}

	/// <summary>
	/// Set names of animation states
	/// </summary>
	protected override void AssignAnimationStateNames()
	{
		_idleAnimFwd = "Player_idleForward";
		_idleAnimBack = "Player_idleBackward";
		_idleAnimRight = "Player_idleRight";
		_idleAnimLeft = "Player_idleLeft";

		_moveAnimFwd = "Player_walkForward";
		_moveAnimBack = "Player_walkBackward";
		_moveAnimRight = "Player_walkRight";
		_moveAnimLeft = "Player_walkLeft";
	}

	/// <summary>
	/// Set up keyboard input
	/// </summary>
	private void SetUpInput()
	{
		_controls = new ChaserControls();
		_moveAction = _controls.PlayerControls.Move;

		_moveAction.performed += ctx => ReadMovementInput(ctx);
		_moveAction.canceled += ctx => CancelMovementInput();
	}

	/// <summary>
	/// Set up the FSM and its states and transitions
	/// </summary>
	protected override void SetUpStateMachine()
	{
		PlayerInputControl inputState = new PlayerInputControl(this);
		PlayerAutoControl autoState = new PlayerAutoControl(this, _levelCompletionChecker, _tilemapManager, _highlighter, _map);

		Func<bool> PlayerTryingToLeave = () => _levelCompletionChecker.ContainsPlayer;
		Func<bool> FinishedAutoMovingPlayer = () => autoState.PlayerReachedDest;

		FSMTransition switchToAutoMovement = new FSMTransition(autoState, PlayerTryingToLeave);
		FSMTransition switchToInputMovement = new FSMTransition(inputState, FinishedAutoMovingPlayer);

		inputState.AddTransition(switchToAutoMovement);
		autoState.AddTransition(switchToInputMovement);

		_stateMachine = new FiniteStateMachine(inputState);
	}

	private void FixedUpdate()
	{
		if (!InputBlocked)
			DoInputMovement();
		
	}

	// use this for FSM/checking if trying to leave?
	private void LateUpdate()
	{
		_stateMachine.Run();
	}

	private void DoInputMovement()
	{
		if (_movement != Vector2.zero)
		{
			_rb.MovePosition(_rb.position + CurrentSpeed * _movement * Time.fixedDeltaTime);
		}
	}

	/// <summary>
	/// Enable controls
	/// </summary>
	private void OnEnable()
	{
		EnableInput();
	}

	/// <summary>
	/// Disable controls
	/// </summary>
	private void OnDisable()
	{
		DisableInput();
	}

	public void EnableInput() => _controls.PlayerControls.Enable();

	public void DisableInput() => _controls.PlayerControls.Disable();

	// TODO: put game over in tractor instead?
	/// <summary>
	/// Handle collisions with triggers
	/// </summary>
	/// <param name="coll">Collider of other object</param>
	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.CompareTag("Collectible"))
		{
			MessageLogger.LogActorMessage("Picked up a collectible!", LogLevel.Debug);

			NumCollectibles++;

			Collectible collectible = coll.GetComponent<Collectible>();

			_level.DeleteCollectible(collectible);
		}
		else if (coll.CompareTag("Tractor"))
		{
			// game over
			MessageLogger.LogActorMessage("Game over...", LogLevel.Debug);
		}
	}

	/// <summary>
	/// Sets player movement according to input
	/// </summary>
	/// <param name="ctx">Context from pressed keys</param>
	private void ReadMovementInput(InputAction.CallbackContext ctx)
	{
		/*
		if (InputBlocked)
		{
			MessageLogger.LogActorMessage("Player input blocked; Cannot move", LogLevel.Verbose);
			return;
		}
		/**/

		_movement = ctx.ReadValue<Vector2>().normalized;

		if (_movement.x > 0f) // moving right
		{
			SetMoveAnimInDirection(MovementVector.Right);
		}
		else if (_movement.x < 0f) // moving left
		{
			SetMoveAnimInDirection(MovementVector.Left);
		}
		else if (_movement.y > 0f) // moving up/backward
		{
			SetMoveAnimInDirection(MovementVector.Up);
		}
		else if (_movement.y < 0f) // moving down/forward
		{
			SetMoveAnimInDirection(MovementVector.Down);
		}
	}

	/// <summary>
	/// Stops movement when input stops
	/// </summary>
	private void CancelMovementInput()
	{
		/*
		if (InputBlocked)
		{
			MessageLogger.LogActorMessage("Player input blocked; Cannot stop", LogLevel.Verbose);
			return;
		}
		/**/

		if (_movement.x > 0f) // stopping right
		{
			SetIdleAnimInDirection(MovementVector.Right);
		}
		else if (_movement.x < 0f) // stopping left
		{
			SetIdleAnimInDirection(MovementVector.Left);
		}
		else if (_movement.y > 0f) // stopping up/backward
		{
			SetIdleAnimInDirection(MovementVector.Up);
		}
		else if (_movement.y < 0f) // stopping down/forward
		{
			SetIdleAnimInDirection(MovementVector.Down);
		}

		_movement = Vector2.zero;
	}

	
}
