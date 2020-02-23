using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
	// fields
	[SerializeField] private bool _debugMode = false;

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

	private void Start()
	{
		_collectibles = new List<Collectible>(FindObjectsOfType<Collectible>());

		if (_debugMode)
		{
			Debug.Log("Number of collectibles in play:" + _collectibles.Count);
		}
	}

	public Vector2 PositionOfCollectible(int i)
	{
		return _collectibles[i].transform.position;
	}

	private bool RemoveCollectibleFromList(Collectible collectible)
	{
		bool status = true;



		return status;
	}

	public void DeleteCollectible(Collectible toDelete)
	{
		// check if collectible exists
		if (_collectibles.Count <= 0)
		{
			Debug.LogError("No collectibles left to delete");
			return;
		}

		if (toDelete == null || !_collectibles.Contains(toDelete))
		{
			Debug.LogError("Trying to delete either nothing or something not originally in the list");
			return;
		}

		if (!_collectibles.Remove(toDelete))
		{
			Debug.LogError("Could not delete collectible");
			return;
		}

		Destroy(toDelete.gameObject);

		if (_collectibles.Count <= 0)
		{
			if (_debugMode)
			{
				Debug.Log("Collected all apples!");
			}

			// remove barrier preventing exit of the forest
			Destroy(_barrier);

			// notify tractor?
		}
	}

	// when player exits forest
	private void GameWon()
	{

	}

}
