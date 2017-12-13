using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class UserBar : MonoBehaviour
    {
        public Text PlayerNameText;
        public Image PlayerAvatarImage;
        public Text MoneyText;
        public Text FriendsText;

        public void OnEnable()
        {
            Load();
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
        }

        private Sprite LoadCenteredSprite(Texture2D texture)
        {
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }

    }
}