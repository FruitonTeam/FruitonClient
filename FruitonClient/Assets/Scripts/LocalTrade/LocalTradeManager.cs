using System.Collections;
using System.Collections.Generic;
using fruiton.kernel;
using UI.Notification;
using UnityEngine;
using UnityEngine.UI;

public class LocalTradeManager : FruitonVisualizerBase
{
    public InputField InputPrice;
    public Text Headline;
    public Texture YesTexture;
    public Texture NoTexture;

    private FridgeFruiton selectedFruiton;
    private string offeredPlayerLogin;

    private static readonly string HEADLINE_TEXT = "Choose a fruiton and a price that will be offered to <b>{0}</b>.";

    protected override void Start()
    {
        base.Start();
        offeredPlayerLogin = Scenes.GetParam(Scenes.OFFERED_PLAYER_LOGIN);
        Headline.GetComponent<Text>().text = string.Format(HEADLINE_TEXT, offeredPlayerLogin);
        InputPrice.onValidateInput += OnlyPositiveDigits;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            MakeOffer();
        }
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
        string priceString = InputPrice.text;
        int price;
        if (!int.TryParse(priceString, out price))
        {
            NotificationManager.Instance.Show(NoTexture, "No price specified", "Enter the price you want to sell for.");
            return;
        }
        Debug.Log("Offering message.");
        string offerBody = string.Format("Fruiton {0} has been offered to {1} for {2} coins.", selectedFruiton.KernelFruiton.name, offeredPlayerLogin, price);
        NotificationManager.Instance.Show(YesTexture, "Offer succesful", offerBody);
        // TODO: Send message to the server.
        Scenes.Load(Scenes.MAIN_MENU_SCENE);

    }

    protected override void InitializeFridgeFruiton(FridgeFruiton fFruiton, Fruiton kFruiton, int fridgeIndex)
    {
        base.InitializeFridgeFruiton(fFruiton, kFruiton, fridgeIndex);
        fFruiton.GetComponent<Button>().onClick.AddListener(() =>
        {
            selectedFruiton = fFruiton;
            ShowTooltip(kFruiton);
        });
    }
}
