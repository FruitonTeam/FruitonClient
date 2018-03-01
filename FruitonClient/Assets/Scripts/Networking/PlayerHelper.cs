using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cz.Cuni.Mff.Fruiton.Dto;
using Newtonsoft.Json;
using Serialization;
using Util;
using Action = System.Action;

namespace Networking
{
    /// <summary>
    /// Contains helper methods for player related http requests to server
    /// </summary>
    public static class PlayerHelper
    {
        /// <summary>
        /// Creates http request to check if player with given username exists.
        /// </summary>
        /// <param name="player">username to check</param>
        /// <param name="success">action to invoke when the request succeeds</param>
        /// <param name="error">action to invoke when the request fails</param>
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

        /// <summary>
        /// Creates http request to get avatar of a player.
        /// </summary>
        /// <param name="player">username of the player</param>
        /// <param name="success">action to invoke when the request succeeds</param>
        /// <param name="error">action to invoke when the request fails</param>
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

        /// <summary>
        /// Creates http request to get logged player's available fruitons.
        /// Stores loaded fruitons in game manager.
        /// </summary>
        /// <param name="success">action to invoke when the request succeeds</param>
        /// <param name="error">action to invoke when the request fails</param>
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

        /// <summary>
        /// Creates http request to get logged player's fruitons that are available for selling.
        /// </summary>
        /// <param name="success">action to invoke when the request succeeds</param>
        /// <param name="error">action to invoke when the request fails</param>
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

        /// <summary>
        /// Creates http request to get logged player's teams.
        /// Stores loaded teams in game manager.
        /// </summary>
        /// <param name="success">action to invoke when the request succeeds</param>
        /// <param name="error">action to invoke when the request fails</param>
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

        /// <summary>
        /// Creates http request to save logged player's team on the server.
        /// </summary>
        /// <param name="fruitonTeam">team to be saved</param>
        /// <param name="success">action to invoke when the request succeeds</param>
        /// <param name="error">action to invoke when the request fails</param>
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
                    NetworkUtils.CreateRequestHeaders(true)
                )
            );
        }

        /// <summary>
        /// Creates http request to remove logged player's team on the server.
        /// </summary>
        /// <param name="teamName">name of the team to be removed</param>
        /// <param name="success">action to invoke when the request succeeds</param>
        /// <param name="error">action to invoke when the request fails</param>
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

        /// <summary>
        /// Creates http request to check whether a user is online.
        /// </summary>
        /// <param name="login">username of the user to check</param>
        /// <param name="success">action to invoke when the request succeeds</param>
        /// <param name="error">action to invoke when the request fails</param>
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

        /// <summary>
        /// Creates http request to get logged player's incoming friend requests.
        /// </summary>
        /// <param name="success">action to invoke when the request succeeds</param>
        /// <param name="error">action to invoke when the request fails</param>
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

        /// <summary>
        /// Creates http request to get logged player's bazaar offers.
        /// </summary>
        /// <param name="success">action to invoke when the request succeeds</param>
        /// <param name="error">action to invoke when the request fails</param>
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

        /// <summary>
        /// Creates http request to get logged player's most recent chat messages with another user.
        /// </summary>
        /// <param name="login">username of the other user</param>
        /// <param name="success">action to invoke when the request succeeds</param>
        /// <param name="error">action to invoke when the request fails</param>
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

        /// <summary>
        /// Creates http request to get logged player's older chat messages with another user.
        /// </summary>
        /// <param name="messageId">id of the latest chat message to be loaded</param>
        /// <param name="success">action to invoke when the request succeeds</param>
        /// <param name="error">action to invoke when the request fails</param>
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

        /// <summary>
        /// Creates http request to get status of a user.
        /// </summary>
        /// <param name="login">username of the user to check</param>
        /// <param name="success">action to invoke when the request succeeds</param>
        /// <param name="error">action to invoke when the request fails</param>
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

        /// <summary>
        /// Creates http request to send response to trade offer.
        /// </summary>
        /// <param name="offerId">if of the offer to respond to</param>
        /// <param name="accepted">true if player accepts the offer</param>
        /// <param name="success">action to invoke when the request succeeds</param>
        /// <param name="error">action to invoke when the request fails</param>
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