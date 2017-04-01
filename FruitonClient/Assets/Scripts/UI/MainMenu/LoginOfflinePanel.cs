using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginOfflinePanel : MainMenuPanel {

    public void ContinueOffline()
    {
        PanelManager.Instance.SwitchPanels(MenuPanel.Main);
    }

    public void BackToLogin()
    {
        PanelManager.Instance.SwitchPanels(MenuPanel.Login);
    }
}
