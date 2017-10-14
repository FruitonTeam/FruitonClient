using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI.Chat
{
    public class FriendListController : ListController<FriendListItem.FriendItemData>
    {
        public ListItemBase ListItem;

        void Start()
        {
            Init();

            RecyclableList.Create(Data.Count, ListItem);
            RecyclableList.gameObject.SetActive(true);
        }

        public void AddItem(string friendName)
        {
            var itemData = new FriendListItem.FriendItemData
            {
                Name = friendName,
                UnreadMessages = 0
            };

            Data.Add(itemData);

            RecyclableList.AddItem(ListItem);
        }

        public string GetFriend(int index)
        {
            return Data[index].Name;
        }

        public List<string> GetAllFriends()
        {
            return Data.Select(friendData => friendData.Name).ToList();
        }

        public void IncrementUnreadCount(string friend)
        {
            foreach (FriendListItem.FriendItemData friendData in Data)
            {
                if (friendData.Name.Equals(friend))
                {
                    friendData.UnreadMessages++;
                    break;
                }
            }
            RecyclableList.NotifyDataChanged();
        }

        public void SetAvatar(string friend, Texture avatar)
        {
            foreach (FriendListItem.FriendItemData friendData in Data)
            {
                if (friendData.Name.Equals(friend))
                {
                    friendData.Avatar = avatar;
                    break;
                }
            }
            RecyclableList.NotifyDataChanged();
        }
    }
}