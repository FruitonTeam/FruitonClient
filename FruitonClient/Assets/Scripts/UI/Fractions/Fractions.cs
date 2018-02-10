using System.Collections;
using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UnityEngine;
using UnityEngine.UI;

public class Fractions : MonoBehaviour
{
    private Fraction chosenFractionName;
    private Button[] fractionButtons;

    public Button Fraction1Button;
    public Button Fraction2Button;
    public Button Fraction3Button;
    public Button JoinButton;

    private static readonly float lowAlpha = 0.4f;
    private static readonly float highAlpha = 1;

    private void Start()
    {
        fractionButtons = new[] { Fraction1Button, Fraction2Button, Fraction3Button };
    }

    public void JoinFraction()
    {
        ConnectionHandler connectionHandler = ConnectionHandler.Instance;

        var setFractionMessage = new SetFraction
        {
            Fraction = chosenFractionName
        };

        WrapperMessage wrapperMessage = new WrapperMessage
        {
            SetFraction = setFractionMessage
        };

        connectionHandler.SendWebsocketMessage(wrapperMessage);

        Scenes.Load(Scenes.MAIN_MENU_SCENE);
    }

    public void ChooseFraction(int fractionId)
    {
        foreach (Button button in fractionButtons)
        {
            button.ChangeAlphaChannel(lowAlpha);
        }

        chosenFractionName = (Fraction) fractionId;

        Button clickedButton = fractionButtons[fractionId - 1];
        clickedButton.ChangeAlphaChannel(highAlpha);

        JoinButton.interactable = true;
    }
}
