using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using DataModels;
using System.IO;
using ProtoBuf;

/// <summary>
/// Singleton used for handling a connection with the server.
/// </summary>
public class ConnectionHandler : MonoBehaviour {

    private static ConnectionHandler instance;
    private const string URL_REGISTRATION = "prak.mff.cuni.cz:8010/api/register";
    private const string URL_LOGIN = "prak.mff.cuni.cz:8010/api/login";
    ModelSerializer mySerializer;

    // Only for testing purposes. Will be deleted later.
    //public void Start()
    //{
    //    //Register("rytmo", "rytmo", "ry@tmo.com", true);
    //    LoginCasual("rytmo", "rytmo", true);

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
        StartCoroutine(Post(www));
    }

    public void LoginCasual(string login, string password, bool useProtobuf)
    {
        LoginForm loginData = new LoginForm(login, password);
        
        Dictionary<string, string> headers = GetRequestHeaders(useProtobuf);
        byte[] binaryData = GetBinaryData(useProtobuf, loginData);

        WWW www = new WWW(URL_LOGIN, binaryData, headers);
        StartCoroutine(Post(www));
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
        
    }

    IEnumerator Post(WWW www)
    {
        yield return www;

        if (string.IsNullOrEmpty(www.error))
            Debug.Log("Post request succeeded.");  //text of success
        else
            Debug.Log("Post request failed.");  //error
    }
}
