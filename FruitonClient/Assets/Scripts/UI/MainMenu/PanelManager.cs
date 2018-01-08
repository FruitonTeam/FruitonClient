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
    public MenuPanel CurrentPanel = MenuPanel.Login;
    public GameObject LoadingIndicator;
    public MessagePanel MessagePanel;

    public int x = 0;

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
    public void FillPanelDictionary()
    {
        var canvas = GameObject.Find("Canvas");

        Panels = new Dictionary<MenuPanel, MainMenuPanel>();
        bool mobileView = false;
#if UNITY_ANDROID
        mobileView = true;
#endif
        MainMenuPanel[] panelComponents = canvas.GetComponentsInChildren<MainMenuPanel>(true);

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

        string isLoggedin;
        if (Scenes.Parameters == null)
            return;
        Scenes.Parameters.TryGetValue(Scenes.IS_LOGGEDIN, out isLoggedin);

        if (isLoggedin == true.ToString())
        {
            ((MainPanel) Instance.Panels[MenuPanel.Main]).EnableOnlineFeatures();
            Instance.SwitchPanels(MenuPanel.Main);
        }
        else if (isLoggedin == false.ToString())
        {
            ((MainPanel)Instance.Panels[MenuPanel.Main]).DisableOnlineFeatures();
            Instance.SwitchPanels(MenuPanel.Main);
        }
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