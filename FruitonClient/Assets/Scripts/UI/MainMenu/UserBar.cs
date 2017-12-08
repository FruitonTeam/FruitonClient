using System;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class UserBar : MonoBehaviour, IOnMessageListener
    {
        public Text PlayerNameText;
        public Image PlayerAvatarImage;
        public Text MoneyText;
        public Text FriendsText;

        private void OnEnable()
        {
            ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.LoggedPlayerInfo, this);
            
            if (GameManager.Instance.PlayerInfoInitialized)
            {
                Init();
            }
        }

        private void OnDisable()
        {
            ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.LoggedPlayerInfo, this);
        }

        private void Init()
        {
            PlayerNameText.text = GameManager.Instance.UserName;
            MoneyText.text = GameManager.Instance.Money.ToString();
            PlayerAvatarImage.sprite = loadCenteredSprite(GameManager.Instance.Avatar);
        }
        
        public void OnMessage(WrapperMessage message)
        {
            Init(message.LoggedPlayerInfo);
        }
        
        private void Init(LoggedPlayerInfo playerInfo)
        {
            PlayerNameText.text = playerInfo.Login;
            MoneyText.text = playerInfo.Money.ToString();

            if (!string.IsNullOrEmpty(playerInfo.Avatar))
            {
                InitAvatarFromBase64(playerInfo.Avatar);
            }
            else
            {
                InitDefaultAvatar();
            }
        }

        private void InitAvatarFromBase64(string base64Avatar)
        {
            var avatarTexture = new Texture2D(0, 0);
            avatarTexture.LoadImage(Convert.FromBase64String(base64Avatar));
            PlayerAvatarImage.sprite = loadCenteredSprite(avatarTexture);
        }

        private void InitDefaultAvatar()
        {
            var avatarTexture = Resources.Load<Texture2D>("Images/avatar_default");
            PlayerAvatarImage.sprite = loadCenteredSprite(avatarTexture);
        }

        private Sprite loadCenteredSprite(Texture2D texture)
        {
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }

    }
}