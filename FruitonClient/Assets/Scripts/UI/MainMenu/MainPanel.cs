using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.MainMenu
{
    public class MainPanel : MainMenuPanel
    {
        public void OnlineContinue()
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Online);
        }
        public void OfflineContinue()
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Offline);
        }
        public void FarmersMarketContinue()
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.FarmersMarket);
        }
        public void StorageContinue()
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Storage);
        }
        public void FridgeContinue()
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Fridge);
        }
        public void PantryContinue()
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Pantry);
        }
        public void Logout()
        {
            PlayerPrefs.SetString("username", "");
            PlayerPrefs.SetString("userpassword", "");
            PlayerPrefs.SetInt("stayloggedin", 0);
            PanelManager.Instance.SwitchPanels(MenuPanel.Login);
        }

        public void ChangeToChatScene() 
        {
            SceneManager.LoadScene(Scenes.CHAT_SCENE);
        }
    }
}
