using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
	[SerializeField] private Player _player;
	private Vector2 _movementDirection;

	private ChaserControls _controls;
	private InputAction _moveAction;

	private void Awake()
	{
		_movementDirection = Vector3.zero;
		SetUpInput();
	}

	/// <summary>
	/// Initialize controls and input actions
	/// </summary>
	private void SetUpInput()
	{
		_controls = new ChaserControls();
		_moveAction = _controls.PlayerControls.Move;

		_moveAction.performed += ctx => ReadMovementInput(ctx);
		_moveAction.canceled += ctx => CancelMovementInput();
	}

	/// <summary>
	/// Applies input values to player
	/// </summary>
	private void FixedUpdate()
	{
		if (!_player.InputBlocked)
			_player.MoveActor(_movementDirection);
	}

	/// <summary>
	/// Enable controls
	/// </summary>
	private void OnEnable()
	{
		_controls.PlayerControls.Enable();
	}

	/// <summary>
	/// Disable controls
	/// </summary>
	private void OnDisable()
	{
		_controls.PlayerControls.Disable();
	}

	/// <summary>
	/// Sets player movement according to input
	/// </summary>
	/// <param name="ctx">Context from pressed keys</param>
	private void ReadMovementInput(InputAction.CallbackContext ctx)
	{
		_movementDirection = ctx.ReadValue<Vector2>();
	}

	/// <summary>
	/// Stops movement when input stops
	/// </summary>
	private void CancelMovementInput()
	{
		_movementDirection = Vector2.zero;
	}

}
