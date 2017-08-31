using System.Collections;
using System.Collections.Generic;
using Networking;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : MainMenuPanel {

    public InputField RegisterName;
    public InputField RegisterPassword;
    public InputField RegisterPasswordRetype;
    public InputField RegisterEmail;

    public void Register()
    {
        string name = RegisterName.text;
        string password = RegisterPassword.text;
        string retypePassword = RegisterPasswordRetype.text;
        string email = RegisterEmail.text;

        ConnectionHandler.Instance.Register(name, password, email, true);
    }

    public void BackToLogin()
    {
        PanelManager.Instance.SwitchPanels(MenuPanel.Login);
    }

}
