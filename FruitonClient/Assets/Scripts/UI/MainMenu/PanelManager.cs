using System;
using System.Collections.Generic;
using Networking;
using UI.MainMenu;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public MenuPanel CurrentPanel;
    public GameObject LoadingIndicator;
    public MessagePanel MessagePanel;

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
        Panels = new Dictionary<MenuPanel, MainMenuPanel>();
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


        if (Scenes.IsActive(Scenes.MAIN_MENU_SCENE))
        {
            CurrentPanel = MenuPanel.Main;
            if (ConnectionHandler.Instance.IsLogged())
            {
                ((MainPanel)Panels[MenuPanel.Main]).EnableOnlineFeatures();
            }
            else
            {
                ((MainPanel)Panels[MenuPanel.Main]).DisableOnlineFeatures();
            }
        }
        else if (Scenes.IsActive(Scenes.LOGIN_SCENE))
        {
            CurrentPanel = MenuPanel.Login;
        }
        else
        {
            throw new NotSupportedException("Panel manager is not supported in scene " + Scenes.GetActive());
        }

        SwitchPanels(CurrentPanel);
    }

    public void SwitchPanels(MenuPanel panel)
    {
        HideLoadingIndicator();
        if (Panels.ContainsKey(panel))
        {
            if (Panels.ContainsKey(CurrentPanel))
            {
                Panels[CurrentPanel].SetPanelActive(false);
            }

            Panels[panel].SetPanelActive(true);
            CurrentPanel = panel;
        }
        else
        {
            throw new InvalidOperationException("Scene " + Scenes.GetActive() + " does not contain panel " + panel);
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