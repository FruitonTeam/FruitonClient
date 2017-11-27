using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UI.Notification;
using UnityEngine;

namespace UI.Chat
{
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
            ChatMessage chatMessage = message.ChatMessage;
            PlayerHelper.GetAvatar(chatMessage.Sender,
                avatar =>
                {
                    NotificationManager.Instance.Show(avatar, chatMessage.Sender, chatMessage.Message);
                },
                error =>
                {
                    NotificationManager.Instance.Show(defaultAvatar, chatMessage.Sender, chatMessage.Message);
                });
        }
    }
}