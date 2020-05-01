using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCompletionChecker : MonoBehaviour
{
	[SerializeField] private GameplayManager gameManager;

	/// <summary>
	/// Node player will be sent to if he doesn't have all the collectibles
	/// </summary>
	[SerializeField] private Vector3Int rejectionDestNode;


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
		if (gameManager.NumCollectibles > 0)
		{
			// TODO: display "must get all items" text
			MessageLogger.LogGameplayMessage("Must collect all items before leaving!", LogLevel.Debug);


			// MovementDriver.MoveToDest(player, rejectionDestNode);
		}
		else // game done, move player out of level
		{
			// TODO: display "game won" text
			MessageLogger.LogGameplayMessage("You're winner!!", LogLevel.Debug);

		}

	}

}
