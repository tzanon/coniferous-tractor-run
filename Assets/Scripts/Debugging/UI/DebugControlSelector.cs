using UnityEngine;

public class DebugControlSelector : MonoBehaviour
{
	public GameObject visualControls, aiControls, cameraControls;

	private GameObject[] controls;

	private void Awake()
	{
		controls = new GameObject[] { visualControls, aiControls, cameraControls };
	}

	public void ShowVisualControls()
	{
		ShowControl(visualControls);
	}

	public void ShowAIControls()
	{
		ShowControl(aiControls);
	}

	public void ShowCameraControls()
	{
		ShowControl(cameraControls);
	}

	private void ShowControl(GameObject control)
	{
		foreach (GameObject ctrl in controls)
		{
			if (ctrl == control)
				ctrl.SetActive(true);
			else
				ctrl.SetActive(false);
		}
	}

}
