﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scenes : MonoBehaviour
{
    public static readonly string LOGIN_SCENE = "Login";
    public static readonly string CHAT_SCENE = "ChatScene";
    public static readonly string MAIN_MENU = "MainMenu";
    public static readonly string TEAMS_MANAGEMENT_SCENE = "TeamsManagementScene";
    public static readonly string BATTLE_SCENE = "BattleScene";

    public static readonly string TEAM_MANAGEMENT_STATE = "teamManagementState";
    public static readonly string ONLINE = "online";
    public static readonly string IS_LOGGEDIN = "isLoggedin";

    public static Dictionary<string, string> Parameters { get; private set; }

    public static void Load(string sceneName, Dictionary<string, string> parameters = null)
    {
        Parameters = parameters;
        SceneManager.LoadScene(sceneName);
    }

    public static void Load(string sceneName, string paramKey, string paramValue)
    {
        Parameters = new Dictionary<string, string>();
        Parameters.Add(paramKey, paramValue);
        SceneManager.LoadScene(sceneName);
    }

    public static string GetParam(string paramKey)
    {
        if (Parameters == null) return "";
        return Parameters[paramKey];
    }

    public void LoadMainMenu()
    {
        Load(MAIN_MENU);
    }
    
}