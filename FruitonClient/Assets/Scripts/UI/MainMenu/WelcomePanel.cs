using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WelcomePanel : MainMenuPanel {

    public void WelcomeContinue()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager.HasRememberedUser())
        {
            ConnectionHandler.Instance.LoginCasual(gameManager.UserName, gameManager.UserPassword, true);
        }
        else
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Login);
        }
    }
}
