using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractionPanel : MainMenuPanel
{
    public void JoinContinue()
    {
        PanelManager.Instance.SwitchPanels(MenuPanel.Main);
    }
}
