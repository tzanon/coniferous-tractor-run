using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
	/* fields */

	private List<Collectible> _collectibles;
	[SerializeField] private GameObject _barrier;

	/* properties */

	/// <summary>
	/// Returns array of positions of all existing collectibles
	/// </summary>
	public Vector2[] PositionsOfCollectibles
	{
		get
		{
			Vector2[] positions = new Vector2[_collectibles.Count];
			for (int i = 0; i < _collectibles.Count; i++)
			{
				positions[i] = _collectibles[i].transform.position;
			}
			return positions;
		}
	}

	/// <summary>
	/// Number of collectibles currently in the level
	/// </summary>
	public int NumCollectibles { get => _collectibles.Count; }

	/// <summary>
	/// Whether the level has no collectibles left or not
	/// </summary>
	public bool CollectiblesEmpty { get => _collectibles.Count <= 0; }

	/* Methods */

	private void Start()
	{
		_collectibles = new List<Collectible>(FindObjectsOfType<Collectible>());
		MessageLogger.LogGameplayMessage("Number of collectibles in level: ", LogLevel.Debug, _collectibles.Count);
	}

	/// <summary>
	/// Returns position of collectible at the given index
	/// </summary>
	/// <param name="i">Index of collectible</param>
	/// <returns>Vector3 representing collectible's position</returns>
	public Vector3 PositionOfCollectible(int i) => _collectibles[i].transform.position;

	/// <summary>
	/// Checks if a collectible can be removed from the level's collectible list
	/// </summary>
	/// <param name="collectible">collectible to check</param>
	/// <returns>True if collectible can be removed from the list, false if not</returns>
	private bool CanDelete(Collectible collectible)
	{
		if (_collectibles.Count <= 0)
		{
			MessageLogger.LogGameplayMessage("Trying to delete collectible from empty list", LogLevel.Error);
			return false;
		}

		if (collectible == null || !_collectibles.Contains(collectible))
		{
			MessageLogger.LogGameplayMessage(
				"Trying to delete either nothing or something not in the list",
				LogLevel.Error);
			return false;
		}

		return true;
	}

	// TODO: get rid of this, have barrier trigger auto player movement out of level
	/// <summary>
	/// Removes the invisible barrier at the forest entrance, allowing the player to exit
	/// </summary>
	public void RemoveBarrier()
	{
		if (_barrier)
		{
			Destroy(_barrier);
			MessageLogger.LogGameplayMessage("Barrier removed", LogLevel.Debug);
		}
		else
		{
			MessageLogger.LogGameplayMessage("Already deleted barrier", LogLevel.Error);
		}
	}

	/// <summary>
	/// Removes a collectible from the level and the tracking list
	/// </summary>
	/// <param name="toDelete">Collectible to remove</param>
	public void DeleteCollectible(Collectible toDelete)
	{
		if (!CanDelete(toDelete)) return;

		// remove the collectible from tracking list
		if (!_collectibles.Remove(toDelete))
		{
			MessageLogger.LogGameplayMessage("Could not delete collectible", LogLevel.Error);
			return;
		}

		Destroy(toDelete.gameObject);
		if (CollectiblesEmpty) RemoveBarrier();
	}

	/// <summary>
	/// Ends game and displays winning text
	/// </summary>
	public void GameWon()
	{

	}

	/// <summary>
	/// Ends game and displays losing text
	/// </summary>
	public void GameLost()
	{

	}
}
