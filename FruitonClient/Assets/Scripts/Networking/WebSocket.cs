using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Networking
{
    public class WebSocket
    {
        private readonly string XAuthTokenHeaderKey = "x-auth-token";

        private Uri url;
        private string loginToken;

        private WebSocketSharp.WebSocket socket;
        private Queue<byte[]> messages = new Queue<byte[]>();
        private bool isConnected;
        private string error;

        public WebSocket(Uri url, string loginToken)
        {
            this.url = url;
            this.loginToken = loginToken;

            string protocol = this.url.Scheme;
            if (!protocol.Equals("ws") && !protocol.Equals("wss"))
            {
                throw new ArgumentException("Unsupported protocol: " + protocol);
            }
        }

        public IEnumerator Connect(Action onSucessAction = null, Action onErrorAction = null)
        {
            socket = new WebSocketSharp.WebSocket(url.ToString());
            socket.AddRequestHeader(XAuthTokenHeaderKey, loginToken);
            socket.OnMessage += (sender, e) => messages.Enqueue(e.RawData);
            socket.OnOpen += (sender, e) =>
            {
                Debug.Log("Opened WebSocket connection");
                isConnected = true;
                if (onSucessAction != null)
                {
                    TaskManager.Instance.RunOnMainThread(onSucessAction);
                }
            };
            socket.OnError += (sender, e) =>
            {
                Debug.LogError("WebSocket: " + e.Message);
                error = e.Message;
                if (onErrorAction != null)
                {
                    TaskManager.Instance.RunOnMainThread(onErrorAction);
                }
            };
            socket.ConnectAsync();

            while (!isConnected && error == null)
            {
                yield return 0;
            }
        }

        public void Send(byte[] buffer)
        {
            socket.Send(buffer);
        }

        public byte[] Recv()
        {
            if (messages.Count == 0)
            {
                return null;
            }
            return messages.Dequeue();
        }

        public void SendString(string str)
        {
            Send(Encoding.UTF8.GetBytes(str));
        }

        public string RecvString()
        {
            byte[] retval = Recv();
            if (retval == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(retval);
        }

        public void Close()
        {
            socket.Close();
        }

        public bool IsAlive()
        {
            return socket.IsAlive;
        }
        
    }
}