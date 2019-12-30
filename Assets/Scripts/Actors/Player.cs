using UnityEngine;
using UnityEngine.InputSystem;

using Directions;

public class Player : Actor
{
	public bool debugMode = false;
	public Level level;

	private ChaserControls controls;
	private InputAction moveAction;

	private InputAction moveUp;
	private InputAction moveDown;
	private InputAction moveRight;
	private InputAction moveLeft;

	public float speed = 6.0f;
	private Vector2 _movement = Vector2.zero, _lastMovement = Vector2.zero;

	private int _numCollectibles = 0;

	public int NumCollectibles { get => _numCollectibles; }

	protected override void Awake()
	{
		base.Awake();

		idleAnimFwd = "Player_idleForward";
		idleAnimBack = "Player_idleBackward";
		idleAnimRight = "Player_idleRight";
		idleAnimLeft = "Player_idleLeft";

		moveAnimFwd = "Player_walkForward";
		moveAnimBack = "Player_walkBackward";
		moveAnimRight = "Player_walkRight";
		moveAnimLeft = "Player_walkLeft";

		controls = new ChaserControls();
		moveAction = controls.PlayerControls.Move;
		/*
		moveUp = controls.PlayerControls.MoveUp;
		moveDown = controls.PlayerControls.MoveDown;
		moveRight = controls.PlayerControls.MoveRight;
		moveLeft = controls.PlayerControls.MoveLeft;
		
		moveUp.started += ctx => SetMoveAnimInDirection(MovementVector.Up);
		moveDown.started += ctx => SetMoveAnimInDirection(MovementVector.Down);
		moveRight.started += ctx => SetMoveAnimInDirection(MovementVector.Right);
		moveLeft.started += ctx => SetMoveAnimInDirection(MovementVector.Left);

		moveUp.canceled += ctx => SetIdleAnimInDirection(MovementVector.Up);
		moveDown.canceled += ctx => SetIdleAnimInDirection(MovementVector.Down);
		moveRight.canceled += ctx => SetIdleAnimInDirection(MovementVector.Right);
		moveLeft.canceled += ctx => SetIdleAnimInDirection(MovementVector.Left);
		/**/

		//moveAction.performed += ctx => movement = ctx.ReadValue<Vector2>().normalized;
		moveAction.performed += ctx => ReadMovementInput(ctx);
		//moveAction.canceled += ctx => _movement = Vector2.zero;
		moveAction.canceled += ctx => CancelMovementInput();

		controls.PlayerControls.Enable();
	}

	private void OnEnable()
	{
		controls.PlayerControls.Enable();
	}

	private void OnDisable()
	{
		controls.PlayerControls.Disable();
	}

	private void FixedUpdate()
	{
		//if (_currentDirection != MovementVector.Center && _currentDirection != MovementVector.Null)
		if (_movement != Vector2.zero)
		{
			//Vector2 directionValue = new Vector2(_currentDirection.Value.x, _currentDirection.Value.y);
			rb.MovePosition(rb.position + speed * _movement * Time.fixedDeltaTime);
		}
	}

	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.CompareTag("Collectible"))
		{
			if (debugMode)
				Debug.Log("picked up an apple!");

			_numCollectibles++;

			level.DeleteCollectible(coll.GetComponent<Collectible>());
		}
		else if (coll.CompareTag("Tractor"))
		{
			// game over
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
