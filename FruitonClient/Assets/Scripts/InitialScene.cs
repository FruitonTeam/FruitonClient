﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialScene : MonoBehaviour {

	void Start ()
    {
		SceneManager.LoadScene(Scenes.LOGIN_SCENE);
	}
}
