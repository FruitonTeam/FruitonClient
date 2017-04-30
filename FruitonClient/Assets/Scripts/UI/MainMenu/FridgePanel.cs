using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FridgePanel : MainMenuPanel
{
    public override bool SetPanelActive(bool toggle)
    {
        GameManager gameManager = GameManager.Instance;
        SceneManager.LoadScene("Fridge");
        return true;
    }
}
