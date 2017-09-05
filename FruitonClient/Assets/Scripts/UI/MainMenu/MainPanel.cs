using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.MainMenu
{
    public class MainPanel : MainMenuPanel
    {
        public void OnlineContinue()
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Online);
        }

        public void OfflineContinue()
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Offline);
        }

        public void FarmersMarketContinue()
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.FarmersMarket);
        }

        public void StorageContinue()
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Storage);
        }

        public void TeamManagementContinue()
        {
            GameManager.Instance.GameState = GameState.TEAM_MANAGEMENT;
            SceneManager.LoadScene(Scenes.TEAMS_MANAGEMENT_SCENE);
        }

        public void PantryContinue()
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Pantry);
        }

        public void TeamSelectionContinue()
        {
            GameManager.Instance.GameState = GameState.TEAM_SELECTION;
            SceneManager.LoadScene(Scenes.TEAMS_MANAGEMENT_SCENE);
        }

        public void ChangeToChatScene()
        {
            SceneManager.LoadScene(Scenes.CHAT_SCENE);
        }

        public void LoadBattle()
        {
            if (GameManager.Instance.CurrentFruitonTeam != null)
            {
                SceneManager.LoadScene(Scenes.BATTLE_SCENE);
            }
        }

        public void Logout()
        {
            PlayerPrefs.SetString("username", "");
            PlayerPrefs.SetString("userpassword", "");
            PlayerPrefs.SetInt("stayloggedin", 0);
            PanelManager.Instance.SwitchPanels(MenuPanel.Login);
        }


    }
}
