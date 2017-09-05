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
            Scenes.Load(Scenes.TEAMS_MANAGEMENT_SCENE, FruitonTeamsManager.TEAM_MANAGEMENT_STATE, bool.TrueString);
        }

        public void PantryContinue()
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Pantry);
        }

        public void TeamSelectionContinue()
        {
            Scenes.Load(Scenes.TEAMS_MANAGEMENT_SCENE, "teamManagementState", bool.FalseString);
        }

        public void ChangeToChatScene()
        {
            Scenes.Load(Scenes.CHAT_SCENE);
        }

        public void LoadBattle()
        {
            if (GameManager.Instance.CurrentFruitonTeam != null)
            {
                Scenes.Load(Scenes.BATTLE_SCENE);
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
