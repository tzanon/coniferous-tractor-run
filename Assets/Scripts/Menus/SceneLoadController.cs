using UnityEngine.SceneManagement;

public static class SceneLoadController
{
	public static string SceneToLoad { get; set; }

	/// <summary>
	/// Goes to Loading Screen and sets scene for it to load
	/// </summary>
	/// <param name="sceneName">Scene to go to after loading screen</param>
	public static void ChangeScene(string sceneName)
	{
		SceneToLoad = sceneName;
		SceneManager.LoadScene("LoadingScreen");
	}

	/// <summary>
	/// Load scene without loading screen, usually a menu
	/// </summary>
	/// <param name="menuName">Name of menu scene</param>
	public static void ChangeMenu(string menuName)
	{
		SceneManager.LoadSceneAsync(menuName);
	}
}
