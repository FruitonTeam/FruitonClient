using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FridgePanel : MainMenuPanel
{
    public override bool SetPanelActive(bool toggle)
    {
        if (toggle)
        {
            Debug.Log("Initializing game manager");
            //GameManager gameManager = GameManager.Instance;
            Debug.Log("Game manager initialized, fridge will be loaded");
            SceneManager.LoadScene("Fridge");
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
            Debug.Log("Fridge loaded");
        }
        return true;
    }
}
