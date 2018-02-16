using Cz.Cuni.Mff.Fruiton.Dto;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Chat
{
    public class FriendListItem : ListItemBase
    {
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
                Background.color = isFriend ? FriendColor : NearbyPlayerColor;
            }
        }

        public override void OnLoad(object data)
        {
            var itemData = (FriendItemData) data;
            isFriend = itemData.IsFriend;
            FriendName.text = itemData.Name;
            Background.color = isFriend ? FriendColor : NearbyPlayerColor;
            UnreadCount.text = itemData.UnreadMessages.ToString();
            StatusText.text = isFriend ? itemData.OnlineStatus.ToString() : "Nearby player";
            if (itemData.Avatar != null)
            {
                Avatar.texture = itemData.Avatar;
            }
        }
    }
}