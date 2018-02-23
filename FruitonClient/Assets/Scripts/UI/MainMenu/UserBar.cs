using System.Linq;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UI.Chat;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.MainMenu
{
    public class UserBar : MonoBehaviour, ChatController.IOnFriendsChangedListener, IOnMessageListener, 
        Bazaar.Bazaar.IOnFruitonSoldListener
    {
        public Text PlayerNameText;
        public Image PlayerAvatarImage;
        public Text MoneyText;
        public Text FriendsText;

        private int onlineFriendsCount;

        public void OnEnable()
        {
            Load();
            Bazaar.Bazaar.Instance.AddListener(this);
            ChatController.Instance.AddListener(this);
            ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.StatusChange, this);
        }

        private void OnDisable()
        {
            Bazaar.Bazaar.Instance.RemoveListener(this);
            ChatController.Instance.RemoveListener(this);
            ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.StatusChange, this);
        }
        
        private void Load()
        {
            PlayerNameText.text = GameManager.Instance.UserName;

            int money = GameManager.Instance.Money;
            MoneyText.text = money != -1 ? money.ToString() : "N/A";
            PlayerAvatarImage.sprite = SpriteUtils.TextureToSprite(GameManager.Instance.Avatar);
            RecountOnlineFriends();
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
            RecountOnlineFriends();
        }

        private void RecountOnlineFriends()
        {
            FriendsText.text = GameManager.Instance.Friends.Count(f => f.Status != Status.Offline).ToString();
        }

        public void OnFruitonSold()
        {
            int money = GameManager.Instance.Money;
            MoneyText.text = money != -1 ? money.ToString() : "N/A";
        }
    }
}