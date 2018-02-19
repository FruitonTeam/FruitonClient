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

    public static ChallengeController Instance { get; private set; }

    public bool IsChallengeActive { get; private set; }

    public GameObject ChallengePanel;
    public Texture2D ChallengeImage;

    private Challenge ownChallenge;
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
                IsChallengeActive = false;
                if (ownChallenge != null)
                {
                    SendRevokeChallenge();
                    ownChallenge = null;
                }
                if (currentEnemyChallenge != null)
                {
                    SendChallengeResult(currentEnemyChallenge.ChallengeFrom, false);
                    currentEnemyChallenge = null;
                }
            }
            else if (scene.name == Scenes.BATTLE_SCENE)
            {
                IsChallengeActive = false;
                if (ownChallenge != null)
                {
                    ownChallenge.Team = GameManager.Instance.CurrentFruitonTeam;
                    if (ownChallenge.PickMode == PickMode.StandardPick)
                    {
                        SendChallengeRequest();
                    }
                }
                else if (currentEnemyChallenge != null)
                {
                    if (currentEnemyChallenge.PickMode == PickMode.StandardPick)
                    {
                        SendChallengeResult(currentEnemyChallenge.ChallengeFrom, true);
                    }
                }
                else
                {
                    CancelAllChallengeNotifications();
                }
            }
            else if (scene.name == Scenes.DRAFT_SCENE)
            {
                CancelAllChallengeNotifications();
            }
        };
    }

    public void Show()
    {
        ChallengePanel.SetActive(true);
    }

    public void Hide()
    {
        ChallengePanel.SetActive(false);
    }

    public void OnChallengeModeChosen(int modeId)
    {
        // 0 - Standard Premade
        // 1 - Standard Draft
        // 2 - LMS Premade
        // 3 - LMS Draft
        switch (modeId)
        {
            case 0:
                InitiateChallenge(GameMode.Standard, PickMode.StandardPick);
                break;
            case 1:
                InitiateChallenge(GameMode.Standard, PickMode.Draft);
                break;
            case 2:
                InitiateChallenge(GameMode.LastManStanding, PickMode.StandardPick);
                break;
            case 3:
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

        ownChallenge = new Challenge
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
                {Scenes.BATTLE_TYPE, BattleType.OnlineBattle.ToString()},
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
                        {Scenes.BATTLE_TYPE, BattleType.OnlineBattle.ToString()},
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
            NotificationManager.Instance.Show(ChallengeImage, "Challenge canceled!", ownChallenge.ChallengeFor + " declined your challenge");
            ownChallenge = null;
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

    private void SendChallengeResult(string enemyLogin, bool accepted)
    {
        var challengeResult = new ChallengeResult
        {
            ChallengeFrom = enemyLogin,
            ChallengeAccepted = accepted
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
            Challenge = ownChallenge
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

}