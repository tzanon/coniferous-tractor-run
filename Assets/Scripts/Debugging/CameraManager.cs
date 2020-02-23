using UnityEngine;

public class CameraManager : MonoBehaviour
{
	public bool debugMode;

	[SerializeField] private Camera _playerCam, _tractorCam, _worldCam;
	private Camera[] _cameras;

	void Start()
	{
		_cameras = new Camera[] { _playerCam, _tractorCam, _worldCam };
		SetCameraPlayer();
	}

	public void SetCameraPlayer()
	{
		EnableCamera(_playerCam);
	}

	public void SetCameraTractor()
	{
		EnableCamera(_tractorCam);
	}

	public void SetCameraWorld()
	{
		EnableCamera(_worldCam);
	}

	private void EnableCamera(Camera cam)
	{
		foreach (Camera c in _cameras)
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
