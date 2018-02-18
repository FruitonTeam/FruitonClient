using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    public MenuPanel Name;
    public bool Mobile = false;
    public Button ConnectButton;

    protected virtual void Start()
    {
        if (GameManager.Instance.connectionMode != ConnectionMode.Offline)
        {
            ConnectButton.gameObject.SetActive(false);
        }
    }

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
