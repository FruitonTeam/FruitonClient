using System;
using System.Collections.Generic;
using System.Linq;
using Battle.Model;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using TeamsManagement;
using UI.Notification;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Chat
{
    /// <summary>
    /// Handles challenge window and challenge request logic
    /// </summary>
    public class ChallengeController : MonoBehaviour, IOnMessageListener
    {
        /// <summary>
        /// Stores information about incoming challenges
        /// </summary>
        public class ChallengeData
        {
            /// <summary>
            /// Incoming challenge object
            /// </summary>
            public readonly Challenge Challenge;
            /// <summary>
            /// Id of notification informing player about the challenge
            /// </summary>
            public readonly int NotificationId;

            public ChallengeData(Challenge challenge, int notificationId)
            {
                Challenge = challenge;
                NotificationId = notificationId;
            }
        }

        private static readonly string CHALLENGE_FRIEND_TEXT = "You are about to challenge {0}.\nChoose a game mode:";
        private static readonly string CHALLENGE_NEARBY_PLAYER_TEXT = "To challenge {0}\nchoose a game mode:";
        private static readonly string CHALLENGE_PLAYER_BUSY_TEXT = "You cannot challenge {0} right now. Wait for them to return to main menu then try again.";

        private const int CHALLENGE_MODE_STANDARD_PREMADE = 0;
        private const int CHALLENGE_MODE_STANDARD_DRAFT = 1;
        private const int CHALLENGE_MODE_LMS_PREMADE = 2;
        private const int CHALLENGE_MODE_LMS_DRAFT = 3;

        public static ChallengeController Instance { get; private set; }

        /// <summary>
        /// True if player is currently in the process of initiating challenge
        /// </summary>
        public bool IsChallengeActive { get; private set; }

        public GameObject ChallengePanel;
        public GameObject ChallengeButtons;
        public Texture2D ChallengeImage;
        public Text ChallengeText;

        /// <summary>
        /// Outgoing challenge object
        /// </summary>
        private Challenge myChallenge;
        /// <summary>
        /// Incoming challenge object that player agreed to
        /// </summary>
        private Challenge currentEnemyChallenge;
        /// <summary>
        /// List of incoming challenges
        /// </summary>
        public readonly List<ChallengeData> EnemyChallenges = new List<ChallengeData>();

        private void Awake()
        {
            if (Instance == null)
            {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// Shows challenge window, enables or disables buttons based on selected player status
        /// </summary>
        public void Show()
        {
            ChallengePanel.SetActive(true);
            var challengeText = CHALLENGE_FRIEND_TEXT;
            ChallengeButtons.SetActive(ChatController.Instance.IsSelectedPlayerInMenu);
            if (ChatController.Instance.IsSelectedPlayerInMenu)
            {
                challengeText = ChatController.Instance.IsSelectedPlayerFriend
                    ? CHALLENGE_FRIEND_TEXT
                    : CHALLENGE_NEARBY_PLAYER_TEXT;
            }
            else
            {
                challengeText = CHALLENGE_PLAYER_BUSY_TEXT;
            }
            ChallengeText.text = String.Format(
                challengeText,
                ChatController.Instance.SelectedPlayerLogin);
        }

        /// <summary>
        /// Hides challenge window
        /// </summary>
        public void Hide()
        {
            ChallengePanel.SetActive(false);
        }

        /// <summary>
        /// Updates challenge window to display current information for selected player
        /// </summary>
        public void Refresh()
        {
            if (ChallengePanel.activeInHierarchy)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        /// <summary>
        /// Initiates challenge in selected mode
        /// </summary>
        /// <param name="modeId">id of selected mode</param>
        public void OnChallengeModeChosen(int modeId)
        {
            switch (modeId)
            {
                case CHALLENGE_MODE_STANDARD_PREMADE:
                    InitiateChallenge(GameMode.Standard, PickMode.StandardPick);
                    break;
                case CHALLENGE_MODE_STANDARD_DRAFT:
                    InitiateChallenge(GameMode.Standard, PickMode.Draft);
                    break;
                case CHALLENGE_MODE_LMS_PREMADE:
                    InitiateChallenge(GameMode.LastManStanding, PickMode.StandardPick);
                    break;
                case CHALLENGE_MODE_LMS_DRAFT:
                    InitiateChallenge(GameMode.LastManStanding, PickMode.Draft);
                    break;
            }
        }

        public void OnMessage(WrapperMessage message)
        {
            switch (message.MessageCase)
            {
                case WrapperMessage.MessageOneofCase.Challenge:
                    OnChallengeRequest(message.Challenge);
                    break;
                case WrapperMessage.MessageOneofCase.ChallengeResult:
                    OnChallengeResult(message.ChallengeResult);
                    break;
                case WrapperMessage.MessageOneofCase.RevokeChallenge:
                    OnChallengeRevoked(message.RevokeChallenge);
                    break;
                default:
                    throw new InvalidOperationException("Unknown message type " + message.MessageCase);
            }
        }

        /// <summary>
        /// Removes all pending challenges from notifications and sends rejection message to server
        /// </summary>
        private void CancelAllChallengeNotifications()
        {
            FeedbackNotificationManager.Instance.RemoveNotifications(EnemyChallenges.Select(cd => cd.NotificationId).ToArray());
            foreach (ChallengeData enemyChallenge in EnemyChallenges)
            {
                SendChallengeResult(enemyChallenge.Challenge.ChallengeFrom, false);
            }
            EnemyChallenges.Clear();
        }

        /// <summary>
        /// Loads scene and sends challenge requests based on game and pick mode
        /// </summary>
        /// <param name="gameMode">game mode of challenge to be initiated</param>
        /// <param name="pickMode">pick mode of challenge to be initiated</param>
        private void InitiateChallenge(GameMode gameMode, PickMode pickMode)
        {
            IsChallengeActive = true;

            myChallenge = new Challenge
            {
                GameMode = gameMode,
                PickMode = pickMode,
                Team = null,
                ChallengeFor = ChatController.Instance.SelectedPlayerLogin

            };

            if (pickMode == PickMode.Draft)
            {
                Scenes.Load(Scenes.DRAFT_SCENE);
                SendChallengeRequest();
            }
            else
            {
                var parameters = new Dictionary<string, string>
                {
                    {Scenes.BATTLE_TYPE, BattleType.ChallengeBattle.ToString()},
                    {Scenes.TEAM_MANAGEMENT_STATE, TeamManagementState.CHALLENGE_CHOOSE.ToString()}
                };
                Scenes.Load(Scenes.TEAMS_MANAGEMENT_SCENE, parameters);
            }
        }

        /// <summary>
        /// Creates new challenge notification, stores incoming challenge
        /// </summary>
        /// <param name="challenge">incoming challenge object</param>
        private void OnChallengeRequest(Challenge challenge)
        {
            var notificationId =FeedbackNotificationManager.Instance.Show(
                ChallengeImage,
                challenge.ChallengeFrom + " challenges you!",
                String.Format(
                    "You are challenged by {0} to a {1} game with {2} pick",
                    challenge.ChallengeFrom,
                    challenge.GameMode,
                    challenge.PickMode
                ),
                () =>
                {
                    IsChallengeActive = true;
                    if (challenge.PickMode == PickMode.Draft)
                    {
                        Scenes.Load(Scenes.DRAFT_SCENE);
                        SendChallengeResult(challenge.ChallengeFrom, true);
                    }
                    else
                    {
                        currentEnemyChallenge = challenge;
                        var parameters = new Dictionary<string, string>
                        {
                            {Scenes.BATTLE_TYPE, BattleType.ChallengeBattle.ToString()},
                            {Scenes.TEAM_MANAGEMENT_STATE, TeamManagementState.CHALLENGE_CHOOSE.ToString()}
                        };
                        Scenes.Load(Scenes.TEAMS_MANAGEMENT_SCENE, parameters);
                    }
                },
                () =>
                {
                    SendChallengeResult(challenge.ChallengeFrom, false);
                    EnemyChallenges.RemoveAt(EnemyChallenges.FindIndex(cd => cd.Challenge.ChallengeFrom == challenge.ChallengeFrom));
                }
            );
            EnemyChallenges.Add(new ChallengeData(challenge, notificationId));
        }

        /// <summary>
        /// Returns player to main menu if challenge was rejected
        /// </summary>
        /// <param name="challengeResult">response of enemy player to challenge</param>
        private void OnChallengeResult(ChallengeResult challengeResult)
        {
            if (!challengeResult.ChallengeAccepted)
            {
                Scenes.Load(Scenes.MAIN_MENU_SCENE);
                NotificationManager.Instance.Show(ChallengeImage, "Challenge canceled!", myChallenge.ChallengeFor + " declined your challenge");
                myChallenge = null;
            }
        }

        /// <summary>
        /// Removes challenge request from notifications
        /// </summary>
        /// <param name="revokeChallenge">information about revoked challenge</param>
        private void OnChallengeRevoked(RevokeChallenge revokeChallenge)
        {
            int index = EnemyChallenges.FindIndex(cd => cd.Challenge.ChallengeFrom == revokeChallenge.ChallengeFrom);
            FeedbackNotificationManager.Instance.RemoveNotification(EnemyChallenges[index].NotificationId);
            if(index < 0) { 
                return;
            }
            EnemyChallenges.RemoveAt(index);
        }

        /// <summary>
        /// Handles challenge logic after new scene was loaded
        /// </summary>
        /// <param name="scene">loaded scene</param>
        /// <param name="mode">loaded scene mode</param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ChatController.Instance.Hide();
            if (scene.name == Scenes.MAIN_MENU_SCENE)
            {
                // if user goes to main menu any ongoing challenge is cancelled
                IsChallengeActive = false;
                if (AmIChallenging())
                {
                    SendRevokeChallenge();
                    myChallenge = null;
                }
                else if (AmIBeingChallenged())
                {
                    SendChallengeResult(currentEnemyChallenge.ChallengeFrom, false);
                    currentEnemyChallenge = null;
                }
            }
            else if (scene.name == Scenes.BATTLE_SCENE)
            {
                // if user enters battle scene during standard pick challenge
                // it means they chose their team and we need to send message about it to server
                IsChallengeActive = false;
                if (AmIChallenging())
                {
                    if (myChallenge.PickMode == PickMode.StandardPick)
                    {
                        myChallenge.Team = GameManager.Instance.CurrentFruitonTeam;
                        SendChallengeRequest();
                        CancelAllChallengeNotifications();
                    }
                }
                else if (AmIBeingChallenged())
                {
                    if (currentEnemyChallenge.PickMode == PickMode.StandardPick)
                    {
                        SendChallengeResult(currentEnemyChallenge.ChallengeFrom, true, GameManager.Instance.CurrentFruitonTeam);
                        CancelAllChallengeNotifications();
                    }
                }
                else
                {
                    // if users isn't participating in any challenge and enters the battle scene
                    // server removes all of their pending challenge requests
                    // so we need to remove them from notifications
                    CancelAllChallengeNotifications();
                }
            }
            else if (scene.name == Scenes.DRAFT_SCENE)
            {
                // when user enters draft scene all unanswered challenges are removed by the server
                // so we remove them from notifications
                CancelAllChallengeNotifications();
            }
        }

        /// <summary>
        /// Send response to incoming challenge request to the server
        /// </summary>
        /// <param name="enemyLogin">username of challenging player</param>
        /// <param name="accepted">true if challenge was accepted</param>
        /// <param name="fruitonTeam">team to use in challenge, null if pick mode is draft</param>
        private void SendChallengeResult(string enemyLogin, bool accepted, FruitonTeam fruitonTeam = null)
        {
            var challengeResult = new ChallengeResult
            {
                ChallengeFrom = enemyLogin,
                ChallengeAccepted = accepted,
                Team = fruitonTeam
            };
            var wsMessage = new WrapperMessage
            {
                ChallengeResult = challengeResult
            };
            ConnectionHandler.Instance.SendWebsocketMessage(wsMessage);
        }

        /// <summary>
        /// Sends own challenge to the server
        /// </summary>
        private void SendChallengeRequest()
        {
            var wsMessage = new WrapperMessage
            {
                Challenge = myChallenge
            };
            ConnectionHandler.Instance.SendWebsocketMessage(wsMessage);
        }

        /// <summary>
        /// Send revocation of outgoing challenge to the server
        /// </summary>
        private void SendRevokeChallenge()
        {
            var wsMessage = new WrapperMessage
            {
                RevokeChallenge = new RevokeChallenge()
            };
            ConnectionHandler.Instance.SendWebsocketMessage(wsMessage);
        }

        /// <summary>
        /// True if player has sent a challenge request to an enemy
        /// </summary>
        /// <returns></returns>
        private bool AmIChallenging()
        {
            return myChallenge != null;
        }

        /// <summary>
        /// True if player has accepted an incoming challenge request and is currently picking a team
        /// </summary>
        /// <returns></returns>
        private bool AmIBeingChallenged()
        {
            return currentEnemyChallenge != null;
        }
    }
}