using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    /// <summary>
    /// Handles message panel, used for showing messages to player in a window.
    /// </summary>
    public class MessagePanel : MonoBehaviour
    {
        public GameObject SuccessImage;
        public GameObject ErrorImage;

        private Text textComponent;
        private Button button;

        /// <summary>
        /// Action to be performed when the message window is closed.
        /// </summary>
        private Action onCloseAction;

        /// <summary>
        /// Show message with an info icon. Overwrites previously displayed message.
        /// </summary>
        /// <param name="text">text of the message</param>
        public void ShowInfoMessage(string text)
        {
            Activate();
            SuccessImage.SetActive(true);
            ErrorImage.SetActive(false);
            textComponent.text = text;
            button.Select();
        }

        /// <summary>
        /// Shows message with an error icon. Overwrites previously displayed message.
        /// </summary>
        /// <param name="text">text of the message</param>
        public void ShowErrorMessage(string text)
        {
            Activate();
            SuccessImage.SetActive(false);
            ErrorImage.SetActive(true);
            textComponent.text = text;
            button.Select();
        }

        /// <summary>
        /// Hides currently displayed message.
        /// </summary>
        public void HideMessage()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Loads text and button components of the window, activates the window.
        /// </summary>
        private void Activate()
        {
            if (textComponent == null)
            {
                textComponent = GetComponentInChildren<Text>(true);
            }
            if (button == null)
            {
                button = GetComponentInChildren<Button>(true);
            }
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Sets the action to perform when the window is closed.
        /// </summary>
        /// <param name="action">action to perform</param>
        public void OnClose(Action action)
        {
            onCloseAction = action;
            if (button == null)
            {
                button = GetComponentInChildren<Button>(true);
            }
            button.onClick.AddListener(PerformOnCloseAction);
        }

        private void PerformOnCloseAction()
        {
            if (onCloseAction != null)
            {
                onCloseAction();
            }
            onCloseAction = null;
        }

    }
}