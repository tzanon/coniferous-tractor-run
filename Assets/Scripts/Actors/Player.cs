using UnityEngine;

// TODO: put user input in separate class?
public class Player : Actor
{
	/* fields */

	[SerializeField] private LevelCompletionChecker _levelCompletionChecker;

	[SerializeField] private float _playerSpeed = 5.0f;

	private bool _inputBlocked;

	/* properties */

	/// <summary>
	/// Whether player is controlled by input
	/// </summary>
	public bool InputBlocked
	{
		get => _inputBlocked;
		set
		{
			_inputBlocked = value;
			MoveActor(Vector2.zero);
		}
	}

	/// <summary>
	/// Number of collected items
	/// </summary>
	public int NumCollectibles { get; private set; }

	/* methods */

	protected override void Awake()
	{
		base.Awake();

		CurrentSpeed = _playerSpeed;
		_inputBlocked = false;
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
	/// Set up the FSM and its states and transitions
	/// </summary>
	protected override void SetUpStateMachine()
	{
		base.SetUpStateMachine();

		// create states
		var inputState = new PlayerInputControl(this);
		var autoState = new PlayerAutoControl(this, _tilemapManager, _painter, _navMap, _pathManager, _levelCompletionChecker);
		var idleState = new PlayerIdle(this);

		// create transition triggers
		bool GameOver() => _gameplayManager.GameOver;
		bool PlayerTryingToLeave() => _levelCompletionChecker.ContainsPlayer;
		bool AutoMovementDone() => autoState.ActorReachedDestination;

		// create transitions
		var gameOverIdle = new FSMTransition(idleState, GameOver);
		var switchToAutoMovement = new FSMTransition(autoState, PlayerTryingToLeave);
		var switchToInputMovement = new FSMTransition(inputState, AutoMovementDone);

		// add everything and set starting state
		_stateMachine.AddState(idleState);
		_stateMachine.AddState(inputState, switchToAutoMovement, gameOverIdle);
		_stateMachine.AddState(autoState, switchToInputMovement);
		_stateMachine.CurrentState = inputState;
	}

	/// <summary>
	/// Run FSM
	/// </summary>
	private void LateUpdate()
	{
		_stateMachine?.Run();
	}

	/// <summary>
	/// Handle collisions with triggers
	/// </summary>
	/// <param name="coll">Collider of other object</param>
	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.CompareTag("Collectible"))
		{
			MessageLogger.LogDebugMessage(LogType.Actor, "Picked up a collectible!");

			NumCollectibles++;
			Collectible collectible = coll.GetComponent<Collectible>();
			_gameplayManager.DeleteCollectible(collectible);
		}
	}

}
