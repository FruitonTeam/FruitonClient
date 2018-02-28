using System;
using System.Linq;
using Cz.Cuni.Mff.Fruiton.Dto;
using Extensions;
using Networking;
using UI.Chat;
using UI.Notification;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.MainMenu
{
    /// <summary>
    /// Handles displaying information about user in the top bar in main menu.
    /// </summary>
    public class UserBar : MonoBehaviour, ChatController.IOnFriendsChangedListener, IOnMessageListener, 
        Bazaar.Bazaar.IOnFruitonSoldListener
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

        /// <summary>
        /// Loads current data and registers as a listener on user info related events.
        /// </summary>
        public void OnEnable()
        {
            Load();
            Bazaar.Bazaar.Instance.AddListener(this);
            ChatController.Instance.AddListener(this);
            ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.StatusChange, this);
        }

        /// <summary>
        /// Stops listening on user info related events.
        /// </summary>
        private void OnDisable()
        {
            Bazaar.Bazaar.Instance.RemoveListener(this);
            ChatController.Instance.RemoveListener(this);
            ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.StatusChange, this);
        }
        
        /// <summary>
        /// Loads data of currently logged in user and displays them.
        /// </summary>
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

        /// <summary>
        /// Handles selecting option from dropdown in user bar.
        /// </summary>
        /// <param name="option">id of selected option</param>
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

        /// <summary>
        /// Updates displayed friend count
        /// </summary>
        public void OnFriendAdded()
        {
            RecountOnlineFriends();
        }

        /// <summary>
        /// Updates displayed friend count
        /// </summary>
        public void OnFriendRemoved()
        {
            RecountOnlineFriends();
        }

        /// <summary>
        /// Updates displayed friend count
        /// </summary>
        public void OnMessage(WrapperMessage message)
        {
            RecountOnlineFriends();
        }

        /// <summary>
        /// Counts number of player's friends that are online and displays it.
        /// </summary>
        private void RecountOnlineFriends()
        {
            FriendsText.text = GameManager.Instance.Friends.Count(f => f.Status != Status.Offline).ToString();
        }

        /// <summary>
        /// Updates player's money information.
        /// </summary>
        public void OnFruitonSold()
        {
            int money = GameManager.Instance.Money;
            MoneyText.text = money != -1 ? money.ToString() : "N/A";
            PlayerHelper.GetAvailableFruitons(
                list => Debug.Log("Available fruitons updated after trade: " + list),
                Debug.LogError
                );
        }
    }
}