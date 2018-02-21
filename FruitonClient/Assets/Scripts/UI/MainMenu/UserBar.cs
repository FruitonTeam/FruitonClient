using System;
using System.Linq;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UI.Chat;
using UI.Notification;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.MainMenu
{
    public class UserBar : MonoBehaviour, ChatController.IOnFriendsChangedListener, IOnMessageListener
    {
        public Text PlayerNameText;
        public Text FractionText;
        public Image PlayerAvatarImage;
        public Text MoneyText;
        public Text FriendsText;
        public Dropdown UserDropdown;

        private static readonly string TRIAL_PLAYER_NAME = "<anonymous>";
        private static readonly string TRIAL_FRACTION_TEXT= "Trial mode";
        private static readonly string OFFLINE_FRACTION_TEXT= "Offline mode";

        const int DROPDOWN_SHOW_PROFILE = 0;
        const int DROPDOWN_LOG_OUT = 1;
        const int DROPDOWN_EXIT = 2;
        const int DROPDOWN_CANCEL = 3;

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
            if (GameManager.Instance.IsInTrial)
            {
                PlayerNameText.text = TRIAL_PLAYER_NAME;
                FractionText.text = TRIAL_FRACTION_TEXT;
            }
            else
            {
                PlayerNameText.text = GameManager.Instance.UserName;
                FractionText.text = GameManager.Instance.IsOnline ? GameManager.Instance.Fraction.GetReadableName() : OFFLINE_FRACTION_TEXT;
            }

            int money = GameManager.Instance.Money;
            MoneyText.text = money != -1 ? money.ToString() : "N/A";
            PlayerAvatarImage.sprite = SpriteUtils.TextureToSprite(GameManager.Instance.Avatar);
            RecountOnlineFriends();
        }

        public void OnDropdownOption(int option)
        {
            // reset dropdown value to make it work like a button
            UserDropdown.value = DROPDOWN_CANCEL;
            switch (option)
            {
                case DROPDOWN_SHOW_PROFILE:
                    if (GameManager.Instance.IsOnline)
                    {
                        ConnectionHandler.Instance.OpenUrlAuthorized(
                            "profile/" + Uri.EscapeDataString(GameManager.Instance.UserName));
                    }
                    else
                    {
                        NotificationManager.Instance.Show("Cannot view profile", "You must be registered and online to perform this action.");
                    }
                    break;
                case DROPDOWN_LOG_OUT:
                    MainPanel.Logout();
                    return;
                case DROPDOWN_EXIT:
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;
                case DROPDOWN_CANCEL:
                    return;
            }
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
    }
}