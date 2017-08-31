using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class ScenesTransitions : MonoBehaviour {

        public void LoadMainMenu()
        {
            SceneManager.LoadScene(Scenes.MAIN_MENU);
        }
    }
}
