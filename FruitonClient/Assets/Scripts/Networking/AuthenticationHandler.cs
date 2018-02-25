using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Cz.Cuni.Mff.Fruiton.Dto;
using UnityEngine;
using Util;

namespace Networking
{
    public class AuthenticationHandler : MonoBehaviour, IOnMessageListener
    {
        private static readonly string GOOGLE_ID =
            "827606142557-f63cu712orq80s6do9n6aa8s3eu3h7ag.apps.googleusercontent.com";

        private static readonly string GOOGLE_CLIENT_SECRET = "NyYlQJICuxYX3AnzChou2X8i";

        private static readonly int[] GOOGLE_REDIRECT_PORTS = {9999, 34311, 44873};

        private static readonly string GOOGLE_REDIRECT_URI_TEMPLATE = "http://127.0.0.1:{0}";
        private static readonly string GOOGLE_TOKEN_URI = "https://www.googleapis.com/oauth2/v4/token";

        private static string googleLoginSuccessHtml;
        private static string googleLoginErrorHtml;
        
        public static AuthenticationHandler Instance { get; private set; }

        public string LastPassword { get; private set; }

        private HttpListener googleLoginHttpListener;

        private string googleRedirectUri;
        
        private AuthenticationHandler()
        {
        }
        
        /// <summary>
        /// Sends a registration request to the server.
        /// </summary>
        /// <param name="login"> Must be at least 6 characters long. </param>
        /// <param name="password"></param>
        /// <param name="email"> Must contain '@' </param>
        public void Register(string login, string password, string email)
        {
            var newUser = new RegistrationData
            {
                Login = login,
                Password = password,
                Email = email
            };

            StartCoroutine(ConnectionHandler.Instance.Post("register",
                success =>
                {
                    OnSuccessfulRegistration(login);
                },
                error =>
                {
                    PanelManager.Instance.SwitchPanels(MenuPanel.Register);
                    PanelManager.Instance.ShowErrorMessage(error);
                }, 
                ProtobufUtils.GetBinaryData(newUser),
                NetworkUtils.GetRequestHeaders(true)));
        }

        private void OnSuccessfulRegistration(string login)
        {
            PanelManager panelManager = PanelManager.Instance;
            
            panelManager.SwitchPanels(MenuPanel.Login);
            panelManager.Panels[MenuPanel.Login].GetComponent<Form>().SetValue("name", login);
            panelManager.ShowInfoMessage("User " + login + " successfully registered!");
        }

        public void LoginBasic(string login, string password, bool autoLogin = false)
        {
            var loginData = new LoginData
            {
                Login = login,
                Password = password
            };

            LastPassword = password;

            Action<string> loginOfflineWrapper = (_) => LoginOffline();
            Action<string> errorAction = autoLogin ? loginOfflineWrapper : OnLoginError;            
            
            StartCoroutine(ConnectionHandler.Instance.Post("login",
                ProcessLoginResult,
                errorAction,
                ProtobufUtils.GetBinaryData(loginData),
                NetworkUtils.GetRequestHeaders(true)));
        }
        
        private void ProcessLoginResult(string token)
        {
            if (ConnectionHandler.Instance.IsLogged())
            {
                throw new InvalidOperationException("User is already logged in");
            }
            
            ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.LoggedPlayerInfo, this);

            ConnectionHandler.Instance.Connect(token); 
        }

        private void OnLoginError(string error)
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Login);
            PanelManager.Instance.ShowErrorMessage(error);
        }
        
        public void LoginGoogle()
        {
            if (googleLoginHttpListener == null)
            {
                googleLoginHttpListener = new HttpListener();
            }

            if (!googleLoginHttpListener.IsListening) 
            {
                foreach (int redirectPort in GOOGLE_REDIRECT_PORTS)
                {
                    try
                    {
                        googleLoginHttpListener.Prefixes.Add("http://*:" + redirectPort + "/");
                        googleLoginHttpListener.Start();
                        googleLoginHttpListener.BeginGetContext(ProcessGoogleResult, googleLoginHttpListener);
                        googleRedirectUri = string.Format(GOOGLE_REDIRECT_URI_TEMPLATE, redirectPort);
                        break;
                    }
                    catch (Exception e) // can be thrown if some other process is listening on `redirectPort`
                    {
                        Debug.LogException(e);
                    }     
                }
            }

            if (googleLoginHttpListener.IsListening)
            {
                Application.OpenURL(
                    "https://accounts.google.com/o/oauth2/v2/auth"
                    + "?client_id=" + GOOGLE_ID
                    + "&redirect_uri=" + googleRedirectUri
                    + "&response_type=code"
                    + "&scope=email%20profile"
                );
            }
            else
            {
                OnLoginError("Cannot log in via Google, please use ordinary registration/login");
                googleLoginHttpListener = null; // allows multiple retries
            }
        }

        private void ProcessGoogleResult(IAsyncResult result)
        {
            googleLoginHttpListener = null;
            using (var listener = (HttpListener) result.AsyncState)
            {
                HttpListenerContext context = listener.EndGetContext(result);

                string error = context.Request.QueryString["error"];
                string code = context.Request.QueryString["code"];

                if (ConnectionHandler.Instance.IsLogged()) // user logged in by ordinary means sooner
                {
                    SendResponse(context, string.Format(googleLoginErrorHtml, "You are already logged in."));
                }
                else if (!string.IsNullOrEmpty(code))
                {
                    SendResponse(context, googleLoginSuccessHtml);
                    TaskManager.Instance.RunOnMainThread(() => StartCoroutine(GetGoogleAccessToken(code)));
                }
                else if (!string.IsNullOrEmpty(error))
                {
                    SendResponse(context, string.Format(googleLoginErrorHtml, error));
                }
                else
                {
                    throw new InvalidOperationException("Google login did not return success nor error");
                }
            }
        }

        private void SendResponse(HttpListenerContext context, string responseString)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            HttpListenerResponse response = context.Response;
            response.ContentLength64 = buffer.Length;
            using (Stream output = response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }
        }

        private IEnumerator GetGoogleAccessToken(string authCode)
        {
            var form = new WWWForm();

            var headers = new Dictionary<string, string>
            {
                {"Host", "www.googleapis.com"},
                {"Content-Type", "application/x-www-form-urlencoded"}
            };

            form.AddField("code", authCode + "&");
            form.AddField("client_id", GOOGLE_ID);
            form.AddField("client_secret", GOOGLE_CLIENT_SECRET);
            form.AddField("redirect_uri", googleRedirectUri);
            form.AddField("grant_type", "authorization_code");

            var www = new WWW(GOOGLE_TOKEN_URI, form.data, headers);
            yield return www;
            if (!string.IsNullOrEmpty(www.text))
            {
                string idToken = JsonUtility.FromJson<IdToken>(www.text).id_token;
                StartCoroutine(ConnectionHandler.Instance.Get("loginGoogle?idToken=" + idToken,
                    googleLoginResultJson =>
                    {
                        var googleLoginResult = JsonUtility.FromJson<GoogleLoginResult>(googleLoginResultJson);
                        ProcessLoginResult(googleLoginResult.token);
                    },
                    OnLoginError));
            }
            else
            {
                OnLoginError(www.error);
            }
        }

        public void LoginOffline()
        {
            GameManager.Instance.LoginOffline();
        }
        
        public void OnMessage(WrapperMessage message)
        {
            GameManager.Instance.OnLoggedIn(message.LoggedPlayerInfo);
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
            googleLoginSuccessHtml = Resources.Load<TextAsset>("Html/google_login_success").text;
            googleLoginErrorHtml = Resources.Load<TextAsset>("Html/google_login_error").text;
        }
        
        [Serializable]
        private struct IdToken
        {
            public string id_token;
        }

        [Serializable]
        private struct GoogleLoginResult
        {
            public string token;
            public string login;
        }

    }
}