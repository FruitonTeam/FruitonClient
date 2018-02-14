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
    public static readonly string DRAFT_SCENE = "Draft";

    public static readonly string TEAM_MANAGEMENT_STATE = "teamManagementState";
    public static readonly string BATTLE_TYPE = "battleType";
    public static readonly string GAME_MODE = "gameMode";
    public static readonly string PICK_MODE = "pickMode";
    public static readonly string AI_TYPE = "aiType";
    public static readonly string GAME_READY_MSG = "gameReadyMsg";

    public static Dictionary<string, string> Parameters { get; private set; }
    public static Dictionary<string, object> ObjParams { get; private set; }

    public static void Load(
        string sceneName, 
        Dictionary<string, string> parameters = null, 
        Dictionary<string, object> objParams = null
    )
    {
        Parameters = parameters;
        ObjParams = objParams;
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

    public static bool TryGetParam(string paramKey, out string value)
    {
        value = "";
        if (Parameters == null) return false;

        return Parameters.TryGetValue(paramKey, out value);
    }

    public static object GetObjParam(string paramKey)
    {
        if (ObjParams == null) return null;
        return ObjParams[paramKey];
    }

    public static bool TryGetObjParam(string paramKey, out object value)
    {
        value = null;
        if (ObjParams == null) return false;

        return ObjParams.TryGetValue(paramKey, out value);
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