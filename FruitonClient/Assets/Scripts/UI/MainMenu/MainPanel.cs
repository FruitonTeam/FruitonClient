using System.Collections.Generic;
using Networking;
using UI.Chat;
using UI.Notification;
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

        /*public void ChangeToChatScene()
        {
            Scenes.Load(Scenes.CHAT_SCENE);
        }*/

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
            
            ChatController.Instance.Clear();
            FeedbackNotificationManager.Instance.Clear();
            
            Scenes.Load(Scenes.LOGIN_SCENE);
        }

        public void OpenMarketOnWeb()
        {
            ConnectionHandler.Instance.OpenUrlAuthorized("bazaar");
        }
        
        public void EnableOnlineFeatures()
        {
            ChatButton.interactable = true;
            MoneyButton.interactable = true;
            PlayOnlineButton.interactable = true;
            MarketButton.interactable = true;
            TodoButton.interactable = true;
        }

        public void DisableOnlineFeatures()
        {
            ChatButton.interactable = false;
            MoneyButton.interactable = false;
            PlayOnlineButton.interactable = false;
            MarketButton.interactable = false;
            TodoButton.interactable = false;
        }

    }
}
