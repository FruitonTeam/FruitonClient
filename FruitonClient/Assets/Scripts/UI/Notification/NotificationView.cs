using UnityEngine;
using UnityEngine.UI;

namespace UI.Notification
{
    public class NotificationView : MonoBehaviour
    {
        private static readonly float ANIMATION_TIME = 3.0f;
       
        public RawImage Image;
        public Text Header;
        public Text Text;
        
        private Vector3 originalPosition;

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
            
            iTween.MoveTo(gameObject, iTween.Hash(
                "position", gameObject.transform.position + new Vector3(0, -gameObject.GetComponent<RectTransform>().rect.height),
                "time", ANIMATION_TIME,
                "easetype", iTween.EaseType.easeOutExpo
            ));
            iTween.MoveTo(gameObject, iTween.Hash(
                "position", originalPosition,
                "oncomplete", "OnAnimationComplete",
                "time", ANIMATION_TIME,
                "easetype", iTween.EaseType.easeInExpo,
                "delay", ANIMATION_TIME
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

    }
}