using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scenes : MonoBehaviour
{
    public const string CHAT_SCENE = "ChatScene";
    public const string MAIN_MENU = "MainMenu";
    public const string TEAMS_MANAGEMENT_SCENE = "TeamsManagementScene";
    public const string BATTLE_SCENE = "BattleScene";

    public const string TEAM_MANAGEMENT_STATE = "teamManagementState";
    public const string ONLINE = "online";

    private static Dictionary<string, string> Parameters { get; set; }

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