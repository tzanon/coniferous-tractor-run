using UnityEngine;
using UnityEngine.InputSystem;

using Directions;

public class Player : Actor
{
	[SerializeField] private GameplayManager _level;

	private ChaserControls _controls;
	private InputAction _moveAction;

	[SerializeField] private float _speed = 7.0f;
	private Vector2 _movement = Vector2.zero, _lastMovement = Vector2.zero;

	public int NumCollectibles { get; private set; }

	protected override void Awake()
	{
		base.Awake();

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

	private void OnEnable()
	{
		_controls.PlayerControls.Enable();
	}

	private void OnDisable()
	{
		_controls.PlayerControls.Disable();
	}

	private void FixedUpdate()
	{
		if (_movement != Vector2.zero)
		{
			_rb.MovePosition(_rb.position + _speed * _movement * Time.fixedDeltaTime);
		}
	}

	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.CompareTag("Collectible"))
		{
			//LogDebugMessage("picked up an apple!");
			MessageLogger.LogActorMessage("Picked up a collectible!", MessageLogger.Level.Debug);

			NumCollectibles++;

			Collectible collectible = coll.GetComponent<Collectible>();

			_level.DeleteCollectible(collectible);
		}
		else if (coll.CompareTag("Tractor"))
		{
			// game over
			MessageLogger.LogActorMessage("Game over...", MessageLogger.Level.Debug);
		}
	}

	private void ReadMovementInput(InputAction.CallbackContext ctx)
	{
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

	private void CancelMovementInput()
	{
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
