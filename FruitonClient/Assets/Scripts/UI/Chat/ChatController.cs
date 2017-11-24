using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Chat
{
    public class ChatController : MonoBehaviour, IOnItemSelectedListener, IOnMessageListener
    {
        public Text ChatText;
        public InputField MessageInput;

        public InputField AddFriendInput;

        public FriendListController FriendListController;

        public Text FriendName;

        readonly Dictionary<string, string> friendMessages = new Dictionary<string, string>();

        void Start()
        {
            FriendName.text = "";
            ChatText.text = "";

            MessageInput.enabled = false;
            FriendListController.SetOnItemSelectedListener(this);

            if (!ConnectionHandler.Instance.IsLogged())
            {
                AddFriendInput.enabled = false;
            }
        }

        void OnEnable()
        {
            if (ConnectionHandler.Instance.IsLogged())
            {
                ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.ChatMessage, this);
                ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.ChatMessage, 
                    ChatMessageNotifier.Instance);
            }
        }

        void OnDisable()
        {
            if (ConnectionHandler.Instance.IsLogged())
            {
                ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.ChatMessage, this);
                ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.ChatMessage, 
                    ChatMessageNotifier.Instance);
            }
        }

        public void OnSendClick()
        {
            if (!MessageInput.IsActive())
            {
                return;
            }

            var chatMessage = new ChatMessage
            {
                Message = MessageInput.text,
                Recipient = FriendName.text,
                Sender = GameManager.Instance.UserName
            };
            var wsMessage = new WrapperMessage
            {
                ChatMessage = chatMessage
            };

            ConnectionHandler.Instance.SendWebsocketMessage(wsMessage);

            if (ChatText.text != "")
            {
                ChatText.text += "\n";
            }

            ChatText.text += "You: " + MessageInput.text;

            MessageInput.text = ""; // reset the input field
            FocusOnInput();
        }

        void FocusOnInput()
        {
            MessageInput.enabled = true;
            MessageInput.ActivateInputField();
            MessageInput.Select();
        }

        public void OnAddFriendClick()
        {
            string friendToAdd = AddFriendInput.text;
            AddFriendInput.text = "";

            if (friendToAdd == "")
            {
                return; // cannot add empty friend
            }

            if (!ConnectionHandler.Instance.IsLogged())
            {
                return; // cannot check if user exists
            }

            if (GameManager.Instance.UserName.Equals(friendToAdd))
            {
                // TODO: show message that user cannot add himself as a friend
                return;
            }

            PlayerHelper.Exists(friendToAdd, exists =>
            {
                if (exists)
                {
                    AddFriendToList(friendToAdd);
                }
                else
                {
                    Debug.LogWarning("No user with name " + friendToAdd);
                }
            }, err => { Debug.LogWarning("Error while checking player existence" + err); });
        }

        private void AddFriendToList(string friendToAdd)
        {
            PlayerHelper.GetAvatar(friendToAdd,
                texture => { FriendListController.SetAvatar(friendToAdd, texture); },
                error =>
                {
                    Debug.LogWarning("Could not get avatar for user " + friendToAdd +
                                     ". Default avatar will be used.");
                });
            FriendListController.AddItem(friendToAdd);
            friendMessages[friendToAdd] = "";
        }

        public void OnItemSelected(int index)
        {
            string oldName = FriendName.text;
            if (ChatText.text != "")
            {
                friendMessages[oldName] = ChatText.text;
            }

            string friendName = FriendListController.GetFriend(index);
            FriendName.text = friendName;

            if (friendMessages.ContainsKey(friendName))
            {
                ChatText.text = friendMessages[friendName];
            }
            else
            {
                ChatText.text = "";
            }

            if (ConnectionHandler.Instance.IsLogged())
            {
                FocusOnInput();
            }
        }

        public void OnMessage(WrapperMessage message)
        {
            ChatMessage chatMessage = message.ChatMessage;

            string from = chatMessage.Sender;
            if (string.IsNullOrEmpty(from))
            {
                // we do not know from whom the message comes
                return;
            }

            List<string> friends = FriendListController.GetAllFriends();

            if (!friends.Contains(from))
            {
                AddFriendToList(from);
            }

            AppendNewMessage(from, chatMessage.Message);
        }

        private void AppendNewMessage(string friend, string message)
        {
            if (friend == FriendName.text)
            {
                // update current chat
                if (ChatText.text != "")
                {
                    ChatText.text += "\n";
                }
                ChatText.text += friend + ": " + message;
            }
            else
            {
                if (friendMessages[friend] != "")
                {
                    friendMessages[friend] += "\n";
                }
                friendMessages[friend] += friend + ": " + message;
                FriendListController.IncrementUnreadCount(friend);
            }
        }
        
    }
}