using Cz.Cuni.Mff.Fruiton.Dto;
using Google.Protobuf;
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

    public byte[] GetBinaryData(IMessage protobuf)
    {
        var binaryData = new byte[protobuf.CalculateSize()];
        var stream = new CodedOutputStream(binaryData);
        protobuf.WriteTo(stream);

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
        Debug.Log("Trying to load Salads.");
        if (System.IO.File.Exists(Application.persistentDataPath + "/Salads.dat"))
        {
            MemoryStream memoryStream = new MemoryStream();
            GameManager gameManager = GameManager.Instance;
            FileStream file = File.Open(Application.persistentDataPath + "/Salads.dat", FileMode.Open);
            //SaladList salads = (SaladList)mySerializer.Deserialize(file, null, typeof(SaladList));
            
            SaladList salads = SaladList.Parser.ParseFrom(file);
            gameManager.Salads = salads;
            file.Close();
            Debug.Log("Salads loaded.");
        }
        
    }
}
