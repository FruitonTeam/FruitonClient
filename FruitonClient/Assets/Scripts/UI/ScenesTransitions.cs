﻿using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class ScenesTransitions : MonoBehaviour {

        public void LoadMainMenu()
        {
            Scenes.Load(Scenes.MAIN_MENU);
        }
    }
}
