using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
	private void Start()
	{
		SceneManager.LoadSceneAsync(SceneLoadController.SceneToLoad);
	}
}
