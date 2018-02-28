using Networking;
using UI.Form;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    /// <summary>
    /// Handles login panel in login scene.
    /// </summary>
    public class LoginPanel : MainMenuPanel
    {
        public InputField LoginName;
        public InputField LoginPassword;
        public Toggle LoginStayLoggedIn;
        public Text LoginMessageText;
        public Button LoginButton;

        private Form.Form form;

        /// <summary>
        /// Sets up login form.
        /// </summary>
        private void Awake()
        {
            form = gameObject.AddComponent<Form.Form>().SetInputs(
                LoginButton,
                new FormControl("name", LoginName, Validator.Required("Please enter\nname")),
                new FormControl("password", LoginPassword, Validator.Required("Please enter\npassword")),
                new FormControl(LoginStayLoggedIn),
                new FormControl(LoginButton)
            );
        }

        /// <summary>
        /// Resets login form.
        /// </summary>
        private void OnEnable()
        { 
            form.ResetForm();
        }

        private void Start()
        {
            bool disconnected;
            bool serverDisconnect;
            if (Scenes.TryGetGenericParam(Scenes.DISCONNECTED, out disconnected) && disconnected)
            {
                PanelManager.Instance.ShowErrorMessage("Internet connection lost. Reconnect or continue offline.");
            }
            else if (Scenes.TryGetGenericParam(Scenes.SERVER_DISCONNECT, out serverDisconnect) && serverDisconnect)
            {
                PanelManager.Instance.ShowErrorMessage("Disconnected from the server.");
            }
            else
            {
                GameManager.Instance.AutomaticLogin();            
            }
        
        }

        /// <summary>
        /// Initializes google login proccess.
        /// </summary>
        public void LoginGoogle()
        {
            AuthenticationHandler.Instance.LoginGoogle();
        }

        /// <summary>
        /// Sends login data to server.
        /// </summary>
        public void LoginContinue()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                PanelManager.Instance.ShowErrorMessage("No internet connection. " +
                                                       "Check your connection or practice in trial mode while offline.");
                return;
            }
            GameManager gameManager = GameManager.Instance;
            PanelManager panelManager = PanelManager.Instance;

            gameManager.StayLoggedIn = LoginStayLoggedIn.isOn;
            string username = LoginName.text.Trim();
            string password = LoginPassword.text;
            AuthenticationHandler.Instance.LoginBasic(username, password);
            panelManager.ShowLoadingIndicator();
        }

        /// <summary>
        /// Switches to registration panel.
        /// </summary>
        public void RegistrationContinue()
        {
            PanelManager.Instance.SwitchPanels(MenuPanel.Register);
        }

        /// <summary>
        /// Continues to main menu in offline mode.
        /// </summary>
        public void LoginOffline()
        {
            AuthenticationHandler.Instance.LoginOffline();
        }
    }
}
