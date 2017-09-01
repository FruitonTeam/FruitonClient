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
    // Use this in the following way: Application.persistentDataPath + PERSISTENCE_PATH
    private const string PERSISTENCE_PATH = "/FruitonTeams.dat";
    #endregion

    public byte[] GetBinaryData(IMessage protobuf)
    {
        var binaryData = new byte[protobuf.CalculateSize()];
        var stream = new CodedOutputStream(binaryData);
        protobuf.WriteTo(stream);

        return binaryData;
    }

    public void SerializeFruitonTeams()
    {
        GameManager gameManager = GameManager.Instance;
        byte[] binaryData = GetBinaryData(gameManager.FruitonTeamList);
        Debug.Log("Application persistence data path: " + Application.persistentDataPath);
        FileStream file = File.Create(Application.persistentDataPath + PERSISTENCE_PATH);
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

    public void DeserializeFruitonTeams()
    {
        Debug.Log("Trying to load Fruiton Teams.");
        if (System.IO.File.Exists(Application.persistentDataPath + PERSISTENCE_PATH))
        {
            MemoryStream memoryStream = new MemoryStream();
            GameManager gameManager = GameManager.Instance;
            FileStream file = File.Open(Application.persistentDataPath + PERSISTENCE_PATH, FileMode.Open);
            
            FruitonTeamList fruitonTeamList = FruitonTeamList.Parser.ParseFrom(file);
            gameManager.FruitonTeamList = fruitonTeamList;
            file.Close();
            Debug.Log("Fruiton Teams loaded.");
        }
        
    }
}
