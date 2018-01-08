﻿using Networking;
using UnityEngine.UI;

public class LoginPanel : MainMenuPanel
{
    public InputField LoginName;
    public InputField LoginPassword;
    public Toggle LoginStayLoggedIn;
    public Text LoginMessageText;
    public Button LoginButton;

    private Form form;

    private void Awake()
    {
        form = gameObject.AddComponent<Form>().SetInputs(
            LoginButton,
            new FormControl("name", LoginName, Validator.Required("Please enter name")),
            new FormControl("password", LoginPassword, Validator.Required("Please enter password")),
            new FormControl(LoginStayLoggedIn),
            new FormControl(LoginButton)
        );
    }

    private void OnEnable()
    { 
        form.ResetForm();
    }

    private void Start()
    {
        GameManager.Instance.AutomaticLogin();
    }

    public void LoginGoogle()
    {
        AuthenticationHandler.Instance.LoginGoogle();
    }

    // called after pressing Login Button
    public void LoginContinue()
    {
        GameManager gameManager = GameManager.Instance;
        PanelManager panelManager = PanelManager.Instance;

        gameManager.StayLoggedIn = LoginStayLoggedIn.isOn;
        string username = LoginName.text;
        string password = LoginPassword.text;
        AuthenticationHandler.Instance.LoginBasic(username, password);
        panelManager.ShowLoadingIndicator();
    }

    // called after pressing Registration Button
    public void RegistrationContinue()
    {
        PanelManager.Instance.SwitchPanels(MenuPanel.Register);
    }

    public void LoginOffline()
    {
        AuthenticationHandler.Instance.LoginOffline();
    }
}
