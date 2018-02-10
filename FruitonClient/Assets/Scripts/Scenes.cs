using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scenes : MonoBehaviour
{
    public static readonly string LOGIN_SCENE = "Login";
    public static readonly string MAIN_MENU_SCENE = "MainMenu";
    public static readonly string TEAMS_MANAGEMENT_SCENE = "TeamsManagementScene";
    public static readonly string BATTLE_SCENE = "BattleScene";
    public static readonly string FRACTION_SCENE = "FractionScene";

    public static readonly string TEAM_MANAGEMENT_STATE = "teamManagementState";
    public static readonly string BATTLE_TYPE = "battleType";
    public static readonly string GAME_MODE = "gameMode";
    public static readonly string AI_TYPE = "aiType";

    public static Dictionary<string, string> Parameters { get; private set; }

    public static void Load(string sceneName, Dictionary<string, string> parameters = null)
    {
        Parameters = parameters;
        SceneManager.LoadScene(sceneName);
    }

    public static void Load(string sceneName, string paramKey, string paramValue)
    {
        Parameters = new Dictionary<string, string> {{paramKey, paramValue}};
        SceneManager.LoadScene(sceneName);
    }

    public static string GetParam(string paramKey)
    {
        if (Parameters == null) return "";
        return Parameters[paramKey];
    }

    public void LoadMainMenu()
    {
        Load(MAIN_MENU_SCENE);
    }

    public static bool IsActive(string sceneName)
    {
        return SceneManager.GetActiveScene().name == sceneName;
    }

    public static string GetActive()
    {
        return SceneManager.GetActiveScene().name;
    }
}