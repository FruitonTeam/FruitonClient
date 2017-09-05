using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.MainMenu
{
    public class FridgePanel : MainMenuPanel
    {
        public override bool SetPanelActive(bool toggle)
        {
            if (toggle)
            {
                Debug.Log("Initializing game manager");
                Debug.Log("Game manager initialized, fridge will be loaded");
                Scenes.Load(Scenes.TEAMS_MANAGEMENT_SCENE);
                Debug.Log("Fridge loaded");
            }
            return true;
        }
    }
}
