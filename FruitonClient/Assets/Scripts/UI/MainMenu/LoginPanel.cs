using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MainMenuPanel
{
    public InputField LoginName;
    public InputField LoginPassword;
    public Toggle LoginStayLoggedIn;
    public Text LoginMessageText;

    public enum LoginMessage { ValidUser, NotValidUser, NoConnection}

    //public override bool SetPanelActive(bool toggle)
    //{
    //    if (toggle)
    //    {
    //        LoginMessageText.text = "";
    //        switch (CheckLoginData())
    //        {
    //            case LoginMessage.ValidUser:
    //                if(GameManager.Instance.UserFraction != FractionNames.None)
    //                    PanelManager.Instance.SwitchPanels(MenuPanel.Main);
    //                else
    //                    PanelManager.Instance.SwitchPanels(MenuPanel.Fraction);
    //                return false;
    //            case LoginMessage.NotValidUser:
    //                gameObject.SetActive(true);
    //                return true;
    //            case LoginMessage.NoConnection:
    //                gameObject.SetActive(true);
    //                return true;
    //        }
    //    }
    //    else
    //    {
    //        gameObject.SetActive(false);
    //    }
    //    return true;
    //}

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

        connectionHandler.LoginCasual(name, password, true);

        //switch (CheckLoginData())
        //{
        //    case LoginMessage.ValidUser:
        //        if (GameManager.Instance.UserFraction != FractionNames.None)
        //            PanelManager.Instance.SwitchPanels(MenuPanel.Main);
        //        else
        //            PanelManager.Instance.SwitchPanels(MenuPanel.Fraction);
        //        return;
        //    case LoginMessage.NotValidUser:
        //        LoginMessageText.text = "Rotten bananas! Wrong Name or Password...";
        //        //Debug.Log("Not valid LoginData");
        //        return;
        //    case LoginMessage.NoConnection:
        //        LoginMessageText.text = "No internet connection, no bananas...";
        //        //Debug.Log("No connection to check login data");
        //        return;
        //}
    }

    // called after pressing Registration Button
    public void RegistrationContinue()
    {
        //--TODO--
        
        //PanelManager.Instance.SwitchPanels(MenuPanel.Fraction);
    }
}
