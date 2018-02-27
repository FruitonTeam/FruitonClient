using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialScene : MonoBehaviour {

	void Start ()
    {
        Application.targetFrameRate = 60;
		SceneManager.LoadScene(Scenes.LOGIN_SCENE);
	}
}
