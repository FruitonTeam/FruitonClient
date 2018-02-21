using Networking;
using UnityEngine.UI;

public class RegisterPanel : MainMenuPanel
{
    public InputField RegisterName;
    public InputField RegisterPassword;
    public InputField RegisterPasswordRetype;
    public InputField RegisterEmail;
    public Button RegisterButton;

    private Form form;

    private void Awake()
    {
        form = gameObject.AddComponent<Form>()
            .SetInputs(
                RegisterButton,
                new FormControl("name", RegisterName,
                    Validator.Required("Please enter\nname"),
                    Validator.Regex(@"^[a-zA-Z0-9]+$", "Only letters and\nnumbers are allowed"),
                    Validator.MinLength(4, "Must have at least\n4 characters")
                ),
                new FormControl("password", RegisterPassword,
                    Validator.Required("Please enter\npassword"),
                    Validator.MinLength(6, "Must have at least\n6 characters")
                ),
                new FormControl("retypePassword", RegisterPasswordRetype,
                    Validator.Required("Please retype\nyour password")),
                new FormControl("email", RegisterEmail,
                    Validator.Required("Please enter\nemail"),
                    Validator.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", "Invalid\nemail address")
                ),
                new FormControl(RegisterButton)
            )
            .AddGlobalValidator((values, errors) =>
            {
                if (values["password"] != values["retypePassword"])
                {
                    errors["retypePassword"] = "Passwords\nmust match";
                }
            });
    }

    private void OnEnable()
    {
        form.ResetForm();
    }

    public void Register()
    {
        string login = RegisterName.text;
        string password = RegisterPassword.text;
        string email = RegisterEmail.text;

        PanelManager.Instance.ShowLoadingIndicator();
        AuthenticationHandler.Instance.Register(login, password, email);
    }

    public void BackToLogin()
    {
        PanelManager.Instance.SwitchPanels(MenuPanel.Login);
    }
}