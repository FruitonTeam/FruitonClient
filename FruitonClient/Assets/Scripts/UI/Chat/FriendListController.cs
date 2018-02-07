using System.Collections.Generic;
using System.Linq;
using Cz.Cuni.Mff.Fruiton.Dto;
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

        public void AddItem(string friendName, Status status)
        {
            var itemData = new FriendListItem.FriendItemData
            {
                Name = friendName,
                UnreadMessages = 0,
                OnlineStatus = status
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

        public void ChangeOnlineStatus(string friend, Status newStatus)
        {
            foreach (FriendListItem.FriendItemData friendData in Data)
            {
                if (friendData.Name.Equals(friend))
                {
                    friendData.OnlineStatus = newStatus;
                    break;
                }
            }
            RecyclableList.NotifyDataChanged();
        }

        public void Clear()
        {
            Data = new List<FriendListItem.FriendItemData>();
            RecyclableList.Destroy();
            Start();
        }
        
    }
}