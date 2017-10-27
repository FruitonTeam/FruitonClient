using System.Collections;
using System.Collections.Generic;
using Networking;
using UnityEngine;

public class WelcomePanel : MainMenuPanel {

    public void WelcomeContinue()
    {
        GameManager gameManager = GameManager.Instance;
        if (ConnectionHandler.Instance.IsLogged())
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Main);
            return;
        }
        if (gameManager.HasRememberedUser())
        {
            ConnectionHandler.Instance.LoginBasic(gameManager.UserName, gameManager.UserPassword, true);
        }
        else
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Login);
        }
    }
}
