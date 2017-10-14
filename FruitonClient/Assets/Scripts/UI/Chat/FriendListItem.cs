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
            public Texture Avatar;
        }

        public Image Background;
        public Text FriendName;
        public Text UnreadCount;

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
            if (itemData.Avatar != null)
            {
                Avatar.texture = itemData.Avatar;
            }
        }
    }
}