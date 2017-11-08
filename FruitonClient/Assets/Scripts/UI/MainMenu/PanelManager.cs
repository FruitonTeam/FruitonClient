using System.Collections.Generic;
using UnityEngine;

public enum MenuPanel { Welcome, Login, Fraction, Main, Storage, Fridge, Pantry, Online, Offline, FarmersMarket, LoginOffline, Register }

public class PanelManager : MonoBehaviour {
    public static PanelManager Instance { get; private set; }

    public Dictionary<MenuPanel, MainMenuPanel> Panels = new Dictionary<MenuPanel, MainMenuPanel>();
    public MenuPanel CurrentPanel = MenuPanel.Welcome;

    // Use this for initialization
    void Awake() {
        if (Instance == null)
        {
            Instance = this;
            FillPanelDictionary();

        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    //fill Panels with panels in the scene
    private void FillPanelDictionary() {
        MainMenuPanel[] panelComponents = GetComponentsInChildren<MainMenuPanel>();

        foreach (MainMenuPanel panel in panelComponents)
        {
            if (!Panels.ContainsKey(panel.Name))
            {
                Panels.Add(panel.Name, panel);
                //Debug.Log(panel.Name);
                panel.gameObject.SetActive(false);
            }
            else {
                Debug.Log("Duplicate of panel " + panel.Name);
            }
        }

        Panels[CurrentPanel].SetPanelActive(true);
    }

    public void SwitchPanels(MenuPanel panel)
    {
        if (Panels.ContainsKey(panel))
        {
            // is it possible to close the current panel? e.x. valid login data
            if (Panels[CurrentPanel].SetPanelActive(false)) { 
                // is it possible to open the next panel? e.x. skipping login
                if (Panels[panel].SetPanelActive(true))
                {
                    CurrentPanel = panel;
                }
            }
        }
        else {
            Debug.Log("Scene doesn't contain " + panel);
        }
    }

}
