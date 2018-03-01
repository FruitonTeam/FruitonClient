using System.Collections.Generic;
using Battle.Model;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using TeamsManagement;
using UI.Chat;
using UI.Notification;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class MainPanel : MainMenuPanel
    {
        public Button ChatButton;
        public Button PlayOnlineButton;
        public Button MarketButton;
        public Button TodoButton;

        private void Start()
        {
            if (GameManager.Instance.connectionMode != ConnectionMode.Offline)
            {
                ConnectButton.gameObject.SetActive(false);
            }
        }

        public void TeamManagementContinue()
        {
            Scenes.Load(Scenes.TEAMS_MANAGEMENT_SCENE, Scenes.TEAM_MANAGEMENT_STATE, TeamManagementState.TEAM_MANAGEMENT.ToString());
        }

        /// <summary>
        /// Switch to team management scene with team select mode for online play.
        /// </summary>
        public void TeamSelectionContinueOnline()
        {
            TeamSelectionContinue(BattleType.OnlineBattle, TeamManagementState.ONLINE_CHOOSE);
        }

        /// <summary>
        /// Switch to team management scene with team select mode for local duel.
        /// </summary>
        public void TeamSelectionContinueOffline()
        {
            TeamSelectionContinue(BattleType.LocalDuel, TeamManagementState.LOCAL_CHOOSE_FIRST);
        }

        /// <summary>
        /// Switch to team management scene with team select mode for game vs AI.
        /// </summary>
        public void TeamSelectionContinueAI()
        {
            TeamSelectionContinue(BattleType.AIBattle, TeamManagementState.AI_CHOOSE);
        }

        /// <summary>
        /// Switch to team management scene with team management mode.
        /// </summary>
        private void TeamSelectionContinue(BattleType battleType, TeamManagementState state)
        {
            var parameters = new Dictionary<string, string>
            {
                {Scenes.TEAM_MANAGEMENT_STATE, state.ToString()},
                {Scenes.BATTLE_TYPE, battleType.ToString()}
            };
            Scenes.Load(Scenes.TEAMS_MANAGEMENT_SCENE, parameters);
        }

        /// <summary>
        /// Loads battle scene in tutorial mode.
        /// </summary>
        public void LoadTutorial()
        {
            var param = new Dictionary<string, string>
            {
                {Scenes.BATTLE_TYPE, BattleType.TutorialBattle.ToString()},
                {Scenes.GAME_MODE, GameMode.Standard.ToString()}
            };
            Scenes.Load(Scenes.BATTLE_SCENE, param);
        }

        public static void Logout()
        {
            GameManager.Instance.Logout();
            ConnectionHandler.Instance.Logout();
            PlayerPrefs.SetString("username", "");
            PlayerPrefs.SetString("userpassword", "");
            PlayerPrefs.SetInt("stayloggedin", 0);
            
            ChatController.Instance.Clear();
            
            Scenes.Load(Scenes.LOGIN_SCENE);
        }

        public void OpenMarketOnWeb()
        {
            ConnectionHandler.Instance.OpenUrlAuthorized("bazaar");
        }
        
        public void EnableOnlineFeatures()
        {
            ChatButton.interactable = true;
            PlayOnlineButton.interactable = true;
            MarketButton.interactable = true;
            TodoButton.interactable = true;
        }

        public void DisableOnlineFeatures()
        {
            ChatButton.interactable = false;
            PlayOnlineButton.interactable = false;
            MarketButton.interactable = false;
            TodoButton.interactable = false;
        }

        /// <summary>
        /// Try connecting to the server in offline mode.
        /// </summary>
        public void Connect()
        {
            PanelManager panelManager = PanelManager.Instance;
            panelManager.ShowLoadingIndicator();
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                panelManager.HideLoadingIndicator();
                panelManager.ShowErrorMessage("No internet connection.");
            }
            else
            {
                AuthenticationHandler.Instance.LoginBasic(GameManager.Instance.UserName, GameManager.Instance.UserPassword);
            } 
        }

    }
}
