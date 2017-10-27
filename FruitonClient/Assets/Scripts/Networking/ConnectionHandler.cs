﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Cz.Cuni.Mff.Fruiton.Dto;
using Google.Protobuf;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Networking
{
    /// <summary>
    /// Singleton used for handling a connection with the server.
    /// </summary>
    public class ConnectionHandler : MonoBehaviour, IOnMessageListener
    {
        private static readonly string URL_CHAT = "ws://prak.mff.cuni.cz:8050/fruiton/socket";
        private static readonly string URL_API = "http://prak.mff.cuni.cz:8050/fruiton/api/";
        
        private static readonly string GOOGLE_ID = 
            "827606142557-f63cu712orq80s6do9n6aa8s3eu3h7ag.apps.googleusercontent.com";
        private static readonly string GOOGLE_CLIENT_SECRET = "NyYlQJICuxYX3AnzChou2X8i";

        private static readonly int GOOGLE_REDIRECT_PORT = 9999;
        
        private static readonly string GOOGLE_REDIRECT_URI = "http://127.0.0.1:" + GOOGLE_REDIRECT_PORT;
        private static readonly string GOOGLE_TOKEN_URI = "https://www.googleapis.com/oauth2/v4/token";
        
        /// <summary>
        /// Dummy password for google users.
        /// </summary>
        private static readonly string GOOGLE_PASSWORD = "google_pwd";
        
        private static string googleLoginSuccessHtml;
        private static string googleLoginErrorHtml;
        
        private static ConnectionHandler instance;

        private string loginToken;

        private WebSocket webSocket;

        private static readonly string PROCESS_REGISTRATION_RESULT = "ProcessRegistrationResult";

        private Dictionary<WrapperMessage.MessageOneofCase, List<IOnMessageListener>> listeners =
            new Dictionary<WrapperMessage.MessageOneofCase, List<IOnMessageListener>>();

        private ConnectionHandler()
        {
        }

        public static ConnectionHandler Instance { get; private set; }

        /// <summary>
        /// Sends a registration request to the server.
        /// </summary>
        /// <param name="login"> Must be at least 6 characters long. </param>
        /// <param name="password"></param>
        /// <param name="email"> Must contain '@' </param>
        /// <param name="useProtobuf"> Determines whether protobuf encoding should be used. It is recommended to use protobuf. </param>
        public void Register(string login, string password, string email, bool useProtobuf)
        {
            var newUser = new RegistrationData
            {
                Login = login,
                Password = password,
                Email = email
            };

            var binaryData = new byte[newUser.CalculateSize()];
            var stream = new CodedOutputStream(binaryData);
            newUser.WriteTo(stream);
            var headers = GetRequestHeaders(useProtobuf);

            var www = new WWW(URL_API + "register", binaryData, headers);
            StartCoroutine(PostRegister(www));
        }

        public void LoginBasic(string login, string password, bool useProtobuf)
        {
            var loginData = new LoginData
            {
                Login = login,
                Password = password
            };

            var headers = GetRequestHeaders(useProtobuf);
            var binaryData = getBinaryData(loginData);

            var www = new WWW(URL_API + "login", binaryData, headers);
            StartCoroutine(PostLogin(www, login, password));
        }

        public void SendWebsocketMessage(IMessage message)
        {
            if (!IsLogged())
            {
                return;
            }
            webSocket.Send(getBinaryData(message));
        }

        private void ProcessLoginResult(LoginResultData resultData)
        {
            bool success = resultData.success;
            string login = resultData.login;
            string password = resultData.password;
            var panelManager = PanelManager.Instance;
            var gameManager = GameManager.Instance;
            if (success)
            {
                webSocket = new WebSocket(new Uri(URL_CHAT), loginToken);
                StartCoroutine(webSocket.Connect());

                gameManager.UserName = login;
                gameManager.UserPassword = password;
                panelManager.SwitchPanels(MenuPanel.Main);
            }
            else
            {
                // Perform offline login check
                if (login != "" && password != "" && gameManager.UserName == login 
                    && gameManager.UserPassword == password)
                {
                    // Offline check successful
                    panelManager.SwitchPanels(MenuPanel.LoginOffline);
                }
                else
                {
                    // TODO: error message
                    panelManager.SwitchPanels(MenuPanel.Login);
                }
            }
        }

        private void ProcessRegistrationResult(bool success)
        {
            PanelManager panelManager = PanelManager.Instance;
            if (success)
            {
                panelManager.SwitchPanels(MenuPanel.Login);
            }
            else
            {
                panelManager.SwitchPanels(MenuPanel.Register);
            }
        }

        private Dictionary<string, string> GetRequestHeaders(bool useProtobuf)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (useProtobuf)
            {
                headers.Add("Content-Type", "application/x-protobuf");
            }
            else
            {
                headers.Add("Content-Type", "application/json");
            }
            return headers;
        }

        public void LoginGoogle()
        {
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add("http://*:" + GOOGLE_REDIRECT_PORT + "/");
                listener.Start();
               
                Application.OpenURL(
                    "https://accounts.google.com/o/oauth2/v2/auth" 
                    + "?client_id=" + GOOGLE_ID
                    + "&redirect_uri=" + GOOGLE_REDIRECT_URI
                    + "&response_type=code"
                    + "&scope=email%20profile"
                );
                
                HttpListenerContext context = listener.GetContext();

                string error = context.Request.QueryString["error"];
                string code = context.Request.QueryString["code"];

                string responseString;
                if (!string.IsNullOrEmpty(code))
                {
                    responseString = googleLoginSuccessHtml;
                    StartCoroutine(GetGoogleAccessToken(code));
                }
                else if (!string.IsNullOrEmpty(error))
                {
                    responseString = string.Format(googleLoginErrorHtml, error);
                }
                else
                {
                    throw new InvalidOperationException("Google login did not return success nor error");
                }

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                
                HttpListenerResponse response = context.Response;
                response.ContentLength64 = buffer.Length;
                using (System.IO.Stream output = response.OutputStream)
                {
                    output.Write(buffer, 0, buffer.Length);
                }
            }
        }
        
        private IEnumerator GetGoogleAccessToken(string authCode)
        {
            var form = new WWWForm();

            Dictionary<string, string> headers = new Dictionary<string, string>();

            headers.Add("Host", "www.googleapis.com");
            headers.Add("Content-Type", "application/x-www-form-urlencoded");
            form.AddField("code", authCode + "&");
            form.AddField("client_id", GOOGLE_ID);
            form.AddField("client_secret", GOOGLE_CLIENT_SECRET);
            form.AddField("redirect_uri", GOOGLE_REDIRECT_URI);
            form.AddField("grant_type", "authorization_code");
            
            var www = new WWW(GOOGLE_TOKEN_URI, form.data, headers);
            yield return www;
            if (!string.IsNullOrEmpty(www.text))
            {
                Debug.Log(www.text);
                string idToken = JToken.Parse(www.text)["id_token"].Value<string>();
                StartCoroutine(Get("loginGoogle?idToken=" + idToken,
                    googleLoginResultJson =>
                    {
                        var googleLoginResult = JToken.Parse(googleLoginResultJson);
                        loginToken = googleLoginResult["token"].Value<string>();
                        string login = googleLoginResult["login"].Value<string>();
                        ProcessLoginResult(new LoginResultData(login, GOOGLE_PASSWORD, true));
                    }, 
                    Debug.LogError));
            }
            else
            {
                Debug.Log("Could not login using google on our server " + www.error);
            }
        }

        private IEnumerator PostRegister(WWW www)
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                Debug.Log("[Registration] Post request succeeded."); // text of success
                SendMessage(PROCESS_REGISTRATION_RESULT, true);
            }
            else
            {
                Debug.Log("[Registration] Post request failed."); // error
                SendMessage(PROCESS_REGISTRATION_RESULT, false);
            }
        }

        private IEnumerator PostLogin(WWW www, string login, string password)
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                Debug.Log("[Login] Post request succeeded."); // text of success
                Debug.Log("WWW text: " + www.text);
                loginToken = www.text;
                SendMessage("ProcessLoginResult", new LoginResultData(login, password, true));
            }
            else
            {
                Debug.Log("[Login] Post request failed."); // text of fail
                SendMessage("ProcessLoginResult", new LoginResultData(login, password, false));
            }
        }

        public IEnumerator Get(string query, Action<string> success, Action<string> error)
        {
            var www = new WWW(URL_API + query);
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

        private void Start()
        {
            RegisterListener(WrapperMessage.MessageOneofCase.ErrorMessage, this);
            
            googleLoginSuccessHtml = Resources.Load<TextAsset>("Html/google_login_success").text;
            googleLoginErrorHtml = Resources.Load<TextAsset>("Html/google_login_error").text;
        }

        public bool IsLogged()
        {
            return loginToken != null;
        }

        private byte[] getBinaryData(IMessage protobuf)
        {
            var binaryData = new byte[protobuf.CalculateSize()];
            var stream = new CodedOutputStream(binaryData);
            protobuf.WriteTo(stream);

            return binaryData;
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

        // Because SendMessage can only accept 1 argument
        private struct LoginResultData
        {
            public string login;
            public string password;
            public bool success;

            public LoginResultData(string login, string password, bool success)
            {
                this.login = login;
                this.password = password;
                this.success = success;
            }
        }
    }
}