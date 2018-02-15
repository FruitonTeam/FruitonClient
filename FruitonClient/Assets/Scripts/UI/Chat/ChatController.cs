using System;
using System.Collections;
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

        public GameObject ChatWindow;
        public Text ChatTip;

        public Dropdown FriendActionsDropdown;

        public ScrollRect ScrollRect;

        private readonly Dictionary<string, string> friendMessages = new Dictionary<string, string>();

        private readonly List<IOnFriendAddedListener> onFriendAddedListeners = new List<IOnFriendAddedListener>();

#if UNITY_ANDROID
        private RectTransform chatPanelRect;
#endif
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
#if UNITY_ANDROID
            chatPanelRect = ChatPanel.GetComponent<RectTransform>();
#endif
            FriendName.text = "";
            ChatText.text = "";

            MessageInput.enabled = false;
            MessageInput.text = "";
            // don't allow user to put newlines at the beginning of the message
            MessageInput.onValueChanged.AddListener(text =>
            {
                if (text == "\n")
                {
                    MessageInput.text = "";
                }
            });


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

        private void Update()
        {
            if (!ChatPanel.activeInHierarchy)
            {
                return;
            }

            if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (MessageInput.isFocused)
                {
                    if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                    {
                        OnSendClick();
                    }
                }
                else if (AddFriendInput.isFocused)
                {
                    OnAddFriendClick();
                }
            }

#if UNITY_ANDROID
            if (TouchScreenKeyboard.visible)
            {
                chatPanelRect.offsetMin = new Vector2(chatPanelRect.offsetMin.x, GetKeyboardSize());
            }
            else
            {
                chatPanelRect.offsetMin = new Vector2(chatPanelRect.offsetMin.x, 0);
            }
#endif
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
                Instance.ChatWindow.SetActive(false);
                if (Instance.friendMessages.Count == 0)
                {
                    Instance.ChatTip.text = "You don't have any friends :(";
                }
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
            var message = MessageInput.text.TrimEnd(' ', '\n', '\r');
            if (message.Length == 0)
            {
                return;
            }
            var chatMessage = new ChatMessage
            {
                Message = message,
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

            ChatText.text += "<color=#444455><b>You</b>: " + message +"</color>";

            MessageInput.text = ""; // reset the input field
            Canvas.ForceUpdateCanvases();
            ScrollRect.verticalNormalizedPosition = 0;
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

        public void OnDropdownOption(int option)
        {
            // 0 - show profile
            // 1 - challenge
            // 2 - delete
            // 3 - ignore
            switch (option)
            {
                case 3:
                    return;
                case 0:
                    ConnectionHandler.Instance.OpenUrlAuthorized("profile/" + Uri.EscapeDataString(FriendName.text));
                    break;
                default:
                    // TODO: self-explanatory
                    Debug.LogWarning("THIS FEATURE IS NOT IMPLEMENTED YET");
                    break;
            }
            // reset dropdown value to make it work like a button
            FriendActionsDropdown.value = 3;
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
            ChatTip.text = "Select a friend to challenge or chat with";
        }

        private IEnumerator CancelMessageTextSelection()
        {
            yield return 0;
            MessageInput.MoveTextEnd(false);
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
            ChatWindow.SetActive(true);
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
            var textToAppend = "<color=black><b>" + friend + "</b>: " + message+"</color>";
            if (friend == FriendName.text)
            {
                // update current chat
                if (ChatText.text != "")
                {
                    ChatText.text += "\n";
                }
                ChatText.text += textToAppend;
                if (ScrollRect.verticalNormalizedPosition <= 0)
                {
                    Canvas.ForceUpdateCanvases();
                    ScrollRect.verticalNormalizedPosition = 0;
                }
            }
            else
            {
                if (friendMessages[friend] != "")
                {
                    friendMessages[friend] += "\n";
                }
                friendMessages[friend] += textToAppend;
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

#if UNITY_ANDROID

        private int GetKeyboardSize()
        {
            using (AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject View = UnityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");

                using (AndroidJavaObject Rct = new AndroidJavaObject("android.graphics.Rect"))
                {
                    View.Call("getWindowVisibleDisplayFrame", Rct);

                    return Screen.height - Rct.Call<int>("height");
                }
            }
        }
#endif
    }
}