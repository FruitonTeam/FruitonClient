using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UI.Chat;
using UnityEngine;

namespace UI.Notification
{
    public class FeedbackNotificationManager : MonoBehaviour, IOnMessageListener
    {
        private static readonly string FRIEND_REQUEST_TITLE = "Friend request";

        public static FeedbackNotificationManager Instance { get; private set; }

        private readonly Queue<FeedBackNotificationData> notificationQueue = 
                new Queue<FeedBackNotificationData>();
        
        public FeedbackNotificationView View;

        private void Update()
        {
            if (notificationQueue.Count == 0 || !View.IsAnimationCompleted)
            {
                return;
            }
            View.SetData(notificationQueue.Dequeue());
            View.StartAnimation();
        }

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
        
        public void OnMessage(WrapperMessage message)
        {
            FriendRequest friendRequest = message.FriendRequest;
            ShowFriendRequest(friendRequest.FriendToAdd);
        }

        public void ShowFriendRequest(string friendToAdd)
        {
            PlayerHelper.GetAvatar(friendToAdd, 
                avatar => ShowFriendRequest(friendToAdd, avatar), 
                e => ShowFriendRequest(friendToAdd, null));
        }

        private void ShowFriendRequest(string friendToAdd, Texture avatar)
        {
            Show(avatar, FRIEND_REQUEST_TITLE, friendToAdd,
                () =>
                {
                    FriendRequestResult result = new FriendRequestResult
                    {
                        FriendToAdd = friendToAdd,
                        FriendshipAccepted = true
                    };
                    ConnectionHandler.Instance.SendWebsocketMessage(new WrapperMessage {FriendRequestResult = result});
                    
                    ChatController.Instance.AddFriend(friendToAdd);
                },
                () =>
                {
                    FriendRequestResult result = new FriendRequestResult
                    {
                        FriendToAdd = friendToAdd,
                        FriendshipAccepted = false
                    };
                    ConnectionHandler.Instance.SendWebsocketMessage(new WrapperMessage {FriendRequestResult = result});
                });
        }

        private void Start()
        {
            if (GameManager.Instance.IsOnline)
            {
                ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.FriendRequest, this);
            }
        }

        public void Show(string title, string text, System.Action accept, System.Action decline)
        {
            notificationQueue.Enqueue(new FeedBackNotificationData(null, title, text, accept, decline));
        }
        
        public void Show(Texture image, string title, string text, System.Action accept, System.Action decline)
        {
            notificationQueue.Enqueue(new FeedBackNotificationData(image, title, text, accept, decline));
        }

        public void Clear()
        {
            notificationQueue.Clear();
            View.Hide();
        }

        public class FeedBackNotificationData : NotificationManager.NotificationData
        {
            
            public System.Action Accept { get; private set; }
            public System.Action Decline { get; private set; }

            public FeedBackNotificationData(Texture image, string header, string text, System.Action accept, System.Action decline) 
                    : base(image, header, text)
            {
                Accept = accept;
                Decline = decline;
            }
        }
      
    }
}