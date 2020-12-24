using UnityEngine;

public class MainMenu : MonoBehaviour
{
	public void PressPlay()
	{
		Debug.Log("Going to game");
		SceneLoadController.ChangeScene("DevTestLevel");
	}

	public void PressControls()
	{
		SceneLoadController.ChangeMenu("ControlsMenu");
	}

	public void PressQuit()
	{
		Application.Quit();
	}
}
