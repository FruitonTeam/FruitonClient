using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cz.Cuni.Mff.Fruiton.Dto;
using Newtonsoft.Json;

namespace Networking
{
    public static class PlayerHelper
    {
        public static void Exists(string player, Action<bool> success, Action<string> error)
        {
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Get(
                    "player/exists?login=" + player,
                    result => success(result == "true"),
                    error
                )
            );
        }

        public static void GetAvatar(string player, Action<Texture> success, Action<string> error)
        {
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Get(
                    "player/avatar?login=" + player,
                    base64 =>
                    {
                        var avatarTexture = new Texture2D(0, 0);
                        avatarTexture.LoadImage(Convert.FromBase64String(base64));
                        success(avatarTexture);
                    },
                    error
                )
            );
        }

        public static void GetAvailableFruitons(Action<List<int>> success, Action<string> error)
        {
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Get(
                    "player/availableFruitons?login=" + GameManager.Instance.UserName,
                    jsonString =>
                    {
                        var fruitons = JsonConvert.DeserializeObject<List<int>>(jsonString);
                        GameManager.Instance.AvailableFruitons = fruitons;
                        success(fruitons);
                    },
                    error
                )
            );
        }

        public static void GetAllFruitonTeams(Action<FruitonTeamList> success, Action<string> error)
        {
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Get(
                    "getAllFruitonTeams?login=" + GameManager.Instance.UserName,
                    protobufString =>
                    {
                        byte[] protoMessage = Encoding.ASCII.GetBytes(protobufString);
                        FruitonTeamList fruitomTeamList = FruitonTeamList.Parser.ParseFrom(protoMessage);
                        success(fruitomTeamList);
                    },
                    error
                    )
            );
        }

        public static void UploadFruitonTeam(FruitonTeam fruitonTeam, Action<string> success, Action<string> error)
        {
            byte[] body = Serializer.GetBinaryData(fruitonTeam);
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Post(
                    "addFruitonTeam?login=" + GameManager.Instance.UserName,
                    success,
                    error,
                    body,
                    ConnectionHandler.Instance.GetRequestHeaders(true)
                )
            );
        }
    }
}