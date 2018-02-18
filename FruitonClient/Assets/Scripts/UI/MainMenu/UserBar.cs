using System.Linq;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UI.Chat;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.MainMenu
{
    public class UserBar : MonoBehaviour, ChatController.IOnFriendsChangedListener, IOnMessageListener
    {
        public Text PlayerNameText;
        public Image PlayerAvatarImage;
        public Text MoneyText;
        public Text FriendsText;

        private int onlineFriendsCount;

        public void OnEnable()
        {
            Load();
            ChatController.Instance.AddListener(this);
            ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.StatusChange, this);
        }

        private void OnDisable()
        {
            ChatController.Instance.RemoveListener(this);
            ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.StatusChange, this);
        }

        public void Refresh()
        {
            Load();
        }
        
        private void Load()
        {
            PlayerNameText.text = GameManager.Instance.UserName;

            int money = GameManager.Instance.Money;
            MoneyText.text = money != -1 ? money.ToString() : "N/A";
            PlayerAvatarImage.sprite = SpriteUtils.TextureToSprite(GameManager.Instance.Avatar);
            
            FriendsText.text = GameManager.Instance.Friends.Count(f => f.Status == Status.Online).ToString();
        }

        public void OnFriendAdded()
        {
            RecountOnlineFriends();
        }

        public void OnFriendRemoved()
        {
            RecountOnlineFriends();
        }

        public void OnMessage(WrapperMessage message)
        {
            if (message.OnlineStatusChange.Status == Status.Online)
            {
                FriendsText.text = (int.Parse(FriendsText.text) + 1).ToString();
            }
            else
            {
                FriendsText.text = (int.Parse(FriendsText.text) - 1).ToString();
            }
        }
    }
}