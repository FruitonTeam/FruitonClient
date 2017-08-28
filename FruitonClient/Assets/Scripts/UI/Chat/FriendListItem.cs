using UnityEngine;
using UnityEngine.UI;

namespace UI.Chat
{
    public class FriendListItem : ListItemBase {

        public class FriendItemData
        {
            public string Name;
            public int UnreadMessages;
        }

        public Image Background;
        public Text FriendName;
        public Text UnreadCount;

        public override void Select(bool selected)
        {
            if (selected) {
                Background.color = new Color (0.95f, 1f, 1f);
                UnreadCount.text = "0";
            } else {
                Background.color = new Color (1f, 1f, 0.95f);
            }
        }

        public override void OnLoad(object data)
        {
            FriendItemData itemData = (FriendItemData) data;
            FriendName.text = itemData.Name;
            UnreadCount.text = itemData.UnreadMessages.ToString();
        }

    }
}
