using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Chat
{
    static class StatusExtensions
    {
        public static string GetDescription(this Status status)
        {
            switch (status)
            {
                case Status.Online: return "Online";
                case Status.Offline: return "Offline";
                case Status.InBattle: return "In Battle";
                case Status.InMatchmaking: return "Looking for an opponent";
                case Status.MainMenu: return "Chilling in menu";
            }
            return status.ToString();
        }
    }

    public class FriendListItem : ListItemBase
    {
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

        public override void OnLoad(object data)
        {
            var itemData = (FriendItemData) data;
            isFriend = itemData.IsFriend;
            FriendName.text = itemData.Name;
            UnreadCount.text = itemData.UnreadMessages.ToString();
            StatusText.text = isFriend ? itemData.Status.GetDescription() : "Nearby player";
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
    }
}