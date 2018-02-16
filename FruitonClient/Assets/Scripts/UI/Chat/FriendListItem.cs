using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Chat
{
    public class FriendListItem : ListItemBase
    {
        static Dictionary<Status, string> StatusNameMap = new Dictionary<Status, string>
        {
            {Status.Online, "Online"},
            {Status.Offline, "Offline"},
            {Status.InBattle, "In Battle"},
            {Status.InMatchmaking, "Looking for an opponent"},
            {Status.MainMenu, "Chilling in menu"}
        };

        public class FriendItemData
        {
            public string Name;
            public int UnreadMessages;
            public Status OnlineStatus;
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
            StatusText.text = isFriend ? StatusNameMap[itemData.OnlineStatus] : "Nearby player";
            Background.color = GetStatusColor();
            if (itemData.Avatar != null)
            {
                Avatar.texture = itemData.Avatar;
            }
        }

        private Color GetStatusColor()
        {
            if (StatusText.text == StatusNameMap[Status.Offline])
            {
                return OfflineColor;
            }
            return isFriend ? FriendColor : NearbyPlayerColor;
        }
    }
}