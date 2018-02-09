using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
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
            TeamSelectionContinue(BattleType.OnlineBattle);
        }

        public void TeamSelectionContinueOffline()
        {
            TeamSelectionContinue(BattleType.OfflineBattle);
        }

        public void TeamSelectionContinueAI()
        {
            TeamSelectionContinue(BattleType.AIBattle);
        }

        private void TeamSelectionContinue(BattleType battleType)
        {
            var parameters = new Dictionary<string, string>
            {
                {Scenes.TEAM_MANAGEMENT_STATE, bool.FalseString},
                {Scenes.BATTLE_TYPE, battleType.ToString()}
            };
            Scenes.Load(Scenes.TEAMS_MANAGEMENT_SCENE, parameters);
        }

        public void LoadBattle()
        {
            if (GameManager.Instance.CurrentFruitonTeam != null)
            {
                Scenes.Load(Scenes.BATTLE_SCENE, Scenes.BATTLE_TYPE, Scenes.GetParam(Scenes.BATTLE_TYPE));
            }
        }

        public void LoadTutorial()
        {
            var param = new Dictionary<string, string>
            {
                {Scenes.BATTLE_TYPE, BattleType.TutorialBattle.ToString()},
                {Scenes.GAME_MODE, FindGame.Types.GameMode.Standard.ToString()}
            };
            Scenes.Load(Scenes.BATTLE_SCENE, param);
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
