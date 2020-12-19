using System;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour, IObservable<CollectibleStatus>
{
	/* fields */

	private List<Collectible> _collectibles;
	[SerializeField] private GameObject _barrier;

	private List<IObserver<CollectibleStatus>> _observers;

	/* properties */

	/// <summary>
	/// Returns array of positions of all existing collectibles
	/// </summary>
	public Vector3[] PositionsOfCollectibles
	{
		get
		{
			var positions = new Vector3[_collectibles.Count];
			for (var i = 0; i < _collectibles.Count; i++)
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

	private void Awake()
	{
		_observers = new List<IObserver<CollectibleStatus>>(capacity: 5);
	}

	private void Start()
	{
		_collectibles = new List<Collectible>(FindObjectsOfType<Collectible>());
		//MessageLogger.LogDebugMessage(LogType.Game, "Number of collectibles in level: {0}", _collectibles.Count);
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
	public bool CanDelete(Collectible collectible)
	{
		if (_collectibles.Count <= 0)
		{
			MessageLogger.LogDebugMessage(LogType.Game, "Cannot delete collectible from empty list");
			return false;
		}

		if (collectible == null || !_collectibles.Contains(collectible))
		{
			MessageLogger.LogDebugMessage(LogType.Game, "Cannot delete nothing nor something not in the list");
			return false;
		}

		return true;
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
			MessageLogger.LogErrorMessage(LogType.Game, "Could not delete collectible");
			return;
		}

		var collectibleStatus = new CollectibleStatus(_collectibles.ToArray(), toDelete.transform.position, toDelete.NavpointPositions);
		UpdateObservers(collectibleStatus);

		Destroy(toDelete.gameObject);
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

	private void UpdateObservers(CollectibleStatus status)
	{
		foreach (var observer in _observers)
			observer.OnNext(status);
	}

	public IDisposable Subscribe(IObserver<CollectibleStatus> observer)
	{
		if (!_observers.Contains(observer))
		{
			MessageLogger.LogDebugMessage(LogType.Game, "{0} subscribed to Gameplay Manager", observer);
			_observers.Add(observer);
			var status = new CollectibleStatus(_collectibles.ToArray());
			observer.OnNext(status);
		}

		return new Unsubscriber<CollectibleStatus>(_observers, observer);
	}
}
