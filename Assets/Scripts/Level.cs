using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
	public bool debugMode = false;

	private List<Collectible> collectibles;

	public GameObject barrier;

	public Vector2[] PositionsOfCollectibles
	{
		get
		{
			Vector2[] positions = new Vector2[collectibles.Count];
			for (int i = 0; i < collectibles.Count; i++)
			{
				positions[i] = collectibles[i].transform.position;
			}
			return positions;
		}
	}

	public Camera playerCam, tractorCam, worldCam;
	private Camera[] cameras;

	private void Start()
	{
		collectibles = new List<Collectible>(FindObjectsOfType<Collectible>());
		cameras = new Camera[] { playerCam, tractorCam, worldCam };

		if (debugMode)
		{
			Debug.Log("Num collectibles in play:" + collectibles.Count);
		}

		SetCameraPlayer();
	}

	public Vector2 PositionOfCollectible(int i)
	{
		return collectibles[i].transform.position;
	}

	public void DeleteCollectible(Collectible toDelete)
	{
		if (collectibles.Count <= 0)
		{
			Debug.LogError("No collectibles left to delete");
			return;
		}

		if (toDelete == null || !collectibles.Contains(toDelete))
		{
			Debug.LogError("Trying to delete either nothing or something not originally in the list");
			return;
		}

		if (!collectibles.Remove(toDelete))
		{
			Debug.LogError("Could not delete collectible");
			return;
		}

		Destroy(toDelete.gameObject);

		if (collectibles.Count <= 0)
		{
			if (debugMode)
			{
				Debug.Log("Collected all apples!");
			}

			// TODO: remove barrier preventing exit of the forest
			Destroy(barrier);

			// notify tractor?
		}
	}

	// when player exits forest
	private void GameWon()
	{

	}

	public void SetCameraPlayer()
	{
		EnableCamera(playerCam);
	}

	public void SetCameraTractor()
	{
		EnableCamera(tractorCam);
	}

	public void SetCameraWorld()
	{
		EnableCamera(worldCam);
	}

	private void EnableCamera(Camera cam)
	{
		foreach (Camera c in cameras)
		{
			if (c == cam)
			{
				c.gameObject.SetActive(true);
				c.tag = "MainCamera";
			}
			else
			{
				c.tag = "Untagged";
				c.gameObject.SetActive(false);
			}
		}
	}

}
