using Networking;
using System;
using TeamsManagement;
using UI.Chat;
using UI.Fridge;
using UI.Notification;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fruiton = fruiton.kernel.Fruiton;

namespace Bazaar
{
    public class TradeManager : FruitonVisualizerBase
    {
        public InputField InputPrice;
        public Text Headline;
        public Texture YesTexture;
        public Texture NoTexture;

        private FridgeFruiton selectedFruiton;
        private string offeredPlayerLogin;

        private static readonly string HEADLINE_TEXT = "Choose a fruiton and a price that will be offered to <b>{0}</b>.";
        private static readonly Color SELECTED_COLOR = new Color(251 / 255.0f, 255 / 255.0f, 189 / 255.0f);

        protected override void Start()
        {
            base.Start();
            offeredPlayerLogin = Scenes.GetParam(Scenes.OFFERED_PLAYER_LOGIN);
            Headline.GetComponent<Text>().text = string.Format(HEADLINE_TEXT, offeredPlayerLogin);
            InputPrice.onValidateInput += OnlyPositiveDigits;
            InitializeAllFruitons();
        }

        protected override void UpdateAvailableFruitons()
        {
            PlayerHelper.GetFruitonsAvailableForSelling(FilterManager.UpdateAvailableFruitons, Debug.Log);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                MakeOffer();
            }
            if (Input.GetMouseButton(0) && EventSystem.current.currentSelectedGameObject == null)
            {
                UnselectFruiton();
            }
        }

        private void UnselectFruiton()
        {
            if (selectedFruiton == null) return;
            selectedFruiton.gameObject.GetComponent<Button>().GetComponent<Image>().color = Color.white;
            selectedFruiton = null;
        }

        private char OnlyPositiveDigits(string text, int charIndex, char addedChar) {
            if (addedChar > '9' || addedChar < '0') return '\0';
            return addedChar;
        }

        public void MakeOffer()
        {
            if (selectedFruiton == null)
            {
                Debug.Log("No fruiton selected");
                string clickTap;
#if UNITY_STANDALONE
                clickTap = "clicking on";
#else
            clickTap = "tapping";
#endif
                string body = string.Format("Select one by {0} it.", clickTap);
                NotificationManager.Instance.Show(NoTexture, "No fruiton selected", body);
                return;
            }

            if (!ChatController.Instance.IsSelectedPlayerOnline)
            {
                Debug.LogError("User went offline in the meantime");
                NotificationManager.Instance.Show(NoTexture, "User offline", "User you tried to trade with went offline :(");
                Scenes.Load(Scenes.MAIN_MENU_SCENE);
                return;
            }

            string priceString = InputPrice.text;
            uint price;
            if (!uint.TryParse(priceString, out price))
            {
                NotificationManager.Instance.Show(NoTexture, "No price specified", "Enter the price you want to sell for.");
                return;
            }
        
            TradeBazaar.Instance.SendOffer(selectedFruiton.KernelFruiton.dbId, offeredPlayerLogin, price);
        
            Scenes.Load(Scenes.MAIN_MENU_SCENE);
        
            string offerBody = string.Format("Fruiton {0} has been offered to {1} for {2} coins.", 
                selectedFruiton.KernelFruiton.name, offeredPlayerLogin, price);

            NotificationManager.Instance.Show(YesTexture, "Offer sent", offerBody);
        }

        protected override void InitializeFridgeFruiton(FridgeFruiton fFruiton, Fruiton kFruiton, int fridgeIndex)
        {
            base.InitializeFridgeFruiton(fFruiton, kFruiton, fridgeIndex);
            var button = fFruiton.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                UnselectFruiton();
                selectedFruiton = fFruiton;
                button.GetComponent<Image>().color = SELECTED_COLOR;
                ShowTooltip(kFruiton);
            });
        }
    }
}
