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
        }

        public Image Background;
        public Text FriendName;
        public Text UnreadCount;

        public Text StatusText;

        public RawImage Avatar;

        public Color Color;
        public Color SelectedColor;

        public override void Select(bool selected)
        {
            if (selected)
            {
                Background.color = SelectedColor;
                UnreadCount.text = "0";
            }
            else
            {
                Background.color = Color;
            }
        }

        public override void OnLoad(object data)
        {
            var itemData = (FriendItemData) data;
            FriendName.text = itemData.Name;
            Background.color = Color;
            UnreadCount.text = itemData.UnreadMessages.ToString();
            StatusText.text = itemData.OnlineStatus.ToString();
            if (itemData.Avatar != null)
            {
                Avatar.texture = itemData.Avatar;
            }
        }
    }
}