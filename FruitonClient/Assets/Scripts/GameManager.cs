using System;
using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.fruitDb;
using System.Collections.Generic;
using Networking;
using UnityEngine;
using Util;
using KFruiton = fruiton.kernel.Fruiton;

public enum FractionNames { None, GuacamoleGuerrillas, CranberryCrusade, TzatzikiTsardom }

public class GameManager : MonoBehaviour, IOnMessageListener
{    
    public static GameManager Instance { get; private set; }

    private static readonly string FRUITON_DB_FILE = "FruitonDb.json";
    
    #region Fields

    private Texture2D avatar;
    
    private LoggedPlayerInfo loggedPlayerInfo;
    
    private string userPassword;
    private bool? stayLoggedIn;
    /// <summary> The list of the Fruiton Teams of the current user. </summary>
    private FruitonTeamList fruitonTeamList;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the current fruiton team. (To be used in the battle.)
    /// </summary>
    public FruitonTeam CurrentFruitonTeam { get; set; }

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
            if (loggedPlayerInfo == null)
            {
                return PlayerPrefs.GetString("username", "");
            }
            return loggedPlayerInfo.Login;
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
            return PlayerPrefs.GetInt("ValidUser", 0) == 1;
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

    public IEnumerable<KFruiton> AllFruitons { get; private set; }

    public FruitonTeamList FruitonTeamList
    {
        get
        {
            if (fruitonTeamList == null)
            {
                fruitonTeamList = new FruitonTeamList();
            }
            return fruitonTeamList;
        }
        set
        {
            fruitonTeamList = value; 
        }
    }

    public FruitonDatabase FruitonDatabase { get; set; }

    public List<int> AvailableFruitons { get; set; }

    public Texture2D Avatar
    {
        get
        {
            if (avatar == null)
            {
                if (loggedPlayerInfo != null && !string.IsNullOrEmpty(loggedPlayerInfo.Avatar))
                {
                    avatar = new Texture2D(0, 0);
                    avatar.LoadImage(Convert.FromBase64String(loggedPlayerInfo.Avatar));
                }
                else
                {
                    avatar = Resources.Load<Texture2D>("Images/avatar_default");
                }
            }
            return avatar;
        }
    }

    public int Money
    {
        get
        {
            if (loggedPlayerInfo != null)
            {
                return loggedPlayerInfo.Money;
            }
            return -1; // if we return -1 it will be clear that something is wrong
        }
    }

    public bool PlayerInfoInitialized
    {
        get
        {
            return loggedPlayerInfo != null;
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

    public void Initialize()
    {
        Serializer.DeserializeFruitonTeams();
                
        FruitonDatabase = new FruitonDatabase(KernelUtils.LoadTextResource(FRUITON_DB_FILE));
        AllFruitons = ClientFruitonFactory.CreateAllKernelFruitons();
        AvailableFruitons = Serializer.LoadAvailableFruitons();

        PlayerHelper.GetAllFruitonTeams(ints =>
        {
            fruitonTeamList = ints;
        },
        Debug.Log);
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

    public void OnMessage(WrapperMessage message)
    {
        loggedPlayerInfo = message.LoggedPlayerInfo;
        PersistIfStayLoggedIn();
    }
    
    private void PersistIfStayLoggedIn()
    {
        if (StayLoggedIn)
        {
            Persist();
        }
    }

    private void Persist()
    {
        PlayerPrefs.SetString("username", UserName);
        PlayerPrefs.SetString("userpassword", userPassword);
    }
    
}
