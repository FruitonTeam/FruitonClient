using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Notification
{
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

        public bool IsAnimationCompleted { get; private set; }

        public FeedbackNotificationView()
        {
            IsAnimationCompleted = true;
        }

        public void SetData(FeedbackNotificationManager.FeedBackNotificationData data)
        {
            Image.texture = data.Image;
            Header.text = data.Header;
            Text.text = data.Text;
            acceptAction = data.Accept;
            declineAction = data.Decline;
        }

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

        public void Hide()
        {
            iTween.MoveTo(gameObject, iTween.Hash(
                "position", originalPosition,
                "oncomplete", "OnAnimationComplete",
                "time", HIDE_ANIMATION_TIME_HIDE
            ));
        }

        public void Accept()
        {
            if (acceptAction != null)
            {
                acceptAction.Invoke();
            }
            Hide();
        }

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