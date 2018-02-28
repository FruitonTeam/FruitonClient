using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sets the application frame rate and loads login scene.
/// </summary>
public class InitialScene : MonoBehaviour {

	void Start ()
    {
        Application.targetFrameRate = 60;
		SceneManager.LoadScene(Scenes.LOGIN_SCENE);
	}
}
