using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cz.Cuni.Mff.Fruiton.Dto;
using Newtonsoft.Json;
using Util;
using Action = System.Action;

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

        public static void GetAvatar(string player, Action<Texture2D> success, Action<string> error)
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
                    "secured/player/availableFruitons",
                    jsonString =>
                    {
                        var fruitons = JsonConvert.DeserializeObject<List<int>>(jsonString);
                        GameManager.Instance.AvailableFruitons = fruitons;
                        Serializer.SaveAvailableFruitons(fruitons);
                        success(fruitons);
                    },
                    error
                )
            );
        }
        
        public static void GetFruitonsAvailableForSelling(Action<List<int>> success, Action<string> error)
        {
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Get(
                    "secured/player/fruitonsAvailableForSelling",
                    jsonString =>
                    {
                        var fruitons = JsonConvert.DeserializeObject<List<int>>(jsonString);
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
                    "secured/getAllFruitonTeams",
                    bytes =>
                    {
                        var fruitomTeamList = FruitonTeamList.Parser.ParseFrom(bytes);
                        success(fruitomTeamList);
                    },
                    error
                    )
            );
        }

        public static void UploadFruitonTeam(FruitonTeam fruitonTeam, Action<string> success, Action<string> error)
        {
            if (GameManager.Instance.IsInTrial) return;
            byte[] body = Serializer.GetBinaryData(fruitonTeam);
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Post(
                    "secured/addFruitonTeam",
                    success,
                    error,
                    body,
                    NetworkUtils.GetRequestHeaders(true)
                )
            );
        }

        public static void RemoveFruitonTeam(string teamName, Action<string> success, Action<string> error)
        {
            teamName = Uri.EscapeUriString(teamName);
            Debug.Log("team encoded name = " + teamName);
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Get(
                    "secured/removeFruitonTeam?teamName=" + teamName,
                    success,
                    error
                )
            );
        }

        public static void IsOnline(string login, Action<bool> success, Action<string> error)
        {
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Get(
                    "player/isOnline?login=" + login,
                    result => success(result == "true"),
                    error
                )
            );
        }

        public static void GetFriendRequests(Action<List<string>> success, Action<string> error)
        {
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Get(
                    "secured/player/friendRequests",
                    jsonString =>
                    {
                        var requests = JsonConvert.DeserializeObject<List<string>>(jsonString);
                        success(requests);
                    },
                    error
                )
            );
        }

        public static void GetBazaarOffers(Action<TradeOfferList> success, Action<string> error)
        {
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Get(
                    "secured/bazaar/getTradeOffers",
                    bytes =>
                    {
                        var offers = TradeOfferList.Parser.ParseFrom(bytes);
                        success(offers);
                    },
                    error
                )
            );
        }

        public static void GetMessagesWith(string login, int page, Action<ChatMessages, int> success, 
            Action<string> error)
        {
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Get(
                    "secured/getAllMessagesWithUser?otherUserLogin=" + login + "&page=" + page,
                    bytes =>
                    {
                        var chatMessages = ChatMessages.Parser.ParseFrom(bytes);
                        success(chatMessages, page);
                    },
                    error
                )
            );
        }

        public static void GetMessagesBefore(string messageId, int page, Action<ChatMessages, int> success,
            Action<string> error)
        {
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Get(
                    "secured/getAllMessagesBefore?messageId=" + messageId + "&page=" + page,
                    bytes =>
                    {
                        var chatMessages = ChatMessages.Parser.ParseFrom(bytes);
                        success(chatMessages, page);
                    },
                    error
                )
            );
        }

        public static void GetPlayerStatus(string login, Action<Status> success, Action<string> error)
        {
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Get(
                    "player/status?login=" + login,
                    text =>
                    {
                        Status status = (Status) int.Parse(text);
                        success(status);
                    },
                    error
                )
            );
        }

        public static void ProvideOfferResult(string offerId, bool accepted, Action success, Action<string> error)
        {
            ConnectionHandler.Instance.StartCoroutine(
                ConnectionHandler.Instance.Get(
                    "secured/bazaar/provideResultForTradeOffer?offerId=" + offerId + "&accepted=" + accepted,
                    (Action<string>)(_ => success()),
                    error
                )
            );
        }
        
    }
}