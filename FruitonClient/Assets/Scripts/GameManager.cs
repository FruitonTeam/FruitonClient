using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FractionNames { None, GuacamoleGuerrillas, CranberryCrusade, TzatzikiTsardom }

public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }

    // getters and setters for UserData, currently saved via PlayerPrefs
    public string UserName {
        get
        {
            return PlayerPrefs.GetString("Name", "");
        }
        set{
            PlayerPrefs.SetString("Name", value);
            IsUserValid = false;
        }
    }
    public string UserPassword
    {
        get
        {
            return PlayerPrefs.GetString("Password", "");
        }
        set
        {
            PlayerPrefs.SetString("Password", value);
            IsUserValid = false;
        }
    }
    public bool IsUserValid
    {
        get
        {
            return (PlayerPrefs.GetInt("ValidUser", 0) == 1)? true: false;
        }
        set
        {
            UserFraction = FractionNames.None;
            PlayerPrefs.SetInt("ValidUser", (value) ? 1 : 0);
        }
    }
    public FractionNames UserFraction
    {
        get
        {
            return (FractionNames) PlayerPrefs.GetInt("Fraction", (int) FractionNames.None);
        }
        set
        {
            PlayerPrefs.SetInt("Fraction", (int) value);
        }
    }

    // online check of the LoginData combination, true if online connection was possible 
    public bool OnlineLoginDataCheck()
    {
        //--TODO-- !!!!!!!!
        //+load and set UserFraction if it's already set => skip fraction panel

        //............Debug..........
        bool online = true;
        if (online)
        {
            if (UserName == "Banana" && UserPassword == "hahaha")
            {
                IsUserValid = true;
                //UserFraction = FractionNames.CranberryCrusade;
            }
            else
            {
                IsUserValid = false;
            }
            return true;
        }
        return false;
    }

    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
}
