using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
//using GooglePlayGames;
//using GooglePlayGames.BasicApi;

/// <summary>
/// Singleton used for handling a connection with the server.
/// </summary>
public class ConnectionHandler : MonoBehaviour {

    private static ConnectionHandler instance;
    private const string URL_REGISTRATION = "http://prak.mff.cuni.cz:8010/api/register";
    private const string URL_LOGIN = "http://prak.mff.cuni.cz:8010/api/login";
    private const string GOOGLE_ID = "827606142557-f63cu712orq80s6do9n6aa8s3eu3h7ag.apps.googleusercontent.com";
    private const string GOOGLE_CLIENT_SECRET = "NyYlQJICuxYX3AnzChou2X8i";
    private const string GOOGLE_REDIRECT_URI = "https://oauth2.example.com/code";
    private const string GOOGLE_TOKEN_URI = "https://accounts.google.com/o/oauth2/token";
    private string token = null;
    

    private const string PROCESS_REGISTRATION_RESULT = "ProcessRegistrationResult";

    // Only for testing purposes. Will be deleted later.
    //public void Start()
    //{
    //    //Register("rytmo222", "rytmo", "ry@tmo.com", true);
    //    LoginCasual("rytmo", "rytmo", true);

    //}


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
        var newUser = new RegistrationData();
        newUser.Login = login;
        newUser.Password = password;
        newUser.Email = email;

        var binaryData = new byte[newUser.CalculateSize()];
        var stream = new CodedOutputStream(binaryData);
        newUser.WriteTo(stream);
        Dictionary<string, string> headers = GetRequestHeaders(useProtobuf);
        
        WWW www = new WWW(URL_REGISTRATION, binaryData, headers);
        StartCoroutine(PostRegister(www));
    }

    //To be deleted
    public void TestRegister()
    {
        GameObject.Find("Text").GetComponent<Text>().text = "Clicked" + UnityEngine.Random.value;
        RegistrationData newUser = new RegistrationData();
        newUser.Login = "android";
        newUser.Password = "randomhd";
        newUser.Email = "randosdm@random.com";

        var binaryData = new byte[newUser.CalculateSize()];
        var stream = new CodedOutputStream(binaryData);
        newUser.WriteTo(stream);
        Dictionary<string, string> headers = GetRequestHeaders(true);

        WWW www = new WWW(URL_REGISTRATION, binaryData, headers);
        StartCoroutine(PostRegister(www));
    }

    public void LoginCasual(string login, string password, bool useProtobuf)
    {
        LoginData loginData = new LoginData();
        loginData.Login = login;
        loginData.Password = password;

        var binaryData = new byte[loginData.CalculateSize()];
        var stream = new CodedOutputStream(binaryData);
        loginData.WriteTo(stream);
        Dictionary<string, string> headers = GetRequestHeaders(useProtobuf);

        WWW www = new WWW(URL_LOGIN, binaryData, headers);
        StartCoroutine(PostLogin(www, login, password));
    }

    private void ProcessLoginResult(LoginResultData resultData)
    {
        bool success = resultData.success;
        string login = resultData.login;
        string password = resultData.password;
        PanelManager panelManager = PanelManager.Instance;
        GameManager gameManager = GameManager.Instance;
        if (success)
        {
            gameManager.UserName = login;
            gameManager.UserPassword = password;
            panelManager.SwitchPanels(MenuPanel.Main);
        }
        else
        {
            // Perform offline login check
            if (login != "" && password != "" && gameManager.UserName == login && gameManager.UserPassword == password)
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

        //Social.localUser.Authenticate((bool success) => {
        //    if (success)
        //    {
        //        Debug.Log("Google success");
        //        Debug.Log(Social.localUser.id);
        //        Debug.Log(Social.localUser.userName);
        //    }
        //    else
        //    {
        //        Debug.Log("Google failed");
        //    }
        //});


        //Debug.Log("GetAuthCode");

        //Social.localUser.Authenticate((bool success) =>
        //{
        //    PlayGamesPlatform.Instance.GetServerAuthCode((CommonStatusCodes status, string code) =>
        //    {
        //        Debug.Log("Status: " + status.ToString());
        //        Debug.Log("Code: " + code);
        //    }
        //    );
        //});

    }

    IEnumerator PostRegister(WWW www)
    {
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            Debug.Log("[Registration] Post request succeeded.");  //text of success
            SendMessage(PROCESS_REGISTRATION_RESULT, true);
        }
        else
        {
            Debug.Log("[Registration] Post request failed.");  //error
            SendMessage(PROCESS_REGISTRATION_RESULT, false);
        }
    }

    IEnumerator PostLogin(WWW www, string login, string password)
    {
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            Debug.Log("[Login] Post request succeeded.");  //text of success
            Debug.Log("WWW text: " + www.text);
            token = www.text;  
            SendMessage("ProcessLoginResult", new LoginResultData(login, password, true));
        }
        else
        {
            Debug.Log("[Login] Post request failed.");  //text of fail
            SendMessage("ProcessLoginResult", new LoginResultData(login, password, false));
        }
        
    }

    IEnumerator GetGoogleAccessToken(string auth_code)
    {
        WWWForm form = new WWWForm();

        Dictionary<string, string> headers = new Dictionary<string, string>();

        headers.Add("Host", "www.googleapis.com");
        headers.Add("Content-Type", "application/x-www-form-urlencoded");
        form.AddField("code", auth_code + "&");
        form.AddField("client_id", GOOGLE_ID);
        form.AddField("client_secret", GOOGLE_CLIENT_SECRET);
        form.AddField("redirect_uri", GOOGLE_REDIRECT_URI);
        form.AddField("grant_type", "authorization_code");
        byte[] rawData = form.data;

        WWW www = new WWW(GOOGLE_TOKEN_URI, rawData, headers);
        yield return www;
        if (string.IsNullOrEmpty(www.error))
        {
            Debug.Log("Post request succeeded.");  //text of success
            Debug.Log(www.text);
        }
        else
        {
            Debug.Log("Post request failed.");  //error
            Debug.Log(www.error);
        }
    }

    void Awake()
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
        return token != null;
    }

    // Because SendMessage can only acceppt 1 argument
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
