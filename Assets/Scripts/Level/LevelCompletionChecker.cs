using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCompletionChecker : MonoBehaviour
{
	/* fields */

	[SerializeField] private GameplayManager _gameManager;
	[SerializeField] private TilemapManager _tilemapManager;

	[SerializeField] private Transform _rejectDestination;
	[SerializeField] private Transform _acceptDestination;

	/* properties */

	/// <summary>
	/// Whether player has crossed the checker's boundaries
	/// </summary>
	public bool ContainsPlayer { get; private set; }

	/// <summary>
	/// Node player will be sent to if he doesn't have all the collectibles
	/// </summary>
	private Vector3Int RejectDestNode { get => _tilemapManager.CellOfPosition(_rejectDestination.position); }

	/// <summary>
	/// Node player will be sent to if he has all the collectibles
	/// </summary>
	private Vector3Int AcceptDestNode { get => _tilemapManager.CellOfPosition(_acceptDestination.position); }

	public Vector3Int CurrentDestNode { get; private set; }

	/* methods */

	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.CompareTag("Player"))
		{
			CheckIfLevelComplete(coll.GetComponent<Player>());
			ContainsPlayer = true;

			MessageLogger.LogVerboseMessage(LogType.Game, "Player entered leave area");
		}
	}

	private void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.CompareTag("Player"))
		{
			ContainsPlayer = false;
			MessageLogger.LogVerboseMessage(LogType.Game, "Player exited leave area");
		}
	}

	private void CheckIfLevelComplete(Player player)
	{
		// TODO: set dest node for auto movement to use

		// if game not done, force player back into level
		if (_gameManager.NumCollectibles > 0)
		{
			// TODO: display "must get all items" text
			// reference separate script for displaying messages
			MessageLogger.LogDebugMessage(LogType.Game, "Must collect all items before leaving!");
			CurrentDestNode = RejectDestNode;
		}
		else // game done, move player out of level
		{
			// TODO: display "game won" text
			MessageLogger.LogDebugMessage(LogType.Game, "You're winner!!");
			CurrentDestNode = AcceptDestNode;
		}
	}

}
