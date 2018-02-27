using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cz.Cuni.Mff.Fruiton.Dto;
using Google.Protobuf.Collections;
using Networking;
using UI.Notification;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace UI.Chat
{
    /// <summary>
    /// Handles chat panel, friend list, chat messaging and friend requests
    /// </summary>
    public class ChatController : MonoBehaviour, IOnItemSelectedListener, IOnMessageListener
    {
        /// <summary>
        /// Stores chat history with a friend and additional chat status information
        /// </summary>
        class ChatRecord
        {
            /// <summary>
            /// List of previous chat messages
            /// One entry may contain more than one message, up to the character limit of single unity text gameObject
            /// </summary>
            public List<string> Messages = new List<string>();
            /// <summary>
            /// ID of oldest chat message that was loaded from server
            /// </summary>
            public string LastMessageId;
            /// <summary>
            /// Indicates whether the game is currently loading older messages for given friend
            /// </summary>
            public bool Loading;
            /// <summary>
            /// True if every chat message was already loaded from the server
            /// </summary>
            public bool LoadedEveryMessage;

            public ChatRecord()
            {
                Messages.Add("");
            }
        }

        /// <summary>
        /// Approximate character limit for single text gameObject in Unity.
        /// Text mesh cannot have more than 65534 vertices, Unity UI uses
        /// around 20 vertices per character on average.
        /// </summary>
        static readonly int GAME_OBJECT_TEXT_LIMIT = 3000;

        static readonly string MESSAGE_COLOR_SENT_OLD = "#585850";
        static readonly string MESSAGE_COLOR_SENT_NEW = "#443328";
        static readonly string MESSAGE_COLOR_RECEIVED_OLD = "#333344";
        static readonly string MESSAGE_COLOR_RECEIVED_NEW = "#222266";

        /// <summary>
        /// Time format for messages received on the same day as they were sent
        /// </summary>
        private static readonly string TIME_FORMAT_TODAY = "HH:mm";
        /// <summary>
        /// Time format for messages that were sent day before or earlier
        /// </summary>
        private static readonly string TIME_FORMAT_OLDER = "dd.MM.yyyy HH:mm";

        public static ChatController Instance { get; private set; }

        public string SelectedPlayerLogin { get; private set; }
        public bool IsSelectedPlayerFriend { get; private set; }
        public bool IsSelectedPlayerInMenu { get; private set; }
        public bool IsSelectedPlayerOnline { get; private set; }

        public GameObject LoadingIndicator;
        public Text ChatTextTemplate;
        public InputField MessageInput;
        public InputField AddFriendInput;

        public FriendListController FriendListController;

        public Text FriendName;

        public GameObject ChatPanel;

        public GameObject ChatWindow;
        public Text ChatTip;

        public Dropdown FriendActionsDropdown;

        public ScrollRect ScrollRect;
        public RectTransform ScrollContent;

        private readonly Dictionary<string, ChatRecord> chatRecords = new Dictionary<string, ChatRecord>();

        private readonly List<IOnFriendsChangedListener> onFriendsChangedListeners = new List<IOnFriendsChangedListener>();
        /// <summary>
        /// List of text gameObjects that are used to display chat messages
        /// </summary>
        private readonly List<Text> ChatTexts = new List<Text>();
        private readonly DateTime unixTimeStart = new DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

        private RepeatedField<string> playersOnSameNetwork;

#if UNITY_ANDROID
        private RectTransform chatPanelRect;
#endif
        /// <summary>
        /// Initializes friend list and shows friend reuquest notifications
        /// </summary>
        public void Initialize()
        {
            foreach (Friend f in GameManager.Instance.Friends)
            {
                AddContactToList(f.Login, f.Status);
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
            ChatTextTemplate.gameObject.SetActive(false);

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

            ScrollRect.onValueChanged.AddListener(_ =>
            {
                // even if scrollbar is at the top sometimes it returns just 0.9999999 instead of 1
                if (ScrollRect.verticalNormalizedPosition >= 0.999) 
                {
                    LoadPreviousMessages(FriendName.text);
                }
            });

            FriendListController.SetOnItemSelectedListener(this);

            if (GameManager.Instance.IsOnline)
            {
                Initialize();
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

        /// <summary>
        /// Checks for enter key presses when an input field is focus
        /// </summary>
        private void Update()
        {
            if (!ChatPanel.activeInHierarchy)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
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
            // if android on screen keyboard is active make the chat panel smaller
            // so user can see most recent messages
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

        /// <summary>
        /// Clears friend list
        /// Used when user is logged out
        /// </summary>
        public void Clear()
        {
            FriendName.text = "";

            MessageInput.enabled = false;
            MessageInput.text = "";

            FriendListController.Clear();
            FriendListController.SetOnItemSelectedListener(this);
        }

        /// <summary>
        /// Shows chat panel
        /// </summary>
        public static void Show()
        {
            if (GameManager.Instance.IsOnline)
            {
                Instance.ChatPanel.SetActive(true);
                if (Instance.FriendName.text.IsNullOrEmpty())
                {
                    Instance.ChatWindow.SetActive(false);
                }
                if (Instance.chatRecords.Count == 0)
                {
                    Instance.ChatTip.text = "You don't have any friends :(";
                }
            }
            ChallengeController.Instance.Hide();
        }

        /// <summary>
        /// Hides chat panel
        /// </summary>
        public void Hide()
        {
            ChatPanel.SetActive(false);
        }

        /// <summary>
        /// Sends chat message from message input to server and resets the input
        /// </summary>
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
                Sender = GameManager.Instance.UserName,
                Timestamp = (long) (DateTime.UtcNow - unixTimeStart).TotalSeconds
            };
            var wsMessage = new WrapperMessage
            {
                ChatMessage = chatMessage
            };

            ConnectionHandler.Instance.SendWebsocketMessage(wsMessage);
            AppendNewMessage(chatMessage);

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

        /// <summary>
        /// Checks if username in add friend input is valid, sends friend requests to the server, displays notification
        /// </summary>
        public void OnAddFriendClick()
        {
            string friendToAdd = AddFriendInput.text.Trim();
            AddFriendInput.text = "";

            if (friendToAdd.IsNullOrEmpty())
            {
                return; // ignore
            }

            if (GameManager.Instance.Friends.Select(f => f.Login).Contains(friendToAdd))
            {
                NotificationManager.Instance.ShowError("Invalid action", friendToAdd + " is already your friend!");
                return;
            }

            if (!ConnectionHandler.Instance.IsLogged())
            {
                return; // cannot check if user exists
            }

            if (GameManager.Instance.UserName.Equals(friendToAdd))
            {
                NotificationManager.Instance.ShowError("Invalid action", "You cannot add yourself as a friend!");
                return;
            }

            PlayerHelper.Exists(friendToAdd, exists =>
            {
                if (exists)
                {
                    SendFriendRequest(friendToAdd);
                    NotificationManager.Instance.ShowSuccess("Friend request sent!", friendToAdd + " will be notified about your request");
                }
                else
                {
                    NotificationManager.Instance.ShowError("User not found!", "User with name " + friendToAdd + " does not exist!");
                }
            }, err =>
            {
                NotificationManager.Instance.ShowError("User not found!", "Couldn't find user named " + friendToAdd + "!");
            });
        }

        /// <summary>
        /// Handles selecting option from dropdown in chat window
        /// </summary>
        /// <param name="option">id of selected option</param>
        public void OnDropdownOption(int option)
        {
            // reset dropdown value to make it work like a button
            FriendActionsDropdown.value = ChatDropdownOption.CANCEL;
            switch (option)
            {
                case ChatDropdownOption.SHOW_PROFILE:
                    ConnectionHandler.Instance.OpenUrlAuthorized("profile/" + Uri.EscapeDataString(FriendName.text));
                    break;
                case ChatDropdownOption.CHALLENGE:
                    ChallengeController.Instance.Show();
                    break;
                case ChatDropdownOption.OFFER_FRUITON:
                    Scenes.Load(Scenes.LOCAL_TRADE_SCENE, Scenes.OFFERED_PLAYER_LOGIN, SelectedPlayerLogin);
                    break;
                case ChatDropdownOption.DELETE_FRIEND:
                    FriendRemoval removalMessage = new FriendRemoval
                    {
                        Login = FriendName.text
                    };

                    WrapperMessage ws = new WrapperMessage
                    {
                        FriendRemoval = removalMessage
                    };
                    ConnectionHandler.Instance.SendWebsocketMessage(ws);
                    OnFriendRemoval(removalMessage);
                    break;
                case ChatDropdownOption.CANCEL:
                    return;
            }
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

        /// <summary>
        /// Loads user's status from server and adds them to the friend list
        /// </summary>
        /// <param name="friendToAdd">username of user to add to the friend list</param>
        public void AddFriend(string friendToAdd)
        {
            PlayerHelper.GetPlayerStatus(friendToAdd,
                status => { AddFriend(friendToAdd, status); },
                error =>
                {
                    Debug.LogError("Could not get player's status " + error);
                    AddFriend(friendToAdd, Status.Offline);
                });
        }

        /// <summary>
        /// Add a user to the friend list
        /// </summary>
        /// <param name="friendToAdd">username of user to add to the friend list</param>
        /// <param name="status">status of user to add</param>
        public void AddFriend(string friendToAdd, Status status)
        {
            AddContactToList(friendToAdd, status);

            Friend f = new Friend
            {
                Login = friendToAdd,
                Status = status
            };
            GameManager.Instance.AddFriend(f);

            foreach (IOnFriendsChangedListener listener in onFriendsChangedListeners)
            {
                listener.OnFriendAdded();
            }
        }

        private void AddContactToList(string login, Status status, bool isFriend = true)
        {
            PlayerHelper.GetAvatar(login,
                texture => { FriendListController.SetAvatar(login, texture); },
                error =>
                {
                    Debug.LogWarning("Could not get avatar for user " + login +
                                     ". Default avatar will be used.");
                });
            FriendListController.AddItem(login, status, isFriend);
            if (isFriend)
            {
                chatRecords[login] = new ChatRecord();
            }
            ChatTip.text = "Select a player to challenge or chat with a friend";
        }

        private void RemoveFromContactList(string login)
        {
            FriendListController.RemoveItem(login);
        }

        /// <summary>
        /// Opens chat or challenge window for selected contact, loads stored chat messages
        /// </summary>
        /// <param name="index">index of selected contact</param>
        public void OnContactSelected(int index)
        {
            var friend = FriendListController.GetFriend(index);
            var login = friend.Name;

            SelectedPlayerLogin = login;
            IsSelectedPlayerFriend = chatRecords.ContainsKey(login);
            IsSelectedPlayerInMenu = friend.Status == Status.MainMenu;
            IsSelectedPlayerOnline = friend.Status != Status.Offline;

            if (!IsSelectedPlayerFriend)
            {
                ChatWindow.SetActive(false);
                ChallengeController.Instance.Show();
                FriendName.text = "";
                return;
            }

            ChallengeController.Instance.Hide();

            if (login == FriendName.text)
            {
                return;
            }

            foreach (var chatText in ChatTexts)
            {
                Destroy(chatText.gameObject);
            }
            ChatTexts.Clear();

            FriendName.text = login;

            if (chatRecords.ContainsKey(login))
            {
                foreach (var messages in chatRecords[login].Messages)
                {
                    CreateNewChatText(messages);
                }
                if (chatRecords[login].LastMessageId == null)
                {
                    LoadPreviousMessages(login);
                }
            }
            else
            {
                CreateNewChatText();
            }

            Canvas.ForceUpdateCanvases();
            ScrollRect.verticalNormalizedPosition = 0;

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
                case WrapperMessage.MessageOneofCase.FriendRemoval:
                    OnFriendRemoval(message.FriendRemoval);
                    break;
                case WrapperMessage.MessageOneofCase.StatusChange:
                    OnStatusChange(message.StatusChange);
                    break;
                case WrapperMessage.MessageOneofCase.PlayersOnSameNetworkOnline:
                    OnPlayersOnSameNetworkOnline(message.PlayersOnSameNetworkOnline);
                    break;
                case WrapperMessage.MessageOneofCase.PlayerOnSameNetworkOffline:
                    OnPlayerOnSameNetworkOffline(message.PlayerOnSameNetworkOffline);
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

            AppendNewMessage(chatMessage);
        }

        private void OnFriendRequestResult(FriendRequestResult message)
        {
            if (message.FriendshipAccepted)
            {
                AddFriend(message.FriendToAdd);
            }
        }

        private void OnFriendRemoval(FriendRemoval message)
        {
            var friend = message.Login;
            FriendListController.RemoveItem(friend);
            chatRecords.Remove(friend);
            if (friend == FriendName.text)
            {
                // Unity dropdown crashed if we disable chat window without closing the dropdown first
                StartCoroutine(CloseChatWindowWithDelay());
                FriendName.text = "";
            }
            GameManager.Instance.RemoveFriend(friend);
            foreach (IOnFriendsChangedListener listener in onFriendsChangedListeners)
            {
                listener.OnFriendRemoved();
            }

            UpdatePlayersOnSameNetwork();
        }

        private void OnStatusChange(StatusChange message)
        {
            FriendListController.ChangeStatus(message.Login, message.Status);
            var friend = GameManager.Instance.Friends.FirstOrDefault(f => f.Login == message.Login);
            if (friend != null)
            {
                friend.Status = message.Status;
            }
            if (message.Login == SelectedPlayerLogin)
            {
                IsSelectedPlayerInMenu = message.Status == Status.MainMenu;
                IsSelectedPlayerOnline = message.Status != Status.Offline;
                ChallengeController.Instance.Refresh();
            }
        }

        private void OnPlayersOnSameNetworkOnline(PlayersOnSameNetworkOnline message)
        {
            playersOnSameNetwork = message.Logins;
            UpdatePlayersOnSameNetwork();
        }

        private void UpdatePlayersOnSameNetwork()
        {
            foreach (var login in playersOnSameNetwork)
            {
                if (!chatRecords.ContainsKey(login))
                {
                    AddContactToList(login, Status.MainMenu, false);
                }
            }
        }

        private void OnPlayerOnSameNetworkOffline(PlayerOnSameNetworkOffline message)
        {
            if (!chatRecords.ContainsKey(message.Login))
            {
                RemoveFromContactList(message.Login);
            }
        }

        /// <summary>
        /// Adds chat message to chat records and chat window
        /// (if chat with given user is currently selected) 
        /// </summary>
        /// <param name="msg"></param>
        private void AppendNewMessage(ChatMessage msg)
        {
            var sender = msg.Sender;
            var friend = sender == GameManager.Instance.UserName ? msg.Recipient : sender;
            var textToAppend = FormatChatMessage(msg).ToString();
            var addToNewString = false;

            if (friend == FriendName.text)
            {
                // update current chat
                var lastChatText =  ChatTexts.Last();
                if(lastChatText.text.Length + msg.Message.Length > GAME_OBJECT_TEXT_LIMIT)
                {
                    addToNewString = true;
                    lastChatText = CreateNewChatText();
                }
                if (lastChatText.text != "")
                {
                    lastChatText.text += "\n";
                }
                lastChatText.text += textToAppend;
                if (ScrollRect.verticalNormalizedPosition <= 0)
                {
                    Canvas.ForceUpdateCanvases();
                    ScrollRect.verticalNormalizedPosition = 0;
                }
            }

            // append message to chat records
            var last = chatRecords[friend].Messages.Count - 1;
            if (addToNewString || chatRecords[friend].Messages[last].Length + msg.Message.Length > GAME_OBJECT_TEXT_LIMIT)
            {
                chatRecords[friend].Messages.Add("");
                last++;
            }
            if (chatRecords[friend].Messages[last] != "")
            {
                chatRecords[friend].Messages[last] += "\n";
            }
            chatRecords[friend].Messages[last] += textToAppend;
            FriendListController.IncrementUnreadCount(friend);
        }

        /// <summary>
        /// Adds messages to the beggining of chat
        /// </summary>
        /// <param name="friendName">friend that messages belong to</param>
        /// <param name="messages">text messages to add</param>
        /// <returns>height of prepended gameObject, 0 if chat with given friend isn't active</returns>
        private float PrependOldMessages(string friendName, string messages)
        {
            var height = 0f;
            if (FriendName.text == friendName)
            {
                var addedChatText = CreateNewChatText(messages, true);
                Canvas.ForceUpdateCanvases();
                height = addedChatText.GetComponent<RectTransform>().rect.height;
            }
            chatRecords[friendName].Messages.Insert(0, messages);
            return height;
        }

        /// <summary>
        /// Creates new text game object for chat messages
        /// </summary>
        /// <param name="messages">messages to insert to the gameObject</param>
        /// <param name="old">true if messages were loaded from previous sessions</param>
        /// <returns>Text component of the created game object</returns>
        private Text CreateNewChatText(string messages = "", bool old = false)
        {
            var newTextBlock = Instantiate(ChatTextTemplate);
            newTextBlock.gameObject.SetActive(true);
            newTextBlock.text = messages;
            newTextBlock.transform.SetParent(ChatTextTemplate.transform.parent, false);
            if (old)
            {
                // put old messages at the top (after loading indicator)
                newTextBlock.transform.SetSiblingIndex(1);
                ChatTexts.Insert(0, newTextBlock);
            }
            else
            {
                ChatTexts.Add(newTextBlock);
            }
            return newTextBlock;
        }

        /// <summary>
        /// Load messages from previous session with a friend
        /// </summary>
        private void LoadPreviousMessages(string friendName)
        {
            var record = chatRecords[friendName];
            if (record.Loading || record.LoadedEveryMessage)
            {
                return;
            }
            record.Loading = true;
            RefreshLoadingIndicator();
            if (record.LastMessageId == null)
            {
                PlayerHelper.GetMessagesWith(friendName, 0,
                    (msgs, page) => OnLoadMessagesSuccess(friendName, msgs, true),
                    error => OnLoadMessagesError(friendName, error));
            }
            else
            {
                PlayerHelper.GetMessagesBefore(record.LastMessageId, 0,
                    (msgs, page) => OnLoadMessagesSuccess(friendName, msgs),
                    error => OnLoadMessagesError(friendName, error));

            }
        }

        /// <summary>
        /// Splits loaded messages in smaller parts to fit maximum text limit
        /// and adds them to the chat records and chat window
        /// </summary>
        /// <param name="friendName">username of user that chat messages belong to</param>
        /// <param name="msgs">list of loaded chat messages</param>
        /// <param name="initialLoad">true if the sroll bar in chat window should be moved to the bottom</param>
        private void OnLoadMessagesSuccess(string friendName, ChatMessages msgs, bool initialLoad = false)
        {
            chatRecords[friendName].Loading = false;
            RefreshLoadingIndicator();

            if (msgs.Messages.Count == 0)
            {
                chatRecords[friendName].LoadedEveryMessage = true;
                return;
            }

            var loadedMessages = new StringBuilder();
            bool first = true;
            var loadedMessagesHeight = 0f;
            foreach (var msg in msgs.Messages.Reverse())
            {
                if (!first)
                {
                    loadedMessages.AppendLine();
                }
                else
                {
                    chatRecords[friendName].LastMessageId = msg.Id;
                }
                first = false;
                var newMessage = FormatChatMessage(msg, true);
                if (loadedMessages.Length + newMessage.Length > GAME_OBJECT_TEXT_LIMIT)
                {
                    loadedMessagesHeight += PrependOldMessages(friendName, loadedMessages.ToString());
                    loadedMessages = new StringBuilder();
                }
                loadedMessages.Append(newMessage);
            }
            loadedMessagesHeight += PrependOldMessages(friendName, loadedMessages.ToString());

            if (initialLoad)
            {
                Canvas.ForceUpdateCanvases();
                ScrollRect.verticalNormalizedPosition = 0;
            }
            else
            {
                ScrollContent.anchoredPosition = new Vector2(0, loadedMessagesHeight);
            }
        }

        private void OnLoadMessagesError(string friendName, string error)
        {
            chatRecords[friendName].Loading = false;
            RefreshLoadingIndicator();
            Debug.LogError("Failed to load chat history with " + friendName + ": " + error);
        }

        /// <summary>
        /// Shows or hides loading indicator based on the status of current conversation
        /// </summary>
        private void RefreshLoadingIndicator()
        {
            LoadingIndicator.SetActive(chatRecords[FriendName.text].Loading);
        }

        /// <summary>
        /// Adds username, time and color to the chat message
        /// </summary>
        /// <param name="msg">chat message to format</param>
        /// <param name="old">true if message is from previous session</param>
        /// <returns></returns>
        private StringBuilder FormatChatMessage(ChatMessage msg, bool old=false)
        {
            string color;
            if (msg.Sender == GameManager.Instance.UserName)
            {
                color = old ? MESSAGE_COLOR_SENT_OLD : MESSAGE_COLOR_SENT_NEW;
            }
            else
            {
                color = old ? MESSAGE_COLOR_RECEIVED_OLD : MESSAGE_COLOR_RECEIVED_NEW;
            }
            var time = unixTimeStart.AddSeconds(msg.Timestamp).ToLocalTime();
            var timeFormat = TIME_FORMAT_OLDER;
            if (time.Date == DateTime.Today)
            {
                timeFormat = TIME_FORMAT_TODAY;
            }
            return new StringBuilder("<color=")
                .Append(color)
                .Append("><b>")
                .Append(msg.Sender)
                .Append("</b> [")
                .Append(time.ToString(timeFormat))
                .Append("]: ")
                .Append(msg.Message)
                .Append("</color>");
        }

        /// <summary>
        /// Closes chat dropdown and then chat window after a small delay
        /// Used as workaround for unity issue #892913
        /// </summary>
        private IEnumerator CloseChatWindowWithDelay()
        {
            yield return 1;
            FriendActionsDropdown.Hide();
            yield return new WaitForSecondsRealtime(0.3f);
            ChatWindow.SetActive(false);
        }

        /// <summary>
        /// Registers new listener for friend count changed event
        /// </summary>
        /// <param name="listener">listener object to register</param>
        public void AddListener(IOnFriendsChangedListener listener)
        {
            onFriendsChangedListeners.Add(listener);
        }

        /// <summary>
        /// Removes listener from listening on friend count changed event
        /// </summary>
        /// <param name="listener">listener object to remove</param>
        public void RemoveListener(IOnFriendsChangedListener listener)
        {
            onFriendsChangedListeners.Remove(listener);
        }

        public interface IOnFriendsChangedListener
        {
            void OnFriendAdded();
            void OnFriendRemoved();
        }



#if UNITY_ANDROID
        /// <summary>
        /// Calculates size of android on screen keyboard
        /// </summary>
        /// <returns>size of android on screen keyboard</returns>
        private int GetKeyboardSize()
        {
            using (AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject View =
UnityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");

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