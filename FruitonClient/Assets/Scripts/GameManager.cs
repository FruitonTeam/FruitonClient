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
    public int LastSelectedAIMode { get; set; }
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
            return stayLoggedIn ?? (PlayerPrefs.GetInt(PlayerPrefsKeys.StayLoggedIn, 0) == 1);
        }
        set
        {
            stayLoggedIn = value;
            PlayerPrefs.SetInt(PlayerPrefsKeys.StayLoggedIn, 1);
        }
    }

    // getters and setters for UserData, currently saved via PlayerPrefs
    public string UserName {
        get
        {
            if (loggedPlayerInfo == null)
            {
                return PlayerPrefs.GetString(PlayerPrefsKeys.UserName, "default_login");
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
                userPassword = PlayerPrefs.GetString(PlayerPrefsKeys.UserPassword, "");
            }
            return userPassword;
        }
        set
        {
            userPassword = value;
            if (StayLoggedIn)
            {
                PlayerPrefs.SetString(PlayerPrefsKeys.UserPassword, value);
            }
            else
            {
                PlayerPrefs.SetString(PlayerPrefsKeys.UserPassword, "");
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

    public Fraction Fraction
    {
        get { return loggedPlayerInfo.Fraction; }
    }
    
    [Obsolete("Should be deleted, but first check if it does not harm anything.")]
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
            return fruitonTeamList ?? (fruitonTeamList = new FruitonTeamList());
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

        if (loggedPlayerInfo.Fraction == Fraction.None)
        {
            var param = new Dictionary<string, string>
            {
                {Scenes.BATTLE_TYPE, BattleType.TutorialBattle.ToString()},
                {Scenes.GAME_MODE, GameMode.Standard.ToString()}
            };
            Scenes.Load(Scenes.BATTLE_SCENE, param);
        }
        else
        {
            Scenes.Load(Scenes.MAIN_MENU_SCENE);
        }
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
            PlayerHelper.GetAllFruitonTeams(MergeTeamLists, Debug.Log);
        }
    }

    private void MergeTeamLists(FruitonTeamList serverTeamList)
    {
        RepeatedField<FruitonTeam> serverTeams = serverTeamList.FruitonTeams;
        RepeatedField<FruitonTeam> localTeams = FruitonTeamList.FruitonTeams;
        var serverTeamDb = new Dictionary<string, FruitonTeam>(serverTeams.Count);
        foreach (FruitonTeam serverTeam in serverTeams)
        {
            serverTeamDb.Add(serverTeam.Name, serverTeam);
        }
        var localTeamNames = new HashSet<string>(localTeams.Select(x => x.Name));
        foreach (FruitonTeam localTeam in FruitonTeamList.FruitonTeams)
        {
            FruitonTeam serverTeam;
            bool areNamesSame = serverTeamDb.TryGetValue(localTeam.Name, out serverTeam);
            // Exactly the same team
            if (areNamesSame && AreTeamsEqual(localTeam, serverTeam))
            {
                continue;
            }
            
            // Name clash, different teams
            if (areNamesSame)
            {
                localTeam.Name = GenerateNewName(localTeam.Name, localTeamNames, serverTeamDb);
            }

            // Upload changes
            serverTeamDb.Add(localTeam.Name, localTeam);
            serverTeams.Add(localTeam);
            PlayerHelper.UploadFruitonTeam(localTeam, Debug.Log, Debug.Log);
        }

        FruitonTeamList = serverTeamList;
        Serializer.SerializeFruitonTeams();
    }

    private static string GenerateNewName(
        string oldName, 
        ICollection<string> localNames, 
        IDictionary<string, FruitonTeam> serverNames
    )
    {
        for (int i = 1; i < int.MaxValue; i++)
        {
            string newName = oldName + " (" + i + ")";
            if (!localNames.Contains(newName) && !serverNames.ContainsKey(newName))
                return newName;
        }
        throw new IndexOutOfRangeException("Could not find suitable team name");
    }

    private static bool AreTeamsEqual(FruitonTeam a, FruitonTeam b)
    {
        if (a.FruitonIDs.Count != b.FruitonIDs.Count
            || a.Positions.Count != b.Positions.Count)
        {
            return false;
        }
        for (var i = 0; i < a.FruitonIDs.Count; i++)
        {
            if (a.FruitonIDs[i] != b.FruitonIDs[i])
                return false;
        }

        for (var i = 0; i < a.Positions.Count; i++)
        {
            if (a.Positions[i].X != b.Positions[i].X
                || a.Positions[i].Y != b.Positions[i].Y)
            {
                return false;
            }
        }

        return true;
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
        PlayerPrefs.SetString(PlayerPrefsKeys.UserName, UserName);
        PlayerPrefs.SetString(PlayerPrefsKeys.UserPassword, AuthenticationHandler.Instance.LastPassword);
    }
}
