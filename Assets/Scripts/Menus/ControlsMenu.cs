using UnityEngine;

public class ControlsMenu : MonoBehaviour
{
	public void PressMainMenu()
	{
		//SceneLoadController.ChangeScene("MainMenu");
		SceneLoadController.ChangeMenu("MainMenu");
	}
}
