using System;
using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.fruitDb;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Networking;
using UI.Chat;
using UnityEngine;
using Util;
using KFruiton = fruiton.kernel.Fruiton;

public enum FractionNames { None, GuacamoleGuerrillas, CranberryCrusade, TzatzikiTsardom }

public class PlayerOptions
{
    public int LastSelectedGameMode { get; set; }
}

public class GameManager : IOnMessageListener
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameManager();
            }
            return instance;
        }
    }

    private static readonly string FRUITON_DB_FILE = "FruitonDb.json";

    private static readonly string LOGIN_KEY = "username";
    private static readonly string PASSWORD_KEY = "userpassword";
    
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
                return PlayerPrefs.GetString(LOGIN_KEY, "default_login");
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
                userPassword = PlayerPrefs.GetString(PASSWORD_KEY, "");
            }
            return userPassword;
        }
        set
        {
            userPassword = value;
            if (StayLoggedIn)
            {
                PlayerPrefs.SetString(PASSWORD_KEY, value);
            }
            else
            {
                PlayerPrefs.SetString(PASSWORD_KEY, "");
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

    public void UnlockFruitons(IEnumerable<int> unlockedFruitons)
    {
        if (unlockedFruitons != null)
        {
            foreach (int fruiton in unlockedFruitons)
            {
                if (!AvailableFruitons.Contains(fruiton))
                {
                    AvailableFruitons.Add(fruiton);
                }
            }
        }
    }

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

    public void AddMoney(int toAdd)
    {
        loggedPlayerInfo.Money = loggedPlayerInfo.Money + toAdd;
    }

    public RepeatedField<Quest> Quests
    {
        get
        {
            if (loggedPlayerInfo != null)
            {
                return loggedPlayerInfo.Quests;
            }
            return new RepeatedField<Quest>();
        }
    }

    public RepeatedField<Friend> Friends
    {
        get
        {
            if (loggedPlayerInfo != null)
            {
                return loggedPlayerInfo.FriendList;
            }
            return new RepeatedField<Friend>();
        }
    }

    public bool IsOnline { get; private set; }

    public PlayerOptions PlayerOptions { get; set; }

    #endregion


    #region Public
    
    public void OnMessage(WrapperMessage message)
    {
        OnlineStatusChange onlineStatusChange = message.OnlineStatusChange;
        Friends.Single(f => f.Login == onlineStatusChange.Login).Status = onlineStatusChange.Status;
    }

    public bool HasRememberedUser()
    {
        return UserName != "" && UserPassword != "";
    }

    public void AutomaticLogin()
    {
        if (StayLoggedIn && HasRememberedUser())
        {
            PanelManager.Instance.ShowLoadingIndicator();
            AuthenticationHandler.Instance.LoginBasic(UserName, UserPassword);
        }
    }

    public void LoginOffline()
    {
        IsOnline = false;
        Initialize();
        Scenes.Load(Scenes.MAIN_MENU_SCENE);
    }

    public void OnLoggedIn(LoggedPlayerInfo playerInfo)
    {
        IsOnline = true;
        
        RemoveCachedData();
        
        loggedPlayerInfo = playerInfo;
        if (ChatController.Instance != null)
        {
            ChatController.Instance.Init();
        }

        Initialize();
        PersistIfStayLoggedIn();

        Scenes.Load(Scenes.MAIN_MENU_SCENE);
    }

    public void AddFriend(Friend friend)
    {
        Friends.Add(friend);
    }

    public void SavePlayerSettings()
    {
        Serializer.SavePlayerSettings(PlayerOptions);
    }

    #endregion
    
    private void Initialize()
    {
        PlayerOptions = Serializer.LoadPlayerSettings();
        Serializer.DeserializeFruitonTeams();
        FruitonDatabase = new FruitonDatabase(KernelUtils.LoadTextResource(FRUITON_DB_FILE));
        AllFruitons = ClientFruitonFactory.CreateAllKernelFruitons();
        AvailableFruitons = Serializer.LoadAvailableFruitons();

        if (IsOnline)
        {
            PlayerHelper.GetAllFruitonTeams(ints =>
                {
                    fruitonTeamList = ints;
                },
                Debug.Log);
        }
    }
    
    private void RemoveCachedData()
    {
        avatar = null;
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
        PlayerPrefs.SetString(LOGIN_KEY, UserName);
        PlayerPrefs.SetString(PASSWORD_KEY, AuthenticationHandler.Instance.LastPassword);
    }
}
