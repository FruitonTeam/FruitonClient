using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginPanel : MainMenuPanel
{
    public void LoginContinue()
    {
        PanelManager.Instance.SwitchPanels(MenuPanel.Fraction);
    }

    public void RegistrationContinue()
    {
        //PanelManager.Instance.SwitchPanels(MenuPanel.Fraction);
    }
}
