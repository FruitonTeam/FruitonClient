using System;
using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UnityEngine;

namespace UI.Notification
{
    /// <summary>
    /// Handles in-game notifications.
    /// </summary>
    public class NotificationManager : MonoBehaviour, IOnMessageListener
    {
        public static NotificationManager Instance { get; private set; }

        /// <summary>
        /// Queue containg all notifications waiting to be displayed.
        /// </summary>
        private static readonly Queue<NotificationData> notificationQueue = new Queue<NotificationData>();
        
        public NotificationView View;

        /// <summary>
        /// Default icon for notifications.
        /// </summary>
        public Texture2D ImageInfo;
        /// <summary>
        /// Default icon for "success" notifications.
        /// </summary>
        public Texture2D ImageSuccess;
        /// <summary>
        /// Default icon for error notifications.
        /// </summary>
        public Texture2D ImageError;
        
        /// <summary>
        /// Queues new notification.
        /// </summary>
        /// <param name="image">notification icon</param>
        /// <param name="header">notification title</param>
        /// <param name="text">notification text</param>
        public void Show(Texture image, string header, string text)
        {
            notificationQueue.Enqueue(new NotificationData(image, header, text));
        }

        /// <summary>
        /// Queues new notification with default icon.
        /// </summary>
        /// <param name="header">notification title</param>
        /// <param name="text">notification text</param>
        public void Show(string header, string text)
        {
            Show(ImageInfo, header, text);
        }

        /// <summary>
        /// Queues new notification with checkmark icon.
        /// </summary>
        /// <param name="header">notification title</param>
        /// <param name="text">notification text</param>
        public void ShowSuccess(string header, string text)
        {
            Show(ImageSuccess, header, text);
        }

        /// <summary>
        /// Queues new notification with error icon.
        /// </summary>
        /// <param name="header">notification title</param>
        /// <param name="text">notification text</param>
        public void ShowError(string header, string text)
        {
            Show(ImageError, header, text);
        }

        /// <summary>
        /// Removes all notifications from the queue.
        /// </summary>
        public void Clear()
        {
            notificationQueue.Clear();
            View.Hide();
        }

        /// <summary>
        /// Checks whether next notification can be displayed (if there is any).
        /// </summary>
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

        private void Start()
        {
            if (GameManager.Instance.IsOnline)
            {
                ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.Notification, this);
            }
        }

        public void OnMessage(WrapperMessage message)
        {
            Cz.Cuni.Mff.Fruiton.Dto.Notification n = message.Notification;

            if (!string.IsNullOrEmpty(n.Image))
            {
                var notificationImage = new Texture2D(0, 0);
                notificationImage.LoadImage(Convert.FromBase64String(n.Image));
                Show(notificationImage, n.Title, n.Text);
            }
            else
            {
                Show(n.Title, n.Text);
            }
        }
        
        public class NotificationData
        {
            public Texture Image { get; private set; }

            public string Header { get; private set; }

            public string Text { get; private set; }
            
            public NotificationData(Texture image, string header, string text)
            {
                Image = image;
                Header = header;
                Text = text;
            }

        }
    }
    
}