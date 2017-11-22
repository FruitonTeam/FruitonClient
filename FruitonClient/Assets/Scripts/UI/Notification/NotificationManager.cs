using System.Collections.Generic;
using UnityEngine;

namespace UI.Notification
{
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        private static readonly Queue<NotificationData> NotificationQueue = new Queue<NotificationData>();
        
        public NotificationView View;
        
        public void Show(Texture image, string header, string text)
        {
            NotificationQueue.Enqueue(new NotificationData(image, header, text));
        }

        public void Show(string header, string text)
        {
            NotificationQueue.Enqueue(new NotificationData(null, header, text));
        }

        private void Update()
        {
            if (NotificationQueue.Count == 0 || !View.IsAnimationCompleted)
            {
                return;
            }
            View.SetData(NotificationQueue.Dequeue());
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
            private readonly Texture image;
            private readonly string header;
            private readonly string text;

            public NotificationData(Texture image, string header, string text)
            {
                this.image = image;
                this.header = header;
                this.text = text;
            }

            public Texture Image
            {
                get { return image; }
            }

            public string Header
            {
                get { return header; }
            }

            public string Text
            {
                get { return text; }
            }
        }
    }
    
}