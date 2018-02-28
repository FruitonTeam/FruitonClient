using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sets the application frame rate and loads login scene.
/// </summary>
public class InitialScene : MonoBehaviour {

    static readonly int TARGET_FRAME_RATE = 60;

	void Start ()
    {
        Application.targetFrameRate = TARGET_FRAME_RATE;
		SceneManager.LoadScene(Scenes.LOGIN_SCENE);
	}
}
