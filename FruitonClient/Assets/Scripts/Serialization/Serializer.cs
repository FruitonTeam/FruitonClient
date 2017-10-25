using Cz.Cuni.Mff.Fruiton.Dto;
using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class Serializer {

    #region Fields

    private static readonly string FRUITON_TEAMS_PATH = Path.Combine(Application.persistentDataPath, "FruitonTeams.dat");
    private static readonly string AVAILABLE_FRUITONS_PATH = Path.Combine(Application.persistentDataPath, "AvailableFruitons.json");
    
    #endregion

    public static void SaveAvailableFruitons(List<int> availableFruitons)
    {
        using (StreamWriter sw = File.CreateText(AVAILABLE_FRUITONS_PATH))
        {
            if (GameManager.Instance.StayLoggedIn)
            {
                var js = JsonSerializer.CreateDefault();
                js.Serialize(sw, availableFruitons);
            }
            // Else save empty file
        }
    }

    public static List<int> LoadAvailableFruitons()
    {
        if (GameManager.Instance.StayLoggedIn && File.Exists(AVAILABLE_FRUITONS_PATH))
        {
            using (StreamReader sr = File.OpenText(AVAILABLE_FRUITONS_PATH))
            {
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    JsonSerializer js = JsonSerializer.CreateDefault();
                    var fruitons = js.Deserialize<List<int>>(jr);

                    if (fruitons == null) // File empty or contents corrupted
                    {
                        return Constants.DEFAULT_AVAILABLE_FRUITONS;
                    }
                    return fruitons;
                }
            }
        }
        return Constants.DEFAULT_AVAILABLE_FRUITONS;
    }

    public static byte[] GetBinaryData(IMessage protobuf)
    {
        var binaryData = new byte[protobuf.CalculateSize()];
        var stream = new CodedOutputStream(binaryData);
        protobuf.WriteTo(stream);

        return binaryData;
    }

    public static void SerializeFruitonTeams()
    {
        GameManager gameManager = GameManager.Instance;
        byte[] binaryData = GetBinaryData(gameManager.FruitonTeamList);
        using (FileStream file = File.Create(FRUITON_TEAMS_PATH))
        {
            if (gameManager.StayLoggedIn)
            {
                file.Write(binaryData, 0, binaryData.Length);
            }
            // Else save empty file
        }
    }

    public static void DeserializeFruitonTeams()
    {
        Debug.Log("Trying to load Fruiton Teams.");
        if (File.Exists(FRUITON_TEAMS_PATH))
        {
            using (FileStream file = File.Open(FRUITON_TEAMS_PATH, FileMode.Open))
            {
                FruitonTeamList fruitonTeamList = FruitonTeamList.Parser.ParseFrom(file);
                GameManager.Instance.FruitonTeamList = fruitonTeamList;
            }
            Debug.Log("Fruiton Teams loaded.");
        }
        
    }
}
