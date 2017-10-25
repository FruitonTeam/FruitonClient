using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Networking
{
    public static class PlayerHelper
    {
        public static void Exists(string player, Action<bool> success, Action<string> error)
        {
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Post(
                    "player/exists?login=" + player,
                    result => success(result == "true"),
                    error
                )
            );
        }

        public static void GetAvatar(string player, Action<Texture> success, Action<string> error)
        {
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Post(
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

        public static void GetAvailableFruitons(string player, Action<List<int>> success, Action<string> error)
        {
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Post(
                    "player/availableFruitons?login=" + player,
                    jsonString =>
                    {
                        var fruitons = JsonConvert.DeserializeObject<List<int>>(jsonString);
                        success(fruitons);
                    },
                    error
                )
            );
        }
    }
}