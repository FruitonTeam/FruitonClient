using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Notification
{
    /// <summary>
    /// Handles animation of feedback notifications.
    /// </summary>
    public class FeedbackNotificationView : MonoBehaviour
    {
        private static readonly float SHOW_ANIMATION_TIME = 1.0f;
        private static readonly float HIDE_ANIMATION_TIME_HIDE = 0.5f;

        public RawImage Image;
        public Text Header;
        public Text Text;

        private Vector3 originalPosition;

        private Action acceptAction;
        private Action declineAction;

        /// <summary>
        /// True if no notification is currently being displayed.
        /// </summary>
        public bool IsAnimationCompleted { get; private set; }

        public FeedbackNotificationView()
        {
            IsAnimationCompleted = true;
        }

        /// <summary>
        /// Sets data of currently displayed notification.
        /// </summary>
        /// <param name="data">notification data to set</param>
        public void SetData(FeedbackNotificationManager.FeedBackNotificationData data)
        {
            Image.texture = data.Image;
            Header.text = data.Header;
            Text.text = data.Text;
            acceptAction = data.Accept;
            declineAction = data.Decline;
        }

        /// <summary>
        /// Starts the notification animation.
        /// </summary>
        public void StartAnimation()
        {
            IsAnimationCompleted = false;

            var canvas = gameObject.GetComponentInParent<Canvas>();
            float size = -gameObject.GetComponent<RectTransform>().rect.height * canvas.scaleFactor;

            iTween.MoveTo(gameObject, iTween.Hash(
                "position",
                gameObject.transform.position + new Vector3(0, size),
                "time", SHOW_ANIMATION_TIME,
                "easetype", iTween.EaseType.easeOutExpo
            ));
            
        }

        private void OnAnimationComplete()
        {
            IsAnimationCompleted = true;
        }

        private void Start()
        {
            originalPosition = gameObject.transform.position;
        }

        /// <summary>
        /// Hides the currently displayed notification.
        /// </summary>
        public void Hide()
        {
            iTween.Stop(gameObject);
            iTween.MoveTo(gameObject, iTween.Hash(
                "position", originalPosition,
                "oncomplete", "OnAnimationComplete",
                "time", HIDE_ANIMATION_TIME_HIDE
            ));
        }

        /// <summary>
        /// Invokes current notification's accept action if there is any.
        /// </summary>
        public void Accept()
        {
            if (acceptAction != null)
            {
                acceptAction.Invoke();
            }
            Hide();
        }

        /// <summary>
        /// Invokes current notification's decline action if there is any.
        /// </summary>
        public void Decline()
        {
            if (declineAction != null)
            {
                declineAction.Invoke();
            }
            Hide();
        }

    }
}