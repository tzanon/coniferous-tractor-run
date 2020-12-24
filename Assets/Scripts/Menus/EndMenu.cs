﻿using UnityEngine;

public class EndMenu : MonoBehaviour
{
	public void PressPlay()
	{
		Debug.Log("Going to game");
		SceneLoadController.ChangeScene("DevTestLevel");
	}

	public void PressMainMenu()
	{
		SceneLoadController.ChangeMenu("MainMenu");
	}
}
