using System.Collections.Generic;
using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class MainPanel : MainMenuPanel
    {
        public Button ChatButton;
        public Button MoneyButton;
        public Button PlayOnlineButton;
        public Button MarketButton;
        public Button TodoButton;

        public void OnlineContinue()
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Online);
        }

        public void FarmersMarketContinue()
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.FarmersMarket);
        }

        public void TeamManagementContinue()
        {
            Scenes.Load(Scenes.TEAMS_MANAGEMENT_SCENE, FruitonTeamsManager.TEAM_MANAGEMENT_STATE, bool.TrueString);
        }

        public void TeamSelectionContinueOnline()
        {
            TeamSelectionContinue(true);
        }

        public void TeamSelectionContinueOffline()
        {
            TeamSelectionContinue(false);
        }

        private void TeamSelectionContinue(bool online)
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add(Scenes.TEAM_MANAGEMENT_STATE, bool.FalseString);
            parameters.Add(Scenes.ONLINE, online.ToString());
            Scenes.Load(Scenes.TEAMS_MANAGEMENT_SCENE, parameters);
        }

        public void ChangeToChatScene()
        {
            Scenes.Load(Scenes.CHAT_SCENE);
        }

        public void LoadBattle()
        {
            if (GameManager.Instance.CurrentFruitonTeam != null)
            {
                Scenes.Load(Scenes.BATTLE_SCENE, Scenes.ONLINE, Scenes.GetParam(Scenes.ONLINE));
            }
        }

        public void Logout()
        {
            ConnectionHandler.Instance.Logout();
            PlayerPrefs.SetString("username", "");
            PlayerPrefs.SetString("userpassword", "");
            PlayerPrefs.SetInt("stayloggedin", 0);
            PanelManager.Instance.SwitchPanels(MenuPanel.Login);
        }
        
        public void EnableOnlineFeatures()
        {
            ChatButton.enabled = true;
            MoneyButton.enabled = true;
            PlayOnlineButton.enabled = true;
            MarketButton.enabled = true;
            TodoButton.enabled = true;
        }

        public void DisableOnlineFeatures()
        {
            ChatButton.enabled = false;
            MoneyButton.enabled = false;
            PlayOnlineButton.enabled = false;
            MarketButton.enabled = false;
            TodoButton.enabled = false;
        }

    }
}
