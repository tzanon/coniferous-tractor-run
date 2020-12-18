using UnityEngine;

public class CollectibleStatus
{
	public Collectible[] RemainingCollectibles { get; private set; }
	public Vector3 LastTakenPosition { get; private set; }
	public Vector3[] LastTakenNavpointPositions { get; private set; }

	public bool CollectibleTaken
	{
		get => LastTakenPosition != Vector3.back && LastTakenNavpointPositions.Length > 0;
	}

	public CollectibleStatus(Collectible[] remainingCollectibles)
	{
		RemainingCollectibles = remainingCollectibles;
		LastTakenPosition = Vector3.back;
		LastTakenNavpointPositions = new Vector3[0];
	}

	public CollectibleStatus(Collectible[] remainingCollectibles, Vector3 lastTakenPosition, Vector3[] lastTakenNavpointPositions)
	{
		RemainingCollectibles = remainingCollectibles;
		LastTakenPosition = lastTakenPosition;
		LastTakenNavpointPositions = lastTakenNavpointPositions;
	}
}
