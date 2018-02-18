using UnityEngine;
using UnityEngine.UI;

public class ToggleHandler : MonoBehaviour
{
    public Button loginButton;

    public void OnValueChanged(bool on)
    {
        loginButton.Select();
    }
}
