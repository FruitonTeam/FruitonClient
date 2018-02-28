using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Cz.Cuni.Mff.Fruiton.Dto;
using UnityEngine;

namespace UI.Chat
{
    /// <summary>
    /// Handles friend list in chat panel.
    /// </summary>
    public class FriendListController : ListController<FriendListItem.FriendItemData>
    {
        public ListItemBase ListItem;

        void Start()
        {
            Init();

            RecyclableList.Create(Data.Count, ListItem);
            RecyclableList.gameObject.SetActive(true);
        }

        /// <summary>
        /// Adds new user to the list.
        /// </summary>
        /// <param name="friendName">username of the added user</param>
        /// <param name="status">added user's status</param>
        /// <param name="isFriend">true if the added user is friend</param>
        public void AddItem(string friendName, Status status, bool isFriend = true)
        {
            var itemData = new FriendListItem.FriendItemData
            {
                Name = friendName,
                UnreadMessages = 0,
                Status = status,
                IsFriend = isFriend
            };

            if (isFriend)
            {
                // when adding friend we need remove user with same login from list
                // if they are already there as a nearby player
                if (Data.Exists(d => d.Name == friendName))
                {
                    RemoveItem(friendName);
                }
                if (status == Status.Offline)
                {
                    // offline friends are added to the bottom
                    Data.Add(itemData);
                    RecyclableList.AddItem(ListItem);
                }
                else
                {
                    // online friends are added right behind nearby players
                    var firstFriendIndex = Data.FindIndex(p => p.IsFriend);
                    if (firstFriendIndex < 0)
                    {
                        firstFriendIndex = 0;
                    }
                    Data.Insert(firstFriendIndex, itemData);
                    RecyclableList.InsertItem(firstFriendIndex, ListItem);
                }
            }
            else
            {
                Data.Insert(0, itemData);
                RecyclableList.InsertItem(0, ListItem);
            }
        }

        /// <summary>
        /// Removes a user from the list.
        /// </summary>
        /// <param name="nameToRemove">username of the user to remove</param>
        public void RemoveItem(string nameToRemove)
        {
            int index = Data.FindIndex(data => data.Name == nameToRemove);
            if (index < 0)
            {
                Debug.LogError("Couldn't remove " + nameToRemove + " from contact list (not found)");
                return;
            }
            Data.RemoveAt(index);
            RecyclableList.RemoveItemAt(index);
        }

        /// <param name="index">index of a user to get</param>
        /// <returns>user's data</returns>
        public FriendListItem.FriendItemData GetFriend(int index)
        {
            return Data[index];
        }

        /// <returns>List of usernames from the friend list</returns>
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

        /// <summary>
        /// Set's avatar for a user in the list.
        /// </summary>
        /// <param name="friend">username of the user we want to set avatar to</param>
        /// <param name="avatar">avatar to set</param>
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

        /// <summary>
        /// Change status of a user in the list.
        /// </summary>
        /// <param name="friend">username of the user we want to change status</param>
        /// <param name="newStatus">new status of the user</param>
        public void ChangeStatus(string friend, Status newStatus)
        {
            int index = Data.FindIndex(data => data.Name == friend);
            if (index < 0)
            {
                return;
            }
            var changedData = Data[index];
            var oldStatus = changedData.Status;
            changedData.Status = newStatus;
            if (oldStatus != Status.Offline && newStatus == Status.Offline)
            {
                // if friend went offline we roll every online friend that's after them
                // in the list 1 place forward until we reach offline friend or end of the list
                for (int i = index; i < Data.Count; i++)
                {
                    if (i+1 == Data.Count || Data[i+1].Status == Status.Offline)
                    {
                        Data[i] = changedData;
                        break;
                    }
                    Data[i] = Data[i + 1];
                }
            }
            else if (oldStatus == Status.Offline && newStatus != Status.Offline)
            {
                // if user came online we roll every offline friend in front of him 1 place behind
                // until we reach first online friend of beginning of the list
                for (int i = index; i >= 0; i--)
                {
                    if (i == 0 || Data[i - 1].Status != Status.Offline)
                    {
                        Data[i] = changedData;
                        break;
                    }
                    Data[i] = Data[i - 1];
                }
            }
            RecyclableList.NotifyDataChanged();
        }

        /// <summary>
        /// Removes every user from the list.
        /// </summary>
        public void Clear()
        {
            Data = new List<FriendListItem.FriendItemData>();
            RecyclableList.Destroy();
            Start();
        }
        
    }
}