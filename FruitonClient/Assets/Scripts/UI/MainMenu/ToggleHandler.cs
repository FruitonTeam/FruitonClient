using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class ToggleHandler : MonoBehaviour
    {
        public Button loginButton;

        public void OnValueChanged(bool on)
        {
            loginButton.Select();
        }
    }
}
