using UnityEngine;

/// <summary>
/// 
/// </summary>
public class DebugControlSelector : MonoBehaviour
{
	// set in inspector
	[SerializeField] private GameObject _visualControls, _aiControls, _cameraControls, _loggerControls;

	public GameObject ActiveControlMenu { get; private set; }

	private void Awake()
	{
		_visualControls.SetActive(false);
		_aiControls.SetActive(false);
		_cameraControls.SetActive(false);
		_loggerControls.SetActive(false);
	}

	// Individual display methods to be called from buttons
	public void ShowVisualControls() => ShowControl(_visualControls);
	public void ShowAIControls() => ShowControl(_aiControls);
	public void ShowCameraControls() => ShowControl(_cameraControls);
	public void ShowLoggerControls() => ShowControl(_loggerControls);

	/// <summary>
	/// Hides the currently active control and displays the given one
	/// </summary>
	/// <param name="control">Control menu to display</param>
	private void ShowControl(GameObject control)
	{
		if (!control) return;
		if (ActiveControlMenu) ActiveControlMenu.SetActive(false);
		
		control.SetActive(true);
		ActiveControlMenu = control;
	}

}
