using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UI.Notification;
using UnityEngine;

namespace UI.Chat
{
    /// <summary>
    /// Handled incoming chat messages and displays notification when chat panel isn't active.
    /// </summary>
    public class ChatMessageNotifier : IOnMessageListener
    {
        private static Texture defaultAvatar;
        
        private static ChatMessageNotifier instance;
        
        public static ChatMessageNotifier Instance {
            get 
            {
                if (instance == null)
                {
                    instance = new ChatMessageNotifier();
                    defaultAvatar = Resources.Load<Texture2D>("Images/avatar_default");
                }
                return instance;
            }
        }
        
        public void OnMessage(WrapperMessage message)
        {
            if (!ChatController.Instance.ChatPanel.activeInHierarchy) // show notification only if chat panel is not active
            {
                ChatMessage chatMessage = message.ChatMessage;
                PlayerHelper.GetAvatar(chatMessage.Sender,
                    avatar => { NotificationManager.Instance.Show(avatar, chatMessage.Sender, chatMessage.Message); },
                    error =>
                    {
                        NotificationManager.Instance.Show(defaultAvatar, chatMessage.Sender, chatMessage.Message);
                    });
            }
        }
    }
}