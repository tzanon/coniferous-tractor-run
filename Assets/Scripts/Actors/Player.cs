using UnityEngine;
using UnityEngine.InputSystem;

using Directions;

public class Player : Actor
{
	/* fields */

	[SerializeField] private GameplayManager _level;

	private ChaserControls _controls;
	private InputAction _moveAction;

	[SerializeField] private float _playerSpeed = 5.0f;

	// TODO: delete this?
	private MovementVector _currentMovement;

	/* properties */

	public bool InputBlocked { get; set; }

	public int NumCollectibles { get; private set; }


	/* methods */

	protected override void Awake()
	{
		base.Awake();

		CurrentSpeed = _playerSpeed;
		InputBlocked = false;

		_idleAnimFwd = "Player_idleForward";
		_idleAnimBack = "Player_idleBackward";
		_idleAnimRight = "Player_idleRight";
		_idleAnimLeft = "Player_idleLeft";

		_moveAnimFwd = "Player_walkForward";
		_moveAnimBack = "Player_walkBackward";
		_moveAnimRight = "Player_walkRight";
		_moveAnimLeft = "Player_walkLeft";

		_controls = new ChaserControls();
		_moveAction = _controls.PlayerControls.Move;

		_moveAction.performed += ctx => ReadMovementInput(ctx);
		_moveAction.canceled += ctx => CancelMovementInput();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();


	}

	private void OnEnable()
	{
		_controls.PlayerControls.Enable();
	}

	private void OnDisable()
	{
		_controls.PlayerControls.Disable();
	}

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
		if (InputBlocked)
		{
			MessageLogger.LogActorMessage("Player input blocked; Cannot move", LogLevel.Verbose);
			return;
		}

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
		if (InputBlocked)
		{
			MessageLogger.LogActorMessage("Player input blocked; Cannot stop", LogLevel.Verbose);
			return;
		}

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
