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
    MemoryStream memoryStream;

    // Only for testing purposes. Will be deleted later.
    //public void Start()
    //{
    //    Register("protobu2f", "protobuf", "proto@buf.com", true);

    //}

    private ConnectionHandler()
    {
        mySerializer = new ModelSerializer();
        memoryStream = new MemoryStream();
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
        User newUser = new User(login, password, email);
        mySerializer.Serialize(memoryStream, newUser);
        byte[] binaryData = null;
        Dictionary<string, string> headers = new Dictionary<string, string>();
        if (useProtobuf)
        {
            // Use protobuf
            binaryData = memoryStream.ToArray();
            headers.Add("Content-Type", "application/x-protobuf");
        }
        else
        {
            // Use simple JSON
            string serializedMessage = System.Convert.ToBase64String(memoryStream.ToArray());
            serializedMessage = JsonUtility.ToJson(newUser);
            binaryData = System.Text.Encoding.ASCII.GetBytes(serializedMessage.ToCharArray());
            headers.Add("Content-Type", "application/json");
        }

        WWWForm form = new WWWForm();
        WWW www = new WWW(URL_REGISTRATION, binaryData, headers);
        StartCoroutine(Post(www));
    }

    public void LoginCasual()
    {

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
