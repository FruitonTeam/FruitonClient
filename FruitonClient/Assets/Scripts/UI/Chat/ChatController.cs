using System;
using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UI.Notification;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Chat
{
    public class ChatController : MonoBehaviour, IOnItemSelectedListener, IOnMessageListener
    {
        
        public static ChatController Instance { get; private set; }
        
        public Text ChatText;
        public InputField MessageInput;

        public InputField AddFriendInput;

        public FriendListController FriendListController;

        public Text FriendName;

        public GameObject ChatPanel;

        private readonly Dictionary<string, string> friendMessages = new Dictionary<string, string>();

        private readonly List<IOnFriendAddedListener> onFriendAddedListeners = new List<IOnFriendAddedListener>();
        
        public void Init()
        {
            foreach (Friend f in GameManager.Instance.Friends)
            {
                AddFriendToList(f.Login, f.Status);
            }
            
            PlayerHelper.GetFriendRequests(requests =>
            {
                foreach (string friendRequestLogin in requests)
                {
                    FeedbackNotificationManager.Instance.ShowFriendRequest(friendRequestLogin);
                }
            }, Debug.LogError);
        }
        
        void Start()
        {
            FriendName.text = "";
            ChatText.text = "";

            MessageInput.enabled = false;
            MessageInput.text = "";
            
            FriendListController.SetOnItemSelectedListener(this);

            if (GameManager.Instance.IsOnline)
            {
                ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.ChatMessage, this);
                ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.FriendRequestResult, this);
                ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.OnlineStatusChange, this);
                
                Init();
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void Clear()
        {
            FriendName.text = "";
            ChatText.text = "";

            MessageInput.enabled = false;
            MessageInput.text = "";
            
            FriendListController.Clear();
            FriendListController.SetOnItemSelectedListener(this);
        }

        public static void Show()
        {
            if (GameManager.Instance.IsOnline)
            {
                Instance.ChatPanel.SetActive(true);
            }
        }

        public void Hide()
        {
            ChatPanel.SetActive(false);
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
                    SendFriendRequest(friendToAdd);
                }
                else
                {
                    Debug.LogWarning("No user with name " + friendToAdd);
                }
            }, err => { Debug.LogWarning("Error while checking player existence" + err); });
        }

        private void SendFriendRequest(string friendToAdd)
        {
            FriendRequest request = new FriendRequest
            {
                FriendToAdd = friendToAdd
            };

            WrapperMessage ws = new WrapperMessage
            {
                FriendRequest = request
            };
            ConnectionHandler.Instance.SendWebsocketMessage(ws);
        }

        public void AddFriend(string friendToAdd)
        {
            PlayerHelper.IsOnline(friendToAdd, 
                isOnline =>
                {
                    AddFriend(friendToAdd, isOnline ? Status.Online : Status.Offline);
                }, error =>
                {
                    Debug.LogError("Could not check if user is online " + error);
                    AddFriend(friendToAdd, Status.Offline);
                });
        }
        
        public void AddFriend(string friendToAdd, Status status)
        {
            AddFriendToList(friendToAdd, status);

            Friend f = new Friend
            {
                Login = friendToAdd,
                Status = status
            };
            GameManager.Instance.AddFriend(f);

            foreach (IOnFriendAddedListener listener in onFriendAddedListeners)
            {
                listener.OnFriendAdded();
            }
        }

        private void AddFriendToList(string friendToAdd, Status status)
        {
            PlayerHelper.GetAvatar(friendToAdd,
                texture => { FriendListController.SetAvatar(friendToAdd, texture); },
                error =>
                {
                    Debug.LogWarning("Could not get avatar for user " + friendToAdd +
                                     ". Default avatar will be used.");
                });
            FriendListController.AddItem(friendToAdd, status);
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
            switch (message.MessageCase)
            {
                case WrapperMessage.MessageOneofCase.ChatMessage:
                    OnChatMessage(message.ChatMessage);
                    break;
                case WrapperMessage.MessageOneofCase.FriendRequestResult:
                    OnFriendRequestResult(message.FriendRequestResult);
                    break;
                case WrapperMessage.MessageOneofCase.OnlineStatusChange:
                    OnOnlineStatusChange(message.OnlineStatusChange);
                    break;
                default:
                    throw new InvalidOperationException("Unknown message type " + message.MessageCase);
            }
        }

        private void OnChatMessage(ChatMessage chatMessage)
        {
            string from = chatMessage.Sender;
            if (string.IsNullOrEmpty(from))
            {
                Debug.LogError("Received chat message from unknown sender");
                return;
            }

            List<string> friends = FriendListController.GetAllFriends();

            if (!friends.Contains(from))
            {
                Debug.LogError("Received chat message from non-friend");
                return;
            }

            AppendNewMessage(from, chatMessage.Message);
        }

        private void OnFriendRequestResult(FriendRequestResult message)
        {
            if (message.FriendshipAccepted)
            {
                // if we get this message then the other friend must have accepted it in his game so he is online
                AddFriend(message.FriendToAdd, Status.Online);
            }
        }

        private void OnOnlineStatusChange(OnlineStatusChange message)
        {
            FriendListController.ChangeOnlineStatus(message.Login, message.Status);
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

        public void AddListener(IOnFriendAddedListener listener)
        {
            onFriendAddedListeners.Add(listener);
        }

        public void RemoveListener(IOnFriendAddedListener listener)
        {
            onFriendAddedListeners.Remove(listener);
        }

        public interface IOnFriendAddedListener
        {
            void OnFriendAdded();
        }
        
    }
}