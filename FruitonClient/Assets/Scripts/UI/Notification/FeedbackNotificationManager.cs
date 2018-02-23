using System.Collections.Generic;
using System.Linq;
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

        private Queue<FeedBackNotificationData> notificationQueue = 
                new Queue<FeedBackNotificationData>();

        private int currentNotificationId;
        
        public FeedbackNotificationView View;

        public Texture2D ImageQuestion;

        private void Update()
        {
            if (notificationQueue.Count == 0 || !View.IsAnimationCompleted)
            {
                return;
            }
            var nextNofitication = notificationQueue.Dequeue();
            currentNotificationId = nextNofitication.Id;
            View.SetData(nextNofitication);
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

        public int Show(string title, string text, System.Action accept, System.Action decline)
        {
            return Show(ImageQuestion, title, text, accept, decline);
        }
        
        public int Show(Texture image, string title, string text, System.Action accept, System.Action decline)
        {
            var notification = new FeedBackNotificationData(image, title, text, accept, decline);
            notificationQueue.Enqueue(notification);
            return notification.Id;
        }

        public void RemoveNotification(int notificationId)
        {
            if (notificationId == currentNotificationId)
            {
                View.Hide();
            }
            notificationQueue = new Queue<FeedBackNotificationData>(notificationQueue.Where(n => n.Id != notificationId));
        }

        public void RemoveNotifications(ICollection<int> notificationIds)
        {
            if (notificationIds.Contains(currentNotificationId))
            {
                View.Hide();
            }
            notificationQueue = new Queue<FeedBackNotificationData>(notificationQueue.Where(n => !notificationIds.Contains(n.Id)));
        }

        public void Clear()
        {
            notificationQueue.Clear();
            View.Hide();
        }

        public class FeedBackNotificationData : NotificationManager.NotificationData
        {
            private static int idCounter;

            public System.Action Accept { get; private set; }
            public System.Action Decline { get; private set; }
            public int Id { get; private set; }

            public FeedBackNotificationData(Texture image, string header, string text, System.Action accept, System.Action decline) 
                    : base(image, header, text)
            {
                Accept = accept;
                Decline = decline;
                Id = idCounter++;
            }
        }
      
    }
}