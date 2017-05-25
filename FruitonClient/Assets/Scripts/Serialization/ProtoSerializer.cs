using DataModels;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ProtoSerializer : MonoBehaviour {

    #region Singleton

    public static ProtoSerializer Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            mySerializer = new ModelSerializer();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region Fields
    private ModelSerializer mySerializer;
    #endregion

    public byte[] GetBinaryData(object data, bool useProtobuf = true)
    {
        byte[] binaryData = null;
        MemoryStream memoryStream = new MemoryStream();
        mySerializer.Serialize(memoryStream, data);
        if (useProtobuf)
        {
            binaryData = memoryStream.ToArray();
            Debug.Log("SERIALIZED: " + binaryData.ToString());
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

    public void SerializeSalads()
    {
        GameManager gameManager = GameManager.Instance;
        byte[] binaryData = GetBinaryData(gameManager.Salads);
        Debug.Log("Application persistence data path: " + Application.persistentDataPath);
        FileStream file = File.Create(Application.persistentDataPath + "/Salads.dat");
        if (gameManager.StayLoggedIn)
        {
            file.Write(binaryData, 0, binaryData.Length);
        }
        else
        {
            file.SetLength(0);
        }
        file.Close();
    }

    public void DeserializeSalads()
    {
        if (System.IO.File.Exists(Application.persistentDataPath + "/Salads.dat"))
        {
            MemoryStream memoryStream = new MemoryStream();
            GameManager gameManager = GameManager.Instance;
            FileStream file = File.Open(Application.persistentDataPath + "/Salads.dat", FileMode.Open);
            SaladList salads = (SaladList)mySerializer.Deserialize(file, null, typeof(SaladList));
            gameManager.Salads = salads;
            file.Close();
        }
        
    }
}
