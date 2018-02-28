using System.Collections.Generic;
using System.Linq;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UI.Chat;
using UnityEngine;

namespace UI.Notification
{
    /// <summary>
    /// Handles notifications that require player's feedback (yes/no question).
    /// </summary>
    public class FeedbackNotificationManager : MonoBehaviour, IOnMessageListener
    {
        private static readonly string FRIEND_REQUEST_TITLE = "Friend request";

        public static FeedbackNotificationManager Instance { get; private set; }

        /// <summary>
        /// Queue containing all notifications to be displayed.
        /// </summary>
        private Queue<FeedBackNotificationData> notificationQueue = 
                new Queue<FeedBackNotificationData>();

        private int currentNotificationId;
        
        public FeedbackNotificationView View;

        /// <summary>
        /// Default icon for feedback notifications.
        /// </summary>
        public Texture2D ImageQuestion;
        /// <summary>
        /// Default icon for friend requests.
        /// </summary>
        public Texture2D ImageFriend;

        /// <summary>
        /// Checks whether next notification can be displayed (if there is any).
        /// </summary>
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

        /// <summary>
        /// Loads user's avatar then queues friend request notification.
        /// </summary>
        /// <param name="friendToAdd">username that sent the request</param>
        public void ShowFriendRequest(string friendToAdd)
        {
            PlayerHelper.GetAvatar(friendToAdd, 
                avatar => ShowFriendRequest(friendToAdd, avatar), 
                e => ShowFriendRequest(friendToAdd, null));
        }

        private void ShowFriendRequest(string friendToAdd, Texture avatar)
        {
            if (avatar == null)
            {
                avatar = ImageFriend;
            }
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

        /// <summary>
        /// Queues new feedback notification with default icon.
        /// </summary>
        /// <param name="title">notification title</param>
        /// <param name="text">notification text</param>
        /// <param name="accept">action to perform when user accepts</param>
        /// <param name="decline">action to perform when user declines</param>
        /// <returns>id of created notification</returns>
        public int Show(string title, string text, System.Action accept, System.Action decline)
        {
            return Show(ImageQuestion, title, text, accept, decline);
        }

        /// <summary>
        /// Queues new feedback notification.
        /// </summary>
        /// <param name="image">notification icon</param>
        /// <param name="title">notification title</param>
        /// <param name="text">notification text</param>
        /// <param name="accept">action to perform when user accepts</param>
        /// <param name="decline">action to perform when user declines</param>
        /// <returns>id of created notification</returns>
        public int Show(Texture image, string title, string text, System.Action accept, System.Action decline)
        {
            var notification = new FeedBackNotificationData(image, title, text, accept, decline);
            notificationQueue.Enqueue(notification);
            return notification.Id;
        }

        /// <summary>
        /// Removes a notification from the queue.
        /// </summary>
        /// <param name="notificationId">if of the notification to remove</param>
        public void RemoveNotification(int notificationId)
        {
            if (notificationId == currentNotificationId)
            {
                View.Hide();
            }
            notificationQueue = new Queue<FeedBackNotificationData>(notificationQueue.Where(n => n.Id != notificationId));
        }

        /// <summary>
        /// Removes notification from the queue.
        /// </summary>
        /// <param name="notificationIds">collection of ids of notifications to remove</param>
        public void RemoveNotifications(ICollection<int> notificationIds)
        {
            if (notificationIds.Contains(currentNotificationId))
            {
                View.Hide();
            }
            notificationQueue = new Queue<FeedBackNotificationData>(notificationQueue.Where(n => !notificationIds.Contains(n.Id)));
        }

        /// <summary>
        /// Removes all notifications from the queue.
        /// </summary>
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