using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UI.Notification;
using UnityEngine;

namespace Bazaar
{
    public class Bazaar : MonoBehaviour, IOnMessageListener
    {
        public static Bazaar Instance { get; private set; }
        
        private readonly List<IOnFruitonSoldListener> listeners = new List<IOnFruitonSoldListener>();
        
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

        public void OnMessage(WrapperMessage message)
        {
            BazaarOfferResult result = message.BazaarOfferResult;
            
            GameManager.Instance.AdjustMoney(result.MoneyChange);
            
            NotifyListeners();
                        
            NotificationManager.Instance.Show("Fruiton sold", "Received money " + result.MoneyChange);
        }

        public void NotifyListeners()
        {
            foreach (IOnFruitonSoldListener listener in listeners)
            {
                listener.OnFruitonSold();
            }
        }

        public void AddListener(IOnFruitonSoldListener listener)
        {
            listeners.Add(listener);
        }

        public void RemoveListener(IOnFruitonSoldListener listener)
        {
            listeners.Remove(listener);
        }
        
        public interface IOnFruitonSoldListener
        {
            void OnFruitonSold();
        }
        
    }
}