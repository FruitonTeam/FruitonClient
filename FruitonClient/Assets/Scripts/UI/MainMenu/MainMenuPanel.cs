using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class MainMenuPanel : MonoBehaviour
    {
        public MenuPanel Name;
        public bool Mobile = false;
        public Button ConnectButton;

        public virtual void SetPanelActive(bool toggle)
        {
            if (toggle)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
