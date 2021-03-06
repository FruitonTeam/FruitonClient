﻿using Cz.Cuni.Mff.Fruiton.Dto;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Chat
{
    /// <summary>
    /// Represents item in the friend list.
    /// </summary>
    public class FriendListItem : ListItemBase
    {
        private static readonly string NEARBY_PLAYER_IN_MENU = "Nearby player";
        private static readonly string NEARBY_PLAYER_BUSY= "Nearby player - Busy";

        public class FriendItemData
        {
            public string Name;
            public int UnreadMessages;
            public Status Status;
            public Texture Avatar;
            public bool IsFriend;
        }

        public Image Background;
        public Text FriendName;
        public Text UnreadCount;

        public Text StatusText;

        public RawImage Avatar;

        public Color FriendColor;
        public Color NearbyPlayerColor;
        public Color OfflineColor;
        public Color SelectedColor;

        private bool isFriend;
        private Status status;

        public override void Select(bool selected)
        {
            if (selected)
            {
                Background.color = SelectedColor;
                UnreadCount.text = "0";
            }
            else
            {
                Background.color = GetStatusColor();
            }
        }

        /// <summary>
        /// Reloads user's information.
        /// </summary>
        /// <param name="data">data to load from</param>
        public override void OnLoad(object data)
        {
            var itemData = (FriendItemData) data;
            isFriend = itemData.IsFriend;
            status = itemData.Status;
            FriendName.text = itemData.Name;
            UnreadCount.text = itemData.UnreadMessages.ToString();
            StatusText.text = GetStatusText();
            Background.color = GetStatusColor();
            if (itemData.Avatar != null)
            {
                Avatar.texture = itemData.Avatar;
            }
        }

        private Color GetStatusColor()
        {
            if (StatusText.text == Status.Offline.GetDescription())
            {
                return OfflineColor;
            }
            return isFriend ? FriendColor : NearbyPlayerColor;
        }

        private string GetStatusText()
        {
            if (isFriend)
            {
                return status.GetDescription();
            }
            return status == Status.MainMenu ? NEARBY_PLAYER_IN_MENU : NEARBY_PLAYER_BUSY;
        }
    }
}