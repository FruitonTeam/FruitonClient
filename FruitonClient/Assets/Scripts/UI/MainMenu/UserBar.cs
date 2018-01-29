﻿using System.Linq;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UI.Chat;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class UserBar : MonoBehaviour, ChatController.IOnFriendAddedListener, IOnMessageListener
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
            ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.OnlineStatusChange, this);
        }

        private void OnDisable()
        {
            ChatController.Instance.RemoveListener(this);
            ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.OnlineStatusChange, this);
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
            PlayerAvatarImage.sprite = LoadCenteredSprite(GameManager.Instance.Avatar);
            
            FriendsText.text = GameManager.Instance.Friends.Count(f => f.Status == Status.Online).ToString();
        }

        private Sprite LoadCenteredSprite(Texture2D texture)
        {
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        public void OnFriendAdded()
        {
            FriendsText.text = GameManager.Instance.Friends.Count(f => f.Status == Status.Online).ToString();
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