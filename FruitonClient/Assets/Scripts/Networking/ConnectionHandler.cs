﻿using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly string XAuthTokenHeaderKey = "x-auth-token";
        
        //private static readonly string URL_WEB = "http://prak.mff.cuni.cz:8050/fruiton/";
        private static readonly string URL_WEB = "http://localhost:8050/";
        
        //private static readonly string URL_WS = "ws://prak.mff.cuni.cz:8050/fruiton/socket";
        private static readonly string URL_WS = "ws://localhost:8050/socket";

        private static readonly string URL_API = URL_WEB + "api/";
        
        private WebSocket webSocket;

        private string token;

        private Dictionary<WrapperMessage.MessageOneofCase, List<IOnMessageListener>> listeners =
            new Dictionary<WrapperMessage.MessageOneofCase, List<IOnMessageListener>>();

        public static ConnectionHandler Instance { get; private set; }

        private ConnectionHandler()
        {
        }

        public void OpenUrlAuthorized(string pageName)
        {
            Application.OpenURL(URL_WEB + pageName + "?" + XAuthTokenHeaderKey + "=" + token);
        }
        
        public IEnumerator Get(string query, Action<string> success, Action<string> error)
        {
            var www = new WWW(URL_API + query, null, AuthHeader());
            Debug.Log("www: " + URL_API + query);
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                success.Invoke(www.text);
            }
            else
            {
                error.Invoke(www.text);
            }
        }

        public IEnumerator Post(
            string query, 
            Action<string> success, 
            Action<string> error, 
            byte[] body = null,
            Dictionary<string, string> headers = null
        ) {
            var www = new WWW(URL_API + query, body, AuthHeader(headers));
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                success.Invoke(www.text);
            }
            else
            {
                error.Invoke(www.error);
            }
        }

        private Dictionary<string, string> AuthHeader(Dictionary<string, string> headers = null)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }
            if (!string.IsNullOrEmpty(token))
            {
                headers[XAuthTokenHeaderKey] = token;        
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
            
            StartCoroutine(webSocket.Connect());
    
            OnConnected();
        }

        private void OnConnected()
        {
            RegisterListener(WrapperMessage.MessageOneofCase.ErrorMessage, this);
            RegisterListener(WrapperMessage.MessageOneofCase.ChatMessage, ChatMessageNotifier.Instance);
            
            if (NotificationManager.Instance != null) // main menu was already created
            {
                RegisterListener(WrapperMessage.MessageOneofCase.Notification, NotificationManager.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.FriendRequest, FeedbackNotificationManager.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.ChatMessage, ChatController.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.FriendRequestResult, ChatController.Instance);
                RegisterListener(WrapperMessage.MessageOneofCase.OnlineStatusChange, ChatController.Instance);                
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
        }

        public void Logout()
        {
            Disconnect();
            webSocket = null;
        }

        public void Reconnect()
        {
            // TODO: implement
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