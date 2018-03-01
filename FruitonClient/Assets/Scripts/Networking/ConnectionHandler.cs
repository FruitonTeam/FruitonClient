//#define LOCAL_SERVER

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
    /// Singleton used for handling http requests and websocket connection with the server.   
    /// </summary>
    public class ConnectionHandler : MonoBehaviour, IOnMessageListener
    {
        private static readonly string X_AUTH_TOKEN_HEADER_KEY = "x-auth-token";

        private static readonly string SET_COOKIE_KEY = "SET-COOKIE";

        private static readonly string COOKIE_KEY = "Cookie";

        private static readonly string SERVER_IP = "195.113.20.59";

#if LOCAL_SERVER
        private static readonly string URL_WEB = "http://localhost:8050/";
        private static readonly string URL_WS = "ws://localhost:8050/socket";
#else
        private static readonly string URL_WEB = "http://prak.mff.cuni.cz:8050/fruiton/";
        private static readonly string URL_WS = "ws://prak.mff.cuni.cz:8050/fruiton/socket";
#endif
        private static readonly string URL_API = URL_WEB + "api/";

        private static readonly string SERVER_DOWN_MESSAGE = "Server unreachable. Please check your internet connection and try again later.";

        /// <summary>
        /// Time (in seconds) to wait for ping when reconnecting.
        /// </summary>
        private static readonly float PING_TIMEOUT = 5;

        /// <summary>
        /// Time (in seconds) remaining for server to respond to ping.
        /// </summary>
        private float pingTimer = PING_TIMEOUT;

        public WebSocket webSocket;

        /// <summary>
        /// Authorization token for current session.
        /// </summary>
        private string token;

        /// <summary>
        /// Cookies for current session.
        /// </summary>
        private string cookies;

        /// <summary>
        /// Dictionary of registered websocket message listeners.
        /// </summary>
        private Dictionary<WrapperMessage.MessageOneofCase, List<IOnMessageListener>> listeners =
            new Dictionary<WrapperMessage.MessageOneofCase, List<IOnMessageListener>>();

        public static ConnectionHandler Instance { get; private set; }

        private ConnectionHandler()
        {
        }

        /// <summary>
        /// Checks if the connection is alive.
        /// </summary>
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

        /// <summary>
        /// Pings the server then tries to reconnect if the ping is successful.
        /// </summary>
        private IEnumerator PingServer()
        {
            Ping ping = new Ping(SERVER_IP);
            while (pingTimer > 0 && !ping.isDone)
            {
                pingTimer -= Time.deltaTime;
                yield return null;
            }
            pingTimer = PING_TIMEOUT;
            if (ping.isDone)
            {
                Reconnect();
            }
            else
            {
                UnableToReconnect();
            }
        }

        /// <summary>
        /// Opens page on fruiton website in browser authorized with current session token.
        /// </summary>
        /// <param name="pagePath">path of the page to open</param>
        public void OpenUrlAuthorized(string pagePath)
        {
            Application.OpenURL(URL_WEB + pagePath + "?" + X_AUTH_TOKEN_HEADER_KEY + "=" + token);
        }

        /// <summary>
        /// Sends `GET` request to server.
        /// </summary>
        /// <param name="query">request url</param>
        /// <param name="success">action to perform when request succeeds</param>
        /// <param name="error">action to perform when request fails</param>
        public IEnumerator Get(string query, Action<string> success, Action<string> error)
        {
            return Get(query, www => success(www.text), error);
        }

        /// <summary>
        /// Sends `GET` request to server.
        /// </summary>
        /// <param name="query">request url</param>
        /// <param name="success">action to perform when request succeeds</param>
        /// <param name="error">action to perform when request fails</param>
        public IEnumerator Get(string query, Action<byte[]> success, Action<string> error)
        {
            return Get(query, www => success(www.bytes), error);
        }

        /// <summary>
        /// Sends `GET` request to server.
        /// </summary>
        /// <param name="query">request url</param>
        /// <param name="success">action to perform when request succeeds</param>
        /// <param name="error">action to perform when request fails</param>
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

        /// <summary>
        /// Sets current cookies.
        /// </summary>
        /// <param name="www">www object to take cookies from</param>
        private void SetCookies(WWW www)
        {
            if (www.responseHeaders.ContainsKey(SET_COOKIE_KEY))
            {
                cookies = www.responseHeaders[SET_COOKIE_KEY];
            }
        }

        /// <summary>
        ///  Sends `POST` request to server.
        /// </summary>
        /// <param name="query">request url</param>
        /// <param name="success">action to perform when request succeeds</param>
        /// <param name="error">action to perform when request fails</param>
        /// <param name="body">request body</param>
        /// <param name="headers">request headers</param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates dictionary containing token and cookies.
        /// </summary>
        /// <param name="headers">preexisting headers dictionary to add token and cookies to</param>
        /// <returns>headers dictionary containing token and cookies</returns>
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

        /// <summary>
        /// Sends websocket message to server.
        /// </summary>
        /// <param name="message">websocket message to send</param>
        public void SendWebsocketMessage(IMessage message)
        {
            if (!IsLogged())
            {
                return;
            }
            webSocket.Send(ProtobufUtils.GetBinaryData(message));
        }

        /// <summary>
        /// Creates websocket connection to server.
        /// </summary>
        /// <param name="token">authorization token</param>
        public void Connect(string token)
        {
            this.token = token;
            webSocket = new WebSocket(new Uri(URL_WS), token);
            StartCoroutine(webSocket.Connect(OnConnected));
        }

        /// <summary>
        /// Registers websocket message listeners.
        /// </summary>
        private void OnConnected()
        {
            RegisterListener(WrapperMessage.MessageOneofCase.ErrorMessage, ServerErrorHandler.Instance);
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

        /// <returns>true if websocket connection is active</returns>
        public bool IsLogged()
        {
            return webSocket != null;
        }

        /// <returns>true if websocket connection to server is alive</returns>
        public bool IsConnectionAlive()
        {
            return webSocket.IsAlive();
        }

        /// <summary>
        /// Hides all notifications, closes websocket connection, removes all websocket message listeners, clears cookies and authorization token.
        /// </summary>
        public void Disconnect()
        {
            NotificationManager.Instance.Clear();
            FeedbackNotificationManager.Instance.Clear();
            if (webSocket != null)
            {
                webSocket.Close();    
            }
            listeners = new Dictionary<WrapperMessage.MessageOneofCase, List<IOnMessageListener>>();
            cookies = string.Empty;
            token = string.Empty;
        }

        /// <summary>
        /// Disconnects from server and destroys websocket connection.
        /// </summary>
        public void Logout()
        {
            Disconnect();
            webSocket = null;
        }

        /// <summary>
        /// Tries to reconnect to server
        /// </summary>
        public void Reconnect()
        {
            StartCoroutine(webSocket.Connect(OnConnected, UnableToReconnect));
        }

        /// <summary>
        /// Loads login scene.
        /// </summary>
        private void UnableToReconnect()
        {
            Logout();
            Scenes.Load(Scenes.LOGIN_SCENE, Scenes.DISCONNECTED, true);
        }

        /// <summary>
        /// Checks if any websocket message was received from the server and alerts listeners
        /// </summary>
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

                if (!IsLogged())
                    break;

                message = webSocket.Recv();
            }
        }

        /// <summary>
        /// Parses websocket message from incoming byte array and alerts corresponding listeners.
        /// </summary>
        /// <param name="message">byte array to parse</param>
        private void OnMessage(byte[] message)
        {
            var wrapperMsg = WrapperMessage.Parser.ParseFrom(message);
            Debug.Log("Received message: " + wrapperMsg);

            // Only connection handler may handle a disconnect message
            if (wrapperMsg.MessageCase == WrapperMessage.MessageOneofCase.Disconnected)
            {
                Logout();
                Scenes.Load(Scenes.LOGIN_SCENE, Scenes.SERVER_DISCONNECT, true);
            }
            else if (listeners.ContainsKey(wrapperMsg.MessageCase))
            {
                foreach (IOnMessageListener listener in listeners[wrapperMsg.MessageCase])
                {
                    listener.OnMessage(wrapperMsg);
                }
            }
        }

        /// <summary>
        /// Registers new websocket message listener.
        /// </summary>
        /// <param name="msgCase">message type for listener to listen to</param>
        /// <param name="listener">listener object</param>
        public void RegisterListener(WrapperMessage.MessageOneofCase msgCase, IOnMessageListener listener)
        {
            Debug.Assert(
                msgCase != WrapperMessage.MessageOneofCase.Disconnected,
                "Disconnect message is handled directly by the connection handler."
            );

            if (!listeners.ContainsKey(msgCase))
            {
                listeners[msgCase] = new List<IOnMessageListener>();
            } else if (listeners[msgCase].Contains(listener))
            {
                return; // do not allow to register the same object multiple times for the same message case
            }
            
            listeners[msgCase].Add(listener);
        }

        /// <summary>
        /// Removes registered websocket message listener.
        /// </summary>
        /// <param name="msgCase">message type to stop lisitening for</param>
        /// <param name="listener">listener object</param>
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

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        private void OnApplicationQuit()
        {
            Disconnect(); // explicitly close the connection so the server does not have to wait for timeout
        }
    }
}