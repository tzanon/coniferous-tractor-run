using UnityEngine;

public class MainMenu : MonoBehaviour
{
	[SerializeField] CanvasGroup loadScreen;

	public void Awake()
	{
		loadScreen.gameObject.SetActive(false);
	}

	public void PressPlay()
	{
		Debug.Log("Going to game");
		SceneLoadController.ChangeScene("DevTestLevel");
	}
}
