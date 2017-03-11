using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WelcomePanel : MainMenuPanel {

    public void WelcomeContinue()
    {
        PanelManager.Instance.SwitchPanels(MenuPanel.Login);
    }
}
