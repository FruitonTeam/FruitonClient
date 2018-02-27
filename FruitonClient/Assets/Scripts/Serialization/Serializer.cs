using System.Collections.Generic;
using System.IO;
using Cz.Cuni.Mff.Fruiton.Dto;
using Google.Protobuf;
using Newtonsoft.Json;
using UnityEngine;

namespace Serialization
{
    public static class Serializer {

        #region Fields

        private static readonly string FRUITON_TEAMS_PATH = Path.Combine(Application.persistentDataPath, "FruitonTeams.dat");
        private static readonly string AVAILABLE_FRUITONS_PATH = Path.Combine(Application.persistentDataPath, "AvailableFruitons.json");
        private static readonly string PLAYER_SETTINGS_PATH = Path.Combine(Application.persistentDataPath, "PlayerSettings.json");

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
                            return Constants.Constants.DEFAULT_AVAILABLE_FRUITONS;
                        }
                        return fruitons;
                    }
                }
            }
            return Constants.Constants.DEFAULT_AVAILABLE_FRUITONS;
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
                if (gameManager.IsInTrial || !gameManager.StayLoggedIn)
                {
                    file.Write(new byte[0], 0, 0);
                }
                else
                {
                    file.Write(binaryData, 0, binaryData.Length);
                }
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

        public static void SavePlayerSettings(PlayerOptions settings)
        {
            using (StreamWriter sw = File.CreateText(PLAYER_SETTINGS_PATH))
            {
                if (GameManager.Instance.StayLoggedIn)
                {
                    var js = JsonSerializer.CreateDefault();
                    js.Serialize(sw, settings);
                }
                // Else save empty file
            }
        }

        public static void ClearPlayerLocalData()
        {
            if (File.Exists(PLAYER_SETTINGS_PATH))
            {
                File.Delete(PLAYER_SETTINGS_PATH);
            }
            if (File.Exists(FRUITON_TEAMS_PATH))
            {
                File.Delete(FRUITON_TEAMS_PATH);
            }
            if (File.Exists(AVAILABLE_FRUITONS_PATH))
            {
                File.Delete(AVAILABLE_FRUITONS_PATH);
            }
        }

        public static PlayerOptions LoadPlayerSettings()
        {
            if (GameManager.Instance.StayLoggedIn && File.Exists(PLAYER_SETTINGS_PATH))
            {
                using (StreamReader sr = File.OpenText(PLAYER_SETTINGS_PATH))
                {
                    using (JsonReader jr = new JsonTextReader(sr))
                    {
                        var js = JsonSerializer.CreateDefault();
                        var settings = js.Deserialize<PlayerOptions>(jr);

                        if (settings == null) // File empty or contents corrupted
                        {
                            return new PlayerOptions();
                        }
                        return settings;
                    }
                }
            }
            return new PlayerOptions();
        }
    }
}
