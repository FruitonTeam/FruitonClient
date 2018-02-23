using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Notification
{
    public class NotificationView : MonoBehaviour
    {
        private static readonly float SHOW_ANIMATION_TIME = 1.0f;
        private static readonly float HIDE_ANIMATION_TIME_HIDE = 0.5f;
        private static readonly float SHOW_TIME = 6.5f;
       
        public RawImage Image;
        public Text Header;
        public Text Text;
        
        private Vector3 originalPosition;
        private Coroutine showingCoroutine;

        public bool IsAnimationCompleted { get; private set; }

        public NotificationView()
        {
            IsAnimationCompleted = true;
        }

        public void SetData(NotificationManager.NotificationData data)
        {
            Image.texture = data.Image;
            Header.text = data.Header;
            Text.text = data.Text;
        }
        
        public void StartAnimation()
        {
            IsAnimationCompleted = false;

            var canvas = gameObject.GetComponentInParent<Canvas>();
            float size = -gameObject.GetComponent<RectTransform>().rect.height * canvas.scaleFactor;

            iTween.MoveTo(gameObject, iTween.Hash(
                "position", gameObject.transform.position + new Vector3(0, size),
                "time", SHOW_ANIMATION_TIME,
                "oncomplete", "OnNotificationShown",
                "easetype", iTween.EaseType.easeOutExpo
            ));
        }

        public void Hide()
        {
            StopCoroutine(showingCoroutine);
            iTween.Stop(gameObject);
            iTween.MoveTo(gameObject, iTween.Hash(
                "position", originalPosition,
                "time", HIDE_ANIMATION_TIME_HIDE,
                "oncomplete", "OnAnimationComplete"
            ));
        }

        private void OnNotificationShown()
        {
            showingCoroutine = StartCoroutine(WaitWhileShowing(SHOW_TIME));
        }

        private void OnAnimationComplete()
        {
            IsAnimationCompleted = true;
        }

        private IEnumerator WaitWhileShowing(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            Hide();
        }

        private void Start()
        {
            originalPosition = gameObject.transform.position;
        }

    }
}