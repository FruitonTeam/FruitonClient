using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPanel : MonoBehaviour {

    public MenuPanel Name;
    public bool Mobile = false;

    public virtual bool SetPanelActive(bool toggle) {
        if (toggle) {
            gameObject.SetActive(true);
        } else {
            gameObject.SetActive(false);
        }
        return true;
    }
}
