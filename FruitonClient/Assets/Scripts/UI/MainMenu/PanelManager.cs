using System.Collections.Generic;
using UnityEngine;

public enum MenuPanel { Welcome, Login, Fraction, Main, Storage, Fridge, Pantry, Online, Offline, FarmersMarket, LoginOffline, Register }

public class PanelManager : MonoBehaviour {
    public static PanelManager Instance { get; private set; }

    public Dictionary<MenuPanel, MainMenuPanel> Panels = new Dictionary<MenuPanel, MainMenuPanel>();
    public MenuPanel CurrentPanel = MenuPanel.Welcome;
    public GameObject LoadingIndicator;
    public MessagePanel MessagePanel;

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
    private void FillPanelDictionary()
    {
        bool mobileView = false;
        #if UNITY_ANDROID //|| UNITY_EDITOR
            //mobileView = true;
        #endif
        MainMenuPanel[] panelComponents = GetComponentsInChildren<MainMenuPanel>(true);

        foreach (MainMenuPanel panel in panelComponents)
        {
            panel.gameObject.SetActive(false);
            if (!Panels.ContainsKey(panel.Name) || (mobileView && panel.Mobile))
            {
                Panels[panel.Name] = panel;
                //Debug.Log(panel.Name);
            }
            else {
                Debug.Log("Duplicate of panel " + panel.Name);
            }
        }

        SwitchPanels(CurrentPanel);
    }

    public void SwitchPanels(MenuPanel panel)
    {
        HideLoadingIndicator();
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

    public void ShowLoadingIndicator()
    {
        LoadingIndicator.SetActive(true);
    }

    public void HideLoadingIndicator()
    {
        LoadingIndicator.SetActive(false);
    }

    public void ShowInfoMessage(string text)
    {
        MessagePanel.ShowInfoMessage(text);
    }

    public void ShowErrorMessage(string text)
    {
        MessagePanel.ShowErrorMessage(text);
    }
}
