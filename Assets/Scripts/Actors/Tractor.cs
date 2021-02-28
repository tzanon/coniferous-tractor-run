using UnityEngine;
using TMPro;

// TODO: make a subscriber of gameplay manager
public class Tractor : Actor//, IObserver<CollectibleStatus>
{
	[SerializeField] private TMP_Text _debugDisplay; // TODO: display destination/status above tractor
	private LineRenderer _lr; // debug: for pointing to dest?

	private bool _calculatingPath; // if calculating, idle

	private const float _defaultSpeed = 4.0f;
	private const float _chaseSpeed = 8.0f;

	protected override void Awake()
	{
		base.Awake();

		CurrentSpeed = _defaultSpeed;
	}

	protected override void AssignAnimationStateNames()
	{
		_idleAnimFwd = "Tractor_idleForward";
		_idleAnimBack = "Tractor_idleBackward";
		_idleAnimRight = "Tractor_idleRight";
		_idleAnimLeft = "Tractor_idleLeft";

		_moveAnimFwd = "Tractor_moveForward";
		_moveAnimBack = "Tractor_moveBackward";
		_moveAnimRight = "Tractor_moveRight";
		_moveAnimLeft = "Tractor_moveLeft";
	}

	protected override void SetUpStateMachine()
	{
		base.SetUpStateMachine();

		// create states
		var errorState = new Error(this);
		var idleState = new Idle(this);
		var findPatrolState = new AutoFindPatrol(this, _tilemapManager, _painter, _navMap, _pathManager);
		var patrolState = new AutoPatrol(this, _tilemapManager, _painter, _navMap, _pathManager);

		// create trigger functions
		bool GameOver() => _gameplayManager.GameOver;
		bool TractorStuck() => Stuck;
		bool ReachedPatrol() => findPatrolState.ActorReachedDestination;

		// create transitions
		var gameOverIdle = new FSMTransition(idleState, GameOver);
		var stuckError = new FSMTransition(errorState, TractorStuck);
		var searchToPatrol = new FSMTransition(patrolState, ReachedPatrol);

		// add everything and set starting state
		_stateMachine.AddState(idleState);
		_stateMachine.AddUniversalTransition(gameOverIdle);

		_stateMachine.AddState(errorState);
		_stateMachine.AddUniversalTransition(stuckError);

		_stateMachine.AddState(patrolState);
		_stateMachine.AddState(findPatrolState, searchToPatrol);

		_stateMachine.CurrentState = findPatrolState;
	}

	private void FixedUpdate()
	{
		_stateMachine?.Run();
	}

	private void LateUpdate()
	{
		UpdateDebugDisplay();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			// game over
			MessageLogger.LogDebugMessage(LogType.Actor, "Tractor hit player! Game over!");
			_gameplayManager.GameLost();
		}
	}

	private void UpdateDebugDisplay()
	{
		var stateName = _stateMachine.CurrentState.ToString();

		if (!_debugDisplay.text.Equals(stateName))
		{
			_debugDisplay.text = stateName;
		}
	}
}
