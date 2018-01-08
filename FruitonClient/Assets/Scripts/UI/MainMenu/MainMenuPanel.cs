using UnityEngine;

public class MainMenuPanel : MonoBehaviour
{
    public MenuPanel Name;
    public bool Mobile = false;

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
