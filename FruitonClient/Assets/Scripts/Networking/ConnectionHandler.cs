using System;
using System.Collections;
using System.Collections.Generic;
using Bazaar;
using Cz.Cuni.Mff.Fruiton.Dto;
using Google.Protobuf;
using UI.Chat;
using UI.Notification;
using UnityEngine;
using Util;

namespace Networking
{
    /// <summary>
    /// Singleton used for handling a connection with the server.
    /// </summary>
    public class ConnectionHandler : MonoBehaviour, IOnMessageListener
    {
        private static readonly string X_AUTH_TOKEN_HEADER_KEY = "x-auth-token";

        private static readonly string SET_COOKIE_KEY = "SET-COOKIE";

        private static readonly string COOKIE_KEY = "Cookie";

        private static readonly string SERVER_IP = "195.113.20.59";

        private static readonly string URL_WEB = "http://prak.mff.cuni.cz:8050/fruiton/";

        //private static readonly string URL_WEB = "http://localhost:8050/";

        private static readonly string URL_WS = "ws://prak.mff.cuni.cz:8050/fruiton/socket";
        //private static readonly string URL_WS = "ws://localhost:8050/socket";

        private static readonly string URL_API = URL_WEB + "api/";

        private static readonly string SERVER_DOWN_MESSAGE = "Server unreachable. Please check your internet connection and try again later.";

        // When reconnecting, wait for ping for this amount of seconds only.
        private float pingTimer = 5;

        public WebSocket webSocket;

        private string token;

        private string cookies;

        private Dictionary<WrapperMessage.MessageOneofCase, List<IOnMessageListener>> listeners =
            new Dictionary<WrapperMessage.MessageOneofCase, List<IOnMessageListener>>();

        public static ConnectionHandler Instance { get; private set; }

        private ConnectionHandler()
        {
        }
        
        // Check if connection is alive.
        public void CheckConnection()
        {
            // Not connected yet.
            if (webSocket == null) return;
            
            if (!webSocket.IsAlive())
            {
                StartCoroutine(PingServer());
            }
            else
            {
                Debug.Log("Websocket OK");
            }
        }

        private IEnumerator PingServer()
        {
            Ping ping = new Ping(SERVER_IP);
            while (pingTimer > 0 && !ping.isDone)
            {
                pingTimer -= Time.deltaTime;
                yield return null;
            }
            pingTimer = 5;
            if (ping.isDone)
            {
                Reconnect();
            }
            else
            {
                UnableToReconnect();
            }
        }

        public void OpenUrlAuthorized(string pageName)
        {
            Application.OpenURL(URL_WEB + pageName + "?" + X_AUTH_TOKEN_HEADER_KEY + "=" + token);
        }
        
        public IEnumerator Get(string query, Action<string> success, Action<string> error)
        {
            return Get(query, www => success(www.text), error);
        }
        
        public IEnumerator Get(string query, Action<byte[]> success, Action<string> error)
        {
            return Get(query, www => success(www.bytes), error);
        }

        private IEnumerator Get(string query, Action<WWW> success, Action<string> error)
        {
            var www = new WWW(URL_API + query, null, GetRequestHeaders());
            Debug.Log("www: " + URL_API + query);
            yield return www;

            SetCookies(www);

            if (string.IsNullOrEmpty(www.error))
            {
                success(www);
            }
            else
            {
                error(www.text);
            }
        }

        private void SetCookies(WWW www)
        {
            if (www.responseHeaders.ContainsKey(SET_COOKIE_KEY))
            {
                cookies = www.responseHeaders[SET_COOKIE_KEY];
            }
        }

        public IEnumerator Post(
            string query,
            Action<string> success,
            Action<string> error,
            byte[] body = null,
            Dictionary<string, string> headers = null
        ) {
            var www = new WWW(URL_API + query, body, GetRequestHeaders(headers));
            yield return www;

            SetCookies(www);
            
            if (string.IsNullOrEmpty(www.error))
            {
                success(www.text);
            }
            else
            {
                // if response doesn't contain any headers
                // we can assume that the request didn't reach the server
                // which means that the server is probably down
                error(www.responseHeaders.Count > 0 ? www.text : SERVER_DOWN_MESSAGE);
            }
        }

        private Dictionary<string, string> GetRequestHeaders(Dictionary<string, string> headers = null)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            if (!string.IsNullOrEmpty(token))
            {
                headers[X_AUTH_TOKEN_HEADER_KEY] = token;  
            }

            if (!string.IsNullOrEmpty(cookies))
            {
                headers[COOKIE_KEY] = cookies;
            }
            return headers;
        }

        public void SendWebsocketMessage(IMessage message)
        {
            if (!IsLogged())
            {
                return;
            }
            webSocket.Send(ProtobufUtils.GetBinaryData(message));
        }

        public void Connect(string token)
        {
            this.token = token;
            webSocket = new WebSocket(new Uri(URL_WS), token);
            StartCoroutine(webSocket.Connect(OnConnected));
        }

        private void OnConnected()
        {
            RegisterListener(WrapperMessage.MessageOneofCase.ErrorMessage, ErrorHandler.Instance);
            RegisterListener(WrapperMessage.MessageOneofCase.ChatMessage, ChatMessageNotifier.Instance);
            
            if (NotificationManager.Instance != null) // main menu was already created
            {
                RegisterListener(WrapperMessage.MessageOneofCase.Notification, NotificationManager.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.FriendRequest, FeedbackNotificationManager.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.ChatMessage, ChatController.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.FriendRequestResult, ChatController.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.FriendRemoval, ChatController.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.StatusChange, ChatController.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.PlayersOnSameNetworkOnline, ChatController.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.PlayerOnSameNetworkOffline, ChatController.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.Challenge, ChallengeController.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.ChallengeResult, ChallengeController.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.RevokeChallenge, ChallengeController.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.TradeOffer, TradeBazaar.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.BazaarOfferResolvedOnTheWeb, TradeBazaar.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.BazaarOfferResult, Bazaar.Bazaar.Instance);
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

        public bool IsLogged()
        {
            return webSocket != null;
        }

        public bool IsConnectionAlive()
        {
            return webSocket.IsAlive();
        }

        public void Disconnect()
        {
            if (webSocket != null)
            {
                webSocket.Close();    
            }
            listeners = new Dictionary<WrapperMessage.MessageOneofCase, List<IOnMessageListener>>();
            cookies = string.Empty;
            token = string.Empty;
        }

        public void Logout()
        {
            Disconnect();
            webSocket = null;
        }

        public void Reconnect()
        {
            StartCoroutine(webSocket.Connect(OnConnected, UnableToReconnect));
        }

        private void UnableToReconnect()
        {
            Scenes.Load(Scenes.LOGIN_SCENE, Scenes.DISCONNECTED, true);
        }

        private void Update()
        {
            if (!IsLogged())
            {
                return;
            }

            byte[] message = webSocket.Recv();
            while (message != null) // process every received message
            {
                OnMessage(message);
                message = webSocket.Recv();
            }
        }

        private void OnMessage(byte[] message)
        {
            var wrapperMsg = WrapperMessage.Parser.ParseFrom(message);
            Debug.Log("Received message: " + wrapperMsg);

            if (listeners.ContainsKey(wrapperMsg.MessageCase))
            {
                foreach (IOnMessageListener listener in listeners[wrapperMsg.MessageCase])
                {
                    listener.OnMessage(wrapperMsg);
                }
            }
        }

        public void RegisterListener(WrapperMessage.MessageOneofCase msgCase, IOnMessageListener listener)
        {
            if (!listeners.ContainsKey(msgCase))
            {
                listeners[msgCase] = new List<IOnMessageListener>();
            } else if (listeners[msgCase].Contains(listener))
            {
                return; // do not allow to register the same object multiple times for the same message case
            }
            
            listeners[msgCase].Add(listener);
        }

        public void UnregisterListener(WrapperMessage.MessageOneofCase msgCase, IOnMessageListener listener)
        {
            if (listeners.ContainsKey(msgCase))
            {
                listeners[msgCase].Remove(listener);
            }
        }

        public void OnMessage(WrapperMessage message)
        {
            Debug.LogError(message.ErrorMessage.Message);
        }

        private void OnApplicationQuit()
        {
            Disconnect(); // explicitly close the connection so the server does not have to wait for timeout
        }
    }
}