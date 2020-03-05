using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
	// fields
	private List<Collectible> _collectibles;
	[SerializeField] private GameObject _barrier;

	// properties
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

	public int NumCollectibles { get => _collectibles.Count; }

	public bool CollectiblesEmpty { get => _collectibles.Count <= 0; }

	private void Start()
	{
		_collectibles = new List<Collectible>(FindObjectsOfType<Collectible>());
		//LogDebugMessage("Number of collectibles in play:" + _collectibles.Count);
		MessageLogger.LogGameplayMessage("Number of collectibles in play: ", LogLevel.Debug, _collectibles.Count);
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
			Debug.LogError("Trying to delete collectible from empty list");
			return false;
		}

		if (collectible == null || !_collectibles.Contains(collectible))
		{
			Debug.LogError("Trying to delete either nothing or something not originally in the list");
			return false;
		}

		return true;
	}

	/// <summary>
	/// Removes the invisible barrier at the forest entrance, allowing the player to exit
	/// </summary>
	public void RemoveBarrier()
	{
		if (_barrier)
		{
			Destroy(_barrier);
			//LogDebugMessage("Barrier removed");
			MessageLogger.LogGameplayMessage("Barrier removed", LogLevel.Debug);
		}
		else
		{
			//LogErrorMessage("Already deleted barrier");
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

		// remove
		if (!_collectibles.Remove(toDelete))
		{
			//LogErrorMessage("Could not delete collectible");
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
