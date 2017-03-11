using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MenuPanel { Welcome, Login, Fraction, Main, Storage, Fridge, Pantry}

public class PanelManager : MonoBehaviour {
    public static PanelManager Instance { get; private set; }

    public Dictionary<MenuPanel, GameObject> Panels = new Dictionary<MenuPanel, GameObject>();
    public MenuPanel CurrentPanel = MenuPanel.Welcome;

    // Use this for initialization
    void Awake() {
        if (Instance == null)
        {
            Instance = this;
            FillPanelDictionary();
            //GetComponent<AudioSource>().enabled = isSoundOn;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    //fill Panels with panels in the scene
    private void FillPanelDictionary() {
        MainMenuPanel[] panelComponents = GetComponentsInChildren<MainMenuPanel>();

        foreach (MainMenuPanel panel in panelComponents)
        {
            Panels.Add(panel.Name, panel.gameObject);
            //Debug.Log(panel.Name);
            panel.gameObject.SetActive(false);
        }

        Panels[CurrentPanel].SetActive(true);
    }

    public void SwitchPanels(MenuPanel panel)
    {
        Panels[CurrentPanel].SetActive(false);
        CurrentPanel = panel;
        Panels[CurrentPanel].SetActive(true);
    }

}
