using System.Collections.Generic;
using Networking;
using UnityEngine;

public enum MenuPanel
{
    Login,
    Fraction,
    Main,
    Fridge,
    Online,
    Offline,
    FarmersMarket,
    Register
}

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance { get; private set; }

    public Dictionary<MenuPanel, MainMenuPanel> Panels = new Dictionary<MenuPanel, MainMenuPanel>();
    public MenuPanel CurrentPanel = MenuPanel.Login;
    public GameObject LoadingIndicator;
    public MessagePanel MessagePanel;

    // Use this for initialization
    void Awake()
    {
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
#if UNITY_ANDROID
        mobileView = true;
#endif
        MainMenuPanel[] panelComponents = GetComponentsInChildren<MainMenuPanel>(true);

        foreach (MainMenuPanel panel in panelComponents)
        {
            panel.gameObject.SetActive(false);
            if (!Panels.ContainsKey(panel.Name) || (mobileView && panel.Mobile))
            {
                Panels[panel.Name] = panel;
            }
        }

        if (ConnectionHandler.Instance.IsLogged())
        {
            SwitchPanels(MenuPanel.Main);
        }
        else
        {
            SwitchPanels(CurrentPanel);    
        }
    }

    public void SwitchPanels(MenuPanel panel)
    {
        HideLoadingIndicator();
        if (Panels.ContainsKey(panel))
        {
            // is it possible to close the current panel? e.x. valid login data
            if (Panels[CurrentPanel].SetPanelActive(false))
            {
                // is it possible to open the next panel? e.x. skipping login
                if (Panels[panel].SetPanelActive(true))
                {
                    CurrentPanel = panel;
                }
            }
        }
        else
        {
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
        if (string.IsNullOrEmpty(text))
        {
            text = "Unknown error.";
        }
        MessagePanel.ShowErrorMessage(text);
    }
    
}