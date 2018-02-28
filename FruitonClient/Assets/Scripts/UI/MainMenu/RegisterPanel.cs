using Networking;
using UI.Form;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class RegisterPanel : MainMenuPanel
    {
        public InputField RegisterName;
        public InputField RegisterPassword;
        public InputField RegisterPasswordRetype;
        public InputField RegisterEmail;
        public Button RegisterButton;

        private Form.Form form;

        /// <summary>
        /// Sets up registration form.
        /// </summary>
        private void Awake()
        {
            form = gameObject.AddComponent<Form.Form>()
                .SetInputs(
                    RegisterButton,
                    new FormControl("name", RegisterName,
                        Validator.Required("Please enter\nname"),
                        Validator.Regex(@"^[a-zA-Z0-9]+$", "Only letters and\nnumbers are allowed"),
                        Validator.MinLength(4, "Must have at least\n4 characters"),
                        Validator.MaxLength(20, "Cannot have more\nthan 20 characters")
                    ),
                    new FormControl("password", RegisterPassword,
                        Validator.Required("Please enter\npassword"),
                        Validator.MinLength(6, "Must have at least\n6 characters"),
                        Validator.MaxLength(50, "Cannot have more\nthan 50 characters")
                    ),
                    new FormControl("retypePassword", RegisterPasswordRetype,
                        Validator.Required("Please retype\nyour password")),
                    new FormControl("email", RegisterEmail,
                        Validator.Required("Please enter\nemail"),
                        Validator.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", "Invalid\nemail address"),
                        Validator.MaxLength(50, "Cannot have more\nthan 50 characters")
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

        /// <summary>
        /// Resets form data.
        /// </summary>
        private void OnEnable()
        {
            form.ResetForm();
        }

        /// <summary>
        /// Sends registation data to server.
        /// </summary>
        public void Register()
        {
            string login = RegisterName.text;
            string password = RegisterPassword.text;
            string email = RegisterEmail.text;

            PanelManager.Instance.ShowLoadingIndicator();
            AuthenticationHandler.Instance.Register(login, password, email);
        }

        /// <summary>
        /// Switches to login panel.
        /// </summary>
        public void BackToLogin()
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Login);
        }
    }
}