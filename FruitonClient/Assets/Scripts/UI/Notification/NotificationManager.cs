using System.Collections.Generic;
using UnityEngine;

namespace UI.Notification
{
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        private static readonly Queue<NotificationData> notificationQueue = new Queue<NotificationData>();
        
        public NotificationView View;
        
        public void Show(Texture image, string header, string text)
        {
            notificationQueue.Enqueue(new NotificationData(image, header, text));
        }

        public void Show(string header, string text)
        {
            notificationQueue.Enqueue(new NotificationData(null, header, text));
        }

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