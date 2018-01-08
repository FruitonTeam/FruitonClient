using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FractionPanel : MainMenuPanel
{
    public int SelectedFractionID = -1;
    public Image[] Markers;

    public override void SetPanelActive(bool toggle)
    {
        if (toggle)
        {
            for (int i = 0; i < Markers.Length; i++)
            {
                ToggleMarker(i, false);
            }
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // toggle between fractions using graphics & effects
    public void ToggleMarker(int markerID, bool toggle) {
        if (toggle)
        {
            Markers[markerID].color = new Color(1, 1, 1, 1);
            SelectedFractionID = markerID;
        }
        else
            Markers[markerID].color = new Color(0, 0, 0, .5f);
    }

    // called after one of the fraction buttons are selected
    public void SelectFraction(int fractionID) {
        if (SelectedFractionID != fractionID) {
            if (SelectedFractionID != -1)
            {
                ToggleMarker(SelectedFractionID, false);
            }
            ToggleMarker(fractionID, true);
        }
    }

    // called after Join button is pressed
    public void JoinContinue()
    {
        GameManager.Instance.UserFraction = (FractionNames) (++SelectedFractionID);
        PanelManager.Instance.SwitchPanels(MenuPanel.Main);
    }
}
