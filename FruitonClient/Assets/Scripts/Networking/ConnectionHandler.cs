using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using DataModels;
using System.IO;
using ProtoBuf;
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
    ModelSerializer mySerializer;

    // Only for testing purposes. Will be deleted later.
    //public void Start()
    //{
    //    Register("rytmo222", "rytmo", "ry@tmo.com", true);
    //    //LoginCasual("rytmo", "rytmo", true);

    //}


    private ConnectionHandler()
    {
        mySerializer = new ModelSerializer();
    }

    public static ConnectionHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ConnectionHandler();
            }
            return instance;
        }
    }

    /// <summary>
    /// Sends a registration request to the server.
    /// </summary>
    /// <param name="login"> Must be at least 6 characters long. </param>
    /// <param name="password"></param>
    /// <param name="email"> Must contain '@' </param>
    /// <param name="useProtobuf"> Determines whether protobuf encoding should be used. It is recommended to use protobuf. </param>
    public void Register(string login, string password, string email, bool useProtobuf)
    {
        RegistrationForm newUser = new RegistrationForm(login, password, email);

        byte[] binaryData = GetBinaryData(useProtobuf, newUser);
        Dictionary<string, string> headers = GetRequestHeaders(useProtobuf);
        
        WWW www = new WWW(URL_REGISTRATION, binaryData, headers);
        StartCoroutine(PostRegister(www));
    }

    //To be deleted
    public void TestRegister()
    {
        GameObject.Find("Text").GetComponent<Text>().text = "Clicked" + Random.value;
        RegistrationForm newUser = new RegistrationForm("android", "randomhhd", "randosdm@random.com");

        byte[] binaryData = GetBinaryData(true, newUser);
        Dictionary<string, string> headers = GetRequestHeaders(true);

        WWW www = new WWW(URL_REGISTRATION, binaryData, headers);
        StartCoroutine(PostRegister(www));
    }

    public void LoginCasual(string login, string password, bool useProtobuf)
    {
        LoginForm loginData = new LoginForm(login, password);
        
        Dictionary<string, string> headers = GetRequestHeaders(useProtobuf);
        byte[] binaryData = GetBinaryData(useProtobuf, loginData);

        WWW www = new WWW(URL_LOGIN, binaryData, headers);
        StartCoroutine(PostLogin(www));
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

    private byte[] GetBinaryData(bool useProtobuf, object data)
    {
        byte[] binaryData = null;
        MemoryStream memoryStream = new MemoryStream();
        mySerializer.Serialize(memoryStream, data);
        if (useProtobuf)
        {
            binaryData = memoryStream.ToArray();
        }
        else
        {
            // Use simple JSON
            string serializedMessage = System.Convert.ToBase64String(memoryStream.ToArray());
            serializedMessage = JsonUtility.ToJson(data);
            binaryData = System.Text.Encoding.ASCII.GetBytes(serializedMessage.ToCharArray());
        }
        return binaryData;
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
            GameObject.Find("Text").GetComponent<Text>().text = www.text;
        }
        else
        {
            Debug.Log("[Registration] Post request failed.");  //error
            GameObject.Find("Text").GetComponent<Text>().text = www.error;
        }
    }

    IEnumerator PostLogin(WWW www)
    {
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            Debug.Log("[Login] Post request succeeded.");  //text of success
        }
        else
        {
            GameObject.Find("Text").GetComponent<Text>().text = "SUCESS";
            Debug.Log("Post request succeeded.");  //text of success
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

}
