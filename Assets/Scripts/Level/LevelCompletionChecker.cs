using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCompletionChecker : MonoBehaviour
{
	[SerializeField] private GameplayManager _gameManager;
	[SerializeField] private TilemapManager _tilemapManager;
	[SerializeField] private NavigationMap _navMap;
	[SerializeField] private MovementDriver _driver;

	[SerializeField] private Transform _rejectionDestination;

	/// <summary>
	/// Node player will be sent to if he doesn't have all the collectibles
	/// </summary>
	public Vector3Int RejectionDestNode { get => _tilemapManager.CellOfPosition(_rejectionDestination.position); }

	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.CompareTag("Player"))
		{
			CheckIfLevelComplete(coll.GetComponent<Player>());
		}
	}

	private void CheckIfLevelComplete(Player player)
	{
		// if game not done, force player back into level
		if (_gameManager.NumCollectibles > 0)
		{
			// TODO: display "must get all items" text
			MessageLogger.LogGameplayMessage("Must collect all items before leaving!", LogLevel.Debug);

			_driver.MovePlayerToNode(player, RejectionDestNode);
		}
		else // game done, move player out of level
		{
			// TODO: display "game won" text
			MessageLogger.LogGameplayMessage("You're winner!!", LogLevel.Debug);

		}
	}

}
