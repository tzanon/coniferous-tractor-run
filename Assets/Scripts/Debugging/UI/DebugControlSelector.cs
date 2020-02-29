using UnityEngine;

public class DebugControlSelector : ScriptBase
{
	[SerializeField] private GameObject _visualControls, _aiControls, _cameraControls;

	private GameObject[] _controls;

	private void Awake()
	{
		_controls = new GameObject[] { _visualControls, _aiControls, _cameraControls };
	}

	public void ShowVisualControls()
	{
		ShowControl(_visualControls);
	}

	public void ShowAIControls()
	{
		ShowControl(_aiControls);
	}

	public void ShowCameraControls()
	{
		ShowControl(_cameraControls);
	}

	private void ShowControl(GameObject control)
	{
		foreach (GameObject ctrl in _controls)
		{
			if (ctrl == control)
				ctrl.SetActive(true);
			else
				ctrl.SetActive(false);
		}
	}

}
