using System;
using System.Collections.Generic;
using Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.MainMenu
{
    public enum MenuPanel
    {
        Login,
        Register,
        Main
    }

    /// <summary>
    /// Handles menu panel switching in login and main menu scenes
    /// </summary>
    public class PanelManager : MonoBehaviour
    {
        public static PanelManager Instance { get; private set; }

        /// <summary>
        /// Maps menu panel enums corresponding menu panels.
        /// </summary>
        public Dictionary<MenuPanel, MainMenuPanel> Panels = new Dictionary<MenuPanel, MainMenuPanel>();
        /// <summary>
        /// Currently active menu panel.
        /// </summary>
        public MenuPanel CurrentPanel;
        /// <summary>
        /// Game object that indicates that an important operation is in progress and user has to wait until it is completed.
        /// </summary>
        public GameObject LoadingIndicator;
        /// <summary>
        /// Panel used for displaying messages for user.
        /// </summary>
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

        /// <summary>
        /// Finds menu panels in the scene and adds them to the <see cref="Panels"/> dictionary
        /// </summary>
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

        /// <summary>
        /// Switches currently active panel
        /// </summary>
        /// <param name="panel">panel to switch to</param>
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

        /// <summary>
        /// Displays loading indicator.
        /// </summary>
        public void ShowLoadingIndicator()
        {
            LoadingIndicator.SetActive(true);
        }

        /// <summary>
        /// Hides loading indicator.
        /// </summary>
        public void HideLoadingIndicator()
        {
            LoadingIndicator.SetActive(false);
        }

        /// <summary>
        /// Displays message in a message window.
        /// </summary>
        /// <param name="text">text of the message</param>
        public void ShowInfoMessage(string text)
        {
            MessagePanel.ShowInfoMessage(text);
        }

        /// <summary>
        /// Displays error message in a message window.
        /// </summary>
        /// <param name="text">text of the message</param>
        public void ShowErrorMessage(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                text = "Unknown error.";
            }
            MessagePanel.ShowErrorMessage(text);
        }
    
    }
}