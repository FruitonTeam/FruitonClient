using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UI.Notification;
using UnityEngine;
using Util;

namespace Bazaar
{
    public class TradeBazaar : MonoBehaviour, IOnMessageListener
    {
        private readonly Dictionary<string, int> offerToNotificationMap = new Dictionary<string, int>();
        
        public static TradeBazaar Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void Init()
        {
            if (GameManager.Instance.IsOnline)
            {
                PlayerHelper.GetBazaarOffers(offers =>
                {
                    foreach (TradeOffer offer in offers.TradeOffers)
                    {
                        ShowOffer(offer);
                    }
                }, Debug.LogError);
            }
        }

        public void OnMessage(WrapperMessage message)
        {
            if (message.MessageCase == WrapperMessage.MessageOneofCase.TradeOffer)
            {
                TradeOffer offer = message.TradeOffer;
                ShowOffer(offer);
            } else if (message.MessageCase == WrapperMessage.MessageOneofCase.BazaarOfferResolvedOnTheWeb)
            {
                DeleteNotification(message.BazaarOfferResolvedOnTheWeb.OfferId);
            }
        }

        private void ShowOffer(TradeOffer offer)
        {
            int notifId = FeedbackNotificationManager.Instance.Show(
                "Offer from " + offer.OfferedFrom, 
                string.Format("Fruiton {0} for {1} coins.", KernelUtils.GetFruitonName(offer.FruitonId), offer.Price),
                () =>
                {
                    PlayerHelper.ProvideOfferResult(offer.OfferId, true, () => {                    
                        GameManager.Instance.AdjustMoney(-(int) offer.Price);
                        Bazaar.Instance.NotifyListeners();
                    }, s => NotificationManager.Instance.Show("Could not buy :(", s));
                },
                () =>
                {
                    PlayerHelper.ProvideOfferResult(offer.OfferId, false, () => {                    
                        // ignore - everything is ok
                    }, s => NotificationManager.Instance.Show("Unknown error", s));
                });

            offerToNotificationMap[offer.OfferId] = notifId;
        }

        public void SendOffer(int fruitonId, string login, uint price)
        {
            TradeOffer offer = new TradeOffer
            {
                FruitonId = fruitonId,
                Price = price,
                OfferedFrom = GameManager.Instance.UserName,
                OfferedTo = login
            };

            WrapperMessage message = new WrapperMessage
            {
                TradeOffer = offer
            };

            ConnectionHandler.Instance.SendWebsocketMessage(message);
        }

        private void DeleteNotification(string offerId)
        {
            if (offerToNotificationMap.ContainsKey(offerId))
            {
                FeedbackNotificationManager.Instance.RemoveNotification(offerToNotificationMap[offerId]);
            }
        }
    }
}