using System;
using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.fruitDb;
using System.Collections.Generic;
using System.Linq;
using Battle.Model;
using Battle.View;
using Bazaar;
using Constants;
using Fruitons;
using Google.Protobuf.Collections;
using Networking;
using Serialization;
using UI.Chat;
using UI.MainMenu;
using UnityEngine;
using Util;
using KFruiton = fruiton.kernel.Fruiton;

public enum ConnectionMode { Offline, Trial, Online }

public class PlayerOptions
{
    public int LastSelectedGameMode { get; set; }
    public int LastSelectedAIMode { get; set; }
    public int LastSelectedLocalGameMode { get; set; }
}

/// <summary>
/// 
/// </summary>
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
    private static readonly int AI_FRUITONS_START_INDEX = 1000;

    #region Fields

    private Texture2D avatar;
    
    private LoggedPlayerInfo loggedPlayerInfo;
    
    private string userPassword;
    /// <summary>True if logged player checked the "stay logged in" checkbox when logging in.</summary>
    private bool? stayLoggedIn = null;
    /// <summary> The list of the Fruiton Teams of the current user. </summary>
    private FruitonTeamList fruitonTeamList;

    #endregion

    #region Properties

    public BattleViewer CurrentBattleViewer { get; set; }

    private bool isInputBlocked;

    public bool IsBattleViewerAnimating
    {
        get
        {
            if (CurrentBattleViewer == null) throw new ArgumentException("Battle viewer is not assigned.");
            return !CurrentBattleViewer.IsInputEnabled;
        }
    }

    public bool IsInputBlocked
    {
        get
        {
            return isInputBlocked;
        }
        set
        {
            isInputBlocked = value;
            if (isInputBlocked)
            {
                CurrentBattleViewer.ShowTranslationArrow();
            }
            else
            {
                CurrentBattleViewer.HideTranslationArrow();
            }
        }
    }

    /// <summary>
    /// Gets or sets the current fruiton team. (To be used in the battle.)
    /// </summary>
    public FruitonTeam CurrentFruitonTeam { get; set; }

    /// <summary>
    /// Gets or sets the fruiton team selected by local opponent (player 2).
    /// </summary>
    public FruitonTeam OfflineOpponentTeam { get; set; }

    public bool StayLoggedIn
    {
        get
        {
            return stayLoggedIn ?? (PlayerPrefs.GetInt(PlayerPrefsKeys.STAY_LOGGED_IN, 0) == 1);
        }
        set
        {
            stayLoggedIn = value;
            PlayerPrefs.SetInt(PlayerPrefsKeys.STAY_LOGGED_IN, stayLoggedIn.Value ? 1 : 0);
        }
    }

    // getters and setters for UserData, currently saved via PlayerPrefs
    public string UserName {
        get
        {
            if (loggedPlayerInfo == null)
            {
                return PlayerPrefs.GetString(PlayerPrefsKeys.USER_NAME, "default_login");
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
                userPassword = PlayerPrefs.GetString(PlayerPrefsKeys.USER_PASSWORD, "");
            }
            return userPassword;
        }
        set
        {
            userPassword = value;
            if (StayLoggedIn)
            {
                PlayerPrefs.SetString(PlayerPrefsKeys.USER_PASSWORD, value);
            }
            else
            {
                PlayerPrefs.SetString(PlayerPrefsKeys.USER_PASSWORD, "");
            }
        }
    }

    /// <summary>
    /// Clears logged player's locally stored data.
    /// </summary>
    public void Logout()
    {
        loggedPlayerInfo = null;
        UserPassword = "";
        avatar = null;
        FruitonTeamList = null;
        StayLoggedIn = false;
        Serializer.ClearPlayerLocalData();
    }

    /// <summary>
    /// Fraction logged player belongs to.
    /// </summary>
    public Fraction Fraction
    {
        get
        {
            if (loggedPlayerInfo == null)
            {
                return Fraction.None;
            }
            return loggedPlayerInfo.Fraction;
        }
        set
        {
            if (loggedPlayerInfo == null)
            {
                Debug.LogError("Cannot set fraction!");
                return;
            }
            loggedPlayerInfo.Fraction = value;
        }
    }

    /// <summary>
    /// List of all fruitons that are in the game (includes fruitons used by AI that are not available to players).
    /// </summary>
    public IEnumerable<KFruiton> AllFruitons { get; private set; }

    /// <summary>
    /// List of all fruitons that players can use in the game.
    /// </summary>
    public IEnumerable<KFruiton> AllPlayableFruitons
    {
        get
        {
            return AllFruitons.Where(fruiton => fruiton.dbId < AI_FRUITONS_START_INDEX);
        }
    }

    /// <summary>
    /// List of logged player's fruiton teams.
    /// </summary>
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

    /// <summary>
    /// List of ids of all fruitons that logged player owns.
    /// </summary>
    public List<int> AvailableFruitons { get; set; }

    /// <summary>
    /// Makes fruitons available to use for logged player.
    /// </summary>
    /// <param name="unlockedFruitons">list of ids of fruitons to be unlocked</param>
    public void UnlockFruitons(IEnumerable<int> unlockedFruitons)
    {
        if (unlockedFruitons != null)
        {
            foreach (int fruiton in unlockedFruitons)
            {
                if (!AvailableFruitons.Contains(fruiton))
                {
                    AvailableFruitons.Add(fruiton);
                    Serializer.SaveAvailableFruitons(AvailableFruitons);
                }
            }
        }
    }

    /// <summary>
    /// Logged player's avatar.
    /// </summary>
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

    /// <summary>
    /// Amount of logged player's money.
    /// </summary>
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

    /// <summary>
    /// Adjusts logged player's money by an amount.
    /// </summary>
    /// <param name="amount">amount to adjust by</param>
    public void AdjustMoney(int amount)
    {
        loggedPlayerInfo.Money = loggedPlayerInfo.Money + amount;
    }

    /// <summary>
    /// List of logged player's currently active quests.
    /// </summary>
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

    /// <summary>
    /// List of logged player's friends.
    /// </summary>
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

    /// <summary>
    /// True if player is logged in and in online mode.
    /// </summary>
    public bool IsOnline
    {
        get
        {
            return connectionMode == ConnectionMode.Online;
        }
    }

    public ConnectionMode connectionMode;

    private PlayerOptions playerOptions = new PlayerOptions();
    /// <summary>
    /// Logged player's last selected game modes.
    /// </summary>
    public PlayerOptions PlayerOptions
    {
        get
        {
            return playerOptions;
        }
        set
        {
            playerOptions = value;
        }
    }

    /// <summary>
    /// True if player isn't logged in.
    /// </summary>
    public bool IsInTrial
    {
        get
        {
            return connectionMode == ConnectionMode.Trial;
        }
    }

    #endregion


    #region Public

    public void OnMessage(WrapperMessage message)
    {
        StatusChange onlineStatusChange = message.StatusChange;
        Friends.Single(f => f.Login == onlineStatusChange.Login).Status = onlineStatusChange.Status;
    }

    /// <returns>true if the game remembers player's login data from their last session</returns>
    public bool HasRememberedUser()
    {
        return UserName != "" && UserPassword != "";
    }

    /// <summary>
    /// Logs player in if game remembers their login data and player checked "stay logged in" checkmark last time they logged in.
    /// </summary>
    public void AutomaticLogin()
    {
        if (StayLoggedIn && HasRememberedUser())
        {
            PanelManager.Instance.ShowLoadingIndicator();
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                LoginOffline();
            }
            else
            {
                AuthenticationHandler.Instance.LoginBasic(UserName, UserPassword, true);
            }
        }

    }

    /// <summary>
    /// Initializes either offline mode or trial mode depending on whether the game remeber's player's login data from last session.
    /// </summary>
    public void LoginOffline()
    {
        if (HasRememberedUser())
        {
            connectionMode = ConnectionMode.Offline;
        }
        else
        {
            connectionMode = ConnectionMode.Trial;
        }
        Initialize();
        Scenes.Load(Scenes.MAIN_MENU_SCENE);
    }

    /// <summary>
    /// Removes any cached data from previous player, stores data of new player, loads main menu scene.
    /// </summary>
    /// <param name="playerInfo">info of newly logged player</param>
    public void OnLoggedIn(LoggedPlayerInfo playerInfo)
    {
        connectionMode = ConnectionMode.Online;
        
        RemoveCachedData();
        
        loggedPlayerInfo = playerInfo;
        if (ChatController.Instance != null)
        {
            ChatController.Instance.Initialize();
        }
        TradeBazaar.Instance.Init();

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

    /// <summary>
    /// Add new friend to game manager's <see cref="Friends"/> list.
    /// </summary>
    /// <param name="friend">username of the friend to add</param>
    public void AddFriend(Friend friend)
    {
        Friends.Add(friend);
    }

    /// <summary>
    /// Removes a friend from game manager's <see cref="Friends"/> list.
    /// </summary>
    /// <param name="friend">username of the friend to remove</param>
    public void RemoveFriend(string friend)
    {
        Friends.Remove(Friends.First(f => f.Login == friend));
    }

    /// <summary>
    /// Stores logged player's data to file.
    /// </summary>
    public void SavePlayerSettings()
    {
        Serializer.SavePlayerSettings(PlayerOptions);
    }

    #endregion
    
    /// <summary>
    /// Loads last saved player's data from local cache and server, sets up fruiton database.
    /// </summary>
    private void Initialize()
    {
        FruitonDatabase = new FruitonDatabase(KernelUtils.LoadTextResource(FRUITON_DB_FILE));
        AllFruitons = ClientFruitonFactory.CreateAllKernelFruitons();

        if (IsInTrial)
        {
            AvailableFruitons = AllPlayableFruitons.Select(fruiton => fruiton.dbId).ToList();
            FruitonTeamList = new FruitonTeamList();
            loggedPlayerInfo = new LoggedPlayerInfo();
        }
        else
        {
            PlayerOptions = Serializer.LoadPlayerSettings();
            Serializer.DeserializeFruitonTeams();
            AvailableFruitons = Serializer.LoadAvailableFruitons();
        }

        if (IsOnline)
        {
            PlayerHelper.GetAllFruitonTeams(MergeTeamLists, Debug.Log);
            PlayerHelper.GetAvailableFruitons((list) => Debug.Log("Available fruitons loaded from server."), Debug.Log);
        }
    }

    /// <summary>
    /// Compares locally stored list of teams is different from the one loaded from the server and merges them together.
    /// </summary>
    /// <param name="serverTeamList">list of teams loaded from the server</param>
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
        foreach (FruitonTeam localTeam in localTeams)
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

    /// <summary>
    /// Generates a new unique name for a team. 
    /// </summary>
    /// <param name="oldName">old name of the team</param>
    /// <param name="localNames">list of names of locally stored teams</param>
    /// <param name="serverNames">list of names of teams loaded from the server</param>
    /// <returns></returns>
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

    /// <param name="a">1st team to compare</param>
    /// <param name="b">2nd team to compare</param>
    /// <returns>true if given teams have same fruitons on same positions</returns>
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

    /// <summary>
    /// Stores logged player's data to unity player prefs if they checked "stay loggen in" checkbocks when logging in.
    /// </summary>
    private void PersistIfStayLoggedIn()
    {
        if (StayLoggedIn)
        {
            Persist();
        }
    }

    /// <summary>
    /// Stores logged player's data to unity player prefs.
    /// </summary>
    private void Persist()
    {
        PlayerPrefs.SetString(PlayerPrefsKeys.USER_NAME, UserName);
        PlayerPrefs.SetString(PlayerPrefsKeys.USER_PASSWORD, AuthenticationHandler.Instance.LastPassword);
    }

    /// <summary>
    /// Removes a quest from game manager's <see cref="Quests"/> list
    /// </summary>
    /// <param name="completedQuestName">name of the quest to remove</param>
    private void CompleteQuest(string completedQuestName)
    {
        Quest completedQuest = Quests.FirstOrDefault(quest => quest.Name == completedQuestName);
        if (completedQuest == null)
        {
            throw new ArgumentNullException(
                String.Join(" ", new string[] { "Quest", completedQuestName, "not found." }));
        }
        else
        {
            Quests.Remove(completedQuest);
        }
    }

    /// <summary>
    /// Removes quests from game manager's <see cref="Quests"/> list
    /// </summary>
    /// <param name="completedQuestName">list of names of the quests to remove</param>
    public void CompleteQuests(IEnumerable<string> completedQuestsNames)
    {
        foreach (string questName in completedQuestsNames)
        {
            CompleteQuest(questName);
        }
    }
}
