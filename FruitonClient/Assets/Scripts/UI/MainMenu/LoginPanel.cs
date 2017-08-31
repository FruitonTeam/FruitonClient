using System.Collections;
using System.Collections.Generic;
using Networking;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MainMenuPanel
{
    public InputField LoginName;
    public InputField LoginPassword;
    public Toggle LoginStayLoggedIn;
    public Text LoginMessageText;

    public enum LoginMessage { ValidUser, NotValidUser, NoConnection}

    // checks whether the LoginData combination is valid
    public LoginMessage CheckLoginData()
    {
        GameManager gameManager = GameManager.Instance;
        
   
        if (!GameManager.Instance.IsUserValid)
        {
            if (GameManager.Instance.OnlineLoginDataCheck())
            {
                if (GameManager.Instance.IsUserValid)
                    return LoginMessage.ValidUser;
                return LoginMessage.NotValidUser;
            }
            return LoginMessage.NoConnection;
        }
        return LoginMessage.ValidUser;
    }

    

    // called after pressing Login Button
    public void LoginContinue()
    {
        GameManager gameManager = GameManager.Instance;
        PanelManager panelManager = PanelManager.Instance;
        ConnectionHandler connectionHandler = ConnectionHandler.Instance;

        gameManager.StayLoggedIn = LoginStayLoggedIn.isOn;
        string name = LoginName.text;
        string password = LoginPassword.text;

        connectionHandler.LoginBasic(name, password, true);

    }

    // called after pressing Registration Button
    public void RegistrationContinue()
    {
        PanelManager.Instance.SwitchPanels(MenuPanel.Register);
    }
}
