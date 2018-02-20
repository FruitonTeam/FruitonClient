using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UI.Chat;
using UI.Notification;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

public class ChallengeController : MonoBehaviour, IOnMessageListener
{
    class ChallengeData
    {
        public readonly Challenge Challenge;
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

    public bool IsChallengeActive { get; private set; }

    public GameObject ChallengePanel;
    public GameObject ChallengeButtons;
    public Texture2D ChallengeImage;
    public Text ChallengeText;

    private Challenge myChallenge;
    private Challenge currentEnemyChallenge;

    private readonly List<ChallengeData> enemyChallenges = new List<ChallengeData>();

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
        SceneManager.sceneLoaded += (scene, mode) =>
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
        };
    }

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

    public void Hide()
    {
        ChallengePanel.SetActive(false);
    }

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

    private void CancelAllChallengeNotifications()
    {
        FeedbackNotificationManager.Instance.RemoveNotifications(enemyChallenges.Select(cd => cd.NotificationId).ToArray());        
        enemyChallenges.Clear();
    }

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
                enemyChallenges.RemoveAt(enemyChallenges.FindIndex(cd => cd.Challenge.ChallengeFrom == challenge.ChallengeFrom));
            }
        );
        enemyChallenges.Add(new ChallengeData(challenge, notificationId));
    }

    private void OnChallengeResult(ChallengeResult challengeResult)
    {
        if (!challengeResult.ChallengeAccepted)
        {
            Scenes.Load(Scenes.MAIN_MENU_SCENE);
            NotificationManager.Instance.Show(ChallengeImage, "Challenge canceled!", myChallenge.ChallengeFor + " declined your challenge");
            myChallenge = null;
        }
    }

    private void OnChallengeRevoked(RevokeChallenge revokeChallenge)
    {
        int index = enemyChallenges.FindIndex(cd => cd.Challenge.ChallengeFrom == revokeChallenge.ChallengeFrom);
        FeedbackNotificationManager.Instance.RemoveNotification(enemyChallenges[index].NotificationId);
        if(index < 0) { 
            return;
        }
        enemyChallenges.RemoveAt(index);
    }

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

    private void SendChallengeRequest()
    {
        var wsMessage = new WrapperMessage
        {
            Challenge = myChallenge
        };
        ConnectionHandler.Instance.SendWebsocketMessage(wsMessage);
    }

    private void SendRevokeChallenge()
    {
        var wsMessage = new WrapperMessage
        {
            RevokeChallenge = new RevokeChallenge()
        };
        ConnectionHandler.Instance.SendWebsocketMessage(wsMessage);
    }

    private bool AmIChallenging()
    {
        return myChallenge != null;
    }

    private bool AmIBeingChallenged()
    {
        return currentEnemyChallenge != null;
    }
}