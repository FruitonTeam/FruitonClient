using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FractionNames { None, GuacamoleGuerrillas, CranberryCrusade, TzatzikiTsardom }

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    #region Fields

    private string userName = null;
    private string userPassword = null;
    private bool? stayLoggedIn;

    #endregion

    #region Properties

    public bool StayLoggedIn
    {
        get
        {
            return stayLoggedIn ?? (PlayerPrefs.GetInt("stayloggedin", 0) == 1);
        }
        set
        {
            stayLoggedIn = value;
            PlayerPrefs.SetInt("stayloggedin", 1);
        }
    }

    // getters and setters for UserData, currently saved via PlayerPrefs
    public string UserName {
        get
        {
            if (userName == null)
            {
                userName = PlayerPrefs.GetString("username", "");
            }
            return userName;
        }
        set {
            userName = value;
            if (StayLoggedIn)
            {
                PlayerPrefs.SetString("username", userName);
            }
            else
            {
                PlayerPrefs.SetString("username", "");
            }
            IsUserValid = false;
        }
    }

    public string UserPassword
    {
        get
        {
            if (userPassword == null)
            {
                userPassword = PlayerPrefs.GetString("userpassword", "");
            }
            return userPassword;
        }
        set
        {
            userPassword = value;
            if (StayLoggedIn)
            {
                PlayerPrefs.SetString("userpassword", value);
            }
            else
            {
                PlayerPrefs.SetString("userpassword", "");
            }
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

    #endregion


    #region Public 

    // online check of the LoginData combination, true if online connection was possible 
    public bool OnlineLoginDataCheck()
    {
        //--TODO-- !!!!!!!!
        //+load and set UserFraction if it's already set => skip fraction panel

        //............Debug..........
        bool online = true;
        if (online)
        {
            //ConnectionHandler.Instance.LoginCasual(UserName, UserPassword, true);
        }
        return false;
    }

    public bool HasRememberedUser()
    {
        return (UserName != "" && UserPassword != "");
    }

    public void LoginOffline()
    {

    }

    #endregion

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
